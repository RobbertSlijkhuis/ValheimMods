using ModularMagic_Core.Configs;
using ModularMagic_Core.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneEntryOptions
    {
        public List<string> allowedWeapons = new List<string>();
        public ItemConfig config = new ItemConfig();
        public string craftingStation = CraftingStationType.RuneTable;
        public string description;
        public string descriptionKey;
        public string id;
        public int minStationLevel = 1;
        public string name;
        public string nameKey;
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
