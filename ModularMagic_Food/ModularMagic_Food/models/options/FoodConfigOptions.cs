#nullable enable
using UnityEngine;

namespace ModularMagic_Food.Models
{
    internal class FoodConfigOptions
    {
        public GameObject prefab;
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

        public FoodConfigOptions(GameObject prefab, string name, string? recipe = null)
        {
            this.prefab = prefab;
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
