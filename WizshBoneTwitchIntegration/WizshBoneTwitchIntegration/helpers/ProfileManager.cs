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

        public static string ActiveProfile { get; private set; } = DefaultProfileName;

        public static string GetActiveRedeemPath()
        {
            return $"{ProfilesPath}/{ActiveProfile}/redeems.yaml";
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
            string redeemPath = $"{profilePath}/redeems.yaml";

            if (Directory.Exists(profilePath))
                return false;

            Directory.CreateDirectory(profilePath);

            // On first-time migration copy the existing redeems.yaml, otherwise write the default template
            if (migrate && File.Exists(WizshBoneTwitchIntegration.redeemsConfigPath))
                File.Copy(WizshBoneTwitchIntegration.redeemsConfigPath, redeemPath);
            else
                ExtraConfigHelper.WriteDefaultRedeemsTo(redeemPath);

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
    }
}