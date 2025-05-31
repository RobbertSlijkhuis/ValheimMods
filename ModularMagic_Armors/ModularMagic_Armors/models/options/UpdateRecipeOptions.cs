#nullable enable

using UnityEngine;

namespace ModularMagic_Armors.Models
{
    internal class UpdateRecipeOptions
    {
        public GameObject? prefab = null;
        public string? name = null;
        public bool? enable = null;
        public string? craftingStation = null;
        public string? requirements = null;
        public int? requiredStationLevel = null;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
        public string? upgradeRequirements = null;
        public int? upgradeMultiplier = null;
        public RecipeSnapShot? fromRecipe = null;
    }
}
