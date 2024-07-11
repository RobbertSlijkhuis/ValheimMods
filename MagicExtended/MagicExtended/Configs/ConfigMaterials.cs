using MagicExtended.Models;
using System;

namespace MagicExtended.Configs
{
    internal static class ConfigMaterials
    {
        // CrudeEitr
        public static string sectionCrudeEitr = "40. Crude Eitr";
        public static string crudeEitrRecipeName = "Recipe_CrudeEitr_DW";
        public static string crudeEitrDefaultRecipe = "GreydwarfEye:3, Resin:3, GribSnow_DW:3";
        public static MaterialConfig crudeEitr = new MaterialConfig();

        // FineEitr
        public static string sectionFineEitr = "41. Fine Eitr";
        public static string fineEitrRecipeName = "Recipe_FineEitr_DW";
        public static string fineEitrDefaultRecipe = "Crystal:3, Coal:3, PircedMushroom_DW:3";
        public static MaterialConfig fineEitr = new MaterialConfig();

        public static void Init()
        {
            InitCrudeEitrConfig();
            InitFineEitrConfig();
        }

        private static void InitCrudeEitrConfig()
        {
            try
            {
                MaterialConfigOptions options = new MaterialConfigOptions()
                {
                    sectionName = sectionCrudeEitr,
                    recipeName = crudeEitrRecipeName,
                    prefab = MagicExtended.Instance.prefabs.crudeEitrPrefab,
                    enable = true,
                    name = "Crude Eitr",
                    description = "Magic in a crude form, but better then the mushrooms",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    recipe = crudeEitrDefaultRecipe,
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
                MaterialConfigOptions options = new MaterialConfigOptions()
                {
                    sectionName = sectionFineEitr,
                    recipeName = fineEitrRecipeName,
                    prefab = MagicExtended.Instance.prefabs.fineEitrPrefab,
                    enable = true,
                    name = "Fine Eitr",
                    description = "Fine Magic to create powerfull weapons, tools and armor!",
                    craftingStation = "Cauldron",
                    minStationLevel = 3,
                    recipe = fineEitrDefaultRecipe,
                };
                fineEitr.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Fine Eitr config: " + error);
            }
        }
    }
}
