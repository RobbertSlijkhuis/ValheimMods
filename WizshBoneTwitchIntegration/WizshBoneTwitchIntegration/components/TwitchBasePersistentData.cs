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

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

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
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchBasePersistentData.Awake failed: " + e);
            }
        }

        public void SetFlag(PersistentComponentFlags flag, bool active)
        {
            try
            {
                if (m_netView == null || !m_netView.IsValid())
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
