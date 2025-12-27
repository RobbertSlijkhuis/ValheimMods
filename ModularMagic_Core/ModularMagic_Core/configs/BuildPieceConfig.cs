using BepInEx.Configuration;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using static Jotunn.Utils.GameConstants;
using System.Security.Policy;

namespace ModularMagic_Core.Configs
{
    internal class BuildPieceConfig
    {
        public static string[] craftingStationOptions = new string[] { 
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter, 
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable, CraftingStationType.RuneTable };

        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<string> recipe;

        private int entryCount = 11;

        public void GenerateConfig(BuildPieceConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Core.Instance.Config;

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
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of this build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
