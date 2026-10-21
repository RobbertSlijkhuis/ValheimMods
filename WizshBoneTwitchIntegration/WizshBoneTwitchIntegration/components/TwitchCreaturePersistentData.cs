using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreaturePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        // Shared with TwitchBasePersistentData's claim watcher, which compares this field for changes.
        internal static readonly int s_creatureDataHash = "CreatureData_WBTI".GetStableHashCode();

        private string m_redeemerName;

        // Manual claims only: the name the claim currently shows, incl. the "[n]" suffix. Computed and
        // written solely by the Twitch-logged-in client (see TwitchCreatureClaim.SetDisplayIndex) - the
        // suffix depends on which of an owner's claims are loaded, so other clients must read it, not
        // recompute it. Empty until that client has numbered the claim.
        private string m_claimDisplayName = "";

        // Manual claims only: the Twitch channel (lowercased) of the streamer who manages this claim -
        // the only one whose client runs its expiry and numbers it. Empty for a "claim:" rename (made
        // by any player), until a streamer's client adopts it for numbering. See TwitchCreatureClaim.
        private string m_claimChannel = "";

        // Manual claims only: set for a "claim:" rename, which never expires. Defaults to true when the
        // field is missing - claims saved before it existed were only persisted while permanent.
        private bool m_claimPermanent = true;

        public string ClaimChannel => m_claimChannel;
        private string m_redeemTitle;
        private string m_savedPrefabName;

        // Non-empty only for a creature summoned by another creature's own vanilla SpawnAbility
        // (e.g. a Warlock/Oozer/Gjall's reinforcements - see CreatureHelper.ApplyInheritedScaling).
        // m_savedPrefabName stays the *parent's* lookup prefab (the one that actually resolves in
        // the redeem's creature list); this holds the child's own real prefab, used to clone the
        // resolved entry and override just its prefabName before applying.
        private string m_actualPrefabName = "";

        private string m_colorOverride;
        private string m_emissionColorOverride;
        private bool m_forceColorOverride;
        public bool m_ignoreWard;
        public bool m_isFollowing;
        public bool m_allowDamageStructures = true;
        public float m_damageScale = 0;
        public float m_healthScale = 0;

        // The prefab's native root scale, read in Awake for every instance (including claimed wild
        // creatures that never run ApplyVariables) so attack/ragdoll scaling is always relative to it.
        private Vector3 m_nativeScale = Vector3.one; // replaced in Awake
        public float NativeScale => m_nativeScale.x;

        // Permanent feature (see CreatureData.fullyPassive). Read by
        // harmony/SpecialRedeemPatchesWBTI.cs's IsEnemy prefix.
        public bool IsFullyPassive;

        // See CreatureData.muteIdleSound. Besides cancelling BaseAI's DoIdleSound and clearing its
        // alert effects, this lets harmony/AnimationSoundPatchesWBTI.cs skip idle and alert sounds
        // spawned from animation events.
        public bool IsIdleSoundMuted;

        // Permanent feature (see CreatureData.alwaysFollowOwner). Read by
        // harmony/SpecialRedeemPatchesWBTI.cs's Follow prefix, to make this creature keep closing
        // the gap instead of stopping ~3m out like a normal followed creature.
        public bool WantsToCloseDistance;

        public string RedeemerName => m_redeemerName;
        public string RedeemTitle => m_redeemTitle;
        public string SavedPrefabName => m_savedPrefabName;
        public string ColorOverride => m_colorOverride;
        public string EmissionColorOverride => m_emissionColorOverride;
        public bool ForceColorOverride => m_forceColorOverride;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();
                m_nativeScale = GetPrefabRootScale();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string creatureStringData = m_netView.GetZDO().GetString(s_creatureDataHash, "");

                if (creatureStringData == "")
                    return;

                string[] data = creatureStringData.Split('|');
                m_redeemerName     = data[0];
                m_redeemTitle      = data[1];
                m_savedPrefabName  = data[2];
                m_ignoreWard       = bool.Parse(data[3]);
                m_isFollowing      = data.Length > 4 && bool.Parse(data[4]);
                m_actualPrefabName = data.Length > 5 ? data[5] : "";
                m_claimDisplayName = data.Length > 6 ? data[6] : "";
                m_claimChannel     = data.Length > 7 ? data[7] : "";
                m_claimPermanent   = data.Length <= 8 || bool.Parse(data[8]);

                TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();

                if (m_redeemTitle == "")
                {
                    // This is the rehydration path for a manual claim, which can be on an AnimalAI
                    // creature (e.g. Deer) with no Humanoid component. ReInit applies the display name
                    // itself (numbered locally on the Twitch-logged-in client, read from the stored
                    // m_claimDisplayName everywhere else) - do not also set Character.m_name here, it
                    // would clobber a just-applied [n] suffix back to the plain name whenever this
                    // happens to be the last of its owner's simultaneously-loaded manual claims to rehydrate.
                    creatureClaim.ReInit(m_redeemerName, isSpawn: false, displayName: m_claimDisplayName, permanent: m_claimPermanent);
                    return;
                }

                RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

                if (redeem == null)
                {
                    Jotunn.Logger.LogError($"Could not find redeem '{m_redeemTitle}' in TwitchCreaturePersistentData, skipping setup.");
                    return;
                }

                if (m_savedPrefabName == "")
                {
                    // SpawnAbility redeems (e.g. biome-prefab showers like "Roots on the line!") use a
                    // placeholder CreatureData with no prefabName, so there's nothing to look up or reapply.
                    creatureClaim.ReInit(m_redeemerName, isSpawn: true);
                    return;
                }

                CreatureData lookupData = RedeemHelper.FindRedeemCreatureData(redeem, m_savedPrefabName);

                if (lookupData == null)
                {
                    Jotunn.Logger.LogError($"Could not find creature '{m_savedPrefabName}' in redeem '{m_redeemTitle}'");
                    return;
                }

                // A non-empty m_actualPrefabName means this instance is a creature summoned by
                // another creature's own SpawnAbility (see CreatureHelper.ApplyInheritedScaling) -
                // m_savedPrefabName is the parent's own lookup prefab (the one that resolves in the
                // redeem's list above), not this instance's actual prefab. Clone the resolved entry
                // for the child (own prefab identity, natural size) before applying.
                CreatureData creatureData = m_actualPrefabName != ""
                    ? CreatureHelper.CloneForChild(lookupData, m_actualPrefabName)
                    : lookupData;

                creatureClaim.ReInit(m_redeemerName, isSpawn: true, creatureData);
                ApplyCreatureData(creatureData);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError($"Error in TwitchCreaturePersistentData: {e}");
                Jotunn.Logger.LogError($"{m_redeemerName}, {m_redeemTitle}, {m_savedPrefabName}");
            }
        }

        // Shared "apply a resolved CreatureData" tail - used by Awake()'s rehydration (direct redeem
        // spawns and inherited summon-children alike) and by SetInherited (the owner's own client,
        // applying immediately rather than waiting for its own Awake()/ZDO round-trip).
        private void ApplyCreatureData(CreatureData creatureData)
        {
            m_colorOverride = creatureData.color;
            m_emissionColorOverride = creatureData.emissionColor;
            m_forceColorOverride = creatureData.forceColor;

            ApplyVariables(creatureData);
            ApplyHumanoid(m_redeemerName, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
            CreatureGearHelper.Apply(gameObject.GetComponent<Humanoid>(), creatureData);
        }

        // Called once by CreatureHelper.ApplyInheritedScaling, on the owning client, right after a
        // creature summoned by another creature's own SpawnAbility is instantiated (a Warlock/Oozer/
        // Gjall's reinforcements). redeemerName/redeemTitle/lookupPrefabName come from the summoning
        // creature's own TwitchCreaturePersistentData; resolvedChildCreatureData is the parent's
        // resolved CreatureData already cloned with prefabName overridden to this child's own prefab.
        public void SetInherited(string redeemerName, string redeemTitle, string lookupPrefabName, CreatureData resolvedChildCreatureData)
        {
            m_redeemerName     = redeemerName;
            m_redeemTitle      = redeemTitle;
            m_savedPrefabName  = lookupPrefabName;
            m_actualPrefabName = resolvedChildCreatureData.prefabName;
            m_ignoreWard       = false;
            m_isFollowing      = false;

            WriteZDO();

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, true);

            TwitchCreatureClaim creatureClaim = gameObject.GetComponent<TwitchCreatureClaim>() ?? gameObject.AddComponent<TwitchCreatureClaim>();
            creatureClaim.ReInit(m_redeemerName, isSpawn: true, resolvedChildCreatureData);

            ApplyCreatureData(resolvedChildCreatureData);
        }

        public void Start()
        {
            try
            {
                if (m_redeemerName != null && RecolorHelper.CanRecolorPetRock(m_redeemerName, gameObject))
                    RecolorHelper.RecolorPetRock(m_redeemerName, gameObject);
                else if (m_redeemerName != null && RecolorHelper.CanRecolorCreature(m_redeemerName, gameObject.name, m_colorOverride, m_emissionColorOverride))
                    RecolorHelper.RecolorCreature(m_redeemerName, gameObject, m_colorOverride, m_forceColorOverride, m_emissionColorOverride);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreaturePersistentData.Start failed: " + e);
            }
        }

        // claimChannel / claimPermanent only apply to a manual claim (no creatureData) - see the fields.
        public void SetData(string name, CreatureData creatureData = null, string redeemTitle = "", bool ignoreWard = false, string claimChannel = "", bool claimPermanent = false)
        {
            m_redeemerName    = name;
            m_redeemTitle     = redeemTitle;
            m_ignoreWard      = ignoreWard;
            m_claimDisplayName = "";
            m_claimChannel    = claimChannel;
            m_claimPermanent  = claimPermanent;
            m_savedPrefabName = creatureData?.prefabName ?? "";
            m_colorOverride   = creatureData?.color;
            m_emissionColorOverride = creatureData?.emissionColor;
            m_forceColorOverride = creatureData?.forceColor ?? false;

            // isFollowing defaults to commandable for new spawns
            m_isFollowing = creatureData?.commandable ?? false;

            WriteZDO();

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, true);

            if (creatureData == null)
                return;

            ApplyVariables(creatureData);
            ApplyHumanoid(name, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
            CreatureGearHelper.Apply(gameObject.GetComponent<Humanoid>(), creatureData);
        }

        // Called by TwitchCreatureClaim.OnDestroy() when a manually-claimed (non-spawn) creature is
        // genuinely unclaimed - without this, the ZDO string field this component reads on Awake()
        // (and the Creature flag that makes TwitchBasePersistentData re-add this component at all)
        // would still be set, so the next reload would re-hydrate the claim right back into existence.
        public void ClearClaimData()
        {
            if (m_netView == null || !m_netView.IsValid())
                return;

            // Whoever releases the claim (chat, expiry, unclaim:) isn't necessarily the owner - without
            // ownership the clear would silently no-op and the claim would come back on the next reload.
            if (!m_netView.IsOwner())
                m_netView.ClaimOwnership();

            m_netView.GetZDO().Set(s_creatureDataHash, "");

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, false);
        }

        // Writes the numbered display name of a manual claim into the ZDO so other clients can show
        // it (see m_claimDisplayName). Caller must already own the ZDO.
        public void SetClaimDisplayName(string displayName)
        {
            if (m_claimDisplayName == displayName)
                return;

            m_claimDisplayName = displayName;
            WriteZDO();
        }

        // A streamer's client adopting an unstamped claim (a "claim:" rename) so it can number it. If two
        // streamers adopt at once the last write wins; the other client sees the winner's channel through
        // OnZdoChanged and from then on only mirrors the claim. Caller must already own the ZDO.
        public void SetClaimChannel(string channel)
        {
            if (m_claimChannel == channel)
                return;

            m_claimChannel = channel;
            WriteZDO();
        }

        // Called by TwitchBasePersistentData's claim watcher when the ZDO string changed while this
        // component was already running - i.e. a manual claim was made, renamed or released by another
        // client after this one loaded the creature. Only manual claims (empty title) are live-updated;
        // a redeem spawn's data is still read once in Awake. The watcher can't tell who wrote the change,
        // so this client's own writes land here too and are no-ops because the state already matches.
        public void OnZdoChanged()
        {
            if (m_redeemTitle != "" || m_netView == null || !m_netView.IsValid())
                return;

            string raw = m_netView.GetZDO().GetString(s_creatureDataHash, "");
            TwitchCreatureClaim claim = gameObject.GetComponent<TwitchCreatureClaim>();

            if (raw == "")
            {
                claim?.ApplyRemoteRelease();
                Destroy(this);
                return;
            }

            string[] data = raw.Split('|');
            string userName = data[0];
            string displayName = data.Length > 6 ? data[6] : "";

            m_redeemerName = userName;
            m_claimDisplayName = displayName;
            m_claimChannel = data.Length > 7 ? data[7] : "";
            m_claimPermanent = data.Length <= 8 || bool.Parse(data[8]);

            // Same claimant: only the display name or the managing channel can have changed (e.g. another
            // streamer won an adoption race, which makes this client a read-only mirror).
            if (claim != null && claim.ClaimedUserName == userName)
            {
                claim.ApplyStoredDisplayName(displayName);
                return;
            }

            // A new claimant, or a claim that isn't set up here yet. ReInit replaces any previous assignment.
            if (claim == null)
                claim = gameObject.AddComponent<TwitchCreatureClaim>();

            claim.ReInit(userName, isSpawn: false, displayName: displayName, permanent: m_claimPermanent);
            claim.ApplyRecolor();
        }

        public void SetFollowing(bool following)
        {
            m_isFollowing = following;
            WriteZDO();
        }

        private void WriteZDO()
        {
            if (!m_netView.IsOwner())
                return;

            m_netView.GetZDO().Set(s_creatureDataHash, $"{m_redeemerName}|{m_redeemTitle}|{m_savedPrefabName}|{m_ignoreWard}|{m_isFollowing}|{m_actualPrefabName}|{m_claimDisplayName}|{m_claimChannel}|{m_claimPermanent}");
        }

        private void ApplyVariables(CreatureData creatureData)
        {
            // m_isFollowing is managed via SetData/SetFollowing — not overwritten here
            m_allowDamageStructures = creatureData.allowDamageStructures;
            m_damageScale           = creatureData.damageScale;
            m_healthScale           = creatureData.healthScale;
            transform.localScale    = m_nativeScale * creatureData.size;

            if (creatureData.speedMultiplier != 1f)
            {
                Character character = gameObject.GetComponent<Character>();

                if (character != null)
                {
                    character.m_speed        *= creatureData.speedMultiplier;
                    character.m_walkSpeed    *= creatureData.speedMultiplier;
                    character.m_runSpeed     *= creatureData.speedMultiplier;
                    character.m_flySlowSpeed *= creatureData.speedMultiplier;
                    character.m_flyFastSpeed *= creatureData.speedMultiplier;
                    character.m_swimSpeed    *= creatureData.speedMultiplier;
                }
            }
        }

        // The prefab's own root scale (e.g. TrollFrost is 4). CreatureData.size is a multiplier on
        // it (size 1 = vanilla size), and attack ranges, spawn offsets and ragdoll sizes are all
        // authored against this native scale, so attack/ragdoll scaling is current / native - which
        // works out to just size (see AttackScaleHelper.GetAttackScale). Read from the prefab rather
        // than the instance so it doesn't depend on whether something already changed the
        // instance's scale; when the prefab can't be found the instance's own scale stands in, so the
        // factor is 1 rather than the raw root scale.
        private Vector3 GetPrefabRootScale()
        {
            if (m_netView != null && m_netView.IsValid() && ZNetScene.instance != null)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(m_netView.GetZDO().GetPrefab());

                if (prefab != null && prefab.transform.localScale.x > 0f)
                    return prefab.transform.localScale;
            }

            return transform.localScale;
        }

        private void ApplyHumanoid(string redeemerName, CreatureData creatureData)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();

            if (humanoid == null)
            {
                Jotunn.Logger.LogWarning("Humanoid component is null on " + gameObject.name);
                return;
            }

            humanoid.SetLevel(creatureData.level);
            humanoid.m_boss               = creatureData.isBoss;
            humanoid.m_bossEvent          = creatureData.bossEvent;
            humanoid.m_defeatSetGlobalKey = "";

            if (ProfileSettingsHelper.Current.creaturesSameFaction)
                humanoid.m_faction = Character.Faction.Boss;

            if (creatureData.friendly)
            {
                humanoid.m_faction = Character.Faction.Players;
                humanoid.m_group   = WizshBoneTwitchIntegration.HumanoidGroupSpawnFriendly;
            }

            if (creatureData.group != null)
                humanoid.m_group = creatureData.group;

            if (creatureData.maxHealth > 0)
                humanoid.SetMaxHealth(creatureData.maxHealth);

            if (creatureData.rename)
                humanoid.m_name = creatureData.name ?? redeemerName;

            if (ProfileSettingsHelper.Current.creaturesScaling)
            {
                ValheimCreature valheimCreature = CreatureHelper.GetValheimCreature(gameObject.name);

                if (valheimCreature == null)
                {
                    Jotunn.Logger.LogWarning($"No ValheimCreature entry found for {gameObject.name}, skipping health scaling.");
                    return;
                }

                float playerTier  = ProgressionHelper.GetPlayerTier();
                float healthScale = m_healthScale != 0 ? m_healthScale : ProfileSettingsHelper.Current.creaturesHealthScale;
                float scale       = CreatureHelper.CalculateScale(playerTier, valheimCreature.tier, healthScale);

                humanoid.SetMaxHealth(humanoid.GetMaxHealth() * scale);
                humanoid.SetHealth(humanoid.GetMaxHealth());
            }
        }

        private void ApplyMonsterAI(CreatureData creatureData)
        {
            MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

            if (monsterAI == null)
            {
                Jotunn.Logger.LogWarning("MonsterAI component is null on " + gameObject.name);
                return;
            }

            monsterAI.m_mistVision = creatureData.mistVision;

            // See the field comment on CreatureData.fullyPassive.
            IsFullyPassive = creatureData.fullyPassive;
            IsIdleSoundMuted = creatureData.muteIdleSound;

            if (creatureData.muteIdleSound)
            {
                // BaseAI.Awake() already scheduled DoIdleSound, so cancel it; a zero chance also keeps
                // it silent if anything re-arms the invoke later.
                monsterAI.m_idleSoundChance = 0f;
                monsterAI.CancelInvoke("DoIdleSound");

                // Alert sounds are BaseAI.m_alertedEffects, created in SetAlerted. Swapping in an
                // empty list only affects this instance, not the shared prefab's list.
                monsterAI.m_alertedEffects = new EffectList();
            }
            else if (creatureData.idleSoundInterval > 0)
            {
                monsterAI.m_idleSoundChance = 1f;
                monsterAI.m_idleSoundInterval = creatureData.idleSoundInterval;

                // BaseAI.Awake() already scheduled DoIdleSound via InvokeRepeating using its prefab
                // default interval before we get here - just changing m_idleSoundInterval wouldn't
                // affect that already-running timer, so re-arm it to actually apply the override.
                monsterAI.CancelInvoke("DoIdleSound");
                monsterAI.InvokeRepeating("DoIdleSound", creatureData.idleSoundInterval, creatureData.idleSoundInterval);
            }
        }

        private void ApplyAllowDrops(CreatureData creatureData)
        {
            if (creatureData.allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop == null)
            {
                Jotunn.Logger.LogWarning("CharacterDrop component is null on " + gameObject.name);
                return;
            }

            // CharacterDrop.m_drops isn't just loot - some creatures (e.g. the Oozer's on-death
            // split into two Blobs) use it to spawn another creature as a core mechanic, not an item
            // drop. allowDrops is meant to suppress loot farming, not break that - only strip entries
            // whose prefab is an actual item (has ItemDrop), leaving creature-spawn entries intact.
            characterDrop.m_drops.RemoveAll(drop => drop.m_prefab != null && drop.m_prefab.GetComponent<ItemDrop>() != null);
        }

        private void ApplyTameable(CreatureData creatureData)
        {
            if (!creatureData.friendly)
                return;

            Tameable tameable = gameObject.GetComponent<Tameable>();

            if (tameable == null)
                tameable = gameObject.AddComponent<Tameable>();

            tameable.m_monsterAI.MakeTame();

            if (creatureData.commandable)
            {
                tameable.m_commandable = true;

                // Skipped for alwaysFollowOwner: its target is the named player in the ZDO (below), and
                // this would point every rehydrating client's copy at that client's own player instead.
                if (m_isFollowing && !creatureData.alwaysFollowOwner && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            }

            // See the field comment on CreatureData.alwaysFollowOwner. Player.m_localPlayer
            // is only assigned later by an explicit SetLocalPlayer() call, not during Awake() - on a
            // relog/zone-reload this Awake()-driven rehydration can easily run before that happens, so a
            // one-shot null check here silently drops the follow target. Retry briefly instead.
            if (creatureData.alwaysFollowOwner)
                StartCoroutine(ClaimFollowTargetWhenPlayerReady(tameable.m_monsterAI));

            WantsToCloseDistance = creatureData.alwaysFollowOwner;
        }

        // alwaysFollowOwner: the player to follow is stored by name in the ZDO's vanilla "follow" field
        // (ZDOVars.s_follow). Whenever the follow target is null - after that player died and respawned
        // as a new object, or ownership moved to another client - vanilla Tameable.UpdateSavedFollowTarget
        // looks the named player up again, on whichever client owns the creature.
        //
        // So only the owner writes the name, and only while it is still empty (a fresh spawn, or a
        // creature from before this field was used); every other client leaves the target alone instead
        // of pointing it at its own local player.
        //
        // Bounded wait (10s) rather than an indefinite one, in case Player.m_localPlayer genuinely never
        // shows up (e.g. this instance rehydrating in a context with no local player at all).
        private IEnumerator ClaimFollowTargetWhenPlayerReady(MonsterAI monsterAI)
        {
            float timeout = 10f;

            while (Player.m_localPlayer == null && timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            if (Player.m_localPlayer == null || monsterAI == null || m_netView == null || !m_netView.IsValid() || !m_netView.IsOwner())
                yield break;

            ZDO zdo = m_netView.GetZDO();

            if (!string.IsNullOrEmpty(zdo.GetString(ZDOVars.s_follow)))
                yield break;

            zdo.Set(ZDOVars.s_follow, Player.m_localPlayer.GetPlayerName());
            monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
        }
    }
}
