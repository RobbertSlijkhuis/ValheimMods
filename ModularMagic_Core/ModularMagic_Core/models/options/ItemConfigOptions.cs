#nullable enable
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class ItemConfigOptions
    {
        public GameObject? prefab;
        public string? sectionName;
        public string? recipeName;
        public bool enable = true;
        public string? name;
        public string? description;
        public string? craftingStation;
        public int minStationLevel = 1;
        public string? recipe;
        public string? recipeUpgrade;
        public int recipeMultiplier = 1;

        public ItemConfigOptions(GameObject prefab, string name, string recipe)
        {
            this.prefab = prefab;
            this.name = name;
            this.sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            this.recipeName = $"Recipe_{prefab.name}";
        }
    }
}
