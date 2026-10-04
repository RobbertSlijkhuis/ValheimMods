using System;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Logic behind the "replacing the active profile's redeems while redeems are live" warning
    /// (see gui/dialogs/LiveRedeemsPrompt.cs): switching to another profile, or importing over the
    /// active one. Both only swap local data and never touch Twitch, so with redeems live the old
    /// rewards stay on Twitch while the redeem list underneath them is replaced - hence the
    /// redeems have to be turned off as part of such an action.
    /// </summary>
    internal static class LiveRedeemsHelper
    {
        public static TwitchCustomRewards GetRewards()
        {
            return Game.instance?.gameObject?.GetComponent<TwitchCustomRewards>();
        }

        /// <summary>
        /// Redeems are on AND the user is logged in. <c>m_enabled</c> alone isn't enough: Logout
        /// never resets it, so it can stay true with nothing actually pushed to Twitch.
        /// </summary>
        public static bool AreRedeemsLive()
        {
            TwitchCustomRewards rewards = GetRewards();
            return rewards != null && rewards.m_enabled && rewards.IsLoggedIn;
        }

        /// <summary>
        /// Complete All / Refund All is mid-run. Turning redeems off clears every reward from
        /// Twitch, which would strand the entries it still has queued.
        /// </summary>
        public static bool IsBulkResolveRunning()
        {
            TwitchCustomRewards rewards = GetRewards();
            return rewards != null && rewards.m_bulkResolveHelper.IsRunning;
        }

        /// <summary>
        /// Turns redeems off, then runs <paramref name="perform"/> (returns an error message, or
        /// null on success). The order doesn't matter for correctness - nothing local waits on the
        /// Twitch-side clear - but callers must read anything that depends on <c>m_enabled</c> or
        /// the active profile's settings (e.g. HasUnresolvedRedeems) BEFORE calling this, since
        /// both can change here.
        /// </summary>
        public static string RunTurningRedeemsOff(Func<string> perform)
        {
            GetRewards()?.SetEnableRedeems(false);
            return perform();
        }
    }
}
