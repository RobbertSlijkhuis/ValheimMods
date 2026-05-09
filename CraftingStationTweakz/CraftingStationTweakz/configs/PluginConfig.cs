using BepInEx.Configuration;
using System;

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

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            try
            {
                stationBuildRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Build range"), 20f,
                    new ConfigDescription("The base build range of the crafting station. (only updates on restart for already existing crafting stations)", new AcceptableValueRange<float>(5f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                stationBuildRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.UpdateCraftingStation();
                };

                stationBuildRangePerUpgrade = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Build range increase per upgrade"), 4f,
                    new ConfigDescription("Each upgrade increases the crafting station's build range. (only updates on restart for already existing crafting stations)", new AcceptableValueRange<float>(4f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                stationBuildRangePerUpgrade.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.UpdateCraftingStation();
                };

                extensionRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Upgrade range"), 10f,
                    new ConfigDescription("The maximum distance at which upgrades still connect to the crafting station. (only updates on restart for already existing crafting stations)", new AcceptableValueRange<float>(5f, 50f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                extensionRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.UpdateExtensionPieces();
                };

                extensionSpaceRange = CraftingStationTweakz.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Space requirement between upgrades"), 0f,
                    new ConfigDescription("The minimum required spacing between upgrades in order to place them (game default is 2). (only updates on restart for already existing crafting stations)", new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                extensionSpaceRange.SettingChanged += (obj, attr) =>
                {
                    CraftingStationTweakz.Instance.UpdateExtensionPieces();
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise general config: " + e);
            }
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
