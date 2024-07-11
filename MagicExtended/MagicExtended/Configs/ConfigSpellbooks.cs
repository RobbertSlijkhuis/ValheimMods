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
        public static SpellbookConfig simpleSpellbook = new SpellbookConfig();

        // Advanced Spellbook
        public static string sectionAdvancedSpellbook = "31. Advanced Spellbook";
        public static string advancedSpellbookRecipeName = "Recipe_AdvancedSpellbook_DW";
        public static string advancedSpellbookDefaultRecipe = "SimpleSpellbook_DW:1, Bronze:2, Resin:8, FineEitr_DW:4";
        public static SpellbookConfig advancedSpellbook = new SpellbookConfig();

        // Master Spellbook
        public static string sectionMasterSpellbook = "32. Master Spellbook";
        public static string masterSpellbookRecipeName = "Recipe_MasterSpellbook_DW";
        public static string masterSpellbookDefaultRecipe = "AdvancedSpellbook_DW:1, Bronze:2, Resin:8, Eitr:4";
        public static SpellbookConfig masterSpellbook = new SpellbookConfig();

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
                SpellbookConfigOptions options = new SpellbookConfigOptions()
                {
                    sectionName = sectionSimpleSpellbook,
                    recipeName = simpleSpellbookRecipeName,
                    prefab = MagicExtended.Instance.prefabs.simpleSpellbookPrefab,
                    enable = true,
                    name = "Simple Spellbook",
                    description = "This book has some crudely written instructions on how to attune one self to the use of magic.",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipe = simpleSpellbookDefaultRecipe,
                    eitr = 25f,
                    eitrRegen = 0.1f,
                };
                simpleSpellbook.GenerateConfig(options);
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
                SpellbookConfigOptions options = new SpellbookConfigOptions()
                {
                    sectionName = sectionAdvancedSpellbook,
                    recipeName = advancedSpellbookRecipeName,
                    prefab = MagicExtended.Instance.prefabs.advancedSpellbookPrefab, 
                    enable = true,
                    name = "Advanced Spellbook",
                    description = "A book with more advanced spells and how to improve magical items.Pretty neat!",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipe = advancedSpellbookDefaultRecipe,
                    eitr = 45f,
                    eitrRegen = 0.2f,
                };
                advancedSpellbook.GenerateConfig(options);
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
                SpellbookConfigOptions options = new SpellbookConfigOptions()
                {
                    sectionName = sectionMasterSpellbook,
                    recipeName = masterSpellbookRecipeName,
                    prefab = MagicExtended.Instance.prefabs.masterSpellbookPrefab,
                    enable = true,
                    name = "Master Spellbook",
                    description = "The one book to rule them all... well not really but it sounded cool right?",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = masterSpellbookDefaultRecipe,
                    eitr = 60f,
                    eitrRegen = 0.25f,
                };
                masterSpellbook.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Master Spellbook config: " + error);
            }
        }
    }
}
