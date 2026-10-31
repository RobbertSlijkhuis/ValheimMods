using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    [Flags]
    internal enum PersistentComponentFlags
    {
        None         = 0,
        Freeze       = 1 << 0,
        ToggleDoor   = 1 << 1,
        Piece        = 1 << 2,
        HazardDamage      = 1 << 3,
        ShipSafeZone      = 1 << 4,
        Creature          = 1 << 5,
        Destruction       = 1 << 6,

        // Marker only, no component to rehydrate - see TwitchBasePersistentData.IsRedeemSpawn.
        RedeemSpawn       = 1 << 7,

        // Appended after RedeemSpawn rather than renumbered - existing bits are already saved in
        // ZDOs on live worlds, and renumbering would corrupt that saved flag data.
        ShipIdlePhysics   = 1 << 8,
        SpawnScale        = 1 << 9,
        RagdollRecolor    = 1 << 10,
    }

    internal class TwitchBasePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private static readonly int s_activeComponentsHash = "WBTI_ActiveComponents".GetStableHashCode();

        // General marker for "this instance was spawned by a Twitch redeem". Unlike the other
        // PersistentComponentFlags, it isn't gated by redeem config (e.g. duration == 0 / indefinite
        // means no other persistent component ever gets attached) - it's set unconditionally on every
        // redeem spawn so remove commands can still identify Twitch-spawned objects. Pair with a
        // type-specific component (Door, Ship, ImpactEffect, etc.) to narrow down what kind of object it is.
        private bool m_isRedeemSpawn;
        public bool IsRedeemSpawn => m_isRedeemSpawn;

        public void MarkRedeemSpawn()
        {
            m_isRedeemSpawn = true;
            SetFlag(PersistentComponentFlags.RedeemSpawn, true);
        }

        // Who redeemed this object into the world (Twitch user ID + the redeem's raw, prefix-free
        // title), stored once as "id|rawTitle". Same replicate-through-the-ZDO pattern as the other
        // persistent data: the spawning client writes it once (see RedeemerTagHelper), every
        // client's Awake reads it back - no RPC. Used to credit the streamer's death to a redeem
        // (see DeathCreditHelper); absent for the streamer's own and test redeems, so they never credit.
        private static readonly int s_redeemerHash = "WBTI_Redeemer".GetStableHashCode();
        private string m_redeemerId;
        private string m_redeemerRawTitle;

        public bool HasRedeemer => !string.IsNullOrEmpty(m_redeemerId);
        public string RedeemerId => m_redeemerId;
        public string RedeemerRawTitle => m_redeemerRawTitle;

        public void SetRedeemer(string id, string rawTitle)
        {
            if (string.IsNullOrEmpty(id))
                return;

            m_redeemerId = id;
            m_redeemerRawTitle = rawTitle ?? "";

            // One-time config: written by the client that spawned the object (always its owner then).
            if (m_netView == null || !m_netView.IsValid() || !m_netView.IsOwner())
                return;

            m_netView.GetZDO().Set(s_redeemerHash, $"{m_redeemerId}|{m_redeemerRawTitle}");
        }

        private void ReadRedeemer()
        {
            string stored = m_netView.GetZDO().GetString(s_redeemerHash, "");
            int separator = stored.IndexOf('|');

            if (separator <= 0)
                return;

            m_redeemerId = stored.Substring(0, separator);
            m_redeemerRawTitle = stored.Substring(separator + 1);
        }

        // The id of the redeem that spawned this object (RedeemData.id), which the persistent components resolve
        // their config by. Unlike the redeemer tag above it is stored for every redeem, test and streamer ones too.
        // Absent on objects saved before ids were stored; those resolve by title and get the id written on load.
        private static readonly int s_redeemIdHash = "WBTI_RedeemId".GetStableHashCode();
        private string m_redeemId;

        public string RedeemId => m_redeemId;

        private const float RedeemIdRetryInterval = 2f;

        // Set once: an object keeps the id it was first given, so one whose redeem was deleted is never rebound to
        // another redeem. At spawn the spawner owns the object and the id is written at once. An object saved before
        // ids were stored is migrated by whoever loads it: this client claims it when nobody owns it yet (what ZDOMan
        // does for the first client in range anyway), and otherwise waits for its owner to write the id.
        public void SetRedeemId(string id)
        {
            if (string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(m_redeemId))
                return;

            m_redeemId = id;

            if (m_netView == null || !m_netView.IsValid())
                return;

            if (!m_netView.IsOwner() && !m_netView.HasOwner())
                m_netView.ClaimOwnership();

            if (m_netView.IsOwner())
                m_netView.GetZDO().Set(s_redeemIdHash, id);
            else
                InvokeRepeating(nameof(WriteRedeemIdWhenOwner), RedeemIdRetryInterval, RedeemIdRetryInterval);
        }

        // Runs until the id is in the ZDO: written here once this client owns the object, or by its owner meanwhile.
        private void WriteRedeemIdWhenOwner()
        {
            ZDO zdo = m_netView != null && m_netView.IsValid() ? m_netView.GetZDO() : null;

            if (zdo != null && zdo.GetString(s_redeemIdHash, "") == "")
            {
                if (!m_netView.IsOwner())
                    return;

                zdo.Set(s_redeemIdHash, m_redeemId);
            }

            CancelInvoke(nameof(WriteRedeemIdWhenOwner));
        }

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                // Read before the early return below, which skips objects with no active flags.
                ReadRedeemer();
                m_redeemId = m_netView.GetZDO().GetString(s_redeemIdHash, "");

                StartClaimWatcher();

                var flags = (PersistentComponentFlags)m_netView.GetZDO().GetInt(s_activeComponentsHash, 0);

                m_isRedeemSpawn = flags.HasFlag(PersistentComponentFlags.RedeemSpawn);

                if (flags == PersistentComponentFlags.None)
                    return;

                if (flags.HasFlag(PersistentComponentFlags.Freeze))
                    gameObject.AddComponent<TwitchFreezeData>();

                if (flags.HasFlag(PersistentComponentFlags.ToggleDoor))
                    gameObject.AddComponent<TwitchDoorPersistentData>();

                if (flags.HasFlag(PersistentComponentFlags.Piece))
                    gameObject.AddComponent<TwitchPiecePersistentData>();

                if (flags.HasFlag(PersistentComponentFlags.HazardDamage))
                    gameObject.AddComponent<TwitchPersistentDamage>();

                if (flags.HasFlag(PersistentComponentFlags.ShipSafeZone))
                    TwitchSafeZone.AttachToShip(gameObject);

                if (flags.HasFlag(PersistentComponentFlags.Creature))
                    gameObject.AddComponent<TwitchCreaturePersistentData>();

                if (flags.HasFlag(PersistentComponentFlags.Destruction))
                    gameObject.AddComponent<TwitchPersistentDestruction>();

                if (flags.HasFlag(PersistentComponentFlags.ShipIdlePhysics))
                    gameObject.AddComponent<TwitchShipIdlePhysicsData>();

                if (flags.HasFlag(PersistentComponentFlags.SpawnScale))
                    gameObject.AddComponent<TwitchScaledSpawn>();

                if (flags.HasFlag(PersistentComponentFlags.RagdollRecolor))
                    gameObject.AddComponent<TwitchRagdollRecolor>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchBasePersistentData.Awake failed: " + e);
            }
        }

        // A manual creature claim is made (or renamed/released) by whichever client owns the chat, long
        // after other clients loaded the creature - and a ZDO has no change event, while Awake-time
        // rehydration only covers creatures that load afterwards. So every claimable instance (anything
        // with a BaseAI, or a Tameable for non-creature claims like a Rocky) polls its claim field and
        // hands any change to TwitchCreaturePersistentData. The first read here is the baseline, so a
        // creature that rehydrates normally in Awake isn't applied twice.
        private const float ClaimWatchInterval = 1f;
        private string m_lastClaimData;

        private void StartClaimWatcher()
        {
            if (gameObject.GetComponent<BaseAI>() == null && gameObject.GetComponent<Tameable>() == null)
                return;

            m_lastClaimData = m_netView.GetZDO().GetString(TwitchCreaturePersistentData.s_creatureDataHash, "");

            // Randomized start so every creature loaded in the same frame doesn't poll in the same frame.
            InvokeRepeating(nameof(WatchClaim), ClaimWatchInterval + UnityEngine.Random.value, ClaimWatchInterval);
        }

        private void WatchClaim()
        {
            try
            {
                if (m_netView == null || !m_netView.IsValid())
                    return;

                string current = m_netView.GetZDO().GetString(TwitchCreaturePersistentData.s_creatureDataHash, "");

                if (current == m_lastClaimData)
                    return;

                m_lastClaimData = current;

                TwitchCreaturePersistentData creatureData = gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (creatureData != null)
                    creatureData.OnZdoChanged();
                else if (current != "")
                    gameObject.AddComponent<TwitchCreaturePersistentData>(); // its Awake rehydrates from the ZDO
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchBasePersistentData.WatchClaim failed: " + e);
            }
        }

        public void SetFlag(PersistentComponentFlags flag, bool active)
        {
            try
            {
                if (m_netView == null || !m_netView.IsValid() || !m_netView.IsOwner())
                    return;

                var current = (PersistentComponentFlags)m_netView.GetZDO().GetInt(s_activeComponentsHash, 0);
                var updated = active ? (current | flag) : (current & ~flag);
                m_netView.GetZDO().Set(s_activeComponentsHash, (int)updated);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchBasePersistentData.SetFlag failed: " + e);
            }
        }
    }
}
