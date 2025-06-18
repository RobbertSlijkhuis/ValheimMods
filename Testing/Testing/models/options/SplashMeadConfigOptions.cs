#nullable enable
using BepInEx.Configuration;
using UnityEngine;

namespace Testing.Models
{
    internal class SplashMeadConfigOptions
    {
        public GameObject prefab;
        public string sectionName;
        public string recipeName;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public int minStationLevel = 1;
        public string recipe;
        public int recipeAmount = 1;
        public int maxStackSize = 10;
        public float weight = 1f;
        public int duration = 300;
        public float radius = 4f;

        public SplashMeadConfigOptions(GameObject prefab, string name, string recipe)
        {
            this.prefab = prefab;
            this.name = name;
            sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            recipeName = $"Recipe_{prefab.name}";
        }
    }
}
