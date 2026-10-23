using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// The redemption side of Helix's channel points API: completing or refunding a redemption, and listing the ones
    /// that are still waiting. Both only reach redemptions of rewards this app created. Call them on the main thread
    /// (see <see cref="TwitchHelixClient.SendAsync"/>).
    /// </summary>
    internal static class TwitchRedemptionClient
    {
        private const string RedemptionsPath = "channel_points/custom_rewards/redemptions";
        private const int PageSize = 50;
        private const int MaxPages = 20;

        /// <summary>Sets the redemption to Fulfilled (complete) or Canceled (refund the viewer's points).</summary>
        public static Task<TwitchHelixClient.Response> ResolveAsync(string broadcasterId, CustomRewardEvent entry, CustomRewardRedemptionState state)
        {
            string query = $"id={Uri.EscapeDataString(entry.RedemptionId)}"
                + $"&broadcaster_id={Uri.EscapeDataString(broadcasterId)}"
                + $"&reward_id={Uri.EscapeDataString(entry.CustomRewardId)}";
            string body = new JObject { ["status"] = state == CustomRewardRedemptionState.Fulfilled ? "FULFILLED" : "CANCELED" }.ToString(Formatting.None);

            return TwitchHelixClient.SendAsync(TwitchHelixClient.Patch, RedemptionsPath, query, body);
        }

        /// <summary>
        /// The redemptions of the reward that are still unfulfilled and were made since <paramref name="sinceUtc"/>
        /// (and some older ones: whole pages are returned), newest first. null when Twitch could not be asked, its
        /// answer could not be read, or there are so many waiting that the page limit was hit before reaching
        /// <paramref name="sinceUtc"/>, so the list would be incomplete.
        /// </summary>
        public static async Task<List<CustomRewardEvent>> ListUnfulfilledAsync(string broadcasterId, string rewardId, DateTime sinceUtc)
        {
            List<CustomRewardEvent> found = new List<CustomRewardEvent>();
            string cursor = null;

            try
            {
                for (int page = 0; page < MaxPages; page++)
                {
                    string query = $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}"
                        + $"&reward_id={Uri.EscapeDataString(rewardId)}&status=UNFULFILLED&sort=NEWEST&first={PageSize}"
                        + (cursor == null ? "" : $"&after={Uri.EscapeDataString(cursor)}");

                    TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(HttpMethod.Get, RedemptionsPath, query);

                    if (!response.IsSuccess)
                    {
                        Jotunn.Logger.LogWarning($"[WBTI] Could not read the pending redemptions of a reward: {response}");
                        return null;
                    }

                    JObject json = CustomRewardEventParser.ParseObject(response.Body);

                    // Newest first: once a redemption older than the window shows up, the rest is older still.
                    bool reachedOlder = false;

                    if (json["data"] is JArray data)
                    {
                        foreach (JToken item in data)
                        {
                            CustomRewardEvent redemption = CustomRewardEventParser.FromJson(item);
                            found.Add(redemption);

                            if (CustomRewardEventParser.TryParseRedeemedAt(redemption.RedeemedAt, out DateTime madeAt) && madeAt < sinceUtc)
                                reachedOlder = true;
                        }
                    }

                    cursor = (string)json["pagination"]?["cursor"];

                    if (reachedOlder || string.IsNullOrEmpty(cursor))
                        return found;
                }

                Jotunn.Logger.LogWarning($"[WBTI] Too many redemptions are waiting on a reward to look through them all ({MaxPages * PageSize}).");
                return null;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the pending redemptions of a reward: {e.Message}");
                return null;
            }
        }
    }
}
