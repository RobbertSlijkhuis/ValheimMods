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
    /// be fulfilled or refunded afterwards. A redeem is linked to its reward by the redeem's permanent id, so a
    /// renamed redeem keeps its reward; rewards the sync has no id for yet are matched by title.
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
            /// <summary>The redeem's permanent id; with it the sync finds the redeem's reward again after a rename.</summary>
            public string RedeemId;

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

            /// <summary>Redeem id -> id of its reward on Twitch, for every wanted reward that is on Twitch after the sync.</summary>
            public readonly Dictionary<string, string> RewardIds = new Dictionary<string, string>();

            /// <summary>Set when a newer push or a clear took over before all changes were made.</summary>
            public bool Cancelled;

            /// <summary>True when the current rewards could not even be read.</summary>
            public bool LookupFailed;

            public override string ToString() =>
                $"{Created} created, {Edited} edited, {Deleted} deleted, {Unchanged} unchanged, {Failed} failed, {Skipped} skipped";
        }

        /// <param name="wanted">What should be on Twitch; the caller has checked it fits under the cap.</param>
        /// <param name="rewardIds">Redeem id -> id of the reward the last sync made for it. A reward found by its id is
        /// edited whatever its title is now, so a renamed redeem keeps its reward; anything else is paired by title.</param>
        /// <param name="knownRewards">Our rewards as just read from Twitch, to save reading them again; null reads them here.</param>
        public static async Task<Result> SyncAsync(string broadcasterId, IList<Desired> wanted, IDictionary<string, string> rewardIds, CancellationToken cancel, List<HelixReward> knownRewards = null)
        {
            Result result = new Result();
            List<HelixReward> existing = knownRewards ?? await ListAsync(broadcasterId, onlyOwn: true);

            if (existing == null)
            {
                result.LookupFailed = true;
                return result;
            }

            List<Desired> desired = Normalize(wanted, result);
            Dictionary<Desired, HelixReward> matches = Match(desired, existing, rewardIds, out List<HelixReward> leftovers);

            foreach (KeyValuePair<Desired, HelixReward> match in matches)
            {
                if (!string.IsNullOrEmpty(match.Key.RedeemId))
                    result.RewardIds[match.Key.RedeemId] = match.Value.id;
            }

            // The titles the rewards on Twitch hold right now. Twitch compares them ignoring capitals.
            Dictionary<string, HelixReward> held = new Dictionary<string, HelixReward>(StringComparer.OrdinalIgnoreCase);

            foreach (HelixReward reward in existing)
                held[reward.title ?? ""] = reward;

            // Deletes first so a title or slot they free up is available to the edits and creates.
            foreach (HelixReward reward in leftovers)
            {
                if (cancel.IsCancellationRequested)
                    return Cancelled(result);

                if ((await DeleteAsync(broadcasterId, reward)).IsSuccess)
                {
                    held.Remove(reward.title ?? "");
                    result.Deleted++;
                }
                else
                    result.Failed++;
            }

            List<KeyValuePair<Desired, HelixReward>> edits = new List<KeyValuePair<Desired, HelixReward>>();

            foreach (Desired d in desired)
            {
                if (!matches.TryGetValue(d, out HelixReward reward))
                    continue;

                if (NeedsEdit(reward, d))
                    edits.Add(new KeyValuePair<Desired, HelixReward>(d, reward));
                else
                    result.Unchanged++;
            }

            if (!await EditAllAsync(broadcasterId, edits, held, result, cancel))
                return Cancelled(result);

            foreach (Desired d in desired)
            {
                if (matches.ContainsKey(d))
                    continue;

                if (cancel.IsCancellationRequested)
                    return Cancelled(result);

                TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(
                    HttpMethod.Post, RewardsPath, $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}", BuildBody(d, includeTitle: true));

                if (response.IsSuccess)
                {
                    result.Created++;

                    string createdId = ReadCreatedId(response.Body);

                    if (createdId != null && !string.IsNullOrEmpty(d.RedeemId))
                        result.RewardIds[d.RedeemId] = createdId;
                }
                else
                    result.Failed += Fail("create", d.Title, response);
            }

            return result;
        }

        // The id Twitch gave the reward it just created, from the answer's data[0]. null when it can't be read:
        // the reward is then found by its title the next time.
        private static string ReadCreatedId(string body)
        {
            try
            {
                return JsonConvert.DeserializeObject<HelixRewardList>(body)?.data?[0]?.id;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the id of a created Twitch reward: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Makes the edits. Twitch titles are unique, so a reward can only be given a title that no other reward holds
        /// at that moment: the edits are ordered so each one frees its old title before another needs it (A to B
        /// together with B to C does the second first), and rewards that would have to swap titles are moved aside
        /// under a temporary title first. Returns false when cancelled. A cancel waits until no reward is left under
        /// a temporary title, so the channel never keeps one (the rest of the swap is only a request or two).
        /// </summary>
        private static async Task<bool> EditAllAsync(string broadcasterId, List<KeyValuePair<Desired, HelixReward>> edits,
            Dictionary<string, HelixReward> held, Result result, CancellationToken cancel)
        {
            HashSet<HelixReward> parked = new HashSet<HelixReward>();

            while (edits.Count > 0)
            {
                if (cancel.IsCancellationRequested && !edits.Exists(edit => parked.Contains(edit.Value)))
                    return false;

                int next = edits.FindIndex(edit => !HeldByOther(held, edit));
                bool moveAside = false;

                if (next < 0)
                {
                    // Every remaining edit needs a title that another reward holds. When the holders wait for each other
                    // in a circle (a swap), one of them goes aside.
                    next = edits.FindIndex(edit => !parked.Contains(edit.Value) && InCycle(held, edits, edit.Value));
                    moveAside = next >= 0;

                    // Otherwise the holder isn't going anywhere (a delete failed): let Twitch refuse the edit.
                    if (next < 0)
                        next = 0;
                }

                Desired d = edits[next].Key;
                HelixReward reward = edits[next].Value;

                if (moveAside)
                {
                    string temporary = "~wbti " + Guid.NewGuid().ToString("N").Substring(0, 8);
                    TwitchHelixClient.Response aside = await PatchAsync(broadcasterId, reward, new JObject { ["title"] = temporary }.ToString(Formatting.None));

                    if (aside.IsSuccess)
                    {
                        Retitle(held, reward, temporary);
                        parked.Add(reward);
                    }
                    else
                    {
                        result.Failed += Fail("edit", d.Title, aside);
                        edits.RemoveAt(next);
                    }

                    continue;
                }

                edits.RemoveAt(next);
                TwitchHelixClient.Response response = await PatchAsync(broadcasterId, reward, BuildBody(d, includeTitle: reward.title != d.Title));

                if (response.IsSuccess)
                {
                    Retitle(held, reward, d.Title);
                    result.Edited++;
                }
                else
                    result.Failed += Fail("edit", d.Title, response);
            }

            return true;
        }

        // Whether the reward is part of a circle of edits: the title it needs is held by a reward that is waiting for
        // an edit itself, whose wanted title is held by the next one, and so on until it comes back to this reward.
        // A chain that ends in a reward that isn't going anywhere is not a circle; moving a reward aside there
        // would not help and could leave it under the temporary title.
        private static bool InCycle(Dictionary<string, HelixReward> held, List<KeyValuePair<Desired, HelixReward>> edits, HelixReward start)
        {
            HelixReward current = start;

            for (int step = 0; step <= edits.Count; step++)
            {
                int index = edits.FindIndex(edit => edit.Value == current);

                if (index < 0 || !held.TryGetValue(edits[index].Key.Title, out HelixReward holder) || holder == current)
                    return false;

                if (holder == start)
                    return true;

                current = holder;
            }

            return false;
        }

        private static bool HeldByOther(Dictionary<string, HelixReward> held, KeyValuePair<Desired, HelixReward> edit)
        {
            return held.TryGetValue(edit.Key.Title, out HelixReward holder) && holder != edit.Value;
        }

        private static void Retitle(Dictionary<string, HelixReward> held, HelixReward reward, string title)
        {
            held.Remove(reward.title ?? "");
            reward.title = title;
            held[title] = reward;
        }

        private static Task<TwitchHelixClient.Response> PatchAsync(string broadcasterId, HelixReward reward, string body)
        {
            return TwitchHelixClient.SendAsync(
                TwitchHelixClient.Patch, RewardsPath, $"broadcaster_id={Uri.EscapeDataString(broadcasterId)}&id={Uri.EscapeDataString(reward.id)}", body);
        }

        /// <summary>
        /// Deletes every reward this app owns on the channel and says whether all of them are gone (false
        /// when the rewards couldn't be read or a delete was refused). Never throws; failures are logged.
        /// Stops at the first delete that gets no answer or is refused for the login (401): every one after it
        /// would fail the same way, so a dead connection ends the clear after one request instead of one per reward.
        /// Logging out wipes the stored token while this is still running, so it works with the
        /// <paramref name="token"/> taken before the logout. The ids of the rewards that are confirmed deleted are
        /// added to <paramref name="deletedIds"/> as they go, so a caller also knows what was removed when this
        /// returns false (or is cut short).
        /// </summary>
        public static async Task<bool> ClearAsync(string broadcasterId, string token, ICollection<string> deletedIds = null)
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
                {
                    deletedIds?.Add(reward.id);
                    continue;
                }

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
                    RedeemId = source.RedeemId,
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

        // Pairs wanted rewards with existing ones. A redeem goes to the reward the last sync made for it (found by its
        // id, whatever the title is now - this is what keeps a renamed redeem's reward). Whatever is still unpaired is
        // paired by title: rewards from before this session, or ones whose id is not known. What is left of the
        // existing rewards comes out in leftovers.
        private static Dictionary<Desired, HelixReward> Match(List<Desired> desired, List<HelixReward> existing, IDictionary<string, string> rewardIds, out List<HelixReward> leftovers)
        {
            leftovers = new List<HelixReward>(existing);
            Dictionary<Desired, HelixReward> matches = new Dictionary<Desired, HelixReward>();

            if (rewardIds != null)
            {
                foreach (Desired d in desired)
                {
                    if (string.IsNullOrEmpty(d.RedeemId) || !rewardIds.TryGetValue(d.RedeemId, out string rewardId))
                        continue;

                    HelixReward reward = leftovers.Find(r => r.id == rewardId);

                    if (reward == null)
                        continue;

                    leftovers.Remove(reward);
                    matches[d] = reward;
                }
            }

            foreach (Desired d in desired)
            {
                if (matches.ContainsKey(d))
                    continue;

                HelixReward reward = TakeByTitle(leftovers, d.Title);

                if (reward != null)
                    matches[d] = reward;
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
