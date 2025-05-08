#nullable enable
using UnityEngine;

namespace ModularMagic_IceStaffs.Models
{
    internal class StaffConfigOptions
    {
        public GameObject prefab;
        public string sectionName;
        public string recipeName;
        public string? cooldownStatusEffectName;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public int? minStationLevel = 1;
        public string? recipe;
        public string? recipeUpgrade;
        public int? recipeMultiplier;
        public int? maxQuality;
        public float? movementSpeed;
        public float? damageBlunt;
        public float? damageChop;
        public float? damageFire;
        public float? damageFrost;
        public float? damageLightning;
        public float? damagePickaxe;
        public float? damagePierce;
        public float? damageSlash;
        public float? damageSpirit;
        public int? blockArmor;
        public int? deflectionForce;
        public int? attackForce;
        public int? useEitr;
        public int? useEitrSecondary;
        public float? secondaryCooldown;

        public StaffConfigOptions(GameObject prefab, string name, string recipe, string upgradeRecipe, int sectionIndex)
        {
            this.prefab = prefab;
            this.name = name;
            this.sectionName = $"{sectionIndex}. {name.Replace("'", "")}";
            this.recipe = recipe;
            this.recipeName = $"Recipe_{prefab.name}";
            this.recipeUpgrade = upgradeRecipe;
            this.cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
