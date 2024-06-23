using BepInEx.Configuration;
using MagicExtended.Configs;
using MagicExtended.Helpers;
using MagicExtended.StatusEffects;
using UnityEngine;

namespace MagicExtended.Models
{
    internal class SpellbookConfig
    {
        // Required variables
        public string sectionName;
        public string recipeName;
        public GameObject prefab;

        // Default values
        public bool enable;
        public string name;
        public string description;
        public string craftingStation;
        public int minStationLevel;
        public string recipe;
        public float eitr;
        public float eitrRegen;

        // The config fields to generate
        public ConfigEntry<bool> configEnable;
        public ConfigEntry<string> configName;
        public ConfigEntry<string> configDescription;
        public ConfigEntry<string> configCraftingStation;
        public ConfigEntry<int> configMinStationLevel;
        public ConfigEntry<string> configRecipe;
        public ConfigEntry<float> configEitr;
        public ConfigEntry<float> configEitrRegen;

        public void GenerateConfig()
        {
            ConfigFile Config = MagicExtended.Instance.Config;

            this.configEnable = Config.Bind(new ConfigDefinition(this.sectionName, "Enable"), this.enable,
               new ConfigDescription("Enable " + this.name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configEnable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = this.configEnable.Value,
                });
            };

            this.configName = Config.Bind(new ConfigDefinition(this.sectionName, "Name"), this.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configName.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    name = this.configName.Value,
                });
            };

            this.configDescription = Config.Bind(new ConfigDefinition(this.sectionName, "Description"), this.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configDescription.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    description = this.configDescription.Value,
                });
            }; ;

            this.configCraftingStation = Config.Bind(new ConfigDefinition(this.sectionName, "Crafting station"), this.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configCraftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.CraftingStation,
                    craftingStation = this.configCraftingStation.Value,
                });
            };

            this.configMinStationLevel = Config.Bind(new ConfigDefinition(this.sectionName, "Required station level to craft"), this.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configMinStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.MinRequiredStationLevel,
                    requiredStationLevel = this.configMinStationLevel.Value,
                });
            };

            this.configRecipe = Config.Bind(new ConfigDefinition(this.sectionName, "Crafting costs"), this.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configRecipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Recipe,
                    requirements = this.configRecipe.Value,
                });
            };

            this.configEitr = Config.Bind(new ConfigDefinition(this.sectionName, "Eitr"), this.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configEitr.SettingChanged += (obj, attr) =>
            {
                if (this.configEitr.Value < 0f) return;

                ItemDrop spellBookDrop = prefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr statusEffect = (StatusEffectWithEitr)spellBookDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(this.configEitr.Value);
            };

            this.configEitrRegen = Config.Bind(new ConfigDefinition(this.sectionName, "Eitr regen"), this.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configEitrRegen.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    eitrRegen = this.configEitrRegen.Value,
                });
            };
        }
    }
}
