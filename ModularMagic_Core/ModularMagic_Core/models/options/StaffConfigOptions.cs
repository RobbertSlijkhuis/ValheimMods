#nullable enable

using UnityEngine;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The options of the config of one staff of a staff mod, see StaffConfig. A stat that is null is not bound as a config entry, so a
    /// staff only has the entries of the stats that its mod gives a value.
    /// </summary>
    public class StaffConfigOptions
    {
        public GameObject prefab;
        public string sectionName;
        public string recipeName;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public int? minStationLevel = 1;
        public string? recipe;
        public string? recipeUpgrade;
        public int? recipeMultiplier = 1;

        public int? maxQuality;
        public float? weight;
        public float? maxDurability;
        public float? movementSpeed;

        public float? damageBlunt;
        public float? damageBluntPerLevel;
        public float? damageChop;
        public float? damageChopPerLevel;
        public float? damageFire;
        public float? damageFrost;
        public float? damageLightning;
        public float? damagePickaxe;
        public float? damagePickaxePerLevel;
        public float? damagePierce;
        public float? damagePiercePerLevel;
        public float? damagePoison;
        public float? damagePoisonPerLevel;
        public float? damageSlash;
        public float? damageSpirit;
        public float? damageSpiritPerLevel;

        public int? blockArmor;
        public int? deflectionForce;
        public int? attackForce;
        public int? useEitr;
        public float? projectileVelocity;
        public float? projectileAccuracy;
        public float? projectileBurst;

        // The section index is not used, the section is named after the staff
        public StaffConfigOptions(GameObject prefab, string name, string recipe, string upgradeRecipe, int sectionIndex)
            : this(prefab, name, recipe, upgradeRecipe)
        {
        }

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
