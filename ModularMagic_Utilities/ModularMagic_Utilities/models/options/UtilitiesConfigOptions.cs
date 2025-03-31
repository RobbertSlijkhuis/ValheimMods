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
        public int? recipeMultiplier;
        public float? eitr;
        public float? eitrRegen;
        public float? elementalMagic;
        public float? bloodMagic;

        public float? demister;

        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public UtilitiesConfigOptions(GameObject prefab, string name, string recipe, int sectionIndex)
        {
            this.prefab = prefab;
            this.name = name;
            this.sectionName = $"{sectionIndex}. {name.Replace("'", "")}";
            this.recipe = recipe;
            this.recipeName = $"Recipe_{prefab.name}";
            this.magicStatusEffectName = $"{prefab.name}MagicStatusEffect";
            this.cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
