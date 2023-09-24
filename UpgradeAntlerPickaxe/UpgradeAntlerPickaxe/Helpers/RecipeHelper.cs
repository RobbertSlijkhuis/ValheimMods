using BepInEx;
using Jotunn.Configs;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UpgradeAntlerPickaxe
{
    internal class RecipeHelper
    {
        private static readonly Regex nukeWhiteSpaceRegex = new Regex(@"\s+");
        private static readonly Regex recipeEntryRegex = new Regex(@"^([a-zA-Z]+:[0-9]+)$");

        /**
         * Convert the recipe and return a RequirementConfig array
         */
        public static RequirementConfig[] GetAsRequirementConfigArray(string configRecipe, string upgradeRecipe, int multiplier)
        {
            return GetAsRequirementConfigList(configRecipe, upgradeRecipe, multiplier).ToArray();
        }

        /**
         * Convert the recipe and return a Piece.Requirement array
         */
        public static Piece.Requirement[] GetAsPieceRequirementArray(string configRecipe, string upgradeRecipe, int multiplier)
        {
            try
            {
                List<RequirementConfig> list = GetAsRequirementConfigList(configRecipe, upgradeRecipe, multiplier);
                List<Piece.Requirement> pieceList = new List<Piece.Requirement>();

                foreach (RequirementConfig entry in list)
                {
                    GameObject item = PrefabManager.Instance.GetPrefab(entry.Item);

                    if (item == null)
                        throw new Exception("Could not find item, please check config!");

                    Piece.Requirement requirement = new Piece.Requirement();
                    requirement.m_resItem = item.GetComponent<ItemDrop>();
                    requirement.m_amount = entry.Amount;
                    requirement.m_amountPerLevel = entry.AmountPerLevel;
                    requirement.m_recover = entry.Recover;
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
        public static List<RequirementConfig> GetAsRequirementConfigList(string configRecipe, string upgradeRecipe, int multiplier)
        {
            try
            {
                configRecipe = nukeWhiteSpaceRegex.Replace(configRecipe, "");
                upgradeRecipe = nukeWhiteSpaceRegex.Replace(upgradeRecipe, "");

                if (!IsConfigRecipeValid(configRecipe, upgradeRecipe))
                    throw new Exception("Config is not valid, please check the values!");

                List<RequirementConfig> list = new List<RequirementConfig>();
                string[] recipeEntries = configRecipe.Trim().Split(',');

                foreach (string entry in recipeEntries)
                {
                    RequirementConfig requirement = new RequirementConfig();
                    string[] recipeEntryValues = entry.Split(':');
                    requirement.Item = recipeEntryValues[0];
                    requirement.Amount = Int32.Parse(recipeEntryValues[1]);
                    requirement.Recover = true;

                    if (!upgradeRecipe.IsNullOrWhiteSpace())
                    {
                        string[] upgradeEntries = upgradeRecipe.Trim().Split(',');
                        foreach (string upgradeEntry in upgradeEntries)
                        {
                            string[] upgradeEntryValues = upgradeEntry.Split(':');

                            if (requirement.Item == upgradeEntryValues[0])
                            {
                                requirement.AmountPerLevel = Int32.Parse(upgradeEntryValues[1]) * multiplier;
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
        public static bool IsConfigRecipeValid(string configRecipe, string upgradeRecipe)
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
                        Jotunn.Logger.LogError("Cannot resolve " + entry + " from the crafting recipe");
                    }
                }

                if (upgradeRecipe.IsNullOrWhiteSpace())
                    return isValid;

                string[] upgradeEntries = upgradeRecipe.Split(',');
                foreach (string entry in upgradeEntries)
                {
                    bool isMatch = recipeEntryRegex.IsMatch(entry);
                    if (!isMatch)
                    {
                        isValid = false;
                        Jotunn.Logger.LogError("Cannot resolve " + entry + " from the upgrade recipe");
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
    }
}
