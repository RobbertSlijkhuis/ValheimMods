using System;
using System.Collections.Generic;
using System.IO;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Add/update/delete/toggle/copy logic for redeems, extracted from gui/tabs/RedeemsTab.cs so
    /// it can be called both by the GUI and by automated self-tests.
    ///
    /// Every method takes an optional <c>profileName</c> (null = the active profile). For the
    /// active profile it operates directly on <see cref="RedeemHelper.redeems"/> - the canonical
    /// in-memory list - and persists via <see cref="SaveActive"/> on every call, same as the GUI
    /// always did. For any other profile it only edits that profile's profile.yaml on disk: the
    /// in-memory list, the Twitch rewards and the live components all belong to the active
    /// profile and are left alone (the change takes effect when that profile is switched to).
    /// Targets are matched by <see cref="RedeemData.title"/> there, since a list read from disk
    /// holds different objects than the file is re-read into.
    /// </summary>
    internal static class RedeemManager
    {
        /// <summary>
        /// The Twitch-facing title for <paramref name="redeem"/>: the active profile's
        /// <see cref="ProfileSettingsHelper.Current"/>.<c>redeemTitlePrefix</c> prepended to
        /// <see cref="RedeemData.title"/>. <see cref="RedeemData.title"/> itself is stored
        /// prefix-free (the GUI shows the prefix as a read-only badge next to the title field,
        /// it never gets baked into the saved title) - this is the one place that combines them,
        /// so every place that actually talks to Twitch (<see cref="TwitchIntegration.TwitchCustomRewards.SetRewards"/>,
        /// its redemption-matching lookups, the GUI's list/toasts) goes through it instead of
        /// reading <see cref="RedeemData.title"/> directly.
        ///
        /// Idempotent against a title that already happens to start with the current prefix (e.g.
        /// a redeem saved before this method existed, or one created via GUI_OLD, which always
        /// bakes the prefix into the stored title) - it's stripped first, then re-added exactly
        /// once, so calling this repeatedly or on already-prefixed data never doubles it up.
        /// </summary>
        public static string GetFullTitle(RedeemData redeem)
        {
            return GetFullTitle(redeem?.title, ProfileSettingsHelper.Current.redeemTitlePrefix);
        }

        /// <summary>
        /// Same as <see cref="GetFullTitle(RedeemData)"/> but with <paramref name="profileName"/>'s
        /// prefix (null = the active profile) - for redeems that belong to another profile.
        /// </summary>
        public static string GetFullTitle(RedeemData redeem, string profileName)
        {
            return GetFullTitle(redeem?.title, ProfileSettingsHelper.GetRedeemTitlePrefix(profileName));
        }

        internal static string GetFullTitle(string rawTitle, string prefix)
        {
            rawTitle = rawTitle ?? "";

            if (string.IsNullOrEmpty(prefix))
                return rawTitle;

            string prefixWithSpace = prefix + " ";
            string suffix = rawTitle.StartsWith(prefixWithSpace) ? rawTitle.Substring(prefixWithSpace.Length) : rawTitle;

            return string.IsNullOrEmpty(suffix) ? prefix : prefixWithSpace + suffix;
        }

        private static bool IsActive(string profileName)
        {
            return string.IsNullOrEmpty(profileName) || profileName == ProfileManager.ActiveProfile;
        }

        /// <summary>
        /// The redeems of <paramref name="profileName"/> (null = active). The active profile's is
        /// the live <see cref="RedeemHelper.redeems"/> list itself; another profile's is a fresh
        /// read from disk (empty if the profile doesn't exist - never creates one).
        /// </summary>
        public static List<RedeemData> GetRedeems(string profileName = null)
        {
            if (IsActive(profileName))
                return RedeemHelper.redeems;

            if (!ProfileManager.ProfileExists(profileName) || !File.Exists(ProfileManager.GetRedeemPath(profileName)))
                return new List<RedeemData>();

            return ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(profileName))?.redeems
                   ?? new List<RedeemData>();
        }

        public static bool AddRedeem(RedeemData redeem, out string error, string profileName = null)
        {
            error = null;

            if (!IsActive(profileName))
            {
                return EditOnDisk(profileName, redeems =>
                {
                    if (redeems.Exists(r => r.title == redeem.title))
                        return $"A redeem named '{redeem.title}' already exists.";

                    redeems.Add(redeem);
                    return null;
                }, out error);
            }

            if (RedeemHelper.redeems.Exists(r => r.title == redeem.title))
            {
                error = $"A redeem named '{redeem.title}' already exists.";
                return false;
            }

            RedeemHelper.redeems.Add(redeem);
            return SaveActive(out error);
        }

        /// <summary>
        /// Replaces <paramref name="original"/> with <paramref name="updated"/>. Rejects a title
        /// change that would collide with another existing redeem - the Add/Copy paths already
        /// guard against duplicate titles, this closes the same gap for editing.
        /// </summary>
        public static bool UpdateRedeem(RedeemData original, RedeemData updated, out string error, string profileName = null)
        {
            error = null;
            bool titleChanged = updated.title != original.title;

            if (!IsActive(profileName))
            {
                return EditOnDisk(profileName, redeems =>
                {
                    int diskIndex = redeems.FindIndex(r => r.title == original.title);
                    if (diskIndex < 0)
                        return "Redeem not found.";

                    if (titleChanged && redeems.Exists(r => r.title == updated.title))
                        return $"A redeem named '{updated.title}' already exists.";

                    redeems[diskIndex] = updated;
                    return null;
                }, out error);
            }

            int index = RedeemHelper.redeems.IndexOf(original);
            if (index < 0)
            {
                error = "Redeem not found.";
                return false;
            }

            if (titleChanged && RedeemHelper.redeems.Exists(r => r != original && r.title == updated.title))
            {
                error = $"A redeem named '{updated.title}' already exists.";
                return false;
            }

            RedeemHelper.redeems[index] = updated;
            return SaveActive(out error);
        }

        public static bool DeleteRedeem(RedeemData redeem, out string error, string profileName = null)
        {
            if (!IsActive(profileName))
            {
                return EditOnDisk(profileName, redeems =>
                    redeems.RemoveAll(r => r.title == redeem.title) > 0 ? null : "Redeem not found.", out error);
            }

            RedeemHelper.redeems.Remove(redeem);
            return SaveActive(out error);
        }

        public static bool SetEnabled(RedeemData redeem, bool enabled, out string error, string profileName = null)
        {
            if (!IsActive(profileName))
            {
                bool saved = EditOnDisk(profileName, redeems =>
                {
                    RedeemData target = redeems.Find(r => r.title == redeem.title);
                    if (target == null)
                        return "Redeem not found.";

                    target.enabled = enabled;
                    return null;
                }, out error);

                // The caller's copy (from a disk read) is what its list shows - keep it in step.
                if (saved)
                    redeem.enabled = enabled;

                return saved;
            }

            redeem.enabled = enabled;
            return SaveActive(out error);
        }

        /// <summary>
        /// Copies <paramref name="redeem"/> under <paramref name="newTitle"/> into
        /// <paramref name="profileName"/> itself (null = the active profile). For copying into a
        /// *different* profile than the one the redeem is in, see
        /// <see cref="ProfileManager.CopyRedeemToOtherProfile"/>.
        /// </summary>
        public static bool CopyRedeemWithinProfile(RedeemData redeem, string newTitle, out string error, string profileName = null)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                error = "Please enter a name.";
                return false;
            }

            if (!IsActive(profileName))
            {
                return EditOnDisk(profileName, redeems =>
                {
                    if (redeems.Exists(r => r.title == newTitle))
                        return $"A redeem named '{newTitle}' already exists.";

                    RedeemData diskCopy = redeem.DeepClone<RedeemData>();
                    diskCopy.title = newTitle;
                    redeems.Add(diskCopy);
                    return null;
                }, out error);
            }

            if (RedeemHelper.redeems.Exists(r => r.title == newTitle))
            {
                error = $"A redeem named '{newTitle}' already exists.";
                return false;
            }

            RedeemData copy = redeem.DeepClone<RedeemData>();
            copy.title = newTitle;

            RedeemHelper.redeems.Add(copy);
            return SaveActive(out error);
        }

        /// <summary>
        /// Persists RedeemHelper.redeems (alongside whatever creatureGroups/settings are already
        /// on disk) to the active profile's profile.yaml, backing up first and restoring on
        /// failure, then reloads and pushes to Twitch if logged in. Extracted verbatim from
        /// RedeemsTab.Save().
        /// </summary>
        private static bool SaveActive(out string error)
        {
            error = null;
            string path = ProfileManager.GetActiveRedeemPath();
            string backupPath = path + ".bak";

            try
            {
                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();

                ExtraConfigHelper.WriteRedeemsConfig(path, data.settings, data.creatureGroups, RedeemHelper.redeems);

                RedeemHelper.Reload();

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                if (customRewards.IsLoggedIn && customRewards.m_enabled)
                    customRewards.SetRewards();

                return true;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Failed to save redeems, restoring backup: {ex}");

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    RedeemHelper.Reload();
                }

                error = "Save failed! Restored previous redeems file.";
                return false;
            }
        }

        /// <summary>
        /// Read-modify-write of a non-active profile's profile.yaml: reads its redeems, lets
        /// <paramref name="mutate"/> change them (returns an error message to reject, or null),
        /// then backs up and writes - restoring the backup if the write fails. Doesn't touch
        /// <see cref="RedeemHelper.redeems"/>, Twitch or any live component.
        /// </summary>
        private static bool EditOnDisk(string profileName, Func<List<RedeemData>, string> mutate, out string error)
        {
            error = null;

            if (!ProfileManager.ProfileExists(profileName))
            {
                error = $"Profile '{profileName}' not found.";
                return false;
            }

            string path = ProfileManager.GetRedeemPath(profileName);
            string backupPath = path + ".bak";
            bool backedUp = false;

            try
            {
                ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();
                data.redeems = data.redeems ?? new List<RedeemData>();

                error = mutate(data.redeems);
                if (error != null)
                    return false;

                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, overwrite: true);
                    backedUp = true;
                }

                ExtraConfigHelper.WriteRedeemsConfig(path, data.settings, data.creatureGroups, data.redeems);
                return true;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Failed to save redeems of profile '{profileName}', restoring backup: {ex}");

                if (backedUp && File.Exists(backupPath))
                    File.Copy(backupPath, path, overwrite: true);

                error = "Save failed! Restored previous redeems file.";
                return false;
            }
        }
    }
}
