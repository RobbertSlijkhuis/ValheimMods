#nullable enable
using UnityEngine;

namespace ModularMagic_BloodMagic.Models
{
    internal class WeaponConfigOptions
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

        public float? damageBlunt;
        public float? damageChop;
        public float? damageFire;
        public float? damageFrost;
        public float? damageGeneral;
        public float? damageLightning;
        public float? damagePickaxe;
        public float? damagePierce;
        public float? damagePoison;
        public float? damageSlash;
        public float? damageSpirit;

        public float? damageBluntPerLevel;
        public float? damageChopPerLevel;
        public float? damageFirePerLevel;
        public float? damageFrostPerLevel;
        public float? damageGeneralPerLevel;
        public float? damageLightningPerLevel;
        public float? damagePiercePerLevel;
        public float? damagePickaxePerLevel;
        public float? damagePoisonPerLevel;
        public float? damageSlashPerLevel;
        public float? damageSpiritPerLevel;

        public int blockArmor = 0;
        public int deflectionForce = 20;
        public int attackForce = 20;

        public float maxDurability = 200f;
        public int maxQuality = 4;
        public float movementSpeed = -0.05f;
        public float weight = 0.3f;

        public int attackEitr = 10;
        public float projectileVelocity = 15f;
        public float projectileAccuracy = 1f;
        public float? projectileBurst;

        public string? selectedSecondaryAttack;

        public WeaponConfigOptions(GameObject prefab, string name, string recipe, string upgradeRecipe)
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
