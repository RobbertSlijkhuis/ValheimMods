#nullable enable

using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Armors.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class RecipeHelper
    {
        private static readonly Regex nukeWhiteSpaceRegex = new Regex(@"\s+");
        private static readonly Regex recipeEntryRegex = new Regex(@"^([a-zA-Z0-9_]+:[0-9]+)$");

        /**
         * Convert the recipe and return a RequirementConfig array
         */
        public static RequirementConfig[]? GetAsRequirementConfigArray(string configRecipe, string? upgradeRecipe, int? multiplier)
        {
            List<RequirementConfig>? requirements = GetAsRequirementConfigList(configRecipe, upgradeRecipe, multiplier);

            if (requirements == null)
                return null;

            return requirements.ToArray();
        }

        /**
         * Convert the recipe and return a Piece.Requirement array
         */
        public static Piece.Requirement[]? GetAsPieceRequirementArray(string configRecipe, string? upgradeRecipe, int? multiplier)
        {
            try
            {
                List<RequirementConfig>? list = GetAsRequirementConfigList(configRecipe, upgradeRecipe, multiplier);
                List<Piece.Requirement> pieceList = new List<Piece.Requirement>();

                if (list == null)
                    return null;

                foreach (RequirementConfig entry in list)
                {
                    GameObject item = PrefabManager.Instance.GetPrefab(entry.Item);

                    if (item == null)
                        throw new Exception("Could not find item, please check config!");

                    Piece.Requirement requirement = new Piece.Requirement();
                    requirement.m_resItem = item.GetComponent<ItemDrop>();
                    requirement.m_amount = entry.Amount;
                    requirement.m_recover = entry.Recover;

                    if (upgradeRecipe != null && multiplier != null)
                        requirement.m_amountPerLevel = entry.AmountPerLevel;

                    pieceList.Add(requirement);
                }

                return pieceList.ToArray();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not convert recipe to Piece.Requirement array: " + e);
                return null;
            }
        }

        /**
         * Convert the recipe and return a RequirementConfig list
         */
        public static List<RequirementConfig>? GetAsRequirementConfigList(string configRecipe, string? upgradeRecipe, int? multiplier)
        {
            try
            {
                configRecipe = nukeWhiteSpaceRegex.Replace(configRecipe, "");

                if (upgradeRecipe != null)
                    upgradeRecipe = nukeWhiteSpaceRegex.Replace(upgradeRecipe, "");

                if (!IsConfigRecipeValid(configRecipe, upgradeRecipe))
                {
                    Jotunn.Logger.LogWarning("Config is not valid, please check the values");
                    return null;
                }

                List<RequirementConfig> list = new List<RequirementConfig>();
                string[] recipeEntries = configRecipe.Trim().Split(',');

                foreach (string entry in recipeEntries)
                {
                    RequirementConfig requirement = new RequirementConfig();
                    string[] recipeEntryValues = entry.Split(':');
                    requirement.Item = recipeEntryValues[0];
                    requirement.Amount = Int32.Parse(recipeEntryValues[1]);
                    requirement.Recover = true;

                    if (upgradeRecipe != null && !upgradeRecipe.IsNullOrWhiteSpace() && multiplier != null)
                    {
                        string[] upgradeEntries = upgradeRecipe.Trim().Split(',');
                        foreach (string upgradeEntry in upgradeEntries)
                        {
                            string[] upgradeEntryValues = upgradeEntry.Split(':');

                            if (requirement.Item == upgradeEntryValues[0])
                            {
                                requirement.AmountPerLevel = Int32.Parse(upgradeEntryValues[1]) * (int)multiplier;
                                break;
                            }
                        }
                    }

                    list.Add(requirement);
                }

                return list;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not convert recipe to RequirementConfig list: " + e);
                return null;
            }
        }

        /**
         * Check wether the recipe and/or upgrade recipe are valid
         */
        public static bool IsConfigRecipeValid(string configRecipe, string? upgradeRecipe)
        {
            try
            {
                bool isValid = true;

                string[] recipeEntries = configRecipe.Split(',');
                foreach (string entry in recipeEntries)
                {
                    bool isMatch = recipeEntryRegex.IsMatch(entry);
                    if (!isMatch)
                    {
                        isValid = false;
                        Jotunn.Logger.LogWarning("Cannot resolve " + entry + " from the crafting recipe");
                    }
                }

                if (upgradeRecipe == null || upgradeRecipe.IsNullOrWhiteSpace())
                    return isValid;

                string[] upgradeEntries = upgradeRecipe.Split(',');
                foreach (string entry in upgradeEntries)
                {
                    bool isMatch = recipeEntryRegex.IsMatch(entry);
                    if (!isMatch)
                    {
                        isValid = false;
                        Jotunn.Logger.LogWarning("Cannot resolve " + entry + " from the upgrade recipe");
                    }
                }

                return isValid;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not validate recipe due to an e: " + e);
                return false;
            }
        }

        /**
         * Update recipe related fields when the config changes
         */
        public static void UpdateRecipe(UpdateRecipeOptions options)
        {
            try
            {
                CustomRecipe customRecipe = ItemManager.Instance.GetRecipe(options.name);
                Recipe? recipeObjectDB;

                if (customRecipe != null)
                {
                    _UpdateRecipe(customRecipe.Recipe, options);
                    return;
                }

                if (options.prefab == null)
                    throw new Exception("Could not find recipe of: " + options.name + ", prefab is null");

                recipeObjectDB = GetRecipeFromObjectDB(options.prefab);

                if (recipeObjectDB != null)
                    _UpdateRecipe(recipeObjectDB, options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + e);
            }
        }

        private static void _UpdateRecipe(Recipe recipe, UpdateRecipeOptions options)
        {
            switch (options.updateType)
            {
                case RecipeUpdateType.ALL:
                    if (options.requirements == null)
                        throw new Exception("Requirements is null");

                    Piece.Requirement[]? requirementsAll = GetAsPieceRequirementArray(options.requirements, options.upgradeRequirements, options.upgradeMultiplier);

                    if (requirementsAll == null)
                    {
                        Jotunn.Logger.LogWarning("Cannot update recipe, requirements is null");
                        return;
                    }

                    Jotunn.Logger.LogWarning("Requirements added");
                    recipe.m_resources = requirementsAll;

                    if (options.craftingStation == null || options.craftingStation == "")
                        throw new Exception("Craftingstation is null or empty string");

                    if (options.craftingStation == "None")
                    {
                        recipe.m_craftingStation = null;
                        recipe.m_enabled = true;
                    }
                    else if (options.craftingStation == "Disabled")
                    {
                        recipe.m_craftingStation = null;
                        recipe.m_enabled = false;
                    }
                    else
                    {
                        string pieceName = CraftingStations.GetInternalName(options.craftingStation);
                        recipe.m_enabled = true;
                        recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();

                        Jotunn.Logger.LogWarning("CraftingStation: " + options.craftingStation);
                    }

                    if (options.requiredStationLevel == null || options.requiredStationLevel < 1)
                        throw new Exception("Required station level is null or lower then 1");

                    Jotunn.Logger.LogWarning("RequiredStationLevel: " + options.requiredStationLevel);
                    recipe.m_minStationLevel = (int)options.requiredStationLevel;

                    if (options.enable == null)
                        throw new Exception("Enable is null");

                    Jotunn.Logger.LogWarning("Enable: " + options.enable);
                    recipe.m_enabled = (bool)options.enable;
                    break;
                case RecipeUpdateType.ENABLE:
                    if (options.enable == null)
                        throw new Exception("Enable is null");

                    recipe.m_enabled = (bool)options.enable;
                    break;
                case RecipeUpdateType.RECIPE:
                    if (options.requirements == null)
                        throw new Exception("Requirements is null");

                    Piece.Requirement[]? requirements = GetAsPieceRequirementArray(options.requirements, options.upgradeRequirements, options.upgradeMultiplier);

                    if (requirements == null)
                    {
                        Jotunn.Logger.LogWarning("Cannot update recipe, requirements is null");
                        return;
                    }

                    recipe.m_resources = requirements;
                    break;
                case RecipeUpdateType.CRAFTINGSTATION:
                    if (options.craftingStation == null || options.craftingStation == "")
                        throw new Exception("Craftingstation is null or empty string");

                    if (options.craftingStation == "None")
                    {
                        recipe.m_craftingStation = null;
                        recipe.m_enabled = true;
                    }
                    else if (options.craftingStation == "Disabled")
                    {
                        recipe.m_craftingStation = null;
                        recipe.m_enabled = false;
                    }
                    else
                    {
                        string pieceName = CraftingStations.GetInternalName(options.craftingStation);
                        recipe.m_enabled = true;
                        recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();
                    }
                    break;
                case RecipeUpdateType.MINREQUIREDSTATIONLEVEL:
                    if (options.requiredStationLevel == null || options.requiredStationLevel < 1)
                        throw new Exception("Required station level is null or lower then 1");

                    recipe.m_minStationLevel = (int)options.requiredStationLevel;
                    break;
                case RecipeUpdateType.FROM_RECIPE:
                    if (options.fromRecipe == null)
                        throw new Exception("Field fromRecipe is null");

                    recipe.m_enabled = options.fromRecipe.enabled;
                    recipe.m_craftingStation = options.fromRecipe.craftingStation;
                    recipe.m_resources = options.fromRecipe.resources;
                    recipe.m_minStationLevel = options.fromRecipe.minStationLevel;
                    break;
            }
        }

        public static Recipe? GetRecipeFromObjectDB(GameObject prefab)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Could not find recipe, prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("Could not find recipe of: " + prefab.name + ", itemDrop is null");

                Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

                if (recipe == null)
                    throw new Exception("Could not find any recipe of: " + prefab.name);

                return recipe;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could retrieve recipe from ObjectDB: " + e);
                return null;
            }
        }

        public static RecipeSnapShot? TakeSnapShot(GameObject prefab)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Could not find recipe, prefab is null");

                Recipe? recipe = GetRecipeFromObjectDB(prefab);

                if (recipe == null)
                    throw new Exception("Could not find any recipe of: " + prefab.name);

                RecipeSnapShot newRecipe = new RecipeSnapShot();
                newRecipe.enabled = recipe.m_enabled;
                newRecipe.resources = recipe.m_resources;
                newRecipe.craftingStation = recipe.m_craftingStation;
                newRecipe.minStationLevel = recipe.m_minStationLevel;

                return newRecipe;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could create snapshot of recipe: " + e);
                return null;
            }
        }
    }
}
