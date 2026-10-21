using System.Collections.Generic;
using System.IO;
using System.Linq;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ProfileManager
    {
        // Computed lazily so they pick up ConfigPathHelper's VirtualStore fallback - see
        // ConfigPathHelper.GetEffectiveRoot().
        private static string ProfilesPath => ConfigPathHelper.GetEffectiveRoot() + "/profiles";
        private static string ActiveProfileFile => ProfilesPath + "/active.txt";
        // Formerly "Default". A "Default" profile that already exists on disk is left alone (it is
        // just an ordinary profile now), so only a fresh install or a missing active.txt lands here.
        private const string NormalProfileName = "Normal";
        private const string ChattingOnlyProfileName = "Chatting only";
        private const string DeathWizshProfileName = "I got a DEATH WIZSH!";
        private const string RedeemsStem = "_redeems";

        /// <summary>
        /// Built-in profiles that always exist: they can be neither deleted nor renamed, and
        /// <see cref="Init"/> re-creates any that is missing from its stock content. <c>Locked</c>
        /// ones are also read-only (like a synced profile) and are restored from their stock
        /// content before every read (see <see cref="TryGetLockedSeedWriter"/>), so they can't drift
        /// from what the mod ships. None are locked today: they are ordinary editable profiles
        /// that can be put back to their stock content with <see cref="ResetProfile"/>.
        /// </summary>
        private static readonly (string Name, bool Locked, System.Action<string> WriteSeed)[] ProtectedProfiles =
        {
            (ChattingOnlyProfileName, false, ExtraConfigHelper.WriteChattingOnlyProfileTo),
            (NormalProfileName, false, ExtraConfigHelper.WriteDefaultRedeemsTo),
            // Disabled for this release: resources/profile_death_wizsh.yaml stays embedded, but the
            // profile is not claimed, created, restored or locked. Re-enable by uncommenting.
            // (DeathWizshProfileName, true, ExtraConfigHelper.WriteDeathWizshProfileTo),
        };

        /// <summary>Always exists; can't be deleted or renamed.</summary>
        public static bool IsProtectedProfile(string name)
        {
            foreach (var profile in ProtectedProfiles)
            {
                if (profile.Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>A built-in profile whose content is fixed (redeems, settings, everything).</summary>
        public static bool IsLockedProfile(string name)
        {
            foreach (var profile in ProtectedProfiles)
            {
                if (profile.Locked && profile.Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Content can't be edited: a synced profile (owned by whoever synced it) or a locked
        /// built-in one. Gates every edit, import and copy-into path.
        /// </summary>
        public static bool IsReadOnlyProfile(string name)
        {
            return IsLockedProfile(name) || IsSyncedProfile(name);
        }

        /// <summary>
        /// Puts a built-in profile back to its stock redeems and settings and clears its synced
        /// marker (so it is editable again). The caller reloads the data if it is the active one.
        /// </summary>
        public static bool ResetProfile(string name, out string error)
        {
            error = null;

            foreach (var profile in ProtectedProfiles)
            {
                if (profile.Name != name)
                    continue;

                string path = GetRedeemPath(name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                profile.WriteSeed(path);

                string syncedMarker = $"{ProfilesPath}/{name}/.synced";
                if (File.Exists(syncedMarker))
                    File.Delete(syncedMarker);

                return true;
            }

            error = $"'{name}' is not a built-in profile.";
            return false;
        }

        /// <summary>
        /// True when <paramref name="profileFilePath"/> is the profile.yaml of a locked profile,
        /// with the writer that restores it to its stock content.
        /// </summary>
        public static bool TryGetLockedSeedWriter(string profileFilePath, out System.Action<string> writeSeed)
        {
            writeSeed = null;

            if (string.IsNullOrEmpty(profileFilePath))
                return false;

            string fullPath = Path.GetFullPath(profileFilePath);

            foreach (var profile in ProtectedProfiles)
            {
                if (!profile.Locked)
                    continue;

                if (string.Equals(fullPath, Path.GetFullPath(GetRedeemPath(profile.Name)), System.StringComparison.OrdinalIgnoreCase))
                {
                    writeSeed = profile.WriteSeed;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Long profile names overflow the Profiles/Redeems tabs' fixed-width Name column and the
        /// sidebar's "Profile: {name}" header - capped here so every entry point (Create/Copy/
        /// Rename/Import) enforces it uniformly. GUI callers additionally cap the InputDialog's
        /// input field to this length (see ProfilesTab.cs) so typing past it is prevented rather
        /// than only rejected on Confirm.
        /// </summary>
        internal const int MaxProfileNameLength = 21;
        private static readonly System.Text.RegularExpressions.Regex TrailingDuplicateNumberRegex =
            new System.Text.RegularExpressions.Regex(@" ?\(\d+\)$");

        public static string ActiveProfile { get; private set; } = NormalProfileName;

        /// <summary>Why the active profile's redeems/settings were just replaced.</summary>
        public enum ActiveProfileChange { Sync, Switch, Import, Reload }

        /// <summary>
        /// Raised after the active profile's data was replaced wholesale (switched to another
        /// profile, imported over, reloaded from disk, or overwritten by a sync). Anything holding
        /// a working copy of the old data (e.g. an open redeem editor) must drop it.
        /// </summary>
        public static event System.Action<ActiveProfileChange> ActiveProfileDataReplaced;

        internal static void RaiseActiveProfileDataReplaced(ActiveProfileChange reason)
        {
            try
            {
                ActiveProfileDataReplaced?.Invoke(reason);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogWarning("[WBTI] ActiveProfileDataReplaced handler failed: " + e);
            }
        }

        public static string GetActiveRedeemPath()
        {
            return GetRedeemPath(ActiveProfile);
        }

        /// <summary>
        /// Resolves a profile's data file path - "profile.yaml" (redeems + creatureGroups +
        /// settings; the name "redeems.yaml" stopped fitting once settings moved in). Lazily
        /// migrates a profile still on the old "redeems.yaml" name the first time its path is
        /// resolved after upgrading, so every call site gets the rename for free without having
        /// to hunt down every entry point - one File.Move, then never touched again.
        /// </summary>
        public static string GetRedeemPath(string profileName)
        {
            string profilePath = $"{ProfilesPath}/{profileName}";
            string path = $"{profilePath}/profile.yaml";
            string legacyPath = $"{profilePath}/redeems.yaml";

            if (!File.Exists(path) && File.Exists(legacyPath))
            {
                Jotunn.Logger.LogWarning($"[WBTI] Migrating legacy redeems.yaml to profile.yaml for profile '{profileName}'.");
                File.Move(legacyPath, path);
            }

            return path;
        }

        public static string GetActiveSettingsPath()
        {
            return $"{ProfilesPath}/{ActiveProfile}/settings.yaml";
        }

        public static string GetSettingsPath(string profileName)
        {
            return $"{ProfilesPath}/{profileName}/settings.yaml";
        }

        /// <summary>
        /// Folder holding a profile's per-world leaderboard stats (see <see cref="LeaderboardHelper"/>).
        /// Deliberately outside profile.yaml, so Copy/Export/Import/Sync never carry it.
        /// </summary>
        internal static string GetLeaderboardsFolder(string profileName)
        {
            return $"{ProfilesPath}/{profileName}/leaderboards";
        }

        internal static string GetProfileFolder(string profileName)
        {
            return $"{ProfilesPath}/{profileName}";
        }

        public static List<string> GetProfiles()
        {
            if (!Directory.Exists(ProfilesPath))
                return new List<string>();

            return Directory.GetDirectories(ProfilesPath)
                .Select(d => Path.GetFileName(d))
                .ToList();
        }

        // Names of the built-in profiles this install has already created itself, one per line. A
        // folder with a built-in name that is NOT listed here was made by the user before that name
        // became a built-in (e.g. an older "Chatting only"), so it is renamed instead of overwritten.
        // Kept in the profiles folder (not the BepInEx config) because that folder is shared by every
        // mod-manager profile, just like active.txt.
        private static string ClaimedBuiltInsFile => ProfilesPath + "/builtin.txt";

        /// <summary>
        /// Makes sure every built-in profile exists. The first time a name is claimed, any existing
        /// user profile of that name is set aside under a new name; afterwards the folder is the
        /// built-in's and is only ever re-created when missing (locked ones are restored on read).
        /// A legacy "Default" profile is not a built-in and is left exactly as it is.
        /// </summary>
        private static void ClaimBuiltInProfiles()
        {
            HashSet<string> claimed = File.Exists(ClaimedBuiltInsFile)
                ? new HashSet<string>(File.ReadAllLines(ClaimedBuiltInsFile).Select(line => line.Trim()).Where(line => line.Length > 0))
                : new HashSet<string>();
            bool claimedChanged = false;

            foreach (var profile in ProtectedProfiles)
            {
                if (!claimed.Contains(profile.Name))
                {
                    if (ProfileExists(profile.Name))
                        SetAsideUserProfile(profile.Name);

                    claimed.Add(profile.Name);
                    claimedChanged = true;
                }

                if (ProfileExists(profile.Name))
                    continue;

                Directory.CreateDirectory($"{ProfilesPath}/{profile.Name}");
                profile.WriteSeed(GetRedeemPath(profile.Name));
            }

            if (claimedChanged)
                File.WriteAllLines(ClaimedBuiltInsFile, claimed);
        }

        /// <summary>
        /// Renames a user's own profile that sits on a built-in name to "&lt;name&gt; (old)" (or
        /// "(old 2)" ...), keeping everything in its folder, and follows it if it was the active one.
        /// </summary>
        private static void SetAsideUserProfile(string name)
        {
            string newName = $"{name} (old)";
            for (int n = 2; ProfileExists(newName); n++)
                newName = $"{name} (old {n})";

            try
            {
                Directory.Move($"{ProfilesPath}/{name}", $"{ProfilesPath}/{newName}");
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError($"[WBTI] Could not set aside your profile '{name}' - it will be replaced by the built-in profile of that name: {e}");
                return;
            }

            if (ActiveProfile == name)
            {
                ActiveProfile = newName;
                File.WriteAllText(ActiveProfileFile, newName);
            }

            Jotunn.Logger.LogWarning($"[WBTI] '{name}' is now a built-in profile. Your own profile of that name was renamed to '{newName}'.");
        }

        public static void Init()
        {
            Directory.CreateDirectory(ProfilesPath);

            // Read first: setting aside a user's old profile of a built-in name (below) must be able
            // to move the active profile along with it.
            ActiveProfile = File.Exists(ActiveProfileFile)
                ? File.ReadAllText(ActiveProfileFile).Trim()
                : NormalProfileName;

            ClaimBuiltInProfiles();

            // The other place a profile is seeded with the stock default redeems (CreateProfile
            // makes empty profiles): an active profile whose folder is gone.
            if (!Directory.Exists($"{ProfilesPath}/{ActiveProfile}") && IsValidProfileName(ActiveProfile, out _))
            {
                Directory.CreateDirectory($"{ProfilesPath}/{ActiveProfile}");
                ExtraConfigHelper.WriteDefaultRedeemsTo(GetRedeemPath(ActiveProfile));
            }

            ProfileMigrationHelper.MigrateAllProfilesOnDisk();
        }

        /// <summary>
        /// Creates a profile. With no <paramref name="sourceProfile"/> it is empty (no redeems or
        /// creature groups, default settings); otherwise it is a copy of that profile's
        /// profile.yaml (redeems, creature groups and settings), which needn't be the active
        /// profile. The .synced marker is not copied, so the new profile is locally owned.
        /// The stock default redeems are only seeded by <see cref="Init"/>.
        /// </summary>
        public static bool CreateProfile(string name, out string error, string sourceProfile = null)
        {
            error = null;

            if (!IsValidProfileName(name, out error))
                return false;

            string profilePath = $"{ProfilesPath}/{name}";

            if (Directory.Exists(profilePath))
            {
                error = $"A profile named '{name}' already exists.";
                return false;
            }

            bool copyFromSource = !string.IsNullOrEmpty(sourceProfile);
            string sourcePath = null;

            if (copyFromSource)
            {
                if (!ProfileExists(sourceProfile))
                {
                    error = $"Profile '{sourceProfile}' not found.";
                    return false;
                }

                sourcePath = GetRedeemPath(sourceProfile);

                if (!File.Exists(sourcePath))
                {
                    error = $"Profile '{sourceProfile}' has no profile file to copy.";
                    return false;
                }
            }

            Directory.CreateDirectory(profilePath);

            if (copyFromSource)
                File.Copy(sourcePath, GetRedeemPath(name));
            else
                ExtraConfigHelper.WriteRedeemsConfig(GetRedeemPath(name), null, null, new List<RedeemData>());

            // No settings block is written when empty - a profile.yaml with no "settings:" key is
            // treated as "use ProfileSettingsData defaults" by ProfileSettingsHelper.Reload().

            return true;
        }

        /// <summary>
        /// Copies a profile's profile.yaml (settings and all, since they now live in the same
        /// file) into a new profile with the given name. <paramref name="sourceProfile"/> defaults
        /// to the active profile. The .synced marker is intentionally not copied so the clone is
        /// locally owned. Fails if the name is invalid or a profile with that name already exists.
        /// </summary>
        public static bool CopyProfileTo(string newProfileName, out string error, string sourceProfile = null)
        {
            error = null;
            string sourcePath = GetRedeemPath(sourceProfile ?? ActiveProfile);

            if (!File.Exists(sourcePath))
            {
                error = "Copy failed: profile file not found.";
                return false;
            }

            if (!IsValidProfileName(newProfileName, out error))
                return false;

            if (ProfileExists(newProfileName))
            {
                error = $"A profile named '{newProfileName}' already exists.";
                return false;
            }

            Directory.CreateDirectory($"{ProfilesPath}/{newProfileName}");
            File.Copy(sourcePath, GetRedeemPath(newProfileName));

            return true;
        }

        /// <summary>
        /// Finds an unused profile name based on "{baseName} - copy N", stripping any
        /// existing " - copy N" suffix from baseName first so copies of copies stay clean.
        /// Used to suggest a starting name for the copy dialog.
        /// </summary>
        internal static string GetUniqueProfileName(string baseName)
        {
            int suffixIndex = baseName.LastIndexOf(" - copy ", System.StringComparison.OrdinalIgnoreCase);
            if (suffixIndex >= 0)
            {
                string afterSuffix = baseName.Substring(suffixIndex + " - copy ".Length);
                if (int.TryParse(afterSuffix, out _))
                    baseName = baseName.Substring(0, suffixIndex);
            }

            int counter = 1;
            string candidate;

            do
            {
                candidate = $"{baseName} - copy {counter}";
                counter++;
            }
            while (Directory.Exists($"{ProfilesPath}/{candidate}"));

            return candidate;
        }

        public static bool SelectProfile(string name)
        {
            if (!Directory.Exists($"{ProfilesPath}/{name}"))
                return false;

            ActiveProfile = name;
            File.WriteAllText(ActiveProfileFile, name);
            RedeemHelper.Reload();
            ProfileSettingsHelper.Reload();
            RaiseActiveProfileDataReplaced(ActiveProfileChange.Switch);
            return true;
        }

        public static bool DeleteProfile(string name)
        {
            if (IsProtectedProfile(name))
                return false;

            if (name == ActiveProfile)
                return false;

            string profilePath = $"{ProfilesPath}/{name}";

            if (!Directory.Exists(profilePath))
                return false;

            // A cached, still-unflushed leaderboard for this profile must not be written back into
            // the folder after it's gone.
            LeaderboardHelper.FlushAndForget();

            Directory.Delete(profilePath, recursive: true);
            return true;
        }

        public static bool IsSyncedProfile(string profileName)
        {
            return File.Exists($"{ProfilesPath}/{profileName}/.synced");
        }

        internal static void MarkAsSynced(string profileName)
        {
            File.WriteAllText($"{ProfilesPath}/{profileName}/.synced", "");
        }

        /// <summary>
        /// Resolves the profile name a given import file would target, without touching disk.
        /// Strips the file extension, then a trailing " (N)" - the suffix browsers/Discord add
        /// when a same-named file is downloaded more than once - and then a trailing "_redeems"
        /// stem, so re-downloaded copies of the same export still resolve to the original
        /// profile name instead of suggesting a new one each time. Exports no longer append
        /// "_redeems" (there's nothing left to disambiguate from now that settings live in the
        /// same file), but the strip stays so files exported before that change still resolve
        /// cleanly.
        /// </summary>
        public static string ResolveImportProfileName(string sourceFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(sourceFilePath);

            name = TrailingDuplicateNumberRegex.Replace(name, "");

            if (name.EndsWith(RedeemsStem, System.StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - RedeemsStem.Length);

            return name;
        }

        public static bool ProfileExists(string profileName)
        {
            return Directory.Exists($"{ProfilesPath}/{profileName}");
        }

        /// <summary>
        /// The checks <see cref="ImportProfileTo"/> would reject on. Callers that need to do
        /// something irreversible before importing (e.g. turning redeems off) run this first so
        /// a doomed import doesn't cost the user anything.
        /// </summary>
        public static bool ValidateImport(string sourceFilePath, string targetProfileName, out string error)
        {
            error = null;

            if (!File.Exists(sourceFilePath))
            {
                error = "Import failed: selected file not found.";
                return false;
            }

            if (!IsValidProfileName(targetProfileName, out error))
                return false;

            if (IsReadOnlyProfile(targetProfileName))
            {
                error = $"Cannot import into '{targetProfileName}' - it is a read-only profile.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Imports a yaml file into the given target profile name. If a profile with that
        /// name already exists it is updated (overwritten) - this is intentional, it's how
        /// a user re-imports into the same profile. Otherwise a new profile is created.
        /// </summary>
        public static bool ImportProfileTo(string sourceFilePath, string targetProfileName, out bool profileCreated, out string error)
        {
            profileCreated = false;

            if (!ValidateImport(sourceFilePath, targetProfileName, out error))
                return false;

            string profilePath = $"{ProfilesPath}/{targetProfileName}";
            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
                profileCreated = true;
            }

            File.Copy(sourceFilePath, GetRedeemPath(targetProfileName), overwrite: true);

            // Settings now live inside profile.yaml (see ModData.settings), so importing the
            // file carries them along automatically. A source file with no "settings:" key just
            // leaves the target with none, which ProfileSettingsHelper.Reload() already treats
            // as "use defaults" - no separate settings file needs writing here.

            if (targetProfileName == ActiveProfile)
            {
                RedeemHelper.Reload();
                ProfileSettingsHelper.Reload();
                RaiseActiveProfileDataReplaced(ActiveProfileChange.Import);
            }

            return true;
        }

        /// <summary>
        /// Renames a profile's on-disk folder. If it's the active profile, updates
        /// ActiveProfile/active.txt and reloads redeems. Caller must treat
        /// newName == oldName as a no-op before calling - here it would be misreported
        /// as a name collision since the folder already "has" that name.
        /// </summary>
        public static bool RenameProfile(string oldName, string newName, out string error)
        {
            error = null;

            if (IsProtectedProfile(oldName))
            {
                error = $"Cannot rename '{oldName}' - it is a built-in profile.";
                return false;
            }

            if (IsSyncedProfile(oldName))
            {
                error = $"Cannot rename '{oldName}' - it is a synced (read-only) profile.";
                return false;
            }

            if (!IsValidProfileName(newName, out error))
                return false;

            if (ProfileExists(newName))
            {
                error = $"A profile named '{newName}' already exists.";
                return false;
            }

            string oldPath = $"{ProfilesPath}/{oldName}";
            if (!Directory.Exists(oldPath))
            {
                error = $"Profile '{oldName}' not found.";
                return false;
            }

            // Same as DeleteProfile: flush + drop the leaderboard cache so a pending write can't
            // recreate the old folder after the move.
            LeaderboardHelper.FlushAndForget();

            Directory.Move(oldPath, $"{ProfilesPath}/{newName}");

            if (oldName == ActiveProfile)
            {
                ActiveProfile = newName;
                File.WriteAllText(ActiveProfileFile, newName);
                RedeemHelper.Reload();
                ProfileSettingsHelper.Reload();
            }

            return true;
        }

        /// <summary>
        /// Copies a single redeem (renamed to <paramref name="newTitle"/>) into another profile's
        /// profile.yaml. Used for copying into a profile other than the active one - the active
        /// profile's redeems live in RedeemsTab's in-memory working buffer instead, so callers
        /// should handle that case separately rather than going through this method.
        /// </summary>
        public static bool CopyRedeemToOtherProfile(RedeemData redeem, string targetProfileName, string newTitle, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                error = "Please enter a name.";
                return false;
            }

            if (!ProfileExists(targetProfileName))
            {
                error = $"Profile '{targetProfileName}' not found.";
                return false;
            }

            if (IsReadOnlyProfile(targetProfileName))
            {
                error = $"Cannot copy into '{targetProfileName}' - it is a read-only profile.";
                return false;
            }

            string targetPath = GetRedeemPath(targetProfileName);
            ModData data = ExtraConfigHelper.ReadRedeemsConfig(targetPath) ?? new ModData();
            data.redeems = data.redeems ?? new List<RedeemData>();

            if (data.redeems.Exists(r => RedeemManager.SameTitle(r.title, newTitle)))
            {
                error = $"A redeem named '{newTitle}' already exists in '{targetProfileName}'.";
                return false;
            }

            RedeemData copy = redeem.DeepClone<RedeemData>();
            copy.title = newTitle;
            data.redeems.Add(copy);

            ExtraConfigHelper.WriteRedeemsConfig(targetPath, data.settings, data.creatureGroups, data.redeems);

            return true;
        }

        /// <summary>
        /// internal (not private): <see cref="ProfileSyncHelper"/> also runs a name received from
        /// another player through this before using it in a path.
        /// </summary>
        internal static bool IsValidProfileName(string name, out string error)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Please enter a profile name.";
                return false;
            }

            if (name.Length > MaxProfileNameLength)
            {
                error = $"Profile name must be {MaxProfileNameLength} characters or fewer.";
                return false;
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = "Profile name contains invalid characters.";
                return false;
            }

            // Path separators are already rejected above, but "." and ".." (and, on Windows, a
            // trailing dot/space, which the OS silently drops) would still resolve to the profiles
            // folder itself or its parent instead of a profile folder of their own.
            if (name.Trim('.', ' ').Length == 0 || name != name.TrimEnd('.', ' '))
            {
                error = "Profile name cannot be only dots or end with a dot or space.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Copies a profile's profile.yaml to the given destination path (<paramref name="sourceProfile"/>
        /// defaults to the active profile). Settings travel along for free since they're embedded
        /// in the same file (see ModData.settings).
        /// </summary>
        public static bool ExportProfile(string destFilePath, string sourceProfile = null)
        {
            string sourcePath = GetRedeemPath(sourceProfile ?? ActiveProfile);

            if (!File.Exists(sourcePath))
                return false;

            File.Copy(sourcePath, destFilePath, overwrite: true);
            return true;
        }
    }
}