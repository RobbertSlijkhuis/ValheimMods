using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Tags redeem-spawned objects with who redeemed them (Twitch ID + raw redeem title) on their
    /// <see cref="TwitchBasePersistentData"/>, so a death of the streamer can later be credited to
    /// the right viewer and redeem (<see cref="DeathCreditHelper"/>). Call it next to the existing
    /// SetData of whatever the redeem spawns.
    /// </summary>
    internal static class RedeemerTagHelper
    {
        /// <summary>
        /// Skipped for events without a RedeemerId (console/Redeems-tab test events) and for the
        /// streamer's own redeems (RedeemerId == BroadcasterId) - the same exclusions as the
        /// leaderboard's own recording, so those can never be credited with a death either.
        /// </summary>
        public static void Apply(GameObject target, CustomRewardEvent customRewardEvent)
        {
            if (target == null || !TryResolve(customRewardEvent, out string id, out string rawTitle))
                return;

            target.GetComponent<TwitchBasePersistentData>()?.SetRedeemer(id, rawTitle);
        }

        /// <summary>
        /// The creditable identity of a redeem event: its Twitch ID and the redeem's raw title.
        /// False for events that must never be credited (no RedeemerId - test events; the
        /// streamer's own redeems) or whose redeem can't be found any more.
        /// </summary>
        public static bool TryResolve(CustomRewardEvent customRewardEvent, out string redeemerId, out string rawTitle)
        {
            redeemerId = null;
            rawTitle = null;

            if (customRewardEvent == null
                || string.IsNullOrEmpty(customRewardEvent.RedeemerId)
                || customRewardEvent.RedeemerId == customRewardEvent.BroadcasterId)
                return false;

            // The event carries the Twitch-facing (possibly prefixed) title; the leaderboard keys on
            // the raw RedeemData.title.
            RedeemData redeem = RedeemHelper.GetRedeemByTitle(customRewardEvent.CustomRewardTitle);
            if (redeem == null)
                return false;

            redeemerId = customRewardEvent.RedeemerId;
            rawTitle = redeem.title;
            return true;
        }

        /// <summary>Copies a tag from one object to another (children, propagated fires).</summary>
        public static void Copy(GameObject from, GameObject to)
        {
            if (from == null || to == null)
                return;

            TwitchBasePersistentData source = from.GetComponentInParent<TwitchBasePersistentData>();
            if (source == null || !source.HasRedeemer)
                return;

            to.GetComponent<TwitchBasePersistentData>()?.SetRedeemer(source.RedeemerId, source.RedeemerRawTitle);
        }
    }
}
