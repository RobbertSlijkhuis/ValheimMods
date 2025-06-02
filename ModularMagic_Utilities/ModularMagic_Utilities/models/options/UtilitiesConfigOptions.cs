#nullable enable
using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class UtilitiesConfigOptions
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
        public float weight = 1f;
        public float eitr = 0f;
        public float eitrRegen = 0f;
        public float elementalMagic = 0f;
        public float bloodMagic = 0f;
        public float? demister;

        public string? lightColorPreset;
        public float? lightIntensity;
        public float? lightRange;

        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public UtilitiesConfigOptions(GameObject prefab, string name, string recipe)
        {
            this.prefab = prefab;
            this.name = name;
            sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            recipeName = $"Recipe_{prefab.name}";
            magicStatusEffectName = $"{prefab.name}MagicStatusEffect";
            cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
