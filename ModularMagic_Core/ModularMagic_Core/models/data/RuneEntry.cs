using ModularMagic_Core.Configs;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneEntry
    {
        public List<string> allowedWeapons = new List<string>();
        public bool charged = true;
        public ItemConfig config = new ItemConfig();
        public string craftingStation;
        public string description;
        public string descriptionKey;
        public int level;
        public int minStationLevel;
        public string name;
        public string nameKey;
        public GameObject prefab;
        public string recipe;
        public string type;
        public string value;

        public RuneEntry(RuneEntryOptions options)
        {
            this.allowedWeapons = options.allowedWeapons;
            this.craftingStation = options.craftingStation;
            this.description = options.description;
            this.descriptionKey = options.descriptionKey;
            this.level = options.level;
            this.minStationLevel = options.minStationLevel;
            this.name = $"{options.name} {options.level}";
            this.nameKey = options.nameKey;
            this.prefab = options.prefab;
            this.recipe = options.recipe;
            this.type = options.type;
            this.value = options.value;

            ItemConfigOptions itemConfigOptions = new ItemConfigOptions(prefab, name, recipe)
            {
                description = description,
                craftingStation = craftingStation,
                minStationLevel = minStationLevel,
            };
            config.GenerateConfig(itemConfigOptions);
        }
    }
}
