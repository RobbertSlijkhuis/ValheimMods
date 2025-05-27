#nullable enable
using UnityEngine;

namespace ModularMagic_Armors.Models
{
    internal class ArmorConfigOptions
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
        public string? recipeUpgrade;
        public int recipeMultiplier = 1;
        public float armor = 1f;
        public float armorPerLevel = 2f;
        public float weight = 1f;
        public float maxDurability = 1000f;
        public int maxQuality = 4;
        public float movementSpeed = 0f;
        public float eitr = 0f;
        public float eitrRegen = 0f;
        public float elementalMagic = 0f;
        public float bloodMagic = 0f;
        public float? demister;

        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public ArmorConfigOptions(GameObject prefab, string name, string recipe, string recipeUpgrade)
        {
            this.prefab = prefab;
            this.name = name;
            sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            recipeName = $"Recipe_{prefab.name}";
            this.recipeUpgrade = recipeUpgrade;
            magicStatusEffectName = $"{prefab.name}MagicStatusEffect";
            cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
