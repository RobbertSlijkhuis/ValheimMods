using BepInEx;
using Jotunn.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.IO;
using UnityEngine;
using UpgradeAntlerPickaxe.Configs;

namespace UpgradeAntlerPickaxe
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class UpgradeAntlerPickaxe : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.UpgradeAntlerPickaxe";
        public const string PluginName = "Upgrade Antler Pickaxe";
        public const string PluginVersion = "1.1.5";
        public static UpgradeAntlerPickaxe Instance;
        public static string configFileName = PluginGUID + ".cfg";
        public static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;

        public void Awake()
        {
            try
            {
                Instance = this;
                PluginConfig.Init();

                if (PluginConfig.configEnable.Value)
                {
                    PrefabManager.OnVanillaPrefabsAvailable += PatchStats;
                    ItemManager.OnItemsRegistered += PatchOriginalRecipe;
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with the initialisation of UpgradeAntlerPickaxe: " + error);
            }
        }

        public void PatchStats()
        {
            try
            {
                GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
                ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                UpdateItemStats(antlerPickaxe);

                PrefabManager.OnVanillaPrefabsAvailable -= PatchStats;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Unable to patch stats onto the antler pickaxe: " + error.Message);
            }
        }

        public void UnpatchStats()
        {
            try
            {
                GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
                ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                UpdateItemStats(antlerPickaxe, true);
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

            item.m_itemData.m_shared.m_maxQuality = 4;
            item.m_itemData.m_shared.m_damagesPerLevel.m_pickaxe = 5;
            item.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 5;
            item.m_itemData.m_shared.m_blockPowerPerLevel = 0;
            item.m_itemData.m_shared.m_deflectionForcePerLevel = 5;
            item.m_itemData.m_shared.m_durabilityPerLevel = 50;
        }

        private Recipe GetOriginalRecipe()
        {
            GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");
            ItemDrop antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
            return ObjectDB.instance.GetRecipe(antlerPickaxe.m_itemData);
        }

        private void PatchOriginalRecipe()
        {
            PatchRecipe(RecipeUpdateType.Recipe);
            PatchRecipe(RecipeUpdateType.CraftingStation);
            PatchRecipe(RecipeUpdateType.MinRequiredLevel);
        }

        public void PatchRecipe(RecipeUpdateType updateType = RecipeUpdateType.Recipe)
        {
            try
            {
                Recipe recipe = GetOriginalRecipe();

                if (recipe == null)
                    throw new Exception("Could not find recipe!");

                if (!PluginConfig.configEnable.Value)
                    return;

                switch (updateType)
                {
                    case RecipeUpdateType.Recipe:
                        Piece.Requirement[] requirements = RecipeHelper.GetAsPieceRequirementArray(PluginConfig.configRecipe.Value, PluginConfig.configRecipeUpgrade.Value, PluginConfig.configRecipeMultiplier.Value);

                        if (requirements == null)
                            throw new Exception("Requirements is null");

                        recipe.m_resources = requirements;
                        break;
                    case RecipeUpdateType.CraftingStation:
                        if (PluginConfig.configCraftingStation.Value == "None")
                        {
                            recipe.m_craftingStation = null;
                            recipe.m_enabled = true;
                        }
                        else if (PluginConfig.configCraftingStation.Value == "Disabled")
                        {
                            recipe.m_craftingStation = null;
                            recipe.m_enabled = false;
                        }
                        else
                        {
                            string pieceName = CraftingStations.GetInternalName(PluginConfig.configCraftingStation.Value);
                            recipe.m_enabled = true;
                            recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();
                        }
                        break;
                    case RecipeUpdateType.MinRequiredLevel:
                        recipe.m_minStationLevel = PluginConfig.configMinStationLevel.Value;
                        break;
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + error);
            }
        }
    }
}
