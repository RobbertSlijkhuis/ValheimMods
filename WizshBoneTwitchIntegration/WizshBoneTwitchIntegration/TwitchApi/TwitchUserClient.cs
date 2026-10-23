using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>Who the logged-in player is, from Helix's "Get Users" (no user id given = the token's own user).</summary>
    internal static class TwitchUserClient
    {
        /// <summary>The logged-in user, or null when Twitch could not be asked or its answer could not be read.</summary>
        public static async Task<TwitchUserInfo> GetMyUserAsync()
        {
            TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(HttpMethod.Get, "users");

            if (!response.IsSuccess)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read your Twitch account: {response}");
                return null;
            }

            try
            {
                // An empty list (no account) must reach the check below instead of throwing.
                JToken user = (JObject.Parse(response.Body)["data"] as JArray)?.FirstOrDefault();

                if (user == null || string.IsNullOrEmpty((string)user["id"]))
                {
                    Jotunn.Logger.LogWarning("[WBTI] Twitch returned no account for this login.");
                    return null;
                }

                return new TwitchUserInfo
                {
                    channelId = (string)user["id"],
                    loginName = (string)user["login"] ?? "",
                    displayName = (string)user["display_name"] ?? (string)user["login"] ?? "",
                    broadcasterType = (string)user["broadcaster_type"] ?? ""
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read your Twitch account: {e.Message}");
                return null;
            }
        }
    }
}
