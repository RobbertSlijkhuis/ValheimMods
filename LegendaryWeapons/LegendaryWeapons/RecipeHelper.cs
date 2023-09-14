using BepInEx;
using HarmonyLib;
using System;
using Jotunn.Configs;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpecialWeapons
{
    internal class RecipeHelper
    {
        public List<ConfigRecipeRequirement> requirements = new List<ConfigRecipeRequirement>();
        public CraftingStation craftingStation { get; set; }
        public string recipe { get; set; }
        public string upgrade { get; set; }

        public RecipeHelper(string configRecipe, string upgradeRecipe)
        {
            if (!configRecipe.IsNullOrWhiteSpace())
            {
                this.recipe = configRecipe;
                this.upgrade = upgradeRecipe;
                this.requirements = this.ConvertStringToRequirements(configRecipe, upgradeRecipe);
            }
            else
            {
                Jotunn.Logger.LogError("Could not initialise recipe helper class!");
            }
        }

        private List<ConfigRecipeRequirement> ConvertStringToRequirements(string configRecipe, string upgradeRecipe)
        {
            List<ConfigRecipeRequirement> list = new List<ConfigRecipeRequirement>();
            string[] recipeEntries = configRecipe.Split(',');

            foreach (string entry in recipeEntries)
            {
                ConfigRecipeRequirement requirement = new ConfigRecipeRequirement();
                string[] recipeEntryValues = entry.Split(':');
                requirement.material = recipeEntryValues[0];
                requirement.amount = Int32.Parse(recipeEntryValues[1]);

                if (!upgradeRecipe.IsNullOrWhiteSpace())
                {
                    string[] upgradeEntries = upgradeRecipe.Split(',');
                    foreach (string upgradeEntry in upgradeEntries)
                    {
                        string[] upgradeEntryValues = upgradeEntry.Split(':');

                        if (requirement.material == upgradeEntryValues[0])
                        {
                            requirement.amountPerLevel = Int32.Parse(upgradeEntryValues[1]);
                            break;
                        }
                    }
                }

                list.Add(requirement);
            }

            return list;
        }

        public bool IsConfigRecipeValid()
        {
            bool isValid = true;
            string regex = @"/([a-zA-Z]+:[0-9]+)/g";

            string[] recipeEntries = this.recipe.Split(',');
            foreach (string entry in recipeEntries)
            {
                Match m = Regex.Match(entry, regex);
                if (m.Success)
                {
                    isValid = false;
                    Jotunn.Logger.LogError("Cannot resolve " + entry + " from the crafting recipe");
                }
            }

            string[] upgradeEntries = this.upgrade.Split(',');
            foreach (string entry in upgradeEntries)
            {
                Match m = Regex.Match(entry, regex);
                if (m.Success)
                {
                    isValid = false;
                    Jotunn.Logger.LogError("Cannot resolve " + entry + " from the upgrade recipe");
                }
            }

            return isValid;
        }

        public RequirementConfig[] GetAsRequirementConfig()
        {
            RequirementConfig[] array = new RequirementConfig[this.requirements.Count];
            foreach (ConfigRecipeRequirement configRecipeRequirement in this.requirements)
            {
                RequirementConfig requirement = new RequirementConfig();
                requirement.Item = configRecipeRequirement.material;
                requirement.Amount = configRecipeRequirement.amount;
                requirement.AmountPerLevel = configRecipeRequirement.amountPerLevel;
                requirement.Recover = false;
                array.AddToArray(requirement);
            }

            return array;
        }
    }

    internal class ConfigRecipeRequirement
    {
        public string material;
        public int amount;
        public int amountPerLevel = 0;
    }
}
