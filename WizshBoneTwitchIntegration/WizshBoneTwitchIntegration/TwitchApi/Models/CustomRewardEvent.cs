namespace WizshBoneTwitchIntegration.TwitchApi.Models
{
    /// <summary>
    /// One redemption of a channel point reward, as the redeem code works with it. Built from an EventSub notification
    /// (TwitchEventSubClient), from the Helix redemption list (the catch-up after a lost connection) or locally for
    /// a test redeem (TestRedeemEventHelper). Compared by reference: the same instance goes through the history
    /// and the bulk queue, and carries the local state at the bottom.
    /// </summary>
    public class CustomRewardEvent
    {
        /// <summary>The redemption's own id; Guid.Empty marks a test redeem that never existed on Twitch.</summary>
        public string RedemptionId;

        public string BroadcasterId;
        public string BroadcasterName;

        public string RedeemerId;

        /// <summary>The viewer's display name. Settable: a test redeem can run under the alias.</summary>
        public string RedeemerName;

        public string UserInput;
        public CustomRewardRedemptionState Status;

        public string CustomRewardId;
        public string CustomRewardTitle;
        public long CustomRewardCost;
        public string CustomRewardPrompt;

        public string RedeemedAt;

        // What follows is local state kept by TwitchCustomRewards; nothing of it comes from Twitch.

        /// <summary>The redeem is not in the active profile: left unresolved in the history for the streamer to decide.</summary>
        public bool IsUnknown;

        /// <summary>A complete or refund of this redemption is being sent to Twitch right now.</summary>
        public bool IsResolving;

        /// <summary>Twitch did not accept the last complete or refund, so the redemption is still waiting.</summary>
        public bool ResolveFailed;

        /// <summary>
        /// Why the redeem was rejected, in words for the viewer ("the streamer is inside a Twitch safe zone!"). Set before
        /// the refund; the chat message that tells the viewer follows once the refund result is known.
        /// </summary>
        public string RejectReason;
    }
}
