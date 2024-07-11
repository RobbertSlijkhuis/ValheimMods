#nullable enable

namespace MagicExtended.Models
{
    internal class UpdateRecipeOptions
    {
        public string? craftingStation = null;
        public bool? enable = null;
        public string? name = null;
        public string? requirements = null;
        public int? requiredStationLevel = null;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
        public string? upgradeRequirements = null;
        public int? upgradeMultiplier = null;
    }
}
