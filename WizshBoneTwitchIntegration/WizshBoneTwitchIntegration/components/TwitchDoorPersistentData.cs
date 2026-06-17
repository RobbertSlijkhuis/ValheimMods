using System;
using UnityEngine;
using WizshBoneTwitchIntegration.extensions;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchDoorPersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private Door m_door;

        private static readonly int s_intervalHash = "WBTI_Door_Interval".GetStableHashCode();

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();
                m_door = gameObject.GetComponent<Door>();

                if (m_netView == null || !m_netView.IsValid() || m_door == null)
                    return;

                float interval = m_netView.GetZDO().GetFloat(s_intervalHash, 0f);
                if (interval <= 0f)
                    return;

                m_door.StartCoroutine(m_door.ToggleDoorContinuously(interval));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchDoorPersistentData.Awake failed: " + e);
            }
        }

        public void Initialize(float interval)
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();
                m_door = gameObject.GetComponent<Door>();

                if (m_netView == null || !m_netView.IsValid() || m_door == null || interval <= 0f)
                    return;

                m_netView.GetZDO().Set(s_intervalHash, interval);

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.ToggleDoor, true);

                m_door.StartCoroutine(m_door.ToggleDoorContinuously(interval));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchDoorPersistentData.Initialize failed: " + e);
            }
        }
    }
}
