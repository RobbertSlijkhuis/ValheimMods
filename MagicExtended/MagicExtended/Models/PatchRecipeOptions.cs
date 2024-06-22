namespace MagicExtended.Models
{
    internal class PatchRecipeOptions
    {
        public string craftingStation = null;
        public bool enable;
        public string name = null;
        public string requirements = null;
        public int requiredStationLevel = -1;
        public RecipeUpdateType updateType = RecipeUpdateType.Recipe;
        public string upgradeRequirements = null;
        public int upgradeMultiplier = -1;
    }
}
