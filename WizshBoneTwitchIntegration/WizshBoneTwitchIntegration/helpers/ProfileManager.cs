using System.Collections.Generic;
using System.IO;
using System.Linq;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ProfileManager
    {
        private static readonly string ProfilesPath = WizshBoneTwitchIntegration.customConfigPath + "/profiles";
        private static readonly string ActiveProfileFile = ProfilesPath + "/active.txt";
        private const string DefaultProfileName = "default";
        private const string RedeemsSuffix = "_redeems.yaml";

        public static string ActiveProfile { get; private set; } = DefaultProfileName;

        public static string GetActiveRedeemPath()
        {
            return $"{ProfilesPath}/{ActiveProfile}/redeems.yaml";
        }

        public static string GetRedeemPath(string profileName)
        {
            return $"{ProfilesPath}/{profileName}/redeems.yaml";
        }

        public static List<string> GetProfiles()
        {
            if (!Directory.Exists(ProfilesPath))
                return new List<string>();

            return Directory.GetDirectories(ProfilesPath)
                .Select(d => Path.GetFileName(d))
                .ToList();
        }

        public static void Init()
        {
            Directory.CreateDirectory(ProfilesPath);

            ActiveProfile = File.Exists(ActiveProfileFile)
                ? File.ReadAllText(ActiveProfileFile).Trim()
                : DefaultProfileName;

            if (!Directory.Exists($"{ProfilesPath}/{ActiveProfile}"))
                CreateProfile(ActiveProfile, migrate: true);
        }

        public static bool CreateProfile(string name, bool migrate = false)
        {
            string profilePath = $"{ProfilesPath}/{name}";
            string redeemPath  = $"{profilePath}/redeems.yaml";

            if (Directory.Exists(profilePath))
                return false;

            Directory.CreateDirectory(profilePath);

            if (migrate && File.Exists(WizshBoneTwitchIntegration.redeemsConfigPath))
                File.Copy(WizshBoneTwitchIntegration.redeemsConfigPath, redeemPath);
            else
                ExtraConfigHelper.WriteDefaultRedeemsTo(redeemPath);

            return true;
        }

        /// <summary>
        /// Copies the active profile's redeems.yaml into a new profile with the given name.
        /// The .synced marker is intentionally not copied so the clone is locally owned.
        /// Fails if the name is invalid or a profile with that name already exists.
        /// </summary>
        public static bool CopyProfileTo(string newProfileName, out string error)
        {
            error = null;
            string sourcePath = GetActiveRedeemPath();

            if (!File.Exists(sourcePath))
            {
                error = "Copy failed: active profile file not found.";
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
            return true;
        }

        public static bool DeleteProfile(string name)
        {
            if (name == DefaultProfileName)
                return false;

            if (name == ActiveProfile)
                return false;

            string profilePath = $"{ProfilesPath}/{name}";

            if (!Directory.Exists(profilePath))
                return false;

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
        /// - If the filename matches "{name}_redeems.yaml" → "{name}".
        /// - Otherwise → the filename without its extension.
        /// </summary>
        public static string ResolveImportProfileName(string sourceFilePath)
        {
            string fileName = Path.GetFileName(sourceFilePath);

            return fileName.EndsWith(RedeemsSuffix, System.StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - RedeemsSuffix.Length)
                : Path.GetFileNameWithoutExtension(fileName);
        }

        public static bool ProfileExists(string profileName)
        {
            return Directory.Exists($"{ProfilesPath}/{profileName}");
        }

        /// <summary>
        /// Imports a yaml file into the given target profile name. If a profile with that
        /// name already exists it is updated (overwritten) - this is intentional, it's how
        /// a user re-imports into the same profile. Otherwise a new profile is created.
        /// </summary>
        public static bool ImportProfileTo(string sourceFilePath, string targetProfileName, out bool profileCreated, out string error)
        {
            profileCreated = false;
            error = null;

            if (!File.Exists(sourceFilePath))
            {
                error = "Import failed: selected file not found.";
                return false;
            }

            if (!IsValidProfileName(targetProfileName, out error))
                return false;

            if (IsSyncedProfile(targetProfileName))
            {
                error = $"Cannot import into '{targetProfileName}' - it is a synced (read-only) profile.";
                return false;
            }

            string profilePath = $"{ProfilesPath}/{targetProfileName}";
            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
                profileCreated = true;
            }

            File.Copy(sourceFilePath, GetRedeemPath(targetProfileName), overwrite: true);

            if (targetProfileName == ActiveProfile)
                RedeemHelper.Reload();

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

            Directory.Move(oldPath, $"{ProfilesPath}/{newName}");

            if (oldName == ActiveProfile)
            {
                ActiveProfile = newName;
                File.WriteAllText(ActiveProfileFile, newName);
                RedeemHelper.Reload();
            }

            return true;
        }

        /// <summary>
        /// Copies a single redeem (renamed to <paramref name="newTitle"/>) into another profile's
        /// redeems.yaml. Used for copying into a profile other than the active one - the active
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

            if (IsSyncedProfile(targetProfileName))
            {
                error = $"Cannot copy into '{targetProfileName}' - it is a synced (read-only) profile.";
                return false;
            }

            string targetPath = GetRedeemPath(targetProfileName);
            ModData data = ExtraConfigHelper.ReadRedeemsConfig(targetPath) ?? new ModData();
            data.redeems = data.redeems ?? new List<RedeemData>();

            if (data.redeems.Exists(r => r.title == newTitle))
            {
                error = $"A redeem named '{newTitle}' already exists in '{targetProfileName}'.";
                return false;
            }

            RedeemData copy = redeem.DeepClone<RedeemData>();
            copy.title = newTitle;
            data.redeems.Add(copy);

            ExtraConfigHelper.WriteRedeemsConfig(targetPath, data.creatureGroups, data.redeems);

            return true;
        }

        private static bool IsValidProfileName(string name, out string error)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Please enter a profile name.";
                return false;
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = "Profile name contains invalid characters.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Copies the active profile's redeems.yaml to the given destination path.
        /// </summary>
        public static bool ExportProfile(string destFilePath)
        {
            string sourcePath = GetActiveRedeemPath();

            if (!File.Exists(sourcePath))
                return false;

            File.Copy(sourcePath, destFilePath, overwrite: true);
            return true;
        }
    }
}