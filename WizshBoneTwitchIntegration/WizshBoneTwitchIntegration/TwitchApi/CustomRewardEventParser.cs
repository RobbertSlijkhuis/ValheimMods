using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Turns the JSON Twitch sends about a redemption (an EventSub notification, or an entry of the Helix redemption
    /// list) into a <see cref="CustomRewardEvent"/>. Every string is set (to "" when the JSON lacks it), the way a
    /// test redeem's are.
    /// </summary>
    internal static class CustomRewardEventParser
    {
        /// <summary>
        /// Parses JSON without turning date-looking strings into dates: <c>redeemed_at</c> must stay the text Twitch
        /// sent, not a culture-formatted DateTime.
        /// </summary>
        public static JObject ParseObject(string json)
        {
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None })
                return JObject.Load(reader);
        }

        /// <summary>
        /// The <c>event</c> of a channel.channel_points_custom_reward_redemption.add notification, or one entry of the
        /// <c>data</c> list of Helix's "Get Custom Reward Redemption". Both have the same fields except for the
        /// broadcaster's (<c>broadcaster_user_id</c> in EventSub, <c>broadcaster_id</c> in Helix).
        /// </summary>
        public static CustomRewardEvent FromJson(JToken redemption)
        {
            JToken reward = redemption["reward"];

            return new CustomRewardEvent
            {
                RedemptionId = (string)redemption["id"] ?? "",
                BroadcasterId = (string)(redemption["broadcaster_user_id"] ?? redemption["broadcaster_id"]) ?? "",
                BroadcasterName = (string)(redemption["broadcaster_user_name"] ?? redemption["broadcaster_name"]) ?? "",
                RedeemerId = (string)redemption["user_id"] ?? "",
                RedeemerName = (string)redemption["user_name"] ?? "",
                UserInput = (string)redemption["user_input"] ?? "",
                Status = ParseStatus((string)redemption["status"]),
                CustomRewardId = (string)reward?["id"] ?? "",
                CustomRewardTitle = (string)reward?["title"] ?? "",
                CustomRewardCost = (long?)reward?["cost"] ?? 0,
                CustomRewardPrompt = (string)reward?["prompt"] ?? "",
                RedeemedAt = (string)redemption["redeemed_at"] ?? ""
            };
        }

        public static CustomRewardRedemptionState ParseStatus(string status)
        {
            switch ((status ?? "").ToUpperInvariant())
            {
                case "UNFULFILLED": return CustomRewardRedemptionState.Unfulfilled;
                case "FULFILLED": return CustomRewardRedemptionState.Fulfilled;
                case "CANCELED": return CustomRewardRedemptionState.Canceled;
                default: return CustomRewardRedemptionState.Unknown;
            }
        }

        /// <summary>
        /// Reads Twitch's <c>redeemed_at</c> (RFC 3339 with up to nine fractional digits, which DateTime does not
        /// take). False when it is missing or not a date.
        /// </summary>
        public static bool TryParseRedeemedAt(string redeemedAt, out DateTime utc)
        {
            string trimmed = Regex.Replace(redeemedAt ?? "", @"(\.\d{3})\d+", "$1");

            return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out utc);
        }
    }
}
