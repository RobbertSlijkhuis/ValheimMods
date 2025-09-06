#nullable enable
using UnityEngine;

namespace ModularMagic_EarthStaffs.Models
{
    internal class StaffConfigOptions
    {
        public GameObject prefab;
        public string sectionName;
        public string recipeName;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public int minStationLevel = 1;
        public string? recipe;
        public string? recipeUpgrade;
        public int recipeMultiplier = 1;
        public string? selectedSecondaryAttack;
        public float damageBlunt = 0f;
        public float? damageChop;
        public float? damagePickaxe;
        public float? damagePoison;
        public float damageSpirit = 0f;
        public float damageBluntPerLevel = 0f;
        public float? damageChopPerLevel;
        public float? damagePickaxePerLevel;
        public float? damagePoisonPerLevel;
        public float damageSpiritPerLevel = 0f;
        public int useEitr = 10;
        public float projectileVelocity = 15f;
        public float projectileAccuracy = 1f;
        public float? projectileBurst;
        public float weight = 0.3f;
        public float maxDurability = 200f;
        public int maxQuality = 4;
        public float movementSpeed = -0.05f;
        public int blockArmor = 0;
        public int deflectionForce = 20;
        public int attackForce = 20;

        public StaffConfigOptions(GameObject prefab, string name, string recipe, string upgradeRecipe)
        {
            this.prefab = prefab;
            this.name = name;
            this.sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            this.recipeName = $"Recipe_{prefab.name}";
            this.recipeUpgrade = upgradeRecipe;
        }
    }
}
