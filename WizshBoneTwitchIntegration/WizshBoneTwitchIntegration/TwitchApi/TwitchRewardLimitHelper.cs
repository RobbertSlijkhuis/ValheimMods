using System.Collections.Generic;
using System.Threading.Tasks;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Twitch caps a channel at <see cref="MaxRewards"/> custom channel point rewards, and that cap is
    /// shared with rewards the streamer made by hand or through other tools. This asks Helix
    /// (GET /channel_points/custom_rewards) how many of the channel's rewards are ours and how many are
    /// somebody else's.
    /// </summary>
    internal static class TwitchRewardLimitHelper
    {
        public const int MaxRewards = 50;

        public class ChannelRewardCounts
        {
            /// <summary>Every custom reward on the channel, whoever made it.</summary>
            public int Total;

            /// <summary>Rewards created by this app (the Twitch client id); these are the ones <see cref="TwitchRewardSync"/> manages.</summary>
            public int Ours;

            /// <summary>The rewards counted in <see cref="Ours"/>, so a sync right after the lookup doesn't read them again.</summary>
            public List<HelixReward> OwnRewards;

            /// <summary>Rewards that stay on the channel whatever we push: the streamer's own and other tools'.</summary>
            public int Others => Total - Ours;
        }

        /// <summary>
        /// Counts the channel's rewards. Resolves to null if the lookup can't be done (not logged in, no
        /// token, network or Twitch error), so callers can carry on without the check. Call it on the main
        /// thread (the token and client id are read there) and read the result once <c>IsCompleted</c>.
        /// </summary>
        public static async Task<ChannelRewardCounts> FetchAsync(string broadcasterId)
        {
            if (string.IsNullOrEmpty(broadcasterId))
                return null;

            Task<List<HelixReward>> all = TwitchRewardSync.ListAsync(broadcasterId, onlyOwn: false);
            Task<List<HelixReward>> own = TwitchRewardSync.ListAsync(broadcasterId, onlyOwn: true);
            await Task.WhenAll(all, own);

            // ListAsync has logged why when it couldn't read them.
            if (all.Result == null || own.Result == null)
                return null;

            return new ChannelRewardCounts
            {
                Total = all.Result.Count,
                Ours = own.Result.Count,
                OwnRewards = own.Result,
            };
        }
    }
}
