using BepInEx.Configuration;
using SplashMeads.Helpers;
using SplashMeads.Models;
using SplashMeads.Types;

namespace SplashMeads.Configs
{
    internal class SplashMeadConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { 
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter, 
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<int> recipeAmount;
        public ConfigEntry<float> weight;
        public ConfigEntry<int> maxStackSize;
        public ConfigEntry<int> duration;
        public ConfigEntry<float> radius;

        // Other
        private int entryCount = 10;

        public void GenerateConfig(SplashMeadConfigOptions options)
        {
            ConfigFile Config = SplashMeads.Instance.Config;

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
                new ConfigDescription("The name of the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    description = description.Value,
                });
            }; ;

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

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level"), options.minStationLevel,
                new ConfigDescription("The required station level to craft this item", null,
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

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to craft this item", null,
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

            recipeAmount = Config.Bind(new ConfigDefinition(options.sectionName, "Amount crafted"), options.recipeAmount,
                new ConfigDescription("The amount of this item you'll get when crafting it", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            recipeAmount.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.AMOUNT,
                    amount = recipeAmount.Value,
                });
            };

            weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), options.weight,
                new ConfigDescription("The weight applied to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    weight = weight.Value,
                });
            };

            maxStackSize = Config.Bind(new ConfigDefinition(options.sectionName, "Max stack size"), options.maxStackSize,
                new ConfigDescription("The amount the item will stack",
                new AcceptableValueRange<int>(10, 50),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            maxStackSize.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    maxStackSize = maxStackSize.Value,
                });
            };

            duration = Config.Bind(new ConfigDefinition(options.sectionName, "Duration"), options.duration,
                new ConfigDescription("The duration applied to the status effect (seconds)",
                new AcceptableValueRange<int>(60, 600),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            duration.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateDuration(options.prefab, duration.Value);
            };

            radius = Config.Bind(new ConfigDefinition(options.sectionName, "Radius"), options.radius,
                new ConfigDescription("The radius of the splash and applying status effect (between 2 - 10)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            radius.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateRadius(options.prefab, radius.Value);
            };
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
