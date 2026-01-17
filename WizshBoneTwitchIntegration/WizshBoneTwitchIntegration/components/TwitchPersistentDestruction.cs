using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPersistentDestruction : MonoBehaviour
    {
        public ZNetView m_netView;
        public int m_duration;
        public string m_durationHash = "PersistentDuration_WBTI";
        public string m_started;
        public string m_startedHash = "PersistentStarted_WBTI";
        public EffectList m_onDestroyEffects;

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in mister destruction!");
                return;
            }

            m_started = m_netView.GetZDO().GetString(m_startedHash, "");

            if (m_started == "")
                return;

            m_duration = m_netView.GetZDO().GetInt(m_durationHash, 60);

            DateTime startTime = DateTime.Parse(m_started);
            TimeSpan timeSpan = DateTime.Now.Subtract(startTime);
            float timePassed = (float)timeSpan.TotalSeconds;

            TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
            timedDestruction.m_timeout = timePassed > m_duration ? 0f : m_duration - timePassed;
            timedDestruction.Trigger();
        }

        public void OnDestroy()
        {
            if (m_onDestroyEffects != null)
                m_onDestroyEffects.Create(transform.position, transform.rotation);
        }

        public void SetStarted(int duration, EffectList onDestroyEfects = null)
        {
            TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
            timedDestruction.m_timeout = duration;
            timedDestruction.Trigger();

            DateTime dateTime = DateTime.Now;
            m_netView.GetZDO().Set(m_durationHash, duration);
            m_netView.GetZDO().Set(m_startedHash, dateTime.ToString());

            if (onDestroyEfects != null)
                m_onDestroyEffects = onDestroyEfects;
        }
    }
}
