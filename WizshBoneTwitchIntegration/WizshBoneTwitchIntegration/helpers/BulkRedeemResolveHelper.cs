using System;
using System.Collections;
using System.Collections.Generic;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Throttled queue for resolving many CustomRewardEvents (Complete All / Refund All) against
    /// the real Twitch API, spread across time instead of firing every call in one frame.
    ///
    /// Unlike DetonateHelper/FlashBangHelper this is deliberately instance-scoped (owned by a
    /// single TwitchCustomRewards, see TwitchCustomRewards.m_bulkResolveHelper) rather than a
    /// static/shared queue: Complete All / Refund All only ever operate on one
    /// TwitchCustomRewards' own m_redeemHistory, so there's no need for a queue that survives or
    /// is shared across instances. That also means it needs none of DetonateHelper's/
    /// FlashBangHelper's ResetQueue() plumbing in GameAwake_Postfix - a fresh TwitchCustomRewards
    /// gets a fresh, idle BulkRedeemResolveHelper (IsRunning defaults to false) for free every
    /// world load/relog.
    /// </summary>
    internal class BulkRedeemResolveHelper
    {
        // Real Twitch Helix calls via the native SDK, not local VFX - paced conservatively since
        // we don't have exact rate-limit numbers for the "update redemption status" endpoint.
        // ~3.3 calls/sec keeps a full 500-entry history under ~2.5 minutes worst case while
        // staying comfortably under any plausible per-second bucket limit.
        private const float DelayBetweenResolves = 0.3f;

        private readonly Queue<CustomRewardEvent> m_queue = new Queue<CustomRewardEvent>();

        public bool IsRunning { get; private set; }
        public int Total { get; private set; }
        public int Completed { get; private set; }
        public CustomRewardRedemptionState TargetState { get; private set; }

        /// <summary>
        /// Same eligibility rule as the old synchronous OnCompleteAll/OnRefundAll loops. Exposed
        /// as public static so both the GUI's enqueue-time filter and this class's own
        /// dequeue-time recheck use the exact same rule.
        /// </summary>
        public static bool IsEligible(CustomRewardEvent entry) =>
            entry.Status != CustomRewardRedemptionState.Fulfilled &&
            entry.Status != CustomRewardRedemptionState.Canceled;

        public void EnqueueAll(
            MonoBehaviour host,
            List<CustomRewardEvent> entries,
            CustomRewardRedemptionState targetState,
            Action onProgress,
            Action onFinished)
        {
            if (IsRunning || entries == null || entries.Count == 0)
                return;

            foreach (CustomRewardEvent entry in entries)
                m_queue.Enqueue(entry);

            Total = entries.Count;
            Completed = 0;
            TargetState = targetState;

            host.StartCoroutine(ProcessQueue(onProgress, onFinished));
        }

        private IEnumerator ProcessQueue(Action onProgress, Action onFinished)
        {
            IsRunning = true;
            try
            {
                while (m_queue.Count > 0)
                {
                    CustomRewardEvent entry = m_queue.Dequeue();

                    // Recheck eligibility here, not just at enqueue time: this entry could have
                    // been resolved another way (its own row's Complete/Refund button,
                    // auto-resolve, HandleRedeemException) while it was sitting queued waiting
                    // its turn - resolving it again would be a redundant/wrong API call.
                    if (IsEligible(entry))
                    {
                        try
                        {
                            entry.Status = TargetState;
                            Twitch.API.ResolveCustomReward(entry, TargetState);
                        }
                        catch (Exception e)
                        {
                            // Swallow and continue: one bad entry must not stop the rest of an
                            // unattended background batch from draining (it keeps running even
                            // after the GUI that triggered it closes).
                            Jotunn.Logger.LogWarning($"[WBTI] BulkRedeemResolveHelper: failed to resolve '{entry.CustomRewardTitle}' for {entry.RedeemerName}: {e}");
                        }
                    }

                    Completed++;
                    onProgress?.Invoke();

                    // WaitForSecondsRealtime, not WaitForSeconds: this must keep draining even
                    // while Game.IsPaused() (Time.timeScale == 0) - e.g. the logout/quit confirm
                    // dialog. See TwitchAuth.cs and gui/LogoutProgressPanel.cs for existing
                    // WaitForSecondsRealtime precedent used for the same "must not stall on
                    // pause" reason. (DetonateHelper/FlashBangHelper intentionally use
                    // WaitForSeconds instead, since those ARE in-game simulation effects that
                    // should pause with the game - this is not that case.)
                    yield return new WaitForSecondsRealtime(DelayBetweenResolves);
                }
            }
            finally
            {
                IsRunning = false;
                Total = 0;
                Completed = 0;
                onFinished?.Invoke();
            }
        }
    }
}
