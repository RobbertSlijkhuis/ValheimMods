#nullable enable

using UnityEngine;
using DeathWizshAPI.Models;
using DeathWizshAPI.Types;

namespace DeathWizshAPI.Models
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
