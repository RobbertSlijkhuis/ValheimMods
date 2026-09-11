using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SplashMeads.Models;
using SplashMeads.Types;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SplashMeads.Helpers
{
    internal class RecipeHelper
    {
        private static readonly Regex nukeWhiteSpaceRegex = new Regex(@"\s+");
        private static readonly Regex recipeEntryRegex = new Regex(@"^([a-zA-Z0-9_]+:[0-9]+)$");

        /**
         * Update an already registered recipe
         */
        public static void UpdateRecipe(UpdateRecipeOptions options)
        {
            try
            {
                CustomRecipe customRecipe = ItemManager.Instance.GetRecipe(options.name);
                Recipe recipe = customRecipe != null ? customRecipe.Recipe : GetRecipeFromObjectDB(options.prefab);

                if (recipe == null)
                    throw new Exception("Could not find recipe of: " + options.name);

                switch (options.updateType)
                {
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

                        Piece.Requirement[] requirements = GetAsPieceRequirementArray(options.requirements, null, null);

                        if (requirements == null)
                        {
                            Jotunn.Logger.LogWarning("Cannot update recipe, requirements is null");
                            return;
                        }

                        recipe.m_resources = requirements;
                        break;
                    case RecipeUpdateType.CRAFTINGSTATION:
                        if (string.IsNullOrEmpty(options.craftingStation))
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
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + e);
            }
        }

        /**
         * Convert the recipe and return a RequirementConfig array
         */
        public static RequirementConfig[] GetAsRequirementConfigArray(string recipe, string upgradeRecipe, int? multiplier)
        {
            List<RequirementConfig> requirements = GetAsRequirementConfigList(recipe, upgradeRecipe, multiplier);
            return requirements?.ToArray();
        }

        /**
         * Convert the recipe and return a Piece.Requirement array
         */
        public static Piece.Requirement[] GetAsPieceRequirementArray(string recipe, string upgradeRecipe, int? multiplier)
        {
            List<RequirementConfig> list = GetAsRequirementConfigList(recipe, upgradeRecipe, multiplier);

            if (list == null)
                return null;

            List<Piece.Requirement> pieceList = new List<Piece.Requirement>();

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

        /**
         * Convert the recipe and return a RequirementConfig list
         */
        public static List<RequirementConfig> GetAsRequirementConfigList(string recipe, string upgradeRecipe, int? multiplier)
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

                if (upgradeRecipe != null && !string.IsNullOrWhiteSpace(upgradeRecipe) && multiplier != null)
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

        /**
         * Check wether the recipe & upgrade recipe are valid
         */
        public static bool IsConfigRecipeValid(string configRecipe, string upgradeRecipe)
        {
            try
            {
                bool isValid = true;

                string[] recipeEntries = configRecipe.Split(',');
                foreach (string entry in recipeEntries)
                {
                    if (!recipeEntryRegex.IsMatch(entry))
                    {
                        isValid = false;
                        Jotunn.Logger.LogWarning("Cannot resolve " + entry + " from the crafting recipe");
                    }
                }

                if (string.IsNullOrWhiteSpace(upgradeRecipe))
                    return isValid;

                string[] upgradeEntries = upgradeRecipe.Split(',');
                foreach (string entry in upgradeEntries)
                {
                    if (!recipeEntryRegex.IsMatch(entry))
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

        /**
         * Fallback lookup used when the recipe hasn't been registered with Jotunn's ItemManager yet
         */
        public static Recipe GetRecipeFromObjectDB(GameObject prefab)
        {
            if (prefab == null)
                return null;

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                throw new Exception("Could not find recipe in ObjectDB, no item drop for " + prefab.name + " was found");

            Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

            if (recipe == null)
                throw new Exception("Could not find recipe in ObjectDB, no recipe for " + prefab.name + " was found");

            return recipe;
        }
    }
}
