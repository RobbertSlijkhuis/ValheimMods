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
        public int level = 1;
        public int minStationLevel = 1;
        public string name;
        public string nameKey;
        public GameObject prefab;
        public string recipe;
        public string type;
        public string value;
    }
}
