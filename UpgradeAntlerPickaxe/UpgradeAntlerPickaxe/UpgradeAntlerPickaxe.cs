using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.IO;
using UnityEngine;

namespace UpgradeAntlerPickaxe
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class UpgradeAntlerPickaxe : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.UpgradeAntlerPickaxe";
        public const string PluginName = "Upgrade Antler Pickaxe";
        public const string PluginVersion = "1.1.2";
        private static string configFileName = PluginGUID + ".cfg";
        private static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;

        private string[] configCraftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };

        private ConfigEntry<bool> configEnable;
        private ConfigEntry<bool> configCreateClone;
        private ConfigEntry<string> configName;
        private ConfigEntry<string> configDescription;
        private ConfigEntry<string> configCraftingStation;
        private ConfigEntry<int> configMinStationLevel;
        private ConfigEntry<string> configRecipe;
        private ConfigEntry<string> configRecipeUpgrade;
        private ConfigEntry<int> configRecipeMultiplier;

        /**
         * Called when the mod is being initialised
         */
        private void Awake()
        {
            try
            {
                InitConfig();

                if (configEnable.Value)
                {
                    PrefabManager.OnVanillaPrefabsAvailable += PatchStats;

                    if (!configCreateClone.Value)
                    {
                        AddCustomRecipe();
                        ItemManager.OnItemsRegistered += RemoveOriginalRecipe;
                    }
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with the initialisation of UpgradeAntlerPickaxe: " + error);
            }
        }

        /**
         * Called when the mod is unloaded
         */
        private void OnDestroy()
        {
            Config.Save();
        }

        /**
         * Update the original or clone of the Antler Pickaxe
         */
        private void PatchStats()
        {
            try
            {
                if (!configCreateClone.Value)
                {
                    GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
                    ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                    UpdateItemStats(antlerPickaxe);
                }
                else if (ItemManager.Instance.GetItem("PickaxeAntler_DW") == null)
                {
                    ItemConfig itemConfig = new ItemConfig();
                    itemConfig.CraftingStation = configCraftingStation.Value;
                    itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(configRecipe.Value, configRecipeUpgrade.Value, configRecipeMultiplier.Value);

                    CustomItem antlerPickaxe = new CustomItem("PickaxeAntler_DW", "PickaxeAntler", itemConfig);
                    UpdateItemStats(antlerPickaxe.ItemDrop);
                    ItemManager.Instance.AddItem(antlerPickaxe);
                }
                else
                {
                    CustomItem antlerPickaxe = ItemManager.Instance.GetItem("PickaxeAntler_DW");
                    UpdateItemStats(antlerPickaxe.ItemDrop);
                }

                PrefabManager.OnVanillaPrefabsAvailable -= PatchStats;
                Jotunn.Logger.LogInfo("Successfully patched stats on the antler pickaxe, enjoy!");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to patch stats onto the antler pickaxe: " + error.Message);
            }
        }

        /**
         * Undo any changes made to the original or clone
         */
        private void UnpatchStats()
        {
            try
            {
                if (!configCreateClone.Value)
                {
                    GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
                    ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                    UpdateItemStats(antlerPickaxe, true);
                }
                else
                {
                    CustomItem antlerPickaxe = ItemManager.Instance.GetItem("PickaxeAntler_DW");
                    UpdateItemStats(antlerPickaxe.ItemDrop, true);
                }

                Jotunn.Logger.LogInfo("Successfully unpatched stats of the antler pickaxe, Why u do this?!");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to unpatch stats from the antler pickaxe: " + error.Message);
            }
        }

        /**
         * Update the stats of an item
         */
        private void UpdateItemStats(ItemDrop item, bool unpatch = false)
        {
            if (unpatch)
            {
                item.m_itemData.m_shared.m_maxQuality = 1;
                return;
            }

            item.m_itemData.m_shared.m_name = configName.Value;
            item.m_itemData.m_shared.m_description = configDescription.Value;
            item.m_itemData.m_shared.m_maxQuality = 4;
            item.m_itemData.m_shared.m_damagesPerLevel.m_pickaxe = 5;
            item.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 5;
            item.m_itemData.m_shared.m_blockPowerPerLevel = 0;
            item.m_itemData.m_shared.m_deflectionForcePerLevel = 5;
            item.m_itemData.m_shared.m_durabilityPerLevel = 50;
        }

        /**
         * Add the recipe of the Antler Pickaxe
         */
        private void AddCustomRecipe()
        {
            try
            {
                RecipeConfig recipe = new RecipeConfig();
                recipe.Item = "PickaxeAntler";
                recipe.CraftingStation = configCraftingStation.Value;
                recipe.MinStationLevel = configMinStationLevel.Value;
                recipe.Requirements = RecipeHelper.GetAsRequirementConfigArray(configRecipe.Value, configRecipeUpgrade.Value, configRecipeMultiplier.Value);

                ItemManager.Instance.AddRecipe(new CustomRecipe(recipe));
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not add the recipe for the antler pickaxe: " + error);
            }
        }

        /**
         * Remove the original Antler Pickaxe recipe
         */
        private void RemoveOriginalRecipe()
        {
            try
            {
                GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
                ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                Recipe recipe = ObjectDB.instance.GetRecipe(antlerPickaxe.m_itemData);

                if (recipe != null)
                    ObjectDB.instance.m_recipes.Remove(recipe); // This will remove the first occurence
                else
                    throw new Exception("Could not find recipe to remove!");

                ItemManager.OnItemsRegistered -= RemoveOriginalRecipe;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to remove recipe of the antler pickaxe: " + error.Message);
            }
        }

        /**
         * Update recipe related fields when the config changes
         */
        private void PatchRecipe(RecipeUpdateType updateType = RecipeUpdateType.Recipe)
        {
            try
            {
                string recipeName = "Recipe_" + (configCreateClone.Value ? "PickaxeAntler_DW" : "PickaxeAntler");
                CustomRecipe recipe = ItemManager.Instance.GetRecipe(recipeName);

                if (recipe == null)
                    throw new Exception("Could not find recipe!");

                if (!configEnable.Value)
                    return;

                switch (updateType)
                {
                    case RecipeUpdateType.Recipe:
                        Piece.Requirement[] requirements = RecipeHelper.GetAsPieceRequirementArray(configRecipe.Value, configRecipeUpgrade.Value, configRecipeMultiplier.Value);

                        if (requirements == null)
                            throw new Exception("Requirements is null");

                        foreach(var requirement in requirements)
                        {
                            Jotunn.Logger.LogInfo(requirement.m_resItem?.name);
                            Jotunn.Logger.LogInfo(requirement.m_amount);
                            Jotunn.Logger.LogInfo(requirement.m_amountPerLevel);
                        }

                        recipe.Recipe.m_resources = requirements;
                        break;
                    case RecipeUpdateType.CraftingStation:
                        if (configCraftingStation.Value == "None")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = true;
                        }
                        else if (configCraftingStation.Value == "Disabled")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = false;
                        }
                        else
                        {
                            string pieceName = CraftingStations.GetInternalName(configCraftingStation.Value);
                            recipe.Recipe.m_enabled = true;
                            recipe.Recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();
                        }
                        break;
                    case RecipeUpdateType.MinRequiredLevel:
                        recipe.Recipe.m_minStationLevel = configMinStationLevel.Value;
                        break;
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + error);
            }
        }

        /**
         * Initialise config entries and add the necessary events
         */
        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = false;

                configEnable = Config.Bind(new ConfigDefinition("General", "Enable"), true,
                    new ConfigDescription("Wether or not to enable this mod. When changed while the game is running it will disable the ability to upgrade\nthe pickaxe but will not modify already upgraded ones. They will be reverted back to normal on next game start\nEXCEPT if they were cloned, then they will be deleted!", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configEnable.SettingChanged += (obj, attr) =>
                {
                    if (!configEnable.Value)
                    {
                        UnpatchStats();

                        string cloneAddition = configCreateClone.Value ? " and will be deleted when players relog!" : "!";
                        Jotunn.Logger.LogWarning("UpgradeAntlerPickaxe is now disabled! The Antler Pickaxe can no longer be upgraded" + cloneAddition);
                    }
                    else
                        PatchStats();
                };

                configCreateClone = Config.Bind(new ConfigDefinition("General", "Create clone"), false,
                    new ConfigDescription("Wether to patch the original Antler Pickaxe or to create a clone instead (requires restart)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configName = Config.Bind(new ConfigDefinition("General", "Name"), "Antler pickaxe+",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configName.SettingChanged += (obj, attr) => { PatchStats(); };

                configDescription = Config.Bind(new ConfigDefinition("General", "Description"), "This tool is hard enough to crack even the most stubborn rocks.",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDescription.SettingChanged += (obj, attr) => { PatchStats(); };

                configCraftingStation = Config.Bind(new ConfigDefinition("General", "Crafting station"), "Workbench",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCraftingStation.SettingChanged += (obj, attr) => { PatchRecipe(RecipeUpdateType.CraftingStation); };

                configMinStationLevel = Config.Bind(new ConfigDefinition("General", "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configMinStationLevel.SettingChanged += (obj, attr) => { PatchRecipe(RecipeUpdateType.MinRequiredLevel); };

                configRecipe = Config.Bind(new ConfigDefinition("General", "Crafting costs"), "Wood:10,HardAntler:1",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configRecipe.SettingChanged += (obj, attr) => { PatchRecipe(); };

                configRecipeUpgrade = Config.Bind(new ConfigDefinition("General", "Upgrade costs"), "Wood:4,HardAntler:1",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configRecipeUpgrade.SettingChanged += (obj, attr) => { PatchRecipe(); };

                // Enable SaveOnConfigSet before the last bind allowing the config file to be created on first run
                Config.SaveOnConfigSet = true;

                configRecipeMultiplier = Config.Bind(new ConfigDefinition("General", "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configRecipeMultiplier.SettingChanged += (obj, attr) => { PatchRecipe(); };

                FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, configFileName);
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

        /**
         * Event handler for when the config file changes
         */
        private void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(configFileFullPath))
                return;

            try
            {
                Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }
    }
}

