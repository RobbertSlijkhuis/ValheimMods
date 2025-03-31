using BepInEx.Configuration;
using ModularMagic_Utilities.Models;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Configs
{
    internal static class ConfigUtilities
    {
        public static string book1Name = "Spellbook of the Hearth";
        public static string book1Recipe = "TrollHide:6, Bronze:4, Resin:8, Coal:10";
        public static UtilitiesConfig spellbook1 = new UtilitiesConfig();

        public static string book2Name = "Grimoire of the Storm";
        public static string book2Recipe = "MMU_SpellbookOfTheHearth:1, Silver:4, Thunderstone:2, Chitin:10";
        public static UtilitiesConfig spellbook2 = new UtilitiesConfig();

        public static string book3Name = "Codex of the Asgardian Sorcerer";
        public static string book3Recipe = "MMU_GrimoireOfTheStorm:1, BlackCore:6, Sap:10, Eitr:8";
        public static UtilitiesConfig spellbook3 = new UtilitiesConfig();

        public static string lantern1Name = "Mystical Lantern";
        public static string lantern1Recipe = "RoundLog: 10, Bronze:4, Resin:8, SurtlingCore:2";
        public static UtilitiesConfig lantern1 = new UtilitiesConfig();

        public static string lantern2Name = "Everwinter Lantern";
        public static string lantern2Recipe = "MMU_MythicalLantern:1, Silver:6, Crystal:10, DragonEgg:1";
        public static UtilitiesConfig lantern2 = new UtilitiesConfig();

        public static string lantern3Name = "Mistcaller Lantern";
        public static string lantern3Recipe = "MMU_EverwinterLantern:1, BlackCore:6, BlackMarble:12, Eitr:6";
        public static UtilitiesConfig lantern3 = new UtilitiesConfig();

        // Other
        public static  ConfigEntry<KeyboardShortcut> configUtilityModeKey;
        private static int sectionIndex = 1;

        public static void Init()
        {
            InitGeneralConfig();
            InitSpellbook1Config();
            InitSpellbook2Config();
            InitSpellbook3Config();
            InitLantern1Config();
            InitLantern2Config();
            InitLantern3Config();
        }

        private static void InitGeneralConfig()
        {
            configUtilityModeKey = ModularMagic_Utilities.Instance.Config.Bind($"{sectionIndex}. General", "Utility mode key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Key to change the mode on utilities (applies to lanterns)", null));
            configUtilityModeKey.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("key has been changed to: " + configUtilityModeKey.Value);
            };
            IncrementSectionIndex();
        }

        private static void InitSpellbook1Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.spellbook1Prefab, book1Name, book1Recipe, sectionIndex)
                {
                    description = "The first step on the path to wielding the arcane arts. This humble tome contains some crudely written instructions on how to attune one self to the use of magic.",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    eitr = 20f,
                    eitrRegen = 0.1f,
                    elementalMagic = 4f,
                    bloodMagic = 4f,
                };
                spellbook1.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + book1Name + " config: " + error);
            }
        }

        private static void InitSpellbook2Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.spellbook2Prefab, book2Name, book2Recipe, sectionIndex)
                {
                    description = "As you journey further into the realms of magic, this tome reveals more complex spells, drawn from the primal forces of the world. The Grimoire of the Storm holds the secrets of the gods themselves!",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    eitr = 40f,
                    eitrRegen = 0.15f,
                    elementalMagic = 7f,
                    bloodMagic = 7f,
                };
                spellbook2.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + book2Name + " config: " + error);
            }
        }

        private static void InitSpellbook3Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.spellbook3Prefab, book3Name, book3Recipe, sectionIndex)
                {
                    description = "A tome of unimaginable power, said to have been written by the greatest mages of Asgard. The Codex holds the most potent and intricate spells, woven with the threads of fate itself.\r\n",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    eitr = 50f,
                    eitrRegen = 0.2f,
                    elementalMagic = 10f,
                    bloodMagic = 10f,
                };
                spellbook3.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + book3Name + " config: " + error);
            }
        }

        private static void InitLantern1Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.lantern1Prefab, lantern1Name, lantern1Recipe, sectionIndex)
                {
                    description = "Carved with ancient runes, this lantern channels the power of the gods themselves. Its light flickers with a divine glow, illuminating the paths of those who seek wisdom and courage in the realms.",
                    craftingStation = "Forge",
                    minStationLevel = 1,
                    eitr = 15f,
                    eitrRegen = 0.15f,
                    elementalMagic = 3f,
                    bloodMagic = 3f,
                    demister = 0f,

                };
                lantern1.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern1Name + " config: " + error);
            }
        }

        private static void InitLantern2Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.lantern2Prefab, lantern2Name, lantern2Recipe, sectionIndex)
                {
                    description = "This lantern’s light glows like the pale blue of the northern ice. Said to be crafted by the dwarves who once guarded the icy mountains.",
                    craftingStation = "Forge",
                    minStationLevel = 5,
                    eitr = 35f,
                    eitrRegen = 0.2f,
                    elementalMagic = 6f,
                    bloodMagic = 6f,
                    demister = 0f,
                };
                lantern2.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern2Name + " config: " + error);
            }
        }

        private static void InitLantern3Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.lantern3Prefab, lantern3Name, lantern3Recipe, sectionIndex)
                {
                    description = "A lantern woven from the essence of the mists that shroud the edges of Valheim. The soft glow is calming, though its true power lies in its ability to guide those lost in the fog.",
                    craftingStation = "BlackForge",
                    minStationLevel = 1,
                    eitr = 45f,
                    eitrRegen = 0.25f,
                    elementalMagic = 8f,
                    bloodMagic = 8f,
                    demister = 6f,
                };
                lantern3.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern3Name + " config: " + error);
            }
        }

        private static void IncrementSectionIndex()
        {
            sectionIndex = sectionIndex + 1;
        }
    }
}
