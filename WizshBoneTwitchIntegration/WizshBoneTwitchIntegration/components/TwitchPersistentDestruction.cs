using System;
using System.Collections;
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
        public string m_breakOnDestroyHash = "PersistentBreakOnDestroy_WBTI";
        public bool m_breakOnDestroy;
        public EffectList m_onDestroyEffects;

        public delegate void onEndDelegate(GameObject prefab);
        public onEndDelegate onEnd;

        // Cached at Awake/SetStarted time: by the time OnDestroy runs (e.g. after
        // WearNTear.Destroy(), which tears down the ZDO before Unity's deferred
        // OnDestroy fires), m_netView's ZDO may already be gone and IsOwner() unreliable.
        private bool m_isOwner;

        // Set once the duration timer actually runs out, so OnDestroy can tell a
        // natural expiry apart from an early destruction (e.g. player breaking it).
        private bool m_timerElapsed;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                m_isOwner = m_netView.IsOwner();

                m_started = m_netView.GetZDO().GetString(m_startedHash, "");

                if (m_started == "")
                    return;

                m_duration = m_netView.GetZDO().GetInt(m_durationHash, 60);
                m_breakOnDestroy = m_netView.GetZDO().GetBool(m_breakOnDestroyHash, false);

                DateTime startTime = DateTime.Parse(m_started);
                TimeSpan timeSpan = DateTime.Now.Subtract(startTime);
                float timePassed = (float)timeSpan.TotalSeconds;

                StartDestructionTimer(timePassed > m_duration ? 0f : m_duration - timePassed);
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
                if (m_isOwner && m_timerElapsed && m_onDestroyEffects != null && !(m_breakOnDestroy && gameObject.GetComponent<WearNTear>() != null))
                    m_onDestroyEffects.Create(transform.position, transform.rotation);

                onEnd?.Invoke(gameObject);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPersistentDestruction.OnDestroy failed: " + e);
            }
        }

        public void SetStarted(int duration, EffectList onDestroyEfects = null, bool breakOnDestroy = false)
        {
            if (gameObject.GetComponent<TimedDestruction>() == null && m_breakRoutine == null)
            {
                m_breakOnDestroy = breakOnDestroy;
                m_isOwner = m_netView.IsOwner();

                DateTime dateTime = DateTime.Now;
                m_netView.GetZDO().Set(m_durationHash, duration);
                m_netView.GetZDO().Set(m_startedHash, dateTime.ToString());
                m_netView.GetZDO().Set(m_breakOnDestroyHash, breakOnDestroy);

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.Destruction, true);

                StartDestructionTimer(duration);
            }

            if (onDestroyEfects != null)
                m_onDestroyEffects = onDestroyEfects;
        }

        private Coroutine m_breakRoutine;

        private void StartDestructionTimer(float timeout)
        {
            StartCoroutine(MarkTimerElapsed(timeout));

            if (m_breakOnDestroy && gameObject.GetComponent<WearNTear>() != null)
            {
                m_breakRoutine = StartCoroutine(BreakAfterDelay(timeout));
                return;
            }

            TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
            timedDestruction.m_timeout = timeout;
            timedDestruction.Trigger();
        }

        private IEnumerator MarkTimerElapsed(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            m_timerElapsed = true;
        }

        private IEnumerator BreakAfterDelay(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (m_netView == null || !m_netView.IsOwner())
                yield break;

            gameObject.GetComponent<WearNTear>()?.Destroy();
        }
    }
}
