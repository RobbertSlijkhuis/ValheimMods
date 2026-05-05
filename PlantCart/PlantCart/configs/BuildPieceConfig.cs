using BepInEx.Configuration;
using PlantCart.Helpers;
using PlantCart.Models;
using PlantCart.Types;

namespace PlantCart.Configs
{
    internal class BuildPieceConfig
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
        public ConfigEntry<string> recipe;
        public ConfigEntry<int> inventoryColumns;
        public ConfigEntry<int> inventoryRows;
        public ConfigEntry<float> cartMass;
        public ConfigEntry<float> durabilityLoss;

        // Other
        private int entryCount = 100;

        public void GenerateConfig(BuildPieceConfigOptions options)
        {
            ConfigFile Config = PlantCart.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Wether the recipe for this piece is enabled", null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            enable.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateEnabled(options.prefab, enable.Value);
            };

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
                new ConfigDescription("The name of this build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateName(options.prefab, name.Value);
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of this build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateDescription(options.prefab, description.Value);
            };

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station required to place this piece",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            craftingStation.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateCraftingStation(options.prefab, craftingStation.Value);
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to place this build piece", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            recipe.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateRecipe(options.prefab, recipe.Value);
            };

            inventoryColumns = Config.Bind(new ConfigDefinition(options.sectionName, "Inventory columns"), options.inventoryColumns,
                new ConfigDescription("The width (amount of slots) of the inventory", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            inventoryColumns.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateInventorySize(options.prefab, inventoryColumns.Value, inventoryRows.Value);
            };

            inventoryRows = Config.Bind(new ConfigDefinition(options.sectionName, "Inventory rows"), options.inventoryRows,
                new ConfigDescription("The height (amount of slots) of the inventory", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            inventoryRows.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateInventorySize(options.prefab, inventoryColumns.Value, inventoryRows.Value);
            };

            cartMass = Config.Bind(new ConfigDefinition(options.sectionName, "Cart mass"), 300f,
                new ConfigDescription("The mass of the cart, affects speed and control", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            cartMass.SettingChanged += (obj, attr) =>
            {
                PieceHelper.UpdateMass(options.prefab, cartMass.Value);
            };

            durabilityLoss = Config.Bind(new ConfigDefinition(options.sectionName, "Durability loss"), 1f,
                new ConfigDescription("The amount of durability loss the cart takes on planting", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
