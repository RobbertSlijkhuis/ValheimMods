#nullable enable
using UnityEngine;

namespace MagicExtended.Models
{
    internal class SpellbookConfigOptions
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
        public float? eitr = null;
        public float? eitrRegen = null;
    }
}
