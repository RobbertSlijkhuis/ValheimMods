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

        public delegate void onEndDelegate(GameObject prefab);
        public onEndDelegate onEnd;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

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
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPersistentDestruction.Awake failed: " + e);
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (m_onDestroyEffects != null)
                    m_onDestroyEffects.Create(transform.position, transform.rotation);

                onEnd?.Invoke(gameObject);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPersistentDestruction.OnDestroy failed: " + e);
            }
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
