using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Twitch caps a channel at <see cref="MaxRewards"/> custom channel point rewards, and that cap is
    /// shared with rewards the streamer made by hand or through other tools. The Twitch SDK can only
    /// replace this app's own rewards and has no call that lists a channel's rewards, so this asks
    /// Helix (GET /channel_points/custom_rewards) directly, reusing the SDK login's token (see
    /// <see cref="TwitchTokenCapture"/>; it already holds channel:manage:redemptions, which Helix
    /// accepts for this read).
    /// </summary>
    internal static class TwitchRewardLimitHelper
    {
        public const int MaxRewards = 50;

        private const string RewardsUrl = "https://api.twitch.tv/helix/channel_points/custom_rewards";

        private static readonly HttpClient m_http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        // Every reward object carries exactly one top-level "broadcaster_id" (the nested *_setting objects
        // don't), so counting those counts rewards without a JSON parser. The lookbehind skips a title or
        // prompt that happens to contain the escaped text.
        private static readonly Regex m_rewardMarker = new Regex("(?<!\\\\)\"broadcaster_id\"\\s*:", RegexOptions.Compiled);

        public class ChannelRewardCounts
        {
            /// <summary>Every custom reward on the channel, whoever made it.</summary>
            public int Total;

            /// <summary>Rewards created by this app (the Twitch client id); these are the ones <c>ReplaceCustomRewards</c> replaces.</summary>
            public int Ours;

            /// <summary>Rewards that stay on the channel whatever we push: the streamer's own and other tools'.</summary>
            public int Others => Total - Ours;
        }

        /// <summary>
        /// Counts the channel's rewards. Resolves to null if the lookup can't be done (not logged in, no
        /// token, network or Twitch error), so callers can carry on without the check. Safe to call from
        /// the main thread: the request runs on the thread pool, read the result once <c>IsCompleted</c>.
        /// </summary>
        public static async Task<ChannelRewardCounts> FetchAsync(string broadcasterId)
        {
            string token = TwitchTokenCapture.AccessToken;

            if (string.IsNullOrEmpty(broadcasterId) || string.IsNullOrEmpty(token))
                return null;

            string clientId = TwitchSDKSettings.Instance.ClientId;

            try
            {
                string query = $"?broadcaster_id={Uri.EscapeDataString(broadcasterId)}";

                Task<int> total = CountAsync(query, token, clientId);
                Task<int> ours = CountAsync(query + "&only_manageable_rewards=true", token, clientId);
                await Task.WhenAll(total, ours).ConfigureAwait(false);

                return new ChannelRewardCounts { Total = total.Result, Ours = ours.Result };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the channel's Twitch rewards: {e.Message}");
                return null;
            }
        }

        private static async Task<int> CountAsync(string query, string token, string clientId)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RewardsUrl + query))
            {
                request.Headers.Add("Authorization", "Bearer " + token);
                request.Headers.Add("Client-Id", clientId);

                using (HttpResponseMessage response = await m_http.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Twitch answered {(int)response.StatusCode}: {body}");

                    return m_rewardMarker.Matches(body).Count;
                }
            }
        }
    }
}
