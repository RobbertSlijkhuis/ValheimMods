using BepInEx.Configuration;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Configs
{
    internal static class PluginConfig
    {
        public static string book1Name = "Spellbook of the Hearth";
        public static string book1Recipe = "Bronze:4, TrollHide:6, Resin:8, Coal:10";
        public static UtilitiesConfig spellbook1 = new UtilitiesConfig();

        public static string book2Name = "Grimoire of the Storm";
        public static string book2Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Spellbook1.name}:1, Silver:4, Thunderstone:2, Chitin:10";
        public static UtilitiesConfig spellbook2 = new UtilitiesConfig();

        public static string book3Name = "Codex of the Asgardian Sorcerer";
        public static string book3Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Spellbook2.name}:1, BlackCore:1, Sap:10, Eitr:8";
        public static UtilitiesConfig spellbook3 = new UtilitiesConfig();

        public static string lantern1Name = "Mythical Lantern";
        public static string lantern1Recipe = "Bronze:4, RoundLog: 10, Resin:8, SurtlingCore:2";
        public static UtilitiesConfig lantern1 = new UtilitiesConfig();

        public static string lantern2Name = "Everwinter Lantern";
        public static string lantern2Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Lantern1.name}:1, Silver:4, DragonEgg:1, Crystal:10";
        public static UtilitiesConfig lantern2 = new UtilitiesConfig();

        public static string lantern3Name = "Mistcaller Lantern";
        public static string lantern3Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Lantern2.name}:1, BlackCore:1, BlackMarble:12, Eitr:6";
        public static UtilitiesConfig lantern3 = new UtilitiesConfig();

        public static string piece1Name = "Marble Item Stand";
        public static string piece1Recipe = "BlackMarble:10";
        public static BuildPieceConfig piece1 = new BuildPieceConfig();

        // Other
        public static string generalSectionname = "General";
        public static ConfigEntry<KeyboardShortcut> configLanternModKey;

        public static void Init()
        {
            InitGeneralConfig();
            InitSpellbook1Config();
            InitSpellbook2Config();
            InitSpellbook3Config();
            InitLantern1Config();
            InitLantern2Config();
            InitLantern3Config();
            InitPiece1Config();
        }

        private static void InitGeneralConfig()
        {
            configLanternModKey = ModularMagic_Utilities.Instance.Config.Bind(generalSectionname, "Lantern on/off key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Key to toggle the light on/off of lanterns)", null));
        }

        private static void InitSpellbook1Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Spellbook1, book1Name, book1Recipe)
                {
                    description = "The first step on the path to wielding the arcane arts. This humble tome contains some crudely written instructions on how to attune one self to the use of magic.",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    eitr = 35f,
                    eitrRegen = 0.1f,
                    elementalMagic = 4f,
                    bloodMagic = 4f,
                };
                spellbook1.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + book1Name + " config: " + e);
            }
        }

        private static void InitSpellbook2Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Spellbook2, book2Name, book2Recipe)
                {
                    description = "As you journey further into the realms of magic, this tome reveals more complex spells, drawn from the primal forces of the world. The Grimoire of the Storm holds the secrets of the gods themselves!",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 5,
                    eitr = 50f,
                    eitrRegen = 0.15f,
                    elementalMagic = 8f,
                    bloodMagic = 8f,
                };
                spellbook2.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + book2Name + " config: " + e);
            }
        }

        private static void InitSpellbook3Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Spellbook3, book3Name, book3Recipe)
                {
                    description = "A tome of unimaginable power, said to have been written by the greatest mages of Asgard. The Codex holds the most potent and intricate spells, woven with the threads of fate itself.\r\n",
                    craftingStation = CraftingStationType.GaldrTable,
                    minStationLevel = 1,
                    eitr = 65f,
                    eitrRegen = 0.2f,
                    elementalMagic = 12f,
                    bloodMagic = 12f,
                };
                spellbook3.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + book3Name + " config: " + e);
            }
        }

        private static void InitLantern1Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Lantern1, lantern1Name, lantern1Recipe)
                {
                    description = "Carved with ancient runes, this lantern channels the power of the gods themselves. Its light flickers with a divine glow, illuminating the paths of those who seek wisdom and courage in the realms.",
                    craftingStation = CraftingStationType.Forge,
                    minStationLevel = 1,
                    eitr = 30f,
                    eitrRegen = 0.15f,
                    elementalMagic = 3f,
                    bloodMagic = 3f,
                    demister = 0f,
                    lightColorPreset = LightPresetType.Yellow,
                    lightRange = 30f,
                    lightIntensity = 1.5f,
                };
                lantern1.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern1Name + " config: " + e);
            }
        }

        private static void InitLantern2Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Lantern2, lantern2Name, lantern2Recipe)
                {
                    description = "This lantern’s light glows like the pale blue of the northern ice. Said to be crafted by the dwarves who once guarded the icy mountains.",
                    craftingStation = CraftingStationType.Forge,
                    minStationLevel = 5,
                    eitr = 45f,
                    eitrRegen = 0.2f,
                    elementalMagic = 6f,
                    bloodMagic = 6f,
                    demister = 0f,
                    lightColorPreset = LightPresetType.LightBlue,
                    lightRange = 30f,
                    lightIntensity = 1.5f,
                };
                lantern2.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern2Name + " config: " + e);
            }
        }

        private static void InitLantern3Config()
        {
            try
            {
                UtilitiesConfigOptions options = new UtilitiesConfigOptions(ModularMagic_Utilities.Instance.prefabs.Lantern3, lantern3Name, lantern3Recipe)
                {
                    description = "A lantern woven from the essence of the mists that shroud the edges of Valheim. The soft glow is calming, though its true power lies in its ability to guide those lost in the fog.",
                    craftingStation = CraftingStationType.BlackForge,
                    minStationLevel = 1,
                    eitr = 60f,
                    eitrRegen = 0.25f,
                    elementalMagic = 9f,
                    bloodMagic = 9f,
                    demister = 6f,
                    lightColorPreset = LightPresetType.Pink,
                    lightRange = 30f,
                    lightIntensity = 1.5f,
                };
                lantern3.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + lantern3Name + " config: " + e);
            }
        }

        private static void InitPiece1Config()
        {
            try
            {
                BuildPieceConfigOptions options = new BuildPieceConfigOptions(ModularMagic_Utilities.Instance.prefabs.MarbleItemstand, piece1Name, piece1Recipe)
                {
                    description = "$piece_horizontal",
                    craftingStation = CraftingStationType.Stonecutter,
                    weatherZoneRadius = 30f,
                    enableDome = false,
                    enableProjector = false,
                    lightColorPreset = LightPresetType.Blue,
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
