using SplashMeads.Types;
using UnityEngine;

namespace SplashMeads.Models
{
    internal class UpdateRecipeOptions
    {
        public GameObject prefab = null;
        public string name = null;
        public bool? enable = null;
        public int? amount = null;
        public string craftingStation = null;
        public string requirements = null;
        public int? requiredStationLevel = null;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
    }
}
