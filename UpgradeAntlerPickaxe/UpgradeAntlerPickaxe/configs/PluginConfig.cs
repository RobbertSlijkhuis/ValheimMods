using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using UpgradeAntlerPickaxe.Types;

namespace UpgradeAntlerPickaxe.Configs
{
    internal static class PluginConfig
    {
        public static string sectionGeneral = "General";

        public static string[] craftingStationOptions = new string[] {
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter,
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

        public static ConfigEntry<bool> configEnable;
        public static ConfigEntry<string> configCraftingStation;
        public static ConfigEntry<int> configMinStationLevel;
        public static ConfigEntry<string> configRecipe;
        public static ConfigEntry<string> configRecipeUpgrade;
        public static ConfigEntry<int> configRecipeMultiplier;

        // Other
        private static int entryCount = 1000;
        private static FileSystemWatcher configWatcher;

        public static void Init()
        {
            InitGeneralConfig();
        }

        /**
         * Initialise config entries and add the necessary events
         */
        public static void InitGeneralConfig()
        {
            try
            {
                UpgradeAntlerPickaxe.Instance.Config.SaveOnConfigSet = false;

                configEnable = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Wether or not to enable this mod. When changed while the game is running it will disable the ability to upgrade\nthe pickaxe but will not modify already upgraded ones. They will be reverted back to normal on next game start.", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configEnable.SettingChanged += (obj, attr) =>
                {
                    if (!configEnable.Value)
                    {
                        UpgradeAntlerPickaxe.Instance.UnpatchStats();
                        Jotunn.Logger.LogWarning("UpgradeAntlerPickaxe is now disabled! The Antler Pickaxe can no longer be upgraded");
                    }
                    else
                        UpgradeAntlerPickaxe.Instance.PatchStats();
                };

                configCraftingStation = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Crafting station"), "Workbench",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCraftingStation.SettingChanged += (obj, attr) => { UpgradeAntlerPickaxe.Instance.PatchRecipe(RecipeUpdateType.CraftingStation); };

                configMinStationLevel = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configMinStationLevel.SettingChanged += (obj, attr) => { UpgradeAntlerPickaxe.Instance.PatchRecipe(RecipeUpdateType.MinRequiredLevel); };

                configRecipe = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Crafting costs"), "Wood:10,HardAntler:1",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configRecipe.SettingChanged += (obj, attr) => { UpgradeAntlerPickaxe.Instance.PatchRecipe(); };

                configRecipeUpgrade = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Upgrade costs"), "Wood:4,HardAntler:1",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configRecipeUpgrade.SettingChanged += (obj, attr) => { UpgradeAntlerPickaxe.Instance.PatchRecipe(); };

                // Enable SaveOnConfigSet before the last bind allowing the config file to be created on first run
                UpgradeAntlerPickaxe.Instance.Config.SaveOnConfigSet = true;

                configRecipeMultiplier = UpgradeAntlerPickaxe.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configRecipeMultiplier.SettingChanged += (obj, attr) => { UpgradeAntlerPickaxe.Instance.PatchRecipe(); };

                configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, UpgradeAntlerPickaxe.configFileName);
                configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                configWatcher.IncludeSubdirectories = true;
                configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with initialising the config or config events: " + error);
            }
        }

        private static void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(UpgradeAntlerPickaxe.configFileFullPath))
                return;

            try
            {
                UpgradeAntlerPickaxe.Instance.Config.Reload();
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
