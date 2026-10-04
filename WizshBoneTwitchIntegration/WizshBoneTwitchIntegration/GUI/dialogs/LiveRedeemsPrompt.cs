using System;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The one place a GUI action that replaces the active profile's redeems (switching profile,
    /// importing over the active one) goes through. With redeems live it asks first (Cancel /
    /// Continue, plus Open history when unresolved redeems exist); otherwise it just performs the
    /// action. The decision logic lives in <see cref="LiveRedeemsHelper"/>.
    /// </summary>
    internal static class LiveRedeemsPrompt
    {
        /// <param name="dialog">The calling tab's own dialog instance.</param>
        /// <param name="title">Dialog title, e.g. "Switch Profile".</param>
        /// <param name="actionDescription">What the action does, e.g. "Switching profiles now can break them" - slots into "Redeems are live. {…}, so Continue will turn redeems off first."</param>
        /// <param name="perform">The action itself. Returns an error message, or null on success.</param>
        /// <param name="onOpenHistory">Opens the redeem history; may be null (button then does nothing).</param>
        /// <param name="onDone">Called after <paramref name="perform"/> succeeded.</param>
        /// <param name="onNotDone">Called when the action did not happen (Cancel, Open history, or <paramref name="perform"/> failed).</param>
        /// <param name="liveSuccessToast">Shown only when the action ran after turning redeems off; may be null.</param>
        public static void Request(
            ConfirmDialog dialog,
            string title,
            string actionDescription,
            Func<string> perform,
            Action onOpenHistory,
            Action onDone,
            Action onNotDone,
            string liveSuccessToast = null)
        {
            if (!LiveRedeemsHelper.AreRedeemsLive())
            {
                Finish(perform(), onDone, onNotDone, null);
                return;
            }

            // Read before anything changes: HasUnresolvedRedeems depends on m_enabled and on the
            // active profile's autoResolveRedeems, and the action can change both.
            TwitchCustomRewards rewards = LiveRedeemsHelper.GetRewards();
            bool hasUnresolved = rewards.HasUnresolvedRedeems();
            bool bulkRunning = LiveRedeemsHelper.IsBulkResolveRunning();

            string description = $"Redeems are live. {actionDescription}, so Continue will turn redeems off first.";
            if (bulkRunning)
                description += " Wait for the running bulk resolve to finish before continuing.";
            else if (hasUnresolved)
                description += " Pending redeems can't be resolved once redeems are off.";

            dialog.Show(
                title:          title,
                description:    description,
                onConfirm:      () => Finish(LiveRedeemsHelper.RunTurningRedeemsOff(perform), onDone, onNotDone, liveSuccessToast),
                onCancel:       () => onNotDone?.Invoke(),
                confirmText:    "Continue",
                cancelText:     "Cancel",
                extraText:      hasUnresolved ? "Open history" : null,
                onExtra:        () =>
                {
                    onNotDone?.Invoke();
                    onOpenHistory?.Invoke();
                },
                confirmEnabled: !bulkRunning);
        }

        private static void Finish(string error, Action onDone, Action onNotDone, string successToast)
        {
            if (error != null)
            {
                ToastNotifications.Show(error, ToastType.Error);
                onNotDone?.Invoke();
                return;
            }

            if (successToast != null)
                ToastNotifications.Show(successToast, ToastType.Success);

            onDone?.Invoke();
        }
    }
}
