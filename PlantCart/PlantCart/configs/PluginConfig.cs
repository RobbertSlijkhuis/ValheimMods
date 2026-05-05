using PlantCart.Models;
using PlantCart.Types;
using System;

namespace PlantCart.Configs
{
    internal static class PluginConfig
    {

        private static string piece1Name = "Plant Cart";
        private static string piece1Recipe = $"Wood:20, Copper:6, Silver:10, {PlantCart.Instance.prefabs.Plow.name}:2";
        public static BuildPieceConfig piece1 = new BuildPieceConfig();

        public static string material1Name = "Rusted Plow";
        public static MaterialConfig rustedPlow = new MaterialConfig();

        public static void Init()
        {
            InitPiece1Config();
            InitRustedPlowConfig();
        }

        public static void InitPiece1Config()
        {
            try
            {
                BuildPieceConfigOptions options = new BuildPieceConfigOptions(PlantCart.Instance.prefabs.PlantCart, piece1Name, piece1Recipe)
                {
                    description = "Plant crops effortlessly with a cart that sows seeds as you go.",
                    craftingStation = CraftingStationType.Workbench,
                };
                piece1.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + piece1Name + " config: " + e);
            }
        }

        private static void InitRustedPlowConfig()
        {
            try
            {
                MaterialConfigOptions options = new MaterialConfigOptions(PlantCart.Instance.prefabs.Plow, material1Name, null)
                {
                    description = "An old rusted plow, maybe it can be restored and used for something?",
                    craftingStation = null,
                };
                rustedPlow.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + material1Name + " config: " + error);
            }
        }
    }
}
