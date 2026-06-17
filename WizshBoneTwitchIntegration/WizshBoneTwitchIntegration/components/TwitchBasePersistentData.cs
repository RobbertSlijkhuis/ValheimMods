using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    [Flags]
    internal enum PersistentComponentFlags
    {
        None       = 0,
        Freeze     = 1 << 0,
        ToggleDoor = 1 << 1,
        Piece      = 1 << 2,
    }

    internal class TwitchBasePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private static readonly int s_activeComponentsHash = "WBTI_ActiveComponents".GetStableHashCode();

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                var flags = (PersistentComponentFlags)m_netView.GetZDO().GetInt(s_activeComponentsHash, 0);

                if (flags == PersistentComponentFlags.None)
                    return;

                if (flags.HasFlag(PersistentComponentFlags.Freeze))
                    gameObject.AddComponent<TwitchFreezeData>();

                if (flags.HasFlag(PersistentComponentFlags.ToggleDoor))
                    gameObject.AddComponent<TwitchDoorPersistentData>();

                if (flags.HasFlag(PersistentComponentFlags.Piece))
                    gameObject.AddComponent<TwitchPiecePersistentData>();
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
