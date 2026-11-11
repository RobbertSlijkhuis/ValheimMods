#nullable enable

using ModularMagic_Core.Types;
using UnityEngine;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The options of RecipeHelper.UpdateRecipe. The recipe is found by name (a Jotunn custom recipe); when there is none,
    /// the recipe of the prefab in the ObjectDB is used.
    /// </summary>
    public class UpdateRecipeOptions
    {
        public GameObject? prefab = null;
        public string? name = null;
        public bool? enable = null;
        public int? amount = null;
        public string? craftingStation = null;
        public string? requirements = null;
        public int? requiredStationLevel = null;
        public RecipeUpdateType updateType = RecipeUpdateType.RECIPE;
        public string? upgradeRequirements = null;
        public int? upgradeMultiplier = null;
        public RecipeSnapShot? fromRecipe = null;
    }
}
