using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UpgradeAntlerPickaxe;

namespace ValheimModding
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class UpgradeAntlerPickaxe : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.UpgradeAntlerPickaxe";
        public const string PluginName = "Upgrade Antler Pickaxe";
        public const string PluginVersion = "1.1.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private ConfigEntry<bool> configEnable;
        private ConfigEntry<bool> configCreateClone;
        private ConfigEntry<string> configName;
        private ConfigEntry<string> configDescription;
        private ConfigEntry<string> configCraftingStation;
        private ConfigEntry<string> configRecipe;
        private ConfigEntry<string> configRecipeUpgrade;
        private ConfigEntry<int> configRecipeMultiplier;

        private bool isRecipeInitialised = false;

        private void Awake()
        {
            try
            {
                InitConfig();

                if (configEnable.Value)
                {
                    ConfigRecipe recipe = new ConfigRecipe(configRecipe.Value, configRecipeUpgrade.Value);
                    if (!recipe.IsConfigRecipeValid())
                        throw new Exception("Invalid recipe config");

                    PrefabManager.OnVanillaPrefabsAvailable += PatchStats;

                    if (!configCreateClone.Value)
                    {
                        AddRecipe();
                        ItemManager.OnItemsRegistered += RemoveRecipe; // This will remove the original, but only this time
                    }
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with the initialisation of UpgradeAntlerPickaxe: "+ error);
            }
        }

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
                    ConfigRecipe recipe = new ConfigRecipe(configRecipe.Value, configRecipeUpgrade.Value);
                    ItemConfig itemConfig = new ItemConfig();
                    itemConfig.CraftingStation = configCraftingStation.Value;

                    foreach (var requirement in recipe.requirements)
                    {
                        int multiplier = configRecipeMultiplier.Value != 0 ? configRecipeMultiplier.Value : 1;
                        int amountPerlevel = requirement.amountPerLevel * multiplier;
                        itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
                    }

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
                Jotunn.Logger.LogInfo("Successfully patched stats the antler pickaxe, enjoy!");
            } 
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to patch stats onto the antler pickaxe: "+ error.Message);
            }
        }

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

        private void AddRecipe()
        {
            try
            {
                ConfigRecipe recipe = new ConfigRecipe(configRecipe.Value, configRecipeUpgrade.Value);
                RecipeConfig antlerPickaxeRecipe = new RecipeConfig();
                antlerPickaxeRecipe.Item = "PickaxeAntler";
                antlerPickaxeRecipe.CraftingStation = configCraftingStation.Value;

                foreach (var requirement in recipe.requirements)
                {
                    int multiplier = configRecipeMultiplier.Value != 0 ? configRecipeMultiplier.Value : 1;
                    int amountPerlevel = requirement.amountPerLevel * multiplier;
                    antlerPickaxeRecipe.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
                }

                ItemManager.Instance.AddRecipe(new CustomRecipe(antlerPickaxeRecipe));
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not add the recipe for the antler pickaxe: "+ error);
            }
        }

        private void RemoveRecipe()
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

                if (!isRecipeInitialised)
                    isRecipeInitialised = true;

                ItemManager.OnItemsRegistered -= RemoveRecipe;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to remove recipe of the antler pickaxe: " + error.Message);
            }
        }

        /**
         * Currently stuck at a Index was outside of bounds of the array error
         */
        private void UpdateRecipe()
        {
            try
            {
                Jotunn.Logger.LogWarning("UpdateRecipe()");
                //foreach (var dbRecipe in ObjectDB.instance.m_recipes)
                //{
                //    Jotunn.Logger.LogInfo(dbRecipe.name);
                //}

                ConfigRecipe recipe = new ConfigRecipe(configRecipe.Value, configRecipeUpgrade.Value);
                bool isValid = recipe.IsConfigRecipeValid();

                if (!isValid) 
                    throw new Exception("Cannot update recipe because its not valid!");

                CustomRecipe itemRecipe = ItemManager.Instance.GetRecipe("Recipe_PickaxeAntler" + (configCreateClone.Value == true ? "_DW" : ""));
                List<Piece.Requirement> list = new List<Piece.Requirement>();
                // Mock<global::CraftingStation>.Create(CraftingStation);

                foreach (var requirement in recipe.requirements)
                {
                    Piece.Requirement pieceRequirement = new Piece.Requirement();
                    int multiplier = configRecipeMultiplier.Value != 0 ? configRecipeMultiplier.Value : 1;
                    int amountPerlevel = requirement.amountPerLevel * multiplier;

                    pieceRequirement.m_amountPerLevel = amountPerlevel;
                    pieceRequirement.m_amount = requirement.amount;
                    pieceRequirement.m_resItem = Mock<ItemDrop>.Create(requirement.material);
                    pieceRequirement.m_recover = true;

                    list.Add(pieceRequirement);
                }

                itemRecipe.Recipe.m_resources = list.ToArray();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to update the recipe: "+ error);
            }
        }

        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = true;

                configEnable = base.Config.Bind(new ConfigDefinition("General", "Enable"), true,
                    new ConfigDescription("Wether or not to enable this mod. When changed while the game is running it will disable the ability to upgrade\nthe pickaxe but will not modify already upgraded ones. They will be reverted back to normal on next game start\nEXCEPT if they were cloned, then they will be deleted!", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCreateClone = base.Config.Bind(new ConfigDefinition("General", "Create clone"), false,
                    new ConfigDescription("Wether to patch the original Antler Pickaxe or to create a clone instead", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configName = base.Config.Bind(new ConfigDefinition("General", "Name"), "Antler pickaxe+",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDescription = base.Config.Bind(new ConfigDefinition("General", "Description"), "This tool is hard enough to crack even the most stubborn rocks.",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCraftingStation = base.Config.Bind(new ConfigDefinition("General", "Crafting station"), "Workbench",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configRecipe = base.Config.Bind(new ConfigDefinition("General", "Crafting costs"), "Wood:10,HardAntler:1",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configRecipeUpgrade = base.Config.Bind(new ConfigDefinition("General", "Upgrade costs"), "Wood:4,HardAntler:1",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configRecipeMultiplier = base.Config.Bind(new ConfigDefinition("General", "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configEnable.SettingChanged += (obj, attr) =>
                {
                    if (!configEnable.Value)
                        UnpatchStats();
                    else
                    {
                        PatchStats();
                    }
                };

                configName.SettingChanged += (obj, attr) =>
                {
                    PatchStats();
                };

                configDescription.SettingChanged += (obj, attr) =>
                {
                    PatchStats();
                };

                //configRecipe.SettingChanged += (obj, attr) =>
                //{
                //    UpdateRecipe();
                //};

                //configRecipeUpgrade.SettingChanged += (obj, attr) =>
                //{
                //    UpdateRecipe();
                //};

                //configRecipeMultiplier.SettingChanged += (obj, attr) =>
                //{
                //    UpdateRecipe();
                //};

                SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
                {
                    if (attr.InitialSynchronization)
                    {
                        Jotunn.Logger.LogMessage("Initial Config sync event received");
                    }
                    else
                    {
                        Jotunn.Logger.LogMessage("Config sync event received");

                        if (!configEnable.Value)
                            UnpatchStats();
                        else
                        {
                            PatchStats();
                        }
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with initialising the config or config events: "+ error);
            }
        }
    }
}
