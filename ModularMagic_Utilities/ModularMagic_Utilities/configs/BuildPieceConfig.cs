using BepInEx.Configuration;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using ModularMagic_Utilities.Types;
using static Jotunn.Utils.GameConstants;
using System.Security.Policy;

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
        public ConfigEntry<string> lightColorPreset;
        public ConfigEntry<float> weatherZoneRadius;
        public ConfigEntry<bool> enableDomeVisual;
        public ConfigEntry<float> domeSmoothness;
        public ConfigEntry<bool> enableDomeParticles;
        public ConfigEntry<float> domeParticlesAmount;

        // Other
        private int entryCount = 11;

        public void GenerateConfig(BuildPieceConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Utilities.Instance.Config;

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
              new ConfigDescription("The name of this build piece", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of this build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station required to place this piece",
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
                    new ConfigDescription("Light preset applied to this piece, updates when the piece is toggled off and on",
                    new AcceptableValueList<string>(lanterColorOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            }

            if (options.weatherZoneRadius != null)
            {
                weatherZoneRadius = Config.Bind(new ConfigDefinition(options.sectionName, "Weather zone radius"), (float)options.weatherZoneRadius,
                    new ConfigDescription("Default radius for the weather zone",
                    new AcceptableValueRange<float>(10f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                weatherZoneRadius.SettingChanged += (obj, attr) =>
                {
                    if (weatherZoneRadius.Value <= 0f)
                        return;

                    //UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    //{
                    //    radius = weatherZoneRadius.Value,
                    //});
                };
            }

            if (options.enableDomeVisual != null)
            {
                enableDomeVisual = Config.Bind(new ConfigDefinition(options.sectionName, "Dome sphere visibility"), (bool)options.enableDomeVisual,
                    new ConfigDescription("Specifies whether the dome sphere is visible by default", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                enableDomeVisual.SettingChanged += (obj, attr) =>
                {
                    //UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    //{
                    //    enableDomeVisual = enableDomeVisual.Value,
                    //});
                };
            }

            if (options.enableDomeParticles != null)
            {
                enableDomeParticles = Config.Bind(new ConfigDefinition(options.sectionName, "Dome particles visibility"), (bool)options.enableDomeParticles,
                    new ConfigDescription("Specifies whether the dome particles are visible by default", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                enableDomeParticles.SettingChanged += (obj, attr) =>
                {
                    //UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    //{
                    //    enableDomeParticles = enableDomeParticles.Value,
                    //});
                };
            }

            if (options.domeParticlesAmount != null)
            {
                domeParticlesAmount = Config.Bind(new ConfigDefinition(options.sectionName, "Dome particles amount"), (float)options.domeParticlesAmount,
                    new ConfigDescription("Default number of particles rendered by the dome",
                    new AcceptableValueRange<float>(0f, 1000f),
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                domeParticlesAmount.SettingChanged += (obj, attr) =>
                {
                    //UpdateHelper.UpdateWeatherZone(options.prefab, new UpdateWeatherZoneOptions()
                    //{
                    //    domeParticlesAmount = domeParticlesAmount.Value,
                    //});
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
