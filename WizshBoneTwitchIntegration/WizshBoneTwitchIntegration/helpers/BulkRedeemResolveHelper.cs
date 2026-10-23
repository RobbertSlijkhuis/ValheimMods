using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.TwitchApi.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Throttled queue for resolving many CustomRewardEvents (Complete All / Refund All) against
    /// the real Twitch API, spread across time instead of firing every call in one frame. Each
    /// call is waited for before the next one starts.
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
        // Real Twitch Helix calls, not local VFX - paced conservatively since
        // we don't have exact rate-limit numbers for the "update redemption status" endpoint.
        // ~3.3 calls/sec keeps a full 1000-entry history under ~5 minutes worst case while
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
            TwitchCustomRewards host,
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

            host.StartCoroutine(ProcessQueue(host, onProgress, onFinished));
        }

        private IEnumerator ProcessQueue(TwitchCustomRewards host, Action onProgress, Action onFinished)
        {
            IsRunning = true;
            int failed = 0;

            try
            {
                while (m_queue.Count > 0)
                {
                    CustomRewardEvent entry = m_queue.Dequeue();

                    // Recheck eligibility here, not just at enqueue time: this entry could have
                    // been resolved another way (its own row's Complete/Refund button,
                    // auto-resolve, HandleRedeemException) while it was sitting queued waiting
                    // its turn - resolving it again would be a redundant/wrong API call.
                    // One a row button is completing or refunding right now is left to that call: it is neither ours to
                    // send again nor a failure.
                    if (IsEligible(entry) && !entry.IsResolving)
                    {
                        // Never throws (a failure is a false result) - one bad entry must not stop the rest of an
                        // unattended background batch from draining (it keeps running even after the GUI that
                        // triggered it closes). A failed entry stays unfulfilled in the history.
                        Task<bool> resolve = host.ResolveAsync(entry, TargetState, toastOnFailure: false);

                        while (!resolve.IsCompleted)
                            yield return null;

                        if (resolve.Status != TaskStatus.RanToCompletion || !resolve.Result)
                            failed++;
                    }

                    Completed++;
                    onProgress?.Invoke();

                    // WaitForSecondsRealtime, not WaitForSeconds: this must keep draining even
                    // while Game.IsPaused() (Time.timeScale == 0) - e.g. the logout/quit confirm
                    // dialog. See TwitchAuth.cs and GUI/Hud/LogoutProgressHUD.cs for existing
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

                if (failed > 0)
                    ToastNotifications.Show($"{failed} redeems could not be {(TargetState == CustomRewardRedemptionState.Fulfilled ? "completed" : "refunded")} on Twitch. They stay in the history.", ToastType.Warning);
            }
        }
    }
}
