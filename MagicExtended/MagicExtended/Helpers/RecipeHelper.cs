#nullable enable

using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using MagicExtended.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MagicExtended.Helpers
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
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not convert recipe to Piece.Requirement array: " + error);
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
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not convert recipe to RequirementConfig list: " + error);
                return null;
            }
        }

        /**
         * Check wether the recipe & upgrade recipe are valid
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
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not validate recipe due to an error: " + error);
                return false;
            }
        }

        /**
         * Update recipe related fields when the config changes
         */
        public static void PatchRecipe(PatchRecipeOptions options)
        {
            try
            {
                CustomRecipe recipe = ItemManager.Instance.GetRecipe(options.name); ;

                if (recipe == null)
                    throw new Exception("Could not find recipe: "+ options.name);

                switch (options.updateType)
                {
                    case RecipeUpdateType.Enable:
                        if (options.enable == null)
                            throw new Exception("Enable is null");

                        recipe.Recipe.m_enabled = (bool)options.enable;
                        break;
                    case RecipeUpdateType.Recipe:
                        if (options.requirements == null)
                            throw new Exception("Requirements is null");

                        Piece.Requirement[]? requirements = GetAsPieceRequirementArray(options.requirements, options.upgradeRequirements, options.upgradeMultiplier);

                        if (requirements == null)
                        {
                            // throw new Exception("Requirements is null");
                            Jotunn.Logger.LogWarning("Cannot update recipe, requirements is null");
                            return;
                        }

                        recipe.Recipe.m_resources = requirements;
                        break;
                    case RecipeUpdateType.CraftingStation:
                        if (options.craftingStation == null || options.craftingStation == "")
                            throw new Exception("Craftingstation is null or empty string");

                        if (options.craftingStation == "None")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = true;
                        }
                        else if (options.craftingStation == "Disabled")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = false;
                        }
                        else
                        {
                            string pieceName = CraftingStations.GetInternalName(options.craftingStation);
                            recipe.Recipe.m_enabled = true;
                            recipe.Recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();
                        }
                        break;
                    case RecipeUpdateType.MinRequiredStationLevel:
                        if (options.requiredStationLevel == null || options.requiredStationLevel < 1)
                            throw new Exception("Required station level is null or lower then 1");

                        recipe.Recipe.m_minStationLevel = (int)options.requiredStationLevel;
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
