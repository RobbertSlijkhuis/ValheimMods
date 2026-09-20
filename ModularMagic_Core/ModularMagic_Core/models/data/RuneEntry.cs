using ModularMagic_Core.Configs;
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneEntry
    {
        public ItemConfig config = new ItemConfig();
        public string craftingStation;
        public string description;
        public string id;
        public int minStationLevel;
        public string name;
        public GameObject prefab;
        public string recipe;
        public int[] tiers;
        public string type;
        public string value;

        public RuneEntry(RuneEntryOptions options)
        {
            this.craftingStation = options.craftingStation;
            this.description = options.description;
            this.id = options.id;
            this.minStationLevel = options.minStationLevel;
            this.name = options.name;
            this.prefab = options.prefab;
            this.recipe = options.recipe;
            this.tiers = options.tiers;
            this.type = options.type;
            this.value = options.value;

            ItemConfigOptions itemConfigOptions = new ItemConfigOptions(prefab, name, recipe)
            {
                description = description,
                craftingStation = craftingStation,
                minStationLevel = minStationLevel,
                recipeUpgrade = options.recipeUpgrade,
                recipeMultiplier = options.recipeMultiplier,
            };
            config.GenerateConfig(itemConfigOptions);
        }
    }
}
