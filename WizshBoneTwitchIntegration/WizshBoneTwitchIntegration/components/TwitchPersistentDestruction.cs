using System;
using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

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

        // Lets Pause()/Resume() (called by TwitchPhysicsFreezeData while a TimeStop zone holds
        // this object) stop the countdown and later continue from wherever it left off, instead
        // of losing the elapsed time or restarting the full configured duration.
        private float m_remainingTime;
        private float m_segmentStartTime;
        private bool m_paused;
        private bool m_timerStarted;
        private Coroutine m_timerElapsedRoutine;
        private Coroutine m_destroyRoutine;

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
            // m_timerStarted (rather than m_destroyRoutine/m_breakRoutine being null) is the guard
            // here because Pause() nulls both of those out while the timer is merely paused, not
            // finished - using them would let a second SetStarted() call during a pause re-run this
            // block and reset the ZDO's stored start time/duration.
            if (!m_timerStarted)
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
            m_timerStarted = true;
            m_remainingTime = timeout;
            m_segmentStartTime = Time.time;
            StartSegment();
        }

        // Shared by the initial start and Resume() - both just differ in what m_remainingTime
        // already holds by the time this runs.
        private void StartSegment()
        {
            m_timerElapsedRoutine = StartCoroutine(MarkTimerElapsed(m_remainingTime));

            if (m_breakOnDestroy && gameObject.GetComponent<WearNTear>() != null)
            {
                m_breakRoutine = StartCoroutine(BreakAfterDelay(m_remainingTime));
                return;
            }

            m_destroyRoutine = StartCoroutine(DestroyAfterDelay(m_remainingTime));
        }

        // Called by TwitchPhysicsFreezeData when a TimeStop zone freezes this object - stops
        // whichever destruction path is active (coroutines keep running regardless of this
        // component's enabled state, so they have to be explicitly stopped) and remembers how much
        // time was left.
        public void Pause()
        {
            if (!m_timerStarted || m_paused)
                return;

            m_paused = true;
            m_remainingTime = Mathf.Max(0f, m_remainingTime - (Time.time - m_segmentStartTime));

            if (m_destroyRoutine != null)
            {
                StopCoroutine(m_destroyRoutine);
                m_destroyRoutine = null;
            }

            if (m_breakRoutine != null)
            {
                StopCoroutine(m_breakRoutine);
                m_breakRoutine = null;
            }

            if (m_timerElapsedRoutine != null)
            {
                StopCoroutine(m_timerElapsedRoutine);
                m_timerElapsedRoutine = null;
            }
        }

        // Called by TwitchPhysicsFreezeData on unfreeze - continues the countdown from the
        // remaining time captured by Pause(), rather than restarting the full duration.
        public void Resume()
        {
            if (!m_timerStarted || !m_paused)
                return;

            m_paused = false;
            m_segmentStartTime = Time.time;
            StartSegment();
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

            if (m_netView == null || !m_netView.IsValid())
                yield break;

            // Claim ownership first, mirroring ZNetViewHelper.Destroy() - the client that started
            // this timer may no longer be the ZDO owner by the time it elapses (ownership can drift
            // to another peer while this object stays loaded), and a non-owner's WearNTear.Destroy()
            // never propagates. Without this, a stuck object would silently never break until some
            // other event (e.g. a reload) happened to reclaim ownership or force it another way.
            if (!m_netView.IsOwner())
                m_netView.ClaimOwnership();

            // A redeem boat expiring under the streamer: if they end up swimming right after, this redeemer is the cause.
            DeathCreditHelper.NoteBoatExpiring(gameObject);

            gameObject.GetComponent<WearNTear>()?.Destroy();
        }

        // Used instead of vanilla TimedDestruction for the non-breakOnDestroy path - TimedDestruction's
        // own DestroyNow() only destroys if m_nview.IsOwner(), with no reclaim fallback, so it silently
        // no-ops forever (once per second) if ownership ever drifted away from this client. Claiming
        // ownership here mirrors ZNetViewHelper.Destroy(), which this reuses for the actual destroy.
        private IEnumerator DestroyAfterDelay(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (m_netView == null || !m_netView.IsValid())
                yield break;

            DeathCreditHelper.NoteBoatExpiring(gameObject);

            ZNetViewHelper.Destroy(gameObject);
        }
    }
}
