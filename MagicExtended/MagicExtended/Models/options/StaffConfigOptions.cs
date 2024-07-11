#nullable enable
using UnityEngine;

namespace MagicExtended.Models
{
    internal class StaffConfigOptions
    {
        public GameObject? prefab = null;
        public string? sectionName = null;
        public string? recipeName = null;
        public string? cooldownStatusEffectName = null;

        public bool? enable = null;
        public string? name = null;
        public string? description = null;
        public string? craftingStation = null;
        public int? minStationLevel = null;
        public string? recipe = null;
        public string? recipeUpgrade = null;
        public int? recipeMultiplier = null;
        public int? maxQuality = null;
        public float? movementSpeed = null;
        public float? damageBlunt = null;
        public float? damageChop = null;
        public float? damageFire = null;
        public float? damageFrost = null;
        public float? damageLightning = null;
        public float? damagePickaxe = null;
        public float? damagePierce = null;
        public float? damageSlash = null;
        public float? damageSpirit = null;
        public int? blockArmor = null;
        public int? deflectionForce = null;
        public int? attackForce = null;
        public int? useEitr = null;
        public int? useEitrSecondary = null;
        public float? secondaryCooldown = null;
    }
}
