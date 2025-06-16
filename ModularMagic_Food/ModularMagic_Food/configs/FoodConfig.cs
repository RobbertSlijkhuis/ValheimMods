using BepInEx.Configuration;
using ModularMagic_Food.Helpers;
using ModularMagic_Food.Models;

namespace ModularMagic_Food.Configs
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
        public ConfigEntry<int> groupMin;
        public ConfigEntry<int> groupMax;
        public ConfigEntry<float> scaleMin;
        public ConfigEntry<float> scaleMax;
        public ConfigEntry<float> minAltitude;
        public ConfigEntry<float> maxAltitude;
        public ConfigEntry<int> amount;
        public ConfigEntry<float> respawnTimeMinutes;

        // Other
        private int entryCount = 21;

        public void GenerateConfig(FoodConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Food.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Enable " + name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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
              new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    description = description.Value,
                });
            };

            if (options.recipe != null)
            {
                craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                   new ConfigDescription("The crafting station the item can be crafted in",
                   new AcceptableValueList<string>(craftingStationOptions),
                   new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    weight = weight.Value,
                });
            };

            maxStackSize = Config.Bind(new ConfigDefinition(options.sectionName, "Max stack size"), options.maxStackSize,
                new ConfigDescription("The amount the item will stack", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            maxStackSize.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    maxStackSize = maxStackSize.Value,
                });
            };

            health = Config.Bind(new ConfigDefinition(options.sectionName, "Health"), (float)options.health,
                new ConfigDescription("The amount of health the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            health.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    health = health.Value,
                });
            };

            stamina = Config.Bind(new ConfigDefinition(options.sectionName, "Stamina"), (float)options.stamina,
                new ConfigDescription("The amount of stamina the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            stamina.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    stamina = stamina.Value,
                });
            };

            eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), (float)options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            eitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    eitr = eitr.Value,
                });
            };

            healthRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Health regen"), (float)options.healthRegen,
                new ConfigDescription("The amount of health regen the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            healthRegen.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    healthRegen = healthRegen.Value,
                });
            };

            burnTime = Config.Bind(new ConfigDefinition(options.sectionName, "Burn time"), (float)options.burnTime,
                new ConfigDescription("The amount of time it takes for the item to run out", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            burnTime.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDrop(options.prefab, new UpdateItemDropOptions()
                {
                    burnTime = burnTime.Value,
                });
            };

            if (options.groupMin != null)
            {
                groupMin = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Minimum in a group"), (int)options.groupMin,
                    new ConfigDescription("The minimal amount of pickable mushrooms in a group", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            if (options.groupMax != null)
            {
                groupMax = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Maximum in a group"), (int)options.groupMax,
                    new ConfigDescription("The maximum amount of pickable mushrooms in a group", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            //if (options.scaleMin != null)
            //{
            //    scaleMin = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Minimum size"), (float)options.scaleMin,
            //        new ConfigDescription("The minimum size of this pickable mushroom",
            //        new AcceptableValueRange<float>(0.1f, 10f),
            //        new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            //}

            //if (options.scaleMax != null)
            //{
            //    scaleMax = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Maximum size"), (float)options.scaleMax,
            //        new ConfigDescription("The maxumum size of this pickable mushroom",
            //        new AcceptableValueRange<float>(0.1f, 10f),
            //        new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            //}

            if (options.minAltitude != null)
            {
                minAltitude = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Minimum altitude"), (float)options.minAltitude,
                    new ConfigDescription("The minimum altitude this pickable mushroom will spawn", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            if (options.maxAltitude != null)
            {
                maxAltitude = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Maximum altitude"), (float)options.maxAltitude,
                    new ConfigDescription("The maximum altitude this pickable mushroom will spawn", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            if (options.amount != null)
            {
                amount = Config.Bind(new ConfigDefinition(options.sectionName, (options.pickablePrefab != null ? "(Pickable) " : "") + "Amount"), (int)options.amount,
                    new ConfigDescription("The amount of items" + (options.pickablePrefab != null ? " this pickable mushroom will drop when picked" : " that will be crafted"), null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                amount.SettingChanged += (obj, attr) =>
                {
                    if (options.pickablePrefab != null)
                    {
                        UpdateHelper.UpdatePickable(options.pickablePrefab, new UpdatePickableOptions()
                        {
                            amount = amount.Value,
                        });
                    }
                    else
                    {
                        RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                        {
                            name = options.recipeName,
                            updateType = RecipeUpdateType.AMOUNT,
                            amount = amount.Value,
                        });
                    }
                };
            }

            if (options.respawnTimeMinutes != null)
            {
                respawnTimeMinutes = Config.Bind(new ConfigDefinition(options.sectionName, "(Pickable) Respawn time minutes"), (float)options.respawnTimeMinutes,
                    new ConfigDescription("The amount of time before this pickable mushroom is pickable again", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                respawnTimeMinutes.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdatePickable(options.pickablePrefab, new UpdatePickableOptions()
                    {
                        respawnTimeMinutes = respawnTimeMinutes.Value,
                    });
                };
            }
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
