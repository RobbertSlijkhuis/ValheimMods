using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Makes the rewards this app owns on Twitch match the wanted list by creating, editing and deleting
    /// them one by one through Helix. Unlike the SDK's ReplaceCustomRewards (which re-creates every reward
    /// with a new id on every push) an edited reward keeps its id, so redemptions still pending on it can
    /// be fulfilled or refunded afterwards. Rewards are matched by title; <c>formerTitles</c> tells the sync
    /// that a reward was renamed so it is edited instead of deleted and created again.
    /// </summary>
    internal static class TwitchRewardSync
    {
        private const string RewardsPath = "channel_points/custom_rewards";
        public const int MaxTitleLength = 45;
        public const int MaxPromptLength = 200;

        private static readonly Regex m_hexColor = new Regex("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        /// <summary>One reward we want on Twitch.</summary>
        public class Desired
        {
            public string Title;
            public string Prompt;
            public int Cost;
            public string BackgroundColor;
            public bool UserInput;

            /// <summary>0 = no limit, same for the other two.</summary>
            public int MaxPerStream;
            public int MaxPerUserPerStream;
            public int CooldownSeconds;
        }

        public class Result
        {
            public int Created;
            public int Edited;
            public int Deleted;
            public int Unchanged;
            public int Failed;

            /// <summary>Wanted rewards left out because their title is empty or the same as another one's.</summary>
            public int Skipped;

            /// <summary>Set when a newer push or a clear took over before all changes were made.</summary>
            public bool Cancelled;

            /// <summary>True when the current rewards could not even be read.</summary>
            public bool LookupFailed;

            public override string ToString() =>
                $"{Created} created, {Edited} edited, {Deleted} deleted, {Unchanged} unchanged, {Failed} failed, {Skipped} skipped";
        }

        /// <param name="wanted">What should be on Twitch; the caller has checked it fits under the cap.</param>
        /// <param name="formerTitles">Wanted title -> the titles that redeem had before, the most recent first. A reward
        /// still under one of them is edited instead of deleted and created again.</param>
        /// <param name="knownRewards">Our rewards as just read from Twitch, to save reading them again; null reads them here.</param>
        public static async Task<Result> SyncAsync(string broadcasterId, IList<Desired> wanted, IDictionary<string, List<string>> formerTitles, CancellationToken cancel, List<HelixReward> knownRewards = null)
        {
            Result result = new Result();
            List<HelixReward> existing = knownRewards ?? await ListAsync(broadcasterId, onlyOwn: true);

            if (existing == null)
            {
                result.LookupFailed = true;
                return result;
            }

            List<Desired> desired = Normalize(wanted, result);
            Dictionary<Desired, HelixReward> matches = Match(desired, existing, TruncateFormerTitles(formerTitles), out List<HelixReward> leftovers);

            // Deletes first so a title or slot they free up is available to the edits and creates.
            foreach (HelixReward reward in leftovers)
            {
                if (cancel.IsCancellationRequested)
                    return Cancelled(result);

                if ((await DeleteAsync(broadcasterId, reward)).IsSuccess)
                    result.Deleted++;
                else
                    result.Failed++;
            }

            foreach (Desired d in desired)
            {
                if (!matches.TryGetValue(d, out HelixReward reward))
                    continue;

                if (!NeedsEdit(reward, d))
                {
                    result.Unchanged++;
                    continue;
                }

                if (cancel.IsCancellationRequested)
                    return Cancelled(result);

                TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(
                    TwitchHelixClient.Patch, RewardsPath, $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}&id={Uri.EscapeDataString(reward.id)}",
                    BuildBody(d, includeTitle: reward.title != d.Title));

                if (response.IsSuccess)
                    result.Edited++;
                else
                    result.Failed += Fail("edit", d.Title, response);
            }

            foreach (Desired d in desired)
            {
                if (matches.ContainsKey(d))
                    continue;

                if (cancel.IsCancellationRequested)
                    return Cancelled(result);

                TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(
                    HttpMethod.Post, RewardsPath, $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}", BuildBody(d, includeTitle: true));

                if (response.IsSuccess)
                    result.Created++;
                else
                    result.Failed += Fail("create", d.Title, response);
            }

            return result;
        }

        /// <summary>
        /// Deletes every reward this app owns on the channel and says whether all of them are gone (false
        /// when the rewards couldn't be read or a delete was refused). Never throws; failures are logged.
        /// Stops at the first delete that gets no answer or is refused for the login (401): every one after it
        /// would fail the same way, so a dead connection ends the clear after one request instead of one per reward.
        /// Logging out wipes the stored token while this is still running, so it works with the
        /// <paramref name="token"/> taken before the logout.
        /// </summary>
        public static async Task<bool> ClearAsync(string broadcasterId, string token)
        {
            // Nothing was ever pushed without a channel id.
            if (string.IsNullOrEmpty(broadcasterId))
                return true;

            List<HelixReward> existing = await ListAsync(broadcasterId, onlyOwn: true, token);

            if (existing == null)
                return false;

            bool allDeleted = true;

            foreach (HelixReward reward in existing)
            {
                TwitchHelixClient.Response response = await DeleteAsync(broadcasterId, reward, token);

                if (response.IsSuccess)
                    continue;

                allDeleted = false;

                if (response.Status == 0 || response.Status == 401)
                    break;
            }

            return allDeleted;
        }

        private static Result Cancelled(Result result)
        {
            result.Cancelled = true;
            return result;
        }

        private static int Fail(string action, string title, TwitchHelixClient.Response response)
        {
            Jotunn.Logger.LogWarning($"[WBTI] Twitch rejected the {action} of '{title}': {response}");
            return 1;
        }

        /// <summary>
        /// The channel's rewards: only the ones this app created (Twitch's "manageable" ones) with
        /// <paramref name="onlyOwn"/>, otherwise every reward whoever made it. null when they could not be read.
        /// Call it on the main thread (the token and client id are read there).
        /// </summary>
        public static async Task<List<HelixReward>> ListAsync(string broadcasterId, bool onlyOwn, string token = null)
        {
            if (string.IsNullOrEmpty(broadcasterId))
            {
                Jotunn.Logger.LogWarning("[WBTI] Could not read the Twitch rewards: the channel id is not known yet.");
                return null;
            }

            string query = $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}" + (onlyOwn ? "&only_manageable_rewards=true" : "");
            TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(HttpMethod.Get, RewardsPath, query, tokenOverride: token);

            if (!response.IsSuccess)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the Twitch rewards: {response}");
                return null;
            }

            try
            {
                HelixReward[] rewards = JsonConvert.DeserializeObject<HelixRewardList>(response.Body)?.data;
                return rewards == null ? new List<HelixReward>() : new List<HelixReward>(rewards);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the Twitch rewards: {e.Message}");
                return null;
            }
        }

        private static async Task<TwitchHelixClient.Response> DeleteAsync(string broadcasterId, HelixReward reward, string token = null)
        {
            TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(
                HttpMethod.Delete, RewardsPath, $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}&id={Uri.EscapeDataString(reward.id)}", tokenOverride: token);

            if (!response.IsSuccess)
                Fail("delete", reward.title, response);

            return response;
        }

        // Copies of the wanted rewards cut down to what Twitch accepts, without the second of two with the
        // same title (Twitch titles are unique per channel).
        private static List<Desired> Normalize(IList<Desired> wanted, Result result)
        {
            List<Desired> normalized = new List<Desired>();
            HashSet<string> titles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Desired source in wanted)
            {
                string title = Truncate(source.Title, MaxTitleLength);

                if (string.IsNullOrWhiteSpace(title) || !titles.Add(title))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Skipped a redeem with an empty or duplicate Twitch title: '{source.Title}'.");
                    result.Skipped++;
                    continue;
                }

                if (source.Cost < 1)
                    Jotunn.Logger.LogWarning($"[WBTI] '{title}' costs {source.Cost} points; Twitch's minimum is 1, so it is sent as 1.");

                normalized.Add(new Desired
                {
                    Title = title,
                    Prompt = Truncate(source.Prompt, MaxPromptLength),
                    Cost = Math.Max(1, source.Cost),
                    BackgroundColor = source.BackgroundColor,
                    UserInput = source.UserInput,
                    MaxPerStream = Math.Max(0, source.MaxPerStream),
                    MaxPerUserPerStream = Math.Max(0, source.MaxPerUserPerStream),
                    CooldownSeconds = Math.Max(0, source.CooldownSeconds),
                });
            }

            return normalized;
        }

        // Titles on Twitch and in the wanted list are cut to MaxTitleLength, so the earlier titles (full titles)
        // must be cut the same way or a long title never matches.
        private static Dictionary<string, List<string>> TruncateFormerTitles(IDictionary<string, List<string>> formerTitles)
        {
            if (formerTitles == null)
                return null;

            Dictionary<string, List<string>> truncated = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<string, List<string>> entry in formerTitles)
                truncated[Truncate(entry.Key, MaxTitleLength)] = entry.Value.ConvertAll(title => Truncate(title, MaxTitleLength));

            return truncated;
        }

        // Pairs wanted rewards with existing ones: same title first, then (for a renamed reward) a title
        // it had before, the most recent first. What is left of the existing rewards comes out in leftovers.
        private static Dictionary<Desired, HelixReward> Match(List<Desired> desired, List<HelixReward> existing, Dictionary<string, List<string>> formerTitles, out List<HelixReward> leftovers)
        {
            leftovers = new List<HelixReward>(existing);
            Dictionary<Desired, HelixReward> matches = new Dictionary<Desired, HelixReward>();

            foreach (Desired d in desired)
            {
                HelixReward reward = TakeByTitle(leftovers, d.Title);

                if (reward != null)
                    matches[d] = reward;
            }

            if (formerTitles == null)
                return matches;

            foreach (Desired d in desired)
            {
                if (matches.ContainsKey(d) || !formerTitles.TryGetValue(d.Title, out List<string> earlier))
                    continue;

                foreach (string previousTitle in earlier)
                {
                    HelixReward reward = TakeByTitle(leftovers, previousTitle);

                    if (reward == null)
                        continue;

                    matches[d] = reward;
                    break;
                }
            }

            return matches;
        }

        private static HelixReward TakeByTitle(List<HelixReward> rewards, string title)
        {
            HelixReward found = rewards.Find(r => r.title == title)
                                ?? rewards.Find(r => string.Equals(r.title, title, StringComparison.OrdinalIgnoreCase));

            if (found != null)
                rewards.Remove(found);

            return found;
        }

        private static bool NeedsEdit(HelixReward reward, Desired d)
        {
            if (reward.title != d.Title
                || (reward.prompt ?? "") != (d.Prompt ?? "")
                || reward.cost != d.Cost
                || !reward.is_enabled
                || reward.is_user_input_required != d.UserInput)
                return true;

            if (HasValidColor(d) && !string.Equals(reward.background_color, d.BackgroundColor, StringComparison.OrdinalIgnoreCase))
                return true;

            return LimitDiffers(reward.max_per_stream_setting?.is_enabled ?? false, reward.max_per_stream_setting?.max_per_stream ?? 0, d.MaxPerStream)
                || LimitDiffers(reward.max_per_user_per_stream_setting?.is_enabled ?? false, reward.max_per_user_per_stream_setting?.max_per_user_per_stream ?? 0, d.MaxPerUserPerStream)
                || LimitDiffers(reward.global_cooldown_setting?.is_enabled ?? false, reward.global_cooldown_setting?.global_cooldown_seconds ?? 0, d.CooldownSeconds);
        }

        // A limit of 0 means switched off; its value on Twitch doesn't matter then.
        private static bool LimitDiffers(bool enabledOnTwitch, int valueOnTwitch, int wanted)
        {
            if (enabledOnTwitch != (wanted > 0))
                return true;

            return wanted > 0 && valueOnTwitch != wanted;
        }

        private static bool HasValidColor(Desired d)
        {
            return !string.IsNullOrEmpty(d.BackgroundColor) && m_hexColor.IsMatch(d.BackgroundColor);
        }

        // Twitch wants a value of at least 1 for a limit even when it is switched off.
        private static string BuildBody(Desired d, bool includeTitle)
        {
            // A field Twitch rejects when empty, like background_color, is left out of the object entirely.
            JObject body = new JObject();

            if (includeTitle)
                body["title"] = d.Title;

            body["cost"] = d.Cost;
            body["prompt"] = d.Prompt ?? "";
            body["is_enabled"] = true;
            body["is_user_input_required"] = d.UserInput;
            body["is_max_per_stream_enabled"] = d.MaxPerStream > 0;
            body["max_per_stream"] = Math.Max(1, d.MaxPerStream);
            body["is_max_per_user_per_stream_enabled"] = d.MaxPerUserPerStream > 0;
            body["max_per_user_per_stream"] = Math.Max(1, d.MaxPerUserPerStream);
            body["is_global_cooldown_enabled"] = d.CooldownSeconds > 0;
            body["global_cooldown_seconds"] = Math.Max(1, d.CooldownSeconds);

            if (HasValidColor(d))
                body["background_color"] = d.BackgroundColor;

            return body.ToString(Formatting.None);
        }

        private static string Truncate(string value, int maxLength)
        {
            value = value ?? "";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
