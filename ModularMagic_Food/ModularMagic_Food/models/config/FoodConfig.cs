using BepInEx.Configuration;
using ModularMagic_Food.Helpers;

namespace ModularMagic_Food.Models
{
    internal class FoodConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> health;
        public ConfigEntry<float> stamina;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> healthRegen;
        public ConfigEntry<float> burnTime;

        public void GenerateConfig(FoodConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Food.Instance.Config;

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
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = this.name.Value,
                });
            };

            this.description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = this.description.Value,
                });
            };

            if (options.recipe != null)
            {
                this.craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                   new ConfigDescription("The crafting station the item can be created in",
                   new AcceptableValueList<string>(craftingStationOptions),
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
                    Jotunn.Logger.LogWarning("demister: " + this.minStationLevel.Value);
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
            }

            this.weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), (float)options.weight,
                new ConfigDescription("The amount of weight the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    weight = this.weight.Value,
                });
            };

            this.health = Config.Bind(new ConfigDefinition(options.sectionName, "Health"), (float)options.health,
                new ConfigDescription("The amount of health the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.health.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    health = this.health.Value,
                });
            };

            this.stamina = Config.Bind(new ConfigDefinition(options.sectionName, "Stamina"), (float)options.stamina,
                new ConfigDescription("The amount of stamina the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.stamina.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    stamina = this.stamina.Value,
                });
            };

            this.eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), (float)options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.eitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitr = this.eitr.Value,
                });
            };

            this.healthRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Health regen"), (float)options.healthRegen,
                new ConfigDescription("The amount of health regen the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.healthRegen.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    healthRegen = this.healthRegen.Value,
                });
            };

            this.burnTime = Config.Bind(new ConfigDefinition(options.sectionName, "Burn time"), (float)options.burnTime,
                new ConfigDescription("The amount of time it takes for the item to run out", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.burnTime.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    burnTime = this.burnTime.Value,
                });
            };
        }
    }
}
