using BepInEx.Configuration;
using MagicExtended.Configs;
using MagicExtended.Helpers;
using MagicExtended.StatusEffects;

namespace MagicExtended.Models
{
    internal class SpellbookConfig
    {
        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> eitrRegen;

        public void GenerateConfig(SpellbookConfigOptions options)
        {
            ConfigFile Config = MagicExtended.Instance.Config;

            this.enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), (bool)options.enable,
               new ConfigDescription("Enable " + this.name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = this.enable.Value,
                });
            };

            this.name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.name.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = this.name.Value,
                });
            };

            this.description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.description.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = this.description.Value,
                });
            }; ;

            this.craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = this.craftingStation.Value,
                });
            };

            this.minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), (int)options.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = this.minStationLevel.Value,
                });
            };

            this.recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = this.recipe.Value,
                });
            };

            this.eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), (float)options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.eitr.SettingChanged += (obj, attr) =>
            {
                if (this.eitr.Value < 0f) return;

                ItemDrop spellBookDrop = options.prefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr statusEffect = (StatusEffectWithEitr)spellBookDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(this.eitr.Value);
            };

            this.eitrRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr regen"), (float)options.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.eitrRegen.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitrRegen = this.eitrRegen.Value,
                });
            };
        }
    }
}
