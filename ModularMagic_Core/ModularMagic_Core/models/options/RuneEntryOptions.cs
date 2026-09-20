using ModularMagic_Core.Configs;
using ModularMagic_Core.Types;
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneEntryOptions
    {
        public ItemConfig config = new ItemConfig();
        public string craftingStation = CraftingStationType.RuneTable;
        // The English description, also the text a weapon shows when it has no text for the rune
        public string description;
        // One of RuneId. Which weapons support the rune is decided by the weapon
        public string id;
        public int minStationLevel = 1;
        // The English name, also the text a weapon shows when it has no text for the rune
        public string name;
        public GameObject prefab;
        // The costs to craft the rune (level 1)
        public string recipe;
        // The items that are needed to upgrade the rune, multiplied per level
        public string recipeUpgrade;
        public int recipeMultiplier = 1;
        // The slot tier that is needed per level (index is the level minus one). The number of levels comes from the prefab
        public int[] tiers = new int[] { 1 };
        public string type;
        public string value;
    }
}
