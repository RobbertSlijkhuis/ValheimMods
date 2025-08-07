#nullable enable

using UnityEngine;
using DeathWizshAPI.Helpers.Types;

namespace DeathWizshAPI.Helpers.Models
{
    public class UpdateRecipeOptions
    {
        public GameObject? prefab;
        public string? name;
        public bool? enable;
        public int? amount;
        public string? craftingStation;
        public string? requirements;
        public int? requiredStationLevel;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
        public string? upgradeRequirements;
        public int? upgradeMultiplier;
        public RecipeSnapShot? fromRecipe;
    }
}
