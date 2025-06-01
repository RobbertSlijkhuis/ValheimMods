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
        public ConfigEntry<int> maxStackSize;
        public ConfigEntry<float> health;
        public ConfigEntry<float> stamina;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> healthRegen;
        public ConfigEntry<float> burnTime;

        public void GenerateConfig(FoodConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Food.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Enable " + name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 13 }));
            enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = enable.Value,
                });
            };

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 12 }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 11 }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = description.Value,
                });
            };

            if (options.recipe != null)
            {
                craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                   new ConfigDescription("The crafting station the item can be crafted in",
                   new AcceptableValueList<string>(craftingStationOptions),
                   new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 10 }));
                craftingStation.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                    {
                        name = options.recipeName,
                        updateType = RecipeUpdateType.CRAFTINGSTATION,
                        craftingStation = craftingStation.Value,
                    });
                };

                minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), options.minStationLevel,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 9 }));
                minStationLevel.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                    {
                        name = options.recipeName,
                        updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                        requiredStationLevel = minStationLevel.Value,
                    });
                };

                recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 8 }));
                recipe.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                    {
                        name = options.recipeName,
                        updateType = RecipeUpdateType.RECIPE,
                        requirements = recipe.Value,
                    });
                };
            }

            weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), options.weight,
                new ConfigDescription("The amount of weight the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 7 }));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    weight = weight.Value,
                });
            };

            maxStackSize = Config.Bind(new ConfigDefinition(options.sectionName, "Max stack size"), options.maxStackSize,
                new ConfigDescription("The amount the item will stack", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 6 }));
            maxStackSize.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxStackSize = maxStackSize.Value,
                });
            };

            health = Config.Bind(new ConfigDefinition(options.sectionName, "Health"), (float)options.health,
                new ConfigDescription("The amount of health the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 5 }));
            health.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    health = health.Value,
                });
            };

            stamina = Config.Bind(new ConfigDefinition(options.sectionName, "Stamina"), (float)options.stamina,
                new ConfigDescription("The amount of stamina the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 4 }));
            stamina.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    stamina = stamina.Value,
                });
            };

            eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), (float)options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 3 }));
            eitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitr = eitr.Value,
                });
            };

            healthRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Health regen"), (float)options.healthRegen,
                new ConfigDescription("The amount of health regen the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 2 }));
            healthRegen.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    healthRegen = healthRegen.Value,
                });
            };

            burnTime = Config.Bind(new ConfigDefinition(options.sectionName, "Burn time"), (float)options.burnTime,
                new ConfigDescription("The amount of time it takes for the item to run out", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 1 }));
            burnTime.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    burnTime = burnTime.Value,
                });
            };
        }
    }
}
