using BepInEx.Configuration;
using PlantCart.Helpers;
using PlantCart.Models;
using PlantCart.Types;

namespace PlantCart.Configs
{
    internal class MaterialConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] {
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter,
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

        public static string[] traderOptions = new string[] { TraderType.BogWitch, TraderType.Haldor, TraderType.Hildir };

        // The config fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> trader;
        public ConfigEntry<int> cost;
        public ConfigEntry<int> stack;
        public ConfigEntry<string> requiredGlobalKey;

        // Other
        private int entryCount = 100;

        public void GenerateConfig(MaterialConfigOptions options)
        {
            ConfigFile Config = PlantCart.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), (bool)options.enable,
               new ConfigDescription("Enable " + options.name, null,
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
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    description = description.Value,
                });
            };

            trader = Config.Bind(new ConfigDefinition(options.sectionName, "Trader"), TraderType.Haldor,
                new ConfigDescription("The trader where this item is available",
                new AcceptableValueList<string>(traderOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            trader.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateTrader(new UpdateTraderOptions()
                {
                    trader = trader.Value,
                });
            };

            cost = Config.Bind(new ConfigDefinition(options.sectionName, "Cost"), 300,
                new ConfigDescription("The amount this item cost at the trader", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            cost.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateTrader(new UpdateTraderOptions()
                {
                    cost = cost.Value,
                });
            };

            stack = Config.Bind(new ConfigDefinition(options.sectionName, "Stack"), 1,
                new ConfigDescription("The stack size of this item when buying from a trader", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            stack.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateTrader(new UpdateTraderOptions()
                {
                    stack = stack.Value,
                });
            };

            requiredGlobalKey = Config.Bind(new ConfigDefinition(options.sectionName, "Global Key"), GlobalKeyType.DefeatedModer,
                new ConfigDescription("The global key required for this item to show up at trader (boss killed)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            requiredGlobalKey.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateTrader(new UpdateTraderOptions()
                {
                    globalKey = requiredGlobalKey.Value,
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
