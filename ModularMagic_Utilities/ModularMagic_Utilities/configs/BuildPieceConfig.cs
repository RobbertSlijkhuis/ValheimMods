using BepInEx.Configuration;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using ModularMagic_Utilities.Types;

namespace ModularMagic_Utilities.Configs
{
    internal class BuildPieceConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { 
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter, 
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

        public static string[] lanterColorOptions = new string[] {
            LightPresetType.Red, LightPresetType.Orange, LightPresetType.Yellow, LightPresetType.Green, LightPresetType.LemonGreen,
            LightPresetType.LightBlue, LightPresetType.Blue, LightPresetType.Purple, LightPresetType.Pink, LightPresetType.White };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<string> recipe;
        public ConfigEntry<float> weatherZoneRadius;
        public ConfigEntry<bool> enableDome;
        public ConfigEntry<bool> enableProjector;
        public ConfigEntry<string> lightColorPreset;

        // Other
        private int entryCount = 5;

        public void GenerateConfig(BuildPieceConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Utilities.Instance.Config;

            if (options.weatherZoneRadius != null) entryCount = entryCount + 1;
            if (options.enableDome != null) entryCount = entryCount + 1;
            if (options.enableProjector != null) entryCount = entryCount + 1;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Wether the recipe for this item is enabled", null,
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
              new ConfigDescription("The name of the build piece", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of the build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station required to build this piece",
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

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to place this build piece", null,
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

            if (options.lightColorPreset != null)
            {
                lightColorPreset = Config.Bind(new ConfigDefinition(options.sectionName, "Effects color preset"), options.lightColorPreset,
                    new ConfigDescription("A preset of light colors applied to this piece (This will be updated when you turn off/on the piece)",
                    new AcceptableValueList<string>(lanterColorOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            }

            if (options.weatherZoneRadius != null)
            {
                weatherZoneRadius = Config.Bind(new ConfigDefinition(options.sectionName, "Weather zone radius"), (float)options.weatherZoneRadius,
                    new ConfigDescription("Wether this piece will render a dome (size determined by radius)",
                    new AcceptableValueRange<float>(10f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                weatherZoneRadius.SettingChanged += (obj, attr) =>
                {
                    if (weatherZoneRadius.Value <= 0f)
                        return;

                    UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    {
                        radius = weatherZoneRadius.Value,
                    });
                };
            }

            if (options.enableDome != null)
            {
                enableDome = Config.Bind(new ConfigDefinition(options.sectionName, "Enable dome"), (bool)options.enableDome,
                    new ConfigDescription("Wether this piece will render a dome (size determined by radius)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                enableDome.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    {
                        enableDome = enableDome.Value,
                    });
                };
            }

            if (options.enableProjector != null)
            {
                enableProjector = Config.Bind(new ConfigDefinition(options.sectionName, "Enable radius terrain projector"), (bool)options.enableProjector,
                    new ConfigDescription("Wether this piece will render the radius on terrain (like the workbench radius visual)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                enableProjector.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    {
                        enableProjector = enableProjector.Value,
                    });
                };
            }
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount;
        }
    }
}
