using SeedCart.Models;
using SeedCart.Types;
using System;

namespace SeedCart.Configs
{
    internal static class PluginConfig
    {

        private static string piece1Name = "Seed Cart";
        private static string piece1Recipe = $"Wood:20, BronzeNails:20, Silver:10, {SeedCart.Instance.prefabs.Plow.name}:3";
        public static BuildPieceConfig piece1 = new BuildPieceConfig();

        public static string material1Name = "Rusted Plow";
        public static MaterialConfig rustedPlow = new MaterialConfig();

        // Other

        public static void Init()
        {
            InitPiece1Config();
            InitRustedPlowConfig();
        }

        public static void InitPiece1Config()
        {
            try
            {
                BuildPieceConfigOptions options = new BuildPieceConfigOptions(SeedCart.Instance.prefabs.SeedCart, piece1Name, piece1Recipe)
                {
                    description = "A card that plants your seeds!",
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
                MaterialConfigOptions options = new MaterialConfigOptions(SeedCart.Instance.prefabs.Plow, material1Name, null)
                {
                    description = "An old rusted plow, maybe it can be restored?",
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
