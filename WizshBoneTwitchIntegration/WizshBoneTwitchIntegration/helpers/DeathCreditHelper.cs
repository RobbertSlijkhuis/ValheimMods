using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Credits a death of the streamer to the viewer and redeem that caused it (counted by
    /// <see cref="LeaderboardHelper.RecordDeath"/>). Only the killing blow counts, and only what
    /// can be matched exactly is credited - everything else (vanilla kills, other players,
    /// drowning with no redeem cause, hazards owned by another co-op client) stays uncredited.
    ///
    /// Every damage that reaches the local player gets its credit resolved in
    /// <c>Character.ApplyDamage</c>'s prefix (<see cref="OnApplyDamagePrefix"/>) and committed in
    /// its postfix only if that hit became <c>m_lastHit</c> (<see cref="OnApplyDamagePostfix"/>) -
    /// so the committed credit always belongs to the last damage that landed, and
    /// <see cref="OnLocalPlayerDeath"/> just reads it. Sources, first match wins: a redeem
    /// creature that attacked, a drowning cause, a hazard that hit the player, an active status
    /// effect, a redeem weather zone (freezing), a terrain edit/reset (fall).
    ///
    /// Everything here runs on the streamer's own client only (never an RPC). Redeem objects carry
    /// their redeemer in the ZDO (<see cref="TwitchBasePersistentData.SetRedeemer"/>), so the data
    /// is available to every client regardless of who owns the object.
    /// </summary>
    internal static class DeathCreditHelper
    {
        internal readonly struct Credit
        {
            public readonly string Id;
            public readonly string Title;

            public Credit(string id, string title)
            {
                Id = id;
                Title = title;
            }

            public override string ToString() => $"{Id} / '{Title}'";
        }

        private class TimedCredit
        {
            public Credit Credit;
            public float Time;
            public bool FallEligible;
        }

        private class PendingHazardHit
        {
            public Vector3 Point;
            public Vector3 Dir;
            public HitData.HitType Type;
            public Credit Credit;
            public float Time;
        }

        private class SeTag
        {
            public Credit Credit;
            public HitData.HitType? HitType;
            public bool DrainsStamina;
            public float Time;
        }

        private class ShipHit
        {
            public Credit? Credit;
            public float Time;
        }

        // A fall within this long of a terrain edit/reset is credited to it (edits are capped at
        // 8 m, so any fall they cause happens within a couple of seconds).
        private const float TerrainFallWindow = 5f;

        // A swim spell only takes a redeem cause (terrain edit, knockback, boat lost) from this
        // long before the swimming started - running 8 s first and then jumping in is not a cause.
        private const float SwimCauseWindow = 3f;

        // Swimming counts as one continuous spell while ticks keep coming within this long.
        private const float SwimGap = 1f;
        private const float SpellActiveWindow = 1.5f;

        private const float PendingHazardLifetime = 2f;

        private static readonly AccessTools.FieldRef<Character, HitData> s_lastHitRef =
            AccessTools.FieldRefAccess<Character, HitData>("m_lastHit");

        private static readonly FieldInfo s_envZoneTriggeredField = AccessTools.Field(typeof(EnvZone), "s_triggered");

        // Hazards (Aoe/Projectile/ImpactEffect/Fire) wrap their Damage call in Begin/EndHazard.
        private static readonly Stack<Credit?> s_hazardStack = new Stack<Credit?>();
        private static readonly List<PendingHazardHit> s_pending = new List<PendingHazardHit>();
        private static HitData s_matchedHit;
        private static Credit? s_matchedCredit;

        private static HitData s_candidateHit;
        private static Credit? s_candidate;
        private static string s_candidateSource;

        private static HitData s_committedHit;
        private static Credit? s_committed;
        private static string s_committedSource;

        private static readonly Dictionary<int, SeTag> s_seTags = new Dictionary<int, SeTag>();
        private static readonly Dictionary<ZDOID, ShipHit> s_shipLastHit = new Dictionary<ZDOID, ShipHit>();

        private static TimedCredit s_terrain;
        private static TimedCredit s_knockback;
        private static TimedCredit s_boatLoss;

        private static float s_lastSwimTick = -100f;
        private static TimedCredit s_spellCause;

        /// <summary>Called on every Game.Awake so a fresh world/relog starts clean.</summary>
        public static void Reset()
        {
            s_hazardStack.Clear();
            s_pending.Clear();
            s_matchedHit = null;
            s_matchedCredit = null;
            s_candidateHit = null;
            s_candidate = null;
            s_committedHit = null;
            s_committed = null;
            s_seTags.Clear();
            s_shipLastHit.Clear();
            s_terrain = null;
            s_knockback = null;
            s_boatLoss = null;
            s_lastSwimTick = -100f;
            s_spellCause = null;
        }

        // ── hazards ──────────────────────────────────────────────────────────

        /// <summary>
        /// Called by a prefix on every hazard method that deals damage: notes whose hazard this is
        /// (read from the hazard's own tag) for the <c>Character.Damage</c> calls inside it.
        /// </summary>
        public static void BeginHazard(Component source)
        {
            Credit? credit = null;
            TwitchBasePersistentData tag = source != null ? source.GetComponentInParent<TwitchBasePersistentData>() : null;

            if (tag != null && tag.HasRedeemer)
                credit = new Credit(tag.RedeemerId, tag.RedeemerRawTitle);

            s_hazardStack.Push(credit);
        }

        /// <summary>Paired finalizer, so an exception inside the hazard method can't leave a stale note.</summary>
        public static void EndHazard()
        {
            if (s_hazardStack.Count > 0)
                s_hazardStack.Pop();
        }

        private static Credit? CurrentHazard => s_hazardStack.Count > 0 ? s_hazardStack.Peek() : null;

        /// <summary>
        /// <c>Character.Damage</c> (the caller side, before the RPC): records the hazard's redeemer
        /// together with the hit's point/direction/type, which survive the network call exactly, so
        /// <see cref="MatchHazard"/> can pair them up on the receiving side.
        /// </summary>
        public static void NoteHazardHit(Character target, HitData hit)
        {
            if (hit == null || !(target is Player))
                return;

            Credit? hazard = CurrentHazard;
            if (!hazard.HasValue)
                return;

            float now = Time.time;
            s_pending.RemoveAll(p => now - p.Time > PendingHazardLifetime);
            s_pending.Add(new PendingHazardHit { Point = hit.m_point, Dir = hit.m_dir, Type = hit.m_hitType, Credit = hazard.Value, Time = now });
        }

        /// <summary>
        /// <c>Character.RPC_Damage</c> on the local player (before the hit is modified): pairs the
        /// incoming hit with a pending hazard record. Hazards owned by another co-op client never
        /// left a record here, so those hits simply stay uncredited.
        /// </summary>
        public static void MatchHazard(Character target, HitData hit)
        {
            if (hit == null || target != Player.m_localPlayer)
                return;

            s_matchedHit = null;
            s_matchedCredit = null;

            float now = Time.time;

            for (int i = s_pending.Count - 1; i >= 0; i--)
            {
                PendingHazardHit pending = s_pending[i];

                if (now - pending.Time > PendingHazardLifetime)
                    continue;

                if (pending.Type == hit.m_hitType && pending.Point == hit.m_point && pending.Dir == hit.m_dir)
                {
                    s_matchedHit = hit;
                    s_matchedCredit = pending.Credit;
                    s_pending.RemoveAt(i);
                    return;
                }
            }
        }

        // ── resolving each hit ───────────────────────────────────────────────

        public static void OnApplyDamagePrefix(Character target, HitData hit)
        {
            if (hit == null || target != Player.m_localPlayer)
                return;

            s_candidate = Resolve(hit, out s_candidateSource);
            s_candidateHit = hit;
        }

        public static void OnApplyDamagePostfix(Character target, HitData hit)
        {
            if (hit == null || target != Player.m_localPlayer || !ReferenceEquals(hit, s_candidateHit))
                return;

            // Only damage that actually landed (became m_lastHit) counts; zero-damage or post-death
            // hits never do.
            if (!ReferenceEquals(s_lastHitRef(target), hit))
                return;

            s_committedHit = hit;
            s_committed = s_candidate;
            s_committedSource = s_candidateSource;

            // A knockback is only a cause for a following swim if a redeem delivered it; a later
            // vanilla knockback replaces it as the latest.
            if (hit.m_pushForce > 0f)
                s_knockback = s_candidate.HasValue ? new TimedCredit { Credit = s_candidate.Value, Time = Time.time } : null;
        }

        private static Credit? Resolve(HitData hit, out string source)
        {
            source = null;

            Credit? credit = CreatureCredit(hit);
            if (credit.HasValue)
            {
                source = "creature";
                return credit;
            }

            HitData.HitType type = hit.m_hitType;

            if (type == HitData.HitType.Drowning)
                return DrowningCredit(out source);

            if (ReferenceEquals(hit, s_matchedHit) && s_matchedCredit.HasValue)
            {
                source = "hazard";
                return s_matchedCredit;
            }

            credit = StatusEffectCredit(type);
            if (credit.HasValue)
            {
                source = "status effect";
                return credit;
            }

            if (type == HitData.HitType.Freezing)
            {
                credit = WeatherCredit();
                if (credit.HasValue)
                {
                    source = "weather";
                    return credit;
                }
            }

            if (type == HitData.HitType.Fall && s_terrain != null && s_terrain.FallEligible && Time.time - s_terrain.Time <= TerrainFallWindow)
            {
                source = "terrain edit";
                return s_terrain.Credit;
            }

            return null;
        }

        /// <summary>
        /// A creature a redeem spawned (never a chat-claimed wild one) landed the hit. The attacker
        /// id travels inside the hit, so this holds whoever owns the creature.
        /// </summary>
        private static Credit? CreatureCredit(HitData hit)
        {
            Character attacker = hit.GetAttacker();

            if (attacker == null || attacker is Player)
                return null;

            TwitchCreatureClaim claim = attacker.GetComponent<TwitchCreatureClaim>();
            if (claim == null || !claim.m_isSpawn)
                return null;

            TwitchBasePersistentData tag = attacker.GetComponent<TwitchBasePersistentData>();
            if (tag == null || !tag.HasRedeemer)
                return null;

            return new Credit(tag.RedeemerId, tag.RedeemerRawTitle);
        }

        private static Credit? WeatherCredit()
        {
            EnvZone zone = s_envZoneTriggeredField?.GetValue(null) as EnvZone;
            TwitchBasePersistentData tag = zone != null ? zone.GetComponent<TwitchBasePersistentData>() : null;

            return tag != null && tag.HasRedeemer ? new Credit(tag.RedeemerId, tag.RedeemerRawTitle) : (Credit?)null;
        }

        // ── status effects ───────────────────────────────────────────────────

        /// <summary>
        /// A redeem applied a status effect: remembers whose it is, for the damage it deals
        /// (poison, burning, ...) and for stamina drain while swimming. A newer application by the
        /// streamer or a test event drops the old tag.
        /// </summary>
        public static void NoteStatusEffect(StatusEffect source, int hash, CustomRewardEvent customRewardEvent)
        {
            if (!RedeemerTagHelper.TryResolve(customRewardEvent, out string id, out string title))
            {
                s_seTags.Remove(hash);
                return;
            }

            HitData.HitType? hitType = null;

            if (source is SE_Poison)
                hitType = HitData.HitType.Poisoned;
            else if (source is SE_Burning)
                hitType = HitData.HitType.Burning;
            else if (source is SE_Stats generic && generic.m_hitType != HitData.HitType.Undefined)
                hitType = generic.m_hitType;

            bool drainsStamina = source is SE_Stats stats
                && (stats.m_staminaOverTime < 0f || stats.m_swimStaminaUseModifier > 0f || stats.m_staminaRegenMultiplier < 1f);

            if (hitType == null && !drainsStamina)
            {
                s_seTags.Remove(hash);
                return;
            }

            s_seTags[hash] = new SeTag { Credit = new Credit(id, title), HitType = hitType, DrainsStamina = drainsStamina, Time = Time.time };
        }

        private static bool IsActive(int hash)
        {
            Player player = Player.m_localPlayer;
            return player != null && player.GetSEMan().HaveStatusEffect(hash);
        }

        private static Credit? StatusEffectCredit(HitData.HitType type)
        {
            SeTag best = null;

            foreach (KeyValuePair<int, SeTag> entry in s_seTags)
            {
                if (entry.Value.HitType != type || !IsActive(entry.Key))
                    continue;

                if (best == null || entry.Value.Time > best.Time)
                    best = entry.Value;
            }

            return best != null ? best.Credit : (Credit?)null;
        }

        private static SeTag ActiveStaminaDrainTag()
        {
            SeTag best = null;

            foreach (KeyValuePair<int, SeTag> entry in s_seTags)
            {
                if (!entry.Value.DrainsStamina || !IsActive(entry.Key))
                    continue;

                if (best == null || entry.Value.Time > best.Time)
                    best = entry.Value;
            }

            return best;
        }

        // ── terrain ──────────────────────────────────────────────────────────

        /// <summary>
        /// A terrain edit was applied. A fall right after is credited to it unless the redeem gave
        /// no-fall-damage; a dip into water right after is a swim cause either way. Any newer edit
        /// replaces the previous one (including the streamer's own, which can't credit anyone).
        /// </summary>
        public static void NoteTerrainEdit(CustomRewardEvent customRewardEvent, bool noFallDamage)
        {
            if (!RedeemerTagHelper.TryResolve(customRewardEvent, out string id, out string title))
            {
                s_terrain = null;
                return;
            }

            s_terrain = new TimedCredit { Credit = new Credit(id, title), Time = Time.time, FallEligible = !noFallDamage };
        }

        /// <summary>
        /// The timed undo of a terrain edit (it can drop the streamer off raised ground, and no
        /// no-fall-damage effect is applied for it), credited to the marker's redeemer.
        /// </summary>
        public static void NoteTerrainReset(GameObject marker)
        {
            TwitchBasePersistentData tag = marker != null ? marker.GetComponent<TwitchBasePersistentData>() : null;

            if (tag == null || !tag.HasRedeemer)
            {
                s_terrain = null;
                return;
            }

            s_terrain = new TimedCredit { Credit = new Credit(tag.RedeemerId, tag.RedeemerRawTitle), Time = Time.time, FallEligible = true };
        }

        // ── boats ────────────────────────────────────────────────────────────

        private static bool IsLocalPlayerAboard(Ship ship)
        {
            Player player = Player.m_localPlayer;
            return player != null && ship != null && (ship.IsPlayerInBoat(player) || player.GetStandingOnShip() == ship);
        }

        private static ZDOID ShipId(Ship ship)
        {
            ZNetView view = ship.GetComponent<ZNetView>();
            return view != null && view.IsValid() ? view.GetZDO().m_uid : ZDOID.None;
        }

        /// <summary>
        /// <c>WearNTear.Damage</c> (caller side, where the hazard/creature is known): remembers who
        /// last hit each ship - overwriting with "nobody" for any other hit, so when it breaks the
        /// stored value is the destroying blow.
        /// </summary>
        public static void NoteShipHit(WearNTear wearNTear, HitData hit)
        {
            Ship ship = wearNTear != null ? wearNTear.GetComponent<Ship>() : null;
            if (ship == null || hit == null)
                return;

            ZDOID id = ShipId(ship);
            if (id == ZDOID.None)
                return;

            float now = Time.time;

            if (s_shipLastHit.Count > 32)
            {
                foreach (ZDOID stale in new List<ZDOID>(s_shipLastHit.Keys))
                {
                    if (now - s_shipLastHit[stale].Time > 300f)
                        s_shipLastHit.Remove(stale);
                }
            }

            s_shipLastHit[id] = new ShipHit { Credit = CreatureCredit(hit) ?? CurrentHazard, Time = now };
        }

        /// <summary>
        /// A ship is being destroyed (<c>WearNTear.Destroy</c>). If the streamer was aboard and a
        /// redeem landed the destroying hit, that redeemer is the cause of whatever swimming follows.
        /// </summary>
        public static void NoteShipDestroyed(WearNTear wearNTear)
        {
            Ship ship = wearNTear != null ? wearNTear.GetComponent<Ship>() : null;
            if (ship == null)
                return;

            ZDOID id = ShipId(ship);

            if (id == ZDOID.None || !s_shipLastHit.TryGetValue(id, out ShipHit last))
                return;

            s_shipLastHit.Remove(id);

            if (last.Credit.HasValue && IsLocalPlayerAboard(ship))
                s_boatLoss = new TimedCredit { Credit = last.Credit.Value, Time = Time.time };
        }

        /// <summary>
        /// A redeem boat's own timer ran out (<c>TwitchPersistentDestruction</c>) with the streamer
        /// aboard: the boat's redeemer is the cause of whatever swimming follows.
        /// </summary>
        public static void NoteBoatExpiring(GameObject boat)
        {
            Ship ship = boat != null ? boat.GetComponent<Ship>() : null;
            if (ship == null || !IsLocalPlayerAboard(ship))
                return;

            TwitchBasePersistentData tag = boat.GetComponent<TwitchBasePersistentData>();
            if (tag == null || !tag.HasRedeemer)
                return;

            s_boatLoss = new TimedCredit { Credit = new Credit(tag.RedeemerId, tag.RedeemerRawTitle), Time = Time.time };
        }

        // ── swimming / drowning ──────────────────────────────────────────────

        /// <summary>
        /// Called on every swim tick of the local player. A swim spell starts when the previous
        /// tick was more than <see cref="SwimGap"/> ago; it takes the most recent redeem cause from
        /// the last <see cref="SwimCauseWindow"/> seconds (or none) and keeps it until it ends.
        /// </summary>
        public static void NoteSwimming()
        {
            float now = Time.time;

            if (now - s_lastSwimTick > SwimGap)
            {
                s_spellCause = LatestWithin(now, SwimCauseWindow, s_terrain, s_knockback, s_boatLoss);

                if (s_spellCause != null)
                    Jotunn.Logger.LogWarning($"[WBTI] Death credit: swim spell started, cause {s_spellCause.Credit}");
            }

            s_lastSwimTick = now;
        }

        private static TimedCredit LatestWithin(float now, float window, params TimedCredit[] candidates)
        {
            TimedCredit best = null;

            foreach (TimedCredit candidate in candidates)
            {
                if (candidate == null || now - candidate.Time > window)
                    continue;

                if (best == null || candidate.Time > best.Time)
                    best = candidate;
            }

            return best;
        }

        /// <summary>
        /// A drowning hit: credited to the swim spell's cause or to an active stamina-draining
        /// status effect, whichever is more recent. Never to anything else.
        /// </summary>
        private static Credit? DrowningCredit(out string source)
        {
            source = null;

            float now = Time.time;
            TimedCredit spell = now - s_lastSwimTick <= SpellActiveWindow ? s_spellCause : null;
            SeTag drain = ActiveStaminaDrainTag();

            if (spell != null && (drain == null || spell.Time >= drain.Time))
            {
                source = "drowning (swim cause)";
                return spell.Credit;
            }

            if (drain != null)
            {
                source = "drowning (stamina drain)";
                return drain.Credit;
            }

            return null;
        }

        // ── death ────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>Player.OnDeath</c> on the local player: credits the death to whoever the killing blow
        /// (the last committed hit) was resolved to.
        /// </summary>
        public static void OnLocalPlayerDeath(Player player)
        {
            if (player != Player.m_localPlayer)
                return;

            HitData lastHit = s_lastHitRef(player);

            if (lastHit == null || !ReferenceEquals(lastHit, s_committedHit) || !s_committed.HasValue)
            {
                Jotunn.Logger.LogWarning("[WBTI] Death credit: nobody credited for this death.");
                return;
            }

            Credit credit = s_committed.Value;
            Jotunn.Logger.LogWarning($"[WBTI] Death credit: {credit} via {s_committedSource}.");

            s_committedHit = null;
            s_committed = null;

            LeaderboardHelper.RecordDeath(credit.Id, credit.Title);
        }
    }
}
