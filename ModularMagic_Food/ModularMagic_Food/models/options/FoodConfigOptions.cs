#nullable enable
using UnityEngine;

namespace ModularMagic_Food.Models
{
    internal class FoodConfigOptions
    {
        public GameObject prefab;
        public GameObject? pickablePrefab;
        public string sectionName;
        public string? recipeName = null;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public int minStationLevel = 1;
        public string? recipe;
        public float weight = 1;
        public int maxStackSize = 50;
        public float health = 0;
        public float healthRegen = 0;
        public float stamina = 0;
        public float eitr = 0;
        public float burnTime = 600;
        public int? groupMin;
        public int? groupMax;
        public float? scaleMin;
        public float? scaleMax;
        public float? minAltitude;
        public float? maxAltitude;
        public int? amount;
        public float? respawnTimeMinutes;

        public FoodConfigOptions(GameObject prefab, string name, string? recipe = null, GameObject? pickablePrefab = null)
        {
            this.prefab = prefab;
            this.pickablePrefab = pickablePrefab;
            this.name = name;
            sectionName = $"{name.Replace("'", "")}";

            if (recipe != null)
            {
                this.recipe = recipe;
                recipeName = $"Recipe_{prefab.name}";
            }
        }
    }
}
