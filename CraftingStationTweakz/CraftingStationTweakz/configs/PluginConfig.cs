using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;

namespace CraftingStationTweakz.Configs
{
    internal static class PluginConfig
    {
        public static string sectionGeneral = "General";
        public static ConfigEntry<float> stationBuildRange;
        public static ConfigEntry<float> stationBuildRangePerUpgrade;
        public static ConfigEntry<float> stationPlayerBaseRange;
        public static ConfigEntry<float> extensionRange;
        public static ConfigEntry<float> extensionSpaceRange;

        // Other
        private static int entryCount = 1000;
        private static FileSystemWatcher configWatcher;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            try
            {
                CraftingStationTweakz.Instance.Config.SaveOnConfigSet = false;

                stationBuildRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Build range"), 20f,
                    new ConfigDescription("The base build range of the crafting station.", new AcceptableValueRange<float>(5f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                stationBuildRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.RequestUpdateCraftingStation();
                };

                stationBuildRangePerUpgrade = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Build range increase per upgrade"), 4f,
                    new ConfigDescription("Each upgrade increases the crafting station's build range.", new AcceptableValueRange<float>(4f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                stationBuildRangePerUpgrade.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.RequestUpdateCraftingStation();
                };

                extensionRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Upgrade range"), 10f,
                    new ConfigDescription("The maximum distance at which upgrades still connect to the crafting station.", new AcceptableValueRange<float>(5f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                extensionRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.RequestUpdateExtensionPieces();
                };

                // Enable SaveOnConfigSet before the last bind allowing the config file to be created on first run
                CraftingStationTweakz.Instance.Config.SaveOnConfigSet = true;

                extensionSpaceRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Space requirement between upgrades"), 0f,
                    new ConfigDescription("The minimum required spacing between upgrades in order to place them (game default is 2).", new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                extensionSpaceRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.RequestUpdateExtensionPieces();
                };

                configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, CraftingStationTweakz.configFileName);
                configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                configWatcher.IncludeSubdirectories = true;
                configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise general config: " + e);
            }
        }

        private static void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(CraftingStationTweakz.configFileFullPath))
                return;

            try
            {
                CraftingStationTweakz.Instance.Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
