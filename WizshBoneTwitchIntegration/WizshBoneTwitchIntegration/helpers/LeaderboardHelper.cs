using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Per-viewer redeem stats (points spent, redeem count, per-redeem counts), kept per profile
    /// and per world in profiles/&lt;profile&gt;/leaderboards/&lt;worldname&gt;_&lt;uid&gt;.yaml - a sidecar
    /// of profile.yaml, so Copy/Export/Import/Sync never carry it, and Rename/Delete of the
    /// profile folder move/remove it for free.
    ///
    /// Viewers are keyed by their Twitch user ID (stable across renames); the display name is just
    /// a label that is refreshed on every redeem, with older names kept in previousNames.
    /// Everything runs on the main thread. Writes are debounced (<see cref="Tick"/>, driven by
    /// <see cref="Components.LeaderboardFlusher"/>) and flushed on world exit / quit.
    /// </summary>
    internal static class LeaderboardHelper
    {
        private const int FormatVersion = 1;
        private const int MaxPreviousNames = 10;
        private const float FlushDelaySeconds = 10f;
        private const string SeedIdPrefix = "seed-";

        private static readonly ISerializer s_serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithQuotingNecessaryStrings() // Twitch IDs are numeric-looking strings - keep them unambiguous
            .Build();

        private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // The (profile, world) currently held in memory. Re-resolved on every record/open rather
        // than once at Awake, so ordering against ZNet setup can't matter.
        private static string s_profile;
        private static long s_worldUid;
        private static string s_filePath;
        private static LeaderboardFile s_data;
        private static bool s_dirty;
        private static float s_dirtySince;

        // ── current world ────────────────────────────────────────────────────

        public static bool TryGetCurrentWorld(out long uid, out string name)
        {
            World world = ZNet.World;
            if (world == null)
            {
                uid = 0;
                name = null;
                return false;
            }

            uid = world.m_uid;
            name = world.m_name ?? "";
            return true;
        }

        /// <summary>
        /// Makes sure the in-memory data matches the active profile + current world, flushing and
        /// reloading if either changed since the last call. False when there is no world.
        /// </summary>
        private static bool EnsureLoaded()
        {
            if (!TryGetCurrentWorld(out long uid, out string name))
                return false;

            string profile = ProfileManager.ActiveProfile;

            if (s_data != null && s_profile == profile && s_worldUid == uid)
            {
                s_data.worldName = name;
                return true;
            }

            Flush();

            string folder = ProfileManager.GetLeaderboardsFolder(profile);

            s_profile = profile;
            s_worldUid = uid;
            // Reuse the existing file for this uid even if the world was renamed since, so a
            // rename doesn't leave a second file behind.
            s_filePath = FindFileForUid(folder, uid) ?? Path.Combine(folder, $"{SanitizeFileName(name)}_{uid}.yaml");
            s_data = ReadFile(s_filePath) ?? new LeaderboardFile();
            s_data.worldName = name;
            s_dirty = false;
            return true;
        }

        // ── recording ────────────────────────────────────────────────────────

        /// <summary>
        /// Counts a redeem that was actually carried out. Skips events without a RedeemerId (test
        /// events from the console command / Redeems tab) and the streamer's own redeems
        /// (RedeemerId == BroadcasterId, which also covers the streamer using the alias).
        /// </summary>
        public static void RecordRedeem(CustomRewardEvent customRewardEvent, RedeemData redeem)
        {
            try
            {
                if (customRewardEvent == null || redeem == null)
                    return;

                if (string.IsNullOrEmpty(customRewardEvent.RedeemerId))
                    return;

                if (customRewardEvent.RedeemerId == customRewardEvent.BroadcasterId)
                    return;

                if (!EnsureLoaded())
                    return;

                Apply(customRewardEvent.RedeemerId, customRewardEvent.RedeemerName, customRewardEvent.CustomRewardCost, redeem.title);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Leaderboard: could not record redeem: {e}");
            }
        }

        private static void Apply(string id, string displayName, long cost, string redeemTitle)
        {
            if (!s_data.viewers.TryGetValue(id, out LeaderboardViewer viewer))
            {
                viewer = new LeaderboardViewer();
                s_data.viewers[id] = viewer;
            }

            UpdateName(viewer, displayName);

            viewer.pointsSpent += cost;
            viewer.redeemCount++;

            if (!string.IsNullOrEmpty(redeemTitle))
            {
                viewer.redeems.TryGetValue(redeemTitle, out int count);
                viewer.redeems[redeemTitle] = count + 1;
            }

            MarkDirty();
        }

        private static void UpdateName(LeaderboardViewer viewer, string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return;

            if (string.IsNullOrEmpty(viewer.name))
            {
                viewer.name = displayName;
                return;
            }

            // A case-only change is the same name, not a rename - just take the new casing.
            if (!string.Equals(viewer.name, displayName, StringComparison.OrdinalIgnoreCase))
            {
                viewer.previousNames.RemoveAll(n => string.Equals(n, viewer.name, StringComparison.OrdinalIgnoreCase));
                viewer.previousNames.Insert(0, viewer.name);
                viewer.previousNames.RemoveAll(n => string.Equals(n, displayName, StringComparison.OrdinalIgnoreCase));

                if (viewer.previousNames.Count > MaxPreviousNames)
                    viewer.previousNames.RemoveRange(MaxPreviousNames, viewer.previousNames.Count - MaxPreviousNames);
            }

            viewer.name = displayName;
        }

        // ── persistence ──────────────────────────────────────────────────────

        private static void MarkDirty()
        {
            if (s_dirty)
                return;

            s_dirty = true;
            s_dirtySince = Time.unscaledTime;
        }

        /// <summary>Called every frame by <see cref="Components.LeaderboardFlusher"/>.</summary>
        public static void Tick()
        {
            if (s_dirty && Time.unscaledTime - s_dirtySince >= FlushDelaySeconds)
                Flush();
        }

        public static void Flush()
        {
            if (!s_dirty || s_data == null)
                return;

            try
            {
                // Never recreate a profile folder that has since been deleted/renamed away -
                // that would show up as a ghost profile.
                if (!Directory.Exists(ProfileManager.GetProfileFolder(s_profile)))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Leaderboard: profile folder for '{s_profile}' is gone, dropping unsaved stats.");
                    s_dirty = false;
                    return;
                }

                WriteFile(s_filePath, s_data);
                s_dirty = false;
            }
            catch (Exception e)
            {
                // Stays dirty, so the next tick retries.
                Jotunn.Logger.LogWarning($"[WBTI] Leaderboard: could not save '{s_filePath}': {e}");
                s_dirtySince = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Flushes, then drops the cache. Called before a profile is renamed or deleted, so a
        /// pending write can't land in the old (now moved/removed) folder.
        /// </summary>
        public static void FlushAndForget()
        {
            Flush();
            s_data = null;
            s_profile = null;
            s_filePath = null;
            s_dirty = false;
        }

        private static void WriteFile(string path, LeaderboardFile data)
        {
            data.version = FormatVersion;

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Temp file + replace, so a crash mid-write can't leave a truncated stats file.
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, s_serializer.Serialize(data));

            if (File.Exists(path))
                File.Replace(tempPath, path, null);
            else
                File.Move(tempPath, path);
        }

        private static LeaderboardFile ReadFile(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                string text = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                LeaderboardFile file = s_deserializer.Deserialize<LeaderboardFile>(text);
                return Normalize(file);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Leaderboard: could not read '{path}', keeping a .bak copy: {e.Message}");

                try { File.Copy(path, path + ".bak", true); }
                catch { /* best effort */ }

                return null;
            }
        }

        // A hand-edited file can carry explicit nulls, which skip the field initializers.
        private static LeaderboardFile Normalize(LeaderboardFile file)
        {
            if (file == null)
                return null;

            file.worldName = file.worldName ?? "";
            file.viewers = file.viewers ?? new Dictionary<string, LeaderboardViewer>();

            foreach (string id in file.viewers.Keys.ToList())
            {
                LeaderboardViewer viewer = file.viewers[id] ?? new LeaderboardViewer();
                viewer.name = viewer.name ?? "";
                viewer.previousNames = viewer.previousNames ?? new List<string>();
                viewer.redeems = viewer.redeems ?? new Dictionary<string, int>();
                file.viewers[id] = viewer;
            }

            return file;
        }

        // ── file naming ──────────────────────────────────────────────────────

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "World";

            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
            return string.IsNullOrEmpty(cleaned) ? "World" : cleaned;
        }

        private static bool TryParseUid(string filePath, out long uid)
        {
            string stem = Path.GetFileNameWithoutExtension(filePath);
            int separator = stem.LastIndexOf('_');
            uid = 0;
            return separator >= 0 && long.TryParse(stem.Substring(separator + 1), out uid);
        }

        private static string FindFileForUid(string folder, long uid)
        {
            if (!Directory.Exists(folder))
                return null;

            foreach (string file in Directory.GetFiles(folder, "*.yaml"))
            {
                if (TryParseUid(file, out long fileUid) && fileUid == uid)
                    return file;
            }

            return null;
        }

        // ── reading for the Leaderboards window ──────────────────────────────

        /// <summary>
        /// Every world with stats under the active profile, oldest-written first, current world
        /// last (always present while in a world, even before its first redeem). The current
        /// world's data is the live in-memory copy, never its (possibly stale) file.
        /// </summary>
        public static List<LeaderboardWorld> LoadWorlds()
        {
            string folder = ProfileManager.GetLeaderboardsFolder(ProfileManager.ActiveProfile);
            bool hasCurrent = EnsureLoaded();
            long currentUid = hasCurrent ? s_worldUid : 0;

            List<(LeaderboardWorld World, DateTime Written)> others = new List<(LeaderboardWorld, DateTime)>();

            if (Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*.yaml"))
                {
                    if (!TryParseUid(file, out long uid))
                        continue;

                    if (hasCurrent && uid == currentUid)
                        continue;

                    LeaderboardFile data = ReadFile(file);
                    if (data == null)
                        continue;

                    string stem = Path.GetFileNameWithoutExtension(file);
                    string name = string.IsNullOrEmpty(data.worldName)
                        ? stem.Substring(0, stem.LastIndexOf('_'))
                        : data.worldName;

                    others.Add((new LeaderboardWorld { uid = uid, name = name, file = data }, File.GetLastWriteTimeUtc(file)));
                }
            }

            List<LeaderboardWorld> worlds = others.OrderBy(o => o.Written).Select(o => o.World).ToList();

            if (hasCurrent)
                worlds.Add(new LeaderboardWorld { uid = currentUid, name = s_data.worldName, isCurrent = true, file = s_data });

            return worlds;
        }

        /// <summary>
        /// Sums the given worlds into one row per viewer: points and redeem counts are added, the
        /// per-redeem counts are summed per title, the name is the one from the newest world.
        /// The favourite is the highest-count redeem; on a tie the first one in the (insertion-
        /// ordered) map wins.
        /// </summary>
        public static List<LeaderboardRow> BuildRows(IEnumerable<LeaderboardWorld> worlds)
        {
            List<string> order = new List<string>();
            Dictionary<string, Accumulator> accumulators = new Dictionary<string, Accumulator>();

            foreach (LeaderboardWorld world in worlds)
            {
                foreach (KeyValuePair<string, LeaderboardViewer> entry in world.file.viewers)
                {
                    if (!accumulators.TryGetValue(entry.Key, out Accumulator acc))
                    {
                        acc = new Accumulator();
                        accumulators[entry.Key] = acc;
                        order.Add(entry.Key);
                    }

                    LeaderboardViewer viewer = entry.Value;

                    // previousNames is newest-first; names are collected oldest-first.
                    for (int i = viewer.previousNames.Count - 1; i >= 0; i--)
                        acc.AddName(viewer.previousNames[i]);
                    acc.AddName(viewer.name);

                    acc.points += viewer.pointsSpent;
                    acc.redeemCount += viewer.redeemCount;

                    foreach (KeyValuePair<string, int> redeem in viewer.redeems)
                    {
                        acc.redeems.TryGetValue(redeem.Key, out int count);
                        acc.redeems[redeem.Key] = count + redeem.Value;
                    }
                }
            }

            List<LeaderboardRow> rows = new List<LeaderboardRow>(order.Count);

            foreach (string id in order)
            {
                Accumulator acc = accumulators[id];
                LeaderboardRow row = new LeaderboardRow
                {
                    id = id,
                    name = acc.names.Count > 0 ? acc.names[acc.names.Count - 1] : id,
                    points = acc.points,
                    redeemCount = acc.redeemCount
                };

                for (int i = acc.names.Count - 2; i >= 0 && row.previousNames.Count < MaxPreviousNames; i--)
                {
                    string candidate = acc.names[i];
                    if (!string.Equals(candidate, row.name, StringComparison.OrdinalIgnoreCase)
                        && !row.previousNames.Exists(n => string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase)))
                        row.previousNames.Add(candidate);
                }

                foreach (KeyValuePair<string, int> redeem in acc.redeems)
                {
                    if (redeem.Value > row.favouriteCount)
                    {
                        row.favouriteTitle = redeem.Key;
                        row.favouriteCount = redeem.Value;
                    }
                }

                rows.Add(row);
            }

            return rows;
        }

        private class Accumulator
        {
            public readonly List<string> names = new List<string>();
            public long points;
            public int redeemCount;
            public readonly Dictionary<string, int> redeems = new Dictionary<string, int>();

            public void AddName(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return;

                // Same name again (possibly new casing): keep one entry, with the newest casing.
                if (names.Count > 0 && string.Equals(names[names.Count - 1], name, StringComparison.OrdinalIgnoreCase))
                    names[names.Count - 1] = name;
                else
                    names.Add(name);
            }
        }

        // ── debug (console commands) ─────────────────────────────────────────

        /// <summary>
        /// Replaces any earlier seeded viewers in the current world with <paramref name="count"/>
        /// fake ones: random spend, two forced ties and one renamed viewer. Returns the number
        /// seeded, or -1 when there is no world.
        /// </summary>
        public static int Seed(int count)
        {
            if (!EnsureLoaded())
                return -1;

            ClearSeeded(all: false);

            List<string> titles = (RedeemHelper.redeems ?? new List<RedeemData>())
                .Select(r => r.title)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            if (titles.Count == 0)
                titles.AddRange(new[] { "Meteor", "Weather", "Surprise chest" });

            System.Random random = new System.Random(Environment.TickCount);

            for (int i = 1; i <= count; i++)
            {
                string id = SeedIdPrefix + i.ToString("000");
                string name = "SeedViewer" + i;

                if (i <= 2)
                {
                    // Identical totals, to exercise tie numbering (1, 1, 3, ...).
                    for (int n = 0; n < 3; n++)
                        Apply(id, name, 500, titles[0]);

                    continue;
                }

                if (i == 3)
                    Apply(id, "OldSeedName" + i, 100, titles[random.Next(titles.Count)]); // shows up in previousNames

                int redeems = random.Next(1, 12);
                for (int n = 0; n < redeems; n++)
                    Apply(id, name, random.Next(1, 20) * 100, titles[random.Next(titles.Count)]);
            }

            return count;
        }

        /// <summary>
        /// Removes the seeded viewers (or, with <paramref name="all"/>, every viewer) from the
        /// current world. Returns how many were removed, or -1 when there is no world.
        /// </summary>
        public static int ClearSeeded(bool all)
        {
            if (!EnsureLoaded())
                return -1;

            List<string> ids = s_data.viewers.Keys
                .Where(id => all || id.StartsWith(SeedIdPrefix, StringComparison.Ordinal))
                .ToList();

            foreach (string id in ids)
                s_data.viewers.Remove(id);

            if (ids.Count > 0)
                MarkDirty();

            return ids.Count;
        }
    }
}
