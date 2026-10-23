namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// What identifies this mod to Twitch. The client id is public (no secret goes with it); it is the one the rewards
    /// are created under, and Twitch only lets the app that created a reward manage its redemptions.
    /// </summary>
    internal static class TwitchApp
    {
        public const string ClientId = "8i260qk16tmvumfssr2h4klu99frjb";

        /// <summary>
        /// Rewards and their redemptions, plus reading and writing chat (IRC). Nothing else is asked for: no bits, no
        /// subscriptions.
        /// </summary>
        public const string LoginScopes = "channel:manage:redemptions chat:read chat:edit channel:bot";
    }
}
