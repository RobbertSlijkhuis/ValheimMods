namespace WizshBoneTwitchIntegration.TwitchApi.Models
{
    /// <summary>Where a redemption stands on Twitch: waiting in the streamer's queue, completed, or refunded.</summary>
    public enum CustomRewardRedemptionState
    {
        Unknown,
        Unfulfilled,
        Fulfilled,
        Canceled
    }
}
