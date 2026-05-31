using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Copies the active profile into a new profile, auto-generating a unique name
        /// using the pattern "{activeProfile} - copy 1", " - copy 2", etc.
        /// The .synced marker is intentionally not copied so the clone is locally owned.
        /// </summary>
        /// <param name="newProfileName">The auto-generated name that was used.</param>
        /// <returns>False if the source yaml is missing.</returns>
        public static bool CopyProfile(out string newProfileName)
        {
            string sourcePath = GetActiveRedeemPath();
            newProfileName = string.Empty;

            if (!File.Exists(sourcePath))
                return false;

            // Strip any existing " - copy N" suffix so copies of copies stay clean
            string baseName = ActiveProfile;
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

            newProfileName = candidate;
            Directory.CreateDirectory($"{ProfilesPath}/{newProfileName}");
            File.Copy(sourcePath, GetRedeemPath(newProfileName));

            return true;
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
        /// Imports a yaml file into a profile resolved from the filename.
        /// - If the filename matches "{name}_redeems.yaml" and that profile exists → overwrites it.
        /// - If the filename matches "{name}_redeems.yaml" but the profile doesn't exist → creates it.
        /// - Otherwise → creates a new profile named after the filename (without extension).
        /// </summary>
        public static bool ImportProfile(string sourceFilePath, out string targetProfileName, out bool profileCreated)
        {
            targetProfileName = string.Empty;
            profileCreated    = false;

            if (!File.Exists(sourceFilePath))
                return false;

            string fileName = Path.GetFileName(sourceFilePath);

            if (fileName.EndsWith(RedeemsSuffix, System.StringComparison.OrdinalIgnoreCase))
                targetProfileName = fileName.Substring(0, fileName.Length - RedeemsSuffix.Length);
            else
                targetProfileName = Path.GetFileNameWithoutExtension(fileName);

            string profilePath = $"{ProfilesPath}/{targetProfileName}";
            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
                ExtraConfigHelper.WriteDefaultRedeemsTo(GetRedeemPath(targetProfileName));
                profileCreated = true;
            }

            File.Copy(sourceFilePath, GetRedeemPath(targetProfileName), overwrite: true);

            if (targetProfileName == ActiveProfile)
                RedeemHelper.Reload();

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