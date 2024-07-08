using BepInEx.Configuration;
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

        public static ConfigEntry<bool> crudeEitrEnable;
        public static ConfigEntry<string> crudeEitrName;
        public static ConfigEntry<string> crudeEitrDescription;
        public static ConfigEntry<string> crudeEitrCraftingStation;
        public static ConfigEntry<int> crudeEitrMinStationLevel;
        public static ConfigEntry<string> crudeEitrRecipe;

        // FineEitr
        public static string sectionFineEitr = "41. Fine Eitr";
        public static string fineEitrRecipeName = "Recipe_FineEitr_DW";
        public static string fineEitrDefaultRecipe = "Crystal:3, Coal:3, PircedMushroom_DW:3";

        public static ConfigEntry<bool> fineEitrEnable;
        public static ConfigEntry<string> fineEitrName;
        public static ConfigEntry<string> fineEitrDescription;
        public static ConfigEntry<string> fineEitrCraftingStation;
        public static ConfigEntry<int> fineEitrMinStationLevel;
        public static ConfigEntry<string> fineEitrRecipe;

        public static void Init()
        {
            InitCrudeEitrConfig();
            InitFineEitrConfig();
        }

        private static void InitCrudeEitrConfig()
        {
            try
            {
                MaterialConfig materialConfig = new MaterialConfig()
                {
                    sectionName = sectionCrudeEitr,
                    recipeName = crudeEitrRecipeName,
                    prefab = MagicExtended.Instance.crudeEitrPrefab,
                    enable = true,
                    name = "Crude Eitr",
                    description = "Magic in a crude form, but better then the mushrooms",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    recipe = crudeEitrDefaultRecipe,
                };
                materialConfig.GenerateConfig();

                crudeEitrEnable = materialConfig.configEnable;
                crudeEitrName = materialConfig.configName;
                crudeEitrDescription = materialConfig.configDescription;
                crudeEitrCraftingStation = materialConfig.configCraftingStation;
                crudeEitrMinStationLevel = materialConfig.configMinStationLevel;
                crudeEitrRecipe = materialConfig.configRecipe;
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
                MaterialConfig materialConfig = new MaterialConfig()
                {
                    sectionName = sectionFineEitr,
                    recipeName = fineEitrRecipeName,
                    prefab = MagicExtended.Instance.fineEitrPrefab,
                    enable = true,
                    name = "Fine Eitr",
                    description = "Fine Magic to create powerfull weapons, tools and armor!",
                    craftingStation = "Cauldron",
                    minStationLevel = 3,
                    recipe = fineEitrDefaultRecipe,
                };
                materialConfig.GenerateConfig();

                fineEitrEnable = materialConfig.configEnable;
                fineEitrName = materialConfig.configName;
                fineEitrDescription = materialConfig.configDescription;
                fineEitrCraftingStation = materialConfig.configCraftingStation;
                fineEitrMinStationLevel = materialConfig.configMinStationLevel;
                fineEitrRecipe = materialConfig.configRecipe;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Fine Eitr config: " + error);
            }
        }
    }
}
