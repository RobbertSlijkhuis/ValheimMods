#nullable enable

using BepInEx;
using DeathWizshAPI.Helpers.Models;
using DeathWizshAPI.Helpers.Types;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeathWizshAPI.Helpers
{
    public class RecipeHelper
    {
        private static readonly Regex nukeWhiteSpaceRegex = new Regex(@"\s+");
        private static readonly Regex recipeEntryRegex = new Regex(@"^([a-zA-Z0-9_]+:[0-9]+)$");

        public static void UpdateRecipe(UpdateRecipeOptions options)
        {
            try
            {
                CustomRecipe customRecipe = ItemManager.Instance.GetRecipe(options.name);

                if (customRecipe != null)
                {
                    UpdateRecipe(customRecipe.Recipe, options);
                    return;
                }

                if (options.prefab == null)
                    throw new Exception("Could not find recipe of: " + options.name + ", prefab is null");

                Recipe? recipeObjectDB;
                recipeObjectDB = GetRecipeFromObjectDB(options.prefab);

                // Errors are logged in GetRecipeFromObjectDB if something is wrong
                if (recipeObjectDB != null)
                    UpdateRecipe(recipeObjectDB, options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + e);
            }
        }

        private static void UpdateRecipe(Recipe recipe, UpdateRecipeOptions options)
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
                    }

                    if (options.requiredStationLevel == null || options.requiredStationLevel < 1)
                        throw new Exception("Required station level is null or lower then 1");

                    recipe.m_minStationLevel = (int)options.requiredStationLevel;

                    if (options.amount != null)
                        recipe.m_amount = (int)options.amount;

                    if (options.enable == null)
                        throw new Exception("Enable is null");

                    recipe.m_enabled = (bool)options.enable;
                    break;
                case RecipeUpdateType.ENABLE:
                    if (options.enable == null)
                        throw new Exception("Enable is null");

                    recipe.m_enabled = (bool)options.enable;
                    break;
                case RecipeUpdateType.AMOUNT:
                    if (options.amount == null)
                        throw new Exception("Amount is null");

                    recipe.m_amount = (int)options.amount;
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

        public static RequirementConfig[]? GetAsRequirementConfigArray(string recipe, string? upgradeRecipe, int? multiplier)
        {
            List<RequirementConfig>? requirements = GetAsRequirementConfigList(recipe, upgradeRecipe, multiplier);

            if (requirements == null)
                return null;

            return requirements.ToArray();
        }

        public static Piece.Requirement[]? GetAsPieceRequirementArray(string recipe, string? upgradeRecipe, int? multiplier)
        {
            List<RequirementConfig>? list = GetAsRequirementConfigList(recipe, upgradeRecipe, multiplier);
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

        public static List<RequirementConfig>? GetAsRequirementConfigList(string recipe, string? upgradeRecipe, int? multiplier)
        {
            recipe = nukeWhiteSpaceRegex.Replace(recipe, "");

            if (upgradeRecipe != null)
                upgradeRecipe = nukeWhiteSpaceRegex.Replace(upgradeRecipe, "");

            if (!IsConfigRecipeValid(recipe, upgradeRecipe))
            {
                Jotunn.Logger.LogWarning("Config is not valid, please check the values");
                return null;
            }

            List<RequirementConfig> list = new List<RequirementConfig>();
            string[] recipeEntries = recipe.Trim().Split(',');

            foreach (string entry in recipeEntries)
            {
                RequirementConfig requirement = new RequirementConfig();
                string[] recipeEntryValues = entry.Split(':');
                requirement.Item = recipeEntryValues[0];
                requirement.Amount = int.Parse(recipeEntryValues[1]);
                requirement.Recover = true;

                if (upgradeRecipe != null && !upgradeRecipe.IsNullOrWhiteSpace() && multiplier != null)
                {
                    string[] upgradeEntries = upgradeRecipe.Trim().Split(',');
                    foreach (string upgradeEntry in upgradeEntries)
                    {
                        string[] upgradeEntryValues = upgradeEntry.Split(':');

                        if (requirement.Item == upgradeEntryValues[0])
                        {
                            requirement.AmountPerLevel = int.Parse(upgradeEntryValues[1]) * (int)multiplier;
                            break;
                        }
                    }
                }

                list.Add(requirement);
            }

            return list;
        }

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
                Jotunn.Logger.LogError("Could not validate recipe due to an error: " + e);
                return false;
            }
        }

        public static Recipe? GetRecipeFromObjectDB(GameObject prefab)
        {
            if (prefab == null)
                throw new Exception("Could not find recipe in ObjectDB, prefab is null");

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                throw new Exception("Could not find recipe in ObjectDB, no item drop for " + prefab.name + " was found");

            Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

            if (recipe == null)
                throw new Exception("Could not find recipe in ObjectDB, no recipe for " + prefab.name + " was found");

            return recipe;
        }

        public static RecipeSnapShot? CreateSnapShot(GameObject prefab)
        {
            if (prefab == null)
                throw new Exception("Could not create recipe snapshot, prefab is null");

            Recipe? recipe = GetRecipeFromObjectDB(prefab);

            if (recipe == null)
                throw new Exception("Could not create recipe snapshot, no recipe for " + prefab.name + " was found");

            RecipeSnapShot newRecipe = new RecipeSnapShot();
            newRecipe.enabled = recipe.m_enabled;
            newRecipe.resources = recipe.m_resources;
            newRecipe.craftingStation = recipe.m_craftingStation;
            newRecipe.minStationLevel = recipe.m_minStationLevel;

            return newRecipe;
        }
    }
}
