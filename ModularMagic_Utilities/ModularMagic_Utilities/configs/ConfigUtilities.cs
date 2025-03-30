using BepInEx.Configuration;
using ModularMagic_Utilities.Models;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Configs
{
    internal static class ConfigUtilities
    {
        // Simple Spellbook
        public static string sectionSimpleSpellbook = "2. Simple Spellbook";
        public static string simpleSpellbookRecipeName = "Recipe_MMU_SimpleSpellbook";
        public static string simpleSpellbookDefaultRecipe = "TrollHide:6, Bronze:4, Resin:8, Coal: 10";
        public static UtilitiesConfig simpleSpellbook = new UtilitiesConfig();

        // Advanced Spellbook
        public static string sectionAdvancedSpellbook = "3. Advanced Spellbook";
        public static string advancedSpellbookRecipeName = "Recipe_MMU_AdvancedSpellbook";
        public static string advancedSpellbookDefaultRecipe = "MMU_SimpleSpellbook:1, Silver:4, Thunderstone:2, Chitin: 10";
        public static UtilitiesConfig advancedSpellbook = new UtilitiesConfig();

        // Master Spellbook
        public static string sectionMasterSpellbook = "4. Master Spellbook";
        public static string masterSpellbookRecipeName = "Recipe_MMU_MasterSpellbook";
        public static string masterSpellbookDefaultRecipe = "MMU_AdvancedSpellbook:1, BlackCore:6, Sap:10, Eitr: 8";
        public static UtilitiesConfig masterSpellbook = new UtilitiesConfig();

        // Mystic Lantern
        public static string sectionMysticLantern = "5. Mystic Lantern";
        public static string mysticLanternRecipeName = "Recipe_MMU_MysticLantern";
        public static string mysticLanternDefaultRecipe = "RoundLog: 10, Bronze:4, Resin:8, SurtlingCore:2";
        public static UtilitiesConfig mysticLantern = new UtilitiesConfig();

        // Ever Winter Lantern
        public static string sectionEverWinterLantern = "6. EverWinter Lantern";
        public static string everWinterLanternRecipeName = "Recipe_MMU_EverWinterLantern";
        public static string everWinterLanternDefaultRecipe = "MMU_MysticLantern:1, Crystal:10, Silver:6, DragonEgg:1";
        public static UtilitiesConfig everWinterLantern = new UtilitiesConfig();

        // Black Core Lantern
        public static string sectionBlackCoreLantern = "7. Black Core Lantern";
        public static string blackCoreLanternRecipeName = "Recipe_MMU_BlackCoreLantern";
        public static string blackCoreLanternDefaultRecipe = "MMU_EverWinterLantern:1, BlackCore:6, BlackMarble:12, Eitr:6";
        public static UtilitiesConfig blackCoreLantern = new UtilitiesConfig();

        // Other
        public static  ConfigEntry<KeyboardShortcut> configUtilityModeKey;
        public static string sectionGeneral = "1. General";

        public static void Init()
        {
            InitGeneralConfig();
            InitSimpleSpellbookConfig();
            InitAdvancedSpellbookConfig();
            InitMasterSpellbookConfig();
            InitMysticLanternConfig();
            InitEverWinterLanternConfig();
            InitBlackCoreLanternConfig();
        }

        private static void InitGeneralConfig()
        {
            configUtilityModeKey = ModularMagic_Utilities.Instance.Config.Bind(sectionGeneral, "Utility mode key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Key to change the mode on utilities (applies to lanterns)", null));
            configUtilityModeKey.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("key has been changed to: " + configUtilityModeKey.Value);
            };
        }

        private static void InitSimpleSpellbookConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionSimpleSpellbook,
                    recipeName = simpleSpellbookRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.simpleSpellbookPrefab,
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
                Jotunn.Logger.LogError("Could not initialise " + sectionSimpleSpellbook + " config: " + error);
            }
        }

        private static void InitAdvancedSpellbookConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionAdvancedSpellbook,
                    recipeName = advancedSpellbookRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.advancedSpellbookPrefab, 
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
                Jotunn.Logger.LogError("Could not initialise " + sectionAdvancedSpellbook + " config: " + error);
            }
        }

        private static void InitMasterSpellbookConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionMasterSpellbook,
                    recipeName = masterSpellbookRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.masterSpellbookPrefab,
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
                Jotunn.Logger.LogError("Could not initialise " + sectionMasterSpellbook + " config: " + error);
            }
        }

        private static void InitMysticLanternConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionMysticLantern,
                    recipeName = mysticLanternRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.mysticLanternPrefab,
                    enable = true,
                    name = "Mystic Lantern",
                    description = "A latern to light you way on your travels",
                    craftingStation = "Forge",
                    minStationLevel = 1,
                    recipe = mysticLanternDefaultRecipe,
                    eitr = 15f,
                    eitrRegen = 0.15f,
                    demister = 0f,
                };
                mysticLantern.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + sectionMysticLantern + " config: " + error);
            }
        }

        private static void InitEverWinterLanternConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionEverWinterLantern,
                    recipeName = everWinterLanternRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.everWinterLanternPrefab,
                    enable = true,
                    name = "Ever Winter Lantern",
                    description = "A lantern to keep you warm in the extreme cold",
                    craftingStation = "Forge",
                    minStationLevel = 5,
                    recipe = everWinterLanternDefaultRecipe,
                    eitr = 37f,
                    eitrRegen = 0.25f,
                    demister = 0f,
                };
                everWinterLantern.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + sectionEverWinterLantern + " config: " + error);
            }
        }

        private static void InitBlackCoreLanternConfig()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions()
                {
                    sectionName = sectionBlackCoreLantern,
                    recipeName = blackCoreLanternRecipeName,
                    prefab = ModularMagic_Utilities.Instance.prefabs.blackCoreLanternPrefab,
                    enable = true,
                    name = "Black Core Lantern",
                    description = "A darknesss eminates from inside, who ever made this dabbled in dark magics",
                    craftingStation = "BlackForge",
                    minStationLevel = 1,
                    recipe = blackCoreLanternDefaultRecipe,
                    eitr = 50f,
                    eitrRegen = 0.30f,
                    demister = 6f,
                };
                blackCoreLantern.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + sectionBlackCoreLantern + " config: " + error);
            }
        }
    }
}
