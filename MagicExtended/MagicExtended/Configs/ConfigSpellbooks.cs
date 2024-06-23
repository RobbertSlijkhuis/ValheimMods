using BepInEx.Configuration;
using MagicExtended.Models;
using System;

namespace MagicExtended.Configs
{
    internal static class ConfigSpellbooks
    {
        // Simple Spellbook
        public static string sectionSimpleSpellbook = "30. Simple Spellbook";
        public static string simpleSpellbookRecipeName = "Recipe_SimpleSpellbook_DW";
        public static string simpleSpellbookDefaultRecipe = "TrollHide:5, Bronze:2, Resin:8, CrudeEitr_DW:4";

        public static ConfigEntry<bool> simpleSpellbookEnable;
        public static ConfigEntry<string> simpleSpellbookName;
        public static ConfigEntry<string> simpleSpellbookDescription;
        public static ConfigEntry<string> simpleSpellbookCraftingStation;
        public static ConfigEntry<int> simpleSpellbookMinStationLevel;
        public static ConfigEntry<string> simpleSpellbookRecipe;
        public static ConfigEntry<float> simpleSpellbookEitr;
        public static ConfigEntry<float> simpleSpellbookEitrRegen;

        // Advanced Spellbook
        public static string sectionAdvancedSpellbook = "31. Advanced Spellbook";
        public static string advancedSpellbookRecipeName = "Recipe_AdvancedSpellbook_DW";
        public static string advancedSpellbookDefaultRecipe = "SimpleSpellbook_DW:1, Bronze:2, Resin:8, FineEitr_DW:4";

        public static ConfigEntry<bool> advancedSpellbookEnable;
        public static ConfigEntry<string> advancedSpellbookName;
        public static ConfigEntry<string> advancedSpellbookDescription;
        public static ConfigEntry<string> advancedSpellbookCraftingStation;
        public static ConfigEntry<int> advancedSpellbookMinStationLevel;
        public static ConfigEntry<string> advancedSpellbookRecipe;
        public static ConfigEntry<float> advancedSpellbookEitr;
        public static ConfigEntry<float> advancedSpellbookEitrRegen;

        // Master Spellbook
        public static string sectionMasterSpellbook = "32. Master Spellbook";
        public static string masterSpellbookRecipeName = "Recipe_MasterSpellbook_DW";
        public static string masterSpellbookDefaultRecipe = "AdvancedSpellbook_DW:1, Bronze:2, Resin:8, Eitr:4";

        public static ConfigEntry<bool> masterSpellbookEnable;
        public static ConfigEntry<string> masterSpellbookName;
        public static ConfigEntry<string> masterSpellbookDescription;
        public static ConfigEntry<string> masterSpellbookCraftingStation;
        public static ConfigEntry<int> masterSpellbookMinStationLevel;
        public static ConfigEntry<string> masterSpellbookRecipe;
        public static ConfigEntry<float> masterSpellbookEitr;
        public static ConfigEntry<float> masterSpellbookEitrRegen;

        public static void Init()
        {
            InitSimpleSpellbookConfig();
            InitAdvancedSpellbookConfig();
            InitMasterSpellbookConfig();
        }

        private static void InitSimpleSpellbookConfig()
        {
            try
            {
                SpellbookConfig spellbookConfig = new SpellbookConfig()
                {
                    sectionName = sectionSimpleSpellbook,
                    recipeName = simpleSpellbookRecipeName,
                    prefab = MagicExtended.Instance.simpleSpellbookPrefab,
                    enable = true,
                    name = "Simple Spellbook",
                    description = "This book has some crudely written instructions on how to attune one self to the use of magic.",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipe = simpleSpellbookDefaultRecipe,
                    eitr = 25f,
                    eitrRegen = 0.1f,
                };
                spellbookConfig.GenerateConfig();

                simpleSpellbookEnable = spellbookConfig.configEnable;
                simpleSpellbookName = spellbookConfig.configName;
                simpleSpellbookDescription = spellbookConfig.configDescription;
                simpleSpellbookCraftingStation = spellbookConfig.configCraftingStation;
                simpleSpellbookMinStationLevel = spellbookConfig.configMinStationLevel;
                simpleSpellbookRecipe = spellbookConfig.configRecipe;
                simpleSpellbookEitr = spellbookConfig.configEitr;
                simpleSpellbookEitrRegen = spellbookConfig.configEitrRegen;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Simple Spellbook config: " + error);
            }
        }

        private static void InitAdvancedSpellbookConfig()
        {
            try
            {
                SpellbookConfig spellbookConfig = new SpellbookConfig()
                {
                    sectionName = sectionAdvancedSpellbook,
                    recipeName = advancedSpellbookRecipeName,
                    prefab = MagicExtended.Instance.advancedSpellbookPrefab, 
                    enable = true,
                    name = "Advanced Spellbook",
                    description = "A book with more advanced spells and how to improve magical items.Pretty neat!",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipe = advancedSpellbookDefaultRecipe,
                    eitr = 45f,
                    eitrRegen = 0.2f,
                };
                spellbookConfig.GenerateConfig();

                advancedSpellbookEnable = spellbookConfig.configEnable;
                advancedSpellbookName = spellbookConfig.configName;
                advancedSpellbookDescription = spellbookConfig.configDescription;
                advancedSpellbookCraftingStation = spellbookConfig.configCraftingStation;
                advancedSpellbookMinStationLevel = spellbookConfig.configMinStationLevel;
                advancedSpellbookRecipe = spellbookConfig.configRecipe;
                advancedSpellbookEitr = spellbookConfig.configEitr;
                advancedSpellbookEitrRegen = spellbookConfig.configEitrRegen;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Advanced Spellbook config: " + error);
            }
        }

        private static void InitMasterSpellbookConfig()
        {
            try
            {
                SpellbookConfig spellbookConfig = new SpellbookConfig()
                {
                    sectionName = sectionMasterSpellbook,
                    recipeName = masterSpellbookRecipeName,
                    prefab = MagicExtended.Instance.masterSpellbookPrefab,
                    enable = true,
                    name = "Master Spellbook",
                    description = "The one book to rule them all... well not really but it sounded cool right?",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = masterSpellbookDefaultRecipe,
                    eitr = 60f,
                    eitrRegen = 0.25f,
                };
                spellbookConfig.GenerateConfig();

                masterSpellbookEnable = spellbookConfig.configEnable;
                masterSpellbookName = spellbookConfig.configName;
                masterSpellbookDescription = spellbookConfig.configDescription;
                masterSpellbookCraftingStation = spellbookConfig.configCraftingStation;
                masterSpellbookMinStationLevel = spellbookConfig.configMinStationLevel;
                masterSpellbookRecipe = spellbookConfig.configRecipe;
                masterSpellbookEitr = spellbookConfig.configEitr;
                masterSpellbookEitrRegen = spellbookConfig.configEitrRegen;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Master Spellbook config: " + error);
            }
        }
    }
}
