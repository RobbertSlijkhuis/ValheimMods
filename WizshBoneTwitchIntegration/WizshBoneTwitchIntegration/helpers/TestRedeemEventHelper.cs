using System;
using TwitchSDK.Interop;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Builds the CustomRewardEvent for a locally triggered ("test") redeem. The SDK's
    // CustomRewardEvent.GetHashCode() dereferences every string field, so an event with any null
    // string throws as soon as it is used in a HashSet (TwitchCustomRewards.m_unknownRedeems) -
    // which broke HasUnresolvedRedeems and with it the redeem toggle, history and profile switching.
    // Every string is therefore set (to "" unless something meaningful applies).
    // RedemptionId = Guid.Empty is also what marks an event as a test redeem in the history.
    internal static class TestRedeemEventHelper
    {
        public static CustomRewardEvent Create(string title, long cost)
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
                CustomRewardTitle = title,
                CustomRewardCost  = cost,
                CustomRewardPrompt = "",
                RedeemedAt        = DateTime.Now.ToShortDateString()
            };
        }
    }
}
