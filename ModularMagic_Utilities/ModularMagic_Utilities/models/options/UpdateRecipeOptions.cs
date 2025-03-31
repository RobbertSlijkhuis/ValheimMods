#nullable enable

namespace ModularMagic_Utilities.Models
{
    internal class UpdateRecipeOptions
    {
        public string? name = null;
        public bool? enable = null;
        public string? craftingStation = null;
        public string? requirements = null;
        public int? requiredStationLevel = null;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
        public string? upgradeRequirements = null;
        public int? upgradeMultiplier = null;
    }
}
