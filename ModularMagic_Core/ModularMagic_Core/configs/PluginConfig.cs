using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;

namespace ModularMagic_Core.Configs
{
    internal static class PluginConfig
    {
        public static string material1Name = "Crude Eitr";
        public static string material1Recipe = "GreydwarfEye:3, Resin:3";
        public static ItemConfig crudeEitr = new ItemConfig();

        public static string material2Name = "Fine Eitr";
        public static string material2Recipe = "Crystal:3, Coal:3";
        public static ItemConfig fineEitr = new ItemConfig();

        private static string piece1Name = "Imbuement Table";
        private static string piece1Recipe = "Stone:20";
        public static BuildPieceConfig piece1 = new BuildPieceConfig();

        public static void Init()
        {
            InitCrudeEitrConfig();
            InitFineEitrConfig();
            InitPiece1Config();
        }

        private static void InitCrudeEitrConfig()
        {
            try
            {
                ItemConfigOptions options = new ItemConfigOptions(ModularMagic_Core.prefabs.EitrCrude, material1Name, material1Recipe)
                {
                    description = "Magic in a crude form, but better then the mushrooms",
                    craftingStation = CraftingStationType.Cauldron,
                };
                crudeEitr.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Crude Eitr config: " + error);
            }
        }

        private static void InitFineEitrConfig()
        {
            try
            {
                ItemConfigOptions options = new ItemConfigOptions(ModularMagic_Core.prefabs.EitrFine, material2Name, material2Recipe)
                {
                    description = "Fine Magic to create powerfull weapons, tools and armor!",
                    craftingStation = CraftingStationType.Cauldron,
                    minStationLevel = 3,
                };
                fineEitr.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Fine Eitr config: " + error);
            }
        }

        public static void InitPiece1Config()
        {
            try
            {
                BuildPieceConfigOptions options = new BuildPieceConfigOptions(ModularMagic_Core.prefabs.ImbuementTable, piece1Name, piece1Recipe)
                {
                    description = "$piece_horizontal",
                    craftingStation = CraftingStationType.Stonecutter,
                    lightColorPreset = LightColorPresetType.Blue,
                };
                piece1.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + piece1Name + " config: " + e);
            }
        }
    }
}
