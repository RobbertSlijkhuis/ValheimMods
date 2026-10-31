using System;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Builds the CustomRewardEvent for a locally triggered ("test") redeem. Every string is set
    // (to "" unless something meaningful applies), like the ones built from Twitch's JSON.
    // RedemptionId = Guid.Empty is also what marks an event as a test redeem in the history.
    internal static class TestRedeemEventHelper
    {
        /// <summary>Whether the event came from <see cref="Create"/>: it exists only in the game, not on Twitch.</summary>
        public static bool IsTestRedeem(CustomRewardEvent entry)
        {
            return entry.RedemptionId == Guid.Empty.ToString();
        }

        /// <summary>
        /// The event carries the redeem's id, which HandleRedeem finds the redeem by, and its Twitch-facing (prefixed)
        /// title, which what the redeem spawns looks the redeem up by again later.
        /// </summary>
        public static CustomRewardEvent Create(RedeemData redeem, long cost)
        {
            return new CustomRewardEvent
            {
                RedemptionId      = Guid.Empty.ToString(),
                BroadcasterId     = "",
                RedeemerId        = "",
                BroadcasterName   = "",
                RedeemerName      = "WizshBone",
                UserInput         = "",
                Status            = CustomRewardRedemptionState.Unfulfilled,
                CustomRewardId    = "",
                CustomRewardTitle = RedeemManager.GetFullTitle(redeem),
                RedeemId          = redeem.id,
                CustomRewardCost  = cost,
                CustomRewardPrompt = "",
                RedeemedAt        = DateTime.Now.ToShortDateString()
            };
        }
    }
}
