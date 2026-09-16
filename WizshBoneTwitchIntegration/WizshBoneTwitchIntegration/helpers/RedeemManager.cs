using System;
using System.IO;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Add/update/delete/toggle/copy logic for redeems in the active profile, extracted from
    /// gui/tabs/RedeemsTab.cs so it can be called both by the GUI and by automated self-tests.
    /// Operates directly on <see cref="RedeemHelper.redeems"/> - the canonical in-memory list -
    /// and persists via <see cref="Save"/> on every call, same as the GUI always did.
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

        internal static string GetFullTitle(string rawTitle, string prefix)
        {
            rawTitle = rawTitle ?? "";

            if (string.IsNullOrEmpty(prefix))
                return rawTitle;

            string prefixWithSpace = prefix + " ";
            string suffix = rawTitle.StartsWith(prefixWithSpace) ? rawTitle.Substring(prefixWithSpace.Length) : rawTitle;

            return string.IsNullOrEmpty(suffix) ? prefix : prefixWithSpace + suffix;
        }

        public static bool AddRedeem(RedeemData redeem, out string error)
        {
            error = null;

            if (RedeemHelper.redeems.Exists(r => r.title == redeem.title))
            {
                error = $"A redeem named '{redeem.title}' already exists.";
                return false;
            }

            RedeemHelper.redeems.Add(redeem);
            return Save(out error);
        }

        /// <summary>
        /// Replaces <paramref name="original"/> with <paramref name="updated"/>. Rejects a title
        /// change that would collide with another existing redeem - the Add/Copy paths already
        /// guard against duplicate titles, this closes the same gap for editing.
        /// </summary>
        public static bool UpdateRedeem(RedeemData original, RedeemData updated, out string error)
        {
            error = null;

            int index = RedeemHelper.redeems.IndexOf(original);
            if (index < 0)
            {
                error = "Redeem not found.";
                return false;
            }

            bool titleChanged = updated.title != original.title;
            if (titleChanged && RedeemHelper.redeems.Exists(r => r != original && r.title == updated.title))
            {
                error = $"A redeem named '{updated.title}' already exists.";
                return false;
            }

            RedeemHelper.redeems[index] = updated;
            return Save(out error);
        }

        public static bool DeleteRedeem(RedeemData redeem, out string error)
        {
            RedeemHelper.redeems.Remove(redeem);
            return Save(out error);
        }

        public static bool SetEnabled(RedeemData redeem, bool enabled, out string error)
        {
            redeem.enabled = enabled;
            return Save(out error);
        }

        /// <summary>
        /// Copies <paramref name="redeem"/> under <paramref name="newTitle"/> into the active
        /// profile itself. For copying into a *different* profile, see
        /// <see cref="ProfileManager.CopyRedeemToOtherProfile"/>.
        /// </summary>
        public static bool CopyRedeemWithinActiveProfile(RedeemData redeem, string newTitle, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                error = "Please enter a name.";
                return false;
            }

            if (RedeemHelper.redeems.Exists(r => r.title == newTitle))
            {
                error = $"A redeem named '{newTitle}' already exists.";
                return false;
            }

            RedeemData copy = redeem.DeepClone<RedeemData>();
            copy.title = newTitle;

            RedeemHelper.redeems.Add(copy);
            return Save(out error);
        }

        /// <summary>
        /// Persists RedeemHelper.redeems (alongside whatever creatureGroups/settings are already
        /// on disk) to the active profile's profile.yaml, backing up first and restoring on
        /// failure, then reloads and pushes to Twitch if logged in. Extracted verbatim from
        /// RedeemsTab.Save().
        /// </summary>
        private static bool Save(out string error)
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
    }
}
