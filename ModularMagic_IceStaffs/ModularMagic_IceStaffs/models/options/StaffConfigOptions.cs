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
        public int minStationLevel = 1;
        public string? recipe;
        public string? recipeUpgrade;
        public int recipeMultiplier = 1;
        public int maxQuality = 4;
        public float movementSpeed = -0.05f;
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
            sectionName = $"{sectionIndex}. {name.Replace("'", "")}";
            this.recipe = recipe;
            recipeName = $"Recipe_{prefab.name}";
            recipeUpgrade = upgradeRecipe;
            cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
