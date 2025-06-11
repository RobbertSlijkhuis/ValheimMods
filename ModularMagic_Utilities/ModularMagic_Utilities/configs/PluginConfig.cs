using BepInEx.Configuration;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace ModularMagic_Utilities.Configs
{
    internal static class PluginConfig
    {
        private static string book1Name = "Spellbook of the Hearth";
        private static string book1Recipe = "Bronze:4, TrollHide:6, Resin:8, Coal:10";
        public static UtilitiesConfig spellbook1 = new UtilitiesConfig();

        private static string book2Name = "Grimoire of the Storm";
        private static string book2Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Spellbook1.name}:1, Silver:4, Thunderstone:2, Chitin:10";
        public static UtilitiesConfig spellbook2 = new UtilitiesConfig();

        private static string book3Name = "Codex of the Asgardian Sorcerer";
        private static string book3Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Spellbook2.name}:1, BlackCore:1, Sap:10, Eitr:8";
        public static UtilitiesConfig spellbook3 = new UtilitiesConfig();

        private static string lantern1Name = "Mythical Lantern";
        private static string lantern1Recipe = "Bronze:4, RoundLog: 10, Resin:8, SurtlingCore:2";
        public static UtilitiesConfig lantern1 = new UtilitiesConfig();

        private static string lantern2Name = "Everwinter Lantern";
        private static string lantern2Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Lantern1.name}:1, Silver:4, DragonEgg:1, Crystal:10";
        public static UtilitiesConfig lantern2 = new UtilitiesConfig();

        private static string lantern3Name = "Mistcaller Lantern";
        private static string lantern3Recipe = $"{ModularMagic_Utilities.Instance.prefabs.Lantern2.name}:1, BlackCore:1, BlackMarble:12, Eitr:6";
        public static UtilitiesConfig lantern3 = new UtilitiesConfig();

        private static string piece1Name = "Marble Item Stand";
        private static string piece1Recipe = "BlackMarble:10, BlackCore:3, MoltenCore:3, CharredCogwheel:1";
        public static BuildPieceConfig piece1 = new BuildPieceConfig();

        public static List<WeatherZoneConfig> weatherConfigList = new List<WeatherZoneConfig>();
        private static string SectorNameWeatherNormal = "Normal environments";
        public static WeatherZoneConfig weatherZoneClear = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneHeathClear = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneLightRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneSwampRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneThunderStorm = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneNoFogThunderStorm = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneMisty = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneSnow = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneSnowStorm = new WeatherZoneConfig();

        private static string SectorNameWeatherMistlands = "Mislands environments";
        public static WeatherZoneConfig weatherZoneMistlandsClear = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneMistlandsRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneMistlandsThunder = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneDarklandsDark = new WeatherZoneConfig();

        private static string SectorNameWeatherAshlands = "Ashlands environments";
        public static WeatherZoneConfig weatherZoneAshlandsAshRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneAshlandsCinderRain = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneAshlandsMisty = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneAshlandsMeteorShower = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneAshlandsStorm = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneAshlandsSeastorm = new WeatherZoneConfig();

        private static string SectorNameWeatherTwilight = "Twilight (Deep North) environments";
        public static WeatherZoneConfig weatherZoneTwilightClear = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneTwilightSnow = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneTwilightSnowStorm = new WeatherZoneConfig();

        private static string SectorNameWeatherBosses = "Bosses environments";
        public static WeatherZoneConfig weatherZoneEikthyr = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneElder = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneBonemass = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneModer = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneYagluth = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneQueen = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneFader = new WeatherZoneConfig();

        private static string SectorNameWeatherDungeons = "Dungeons environments";
        public static WeatherZoneConfig weatherZoneCrypt = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneSunkenCrypt = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneCavesHildir = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneCryptHildir = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneGhosts = new WeatherZoneConfig();
        public static WeatherZoneConfig weatherZoneInfectedMine = new WeatherZoneConfig();

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
            InitWeatherConfig();
        }

        public static void InitGeneralConfig()
        {
            configLanternModKey = ModularMagic_Utilities.Instance.Config.Bind(generalSectionname, "Lantern on/off key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Key to toggle the light on/off of lanterns)", null));
        }

        public static void InitSpellbook1Config()
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

        public static void InitSpellbook2Config()
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

        public static void InitSpellbook3Config()
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

        public static void InitLantern1Config()
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
                    lightColorPreset = LightColorPresetType.Yellow,
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

        public static void InitLantern2Config()
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
                    lightColorPreset = LightColorPresetType.LightBlue,
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

        public static void InitLantern3Config()
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
                    lightColorPreset = LightColorPresetType.Pink,
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

        public static void InitPiece1Config()
        {
            try
            {
                BuildPieceConfigOptions options = new BuildPieceConfigOptions(ModularMagic_Utilities.Instance.prefabs.MarbleItemstand, piece1Name, piece1Recipe)
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

        public static void InitWeatherConfig()
        {
            try
            {
                WeatherZoneConfigOptions optionsClear = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "Clear",
                    prefabName = ModularMagic_Utilities.Instance.prefabs.Spellbook1.name,
                    lightColorPreset = LightColorPresetType.Yellow,
                    order = 10,
                };
                weatherZoneClear.GenerateConfig(optionsClear);
                weatherConfigList.Add(weatherZoneClear);

                WeatherZoneConfigOptions optionsHeathClear = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "Heath_clear",
                    prefabName = ModularMagic_Utilities.Instance.prefabs.Lantern1.name,
                    lightColorPreset = LightColorPresetType.Yellow,
                    order = 9,
                };
                weatherZoneHeathClear.GenerateConfig(optionsHeathClear);
                weatherConfigList.Add(weatherZoneHeathClear);

                WeatherZoneConfigOptions optionsLightRain = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "LightRain",
                    prefabName = "TrophyNeck",
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 8,
                };
                weatherZoneLightRain.GenerateConfig(optionsLightRain);
                weatherConfigList.Add(weatherZoneLightRain);

                WeatherZoneConfigOptions optionsRain = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "Rain",
                    prefabName = "TrophyLeech",
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 7,
                };
                weatherZoneRain.GenerateConfig(optionsRain);
                weatherConfigList.Add(weatherZoneRain);

                WeatherZoneConfigOptions optionsSwampRain = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "SwampRain",
                    prefabName = "TrophyAbomination",
                    lightColorPreset = LightColorPresetType.Green,
                    order = 6,
                };
                weatherZoneSwampRain.GenerateConfig(optionsSwampRain);
                weatherConfigList.Add(weatherZoneSwampRain);

                WeatherZoneConfigOptions optionsThunderStorm = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "ThunderStorm",
                    prefabName = ModularMagic_Utilities.Instance.prefabs.Spellbook2.name,
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 5,
                };
                weatherZoneThunderStorm.GenerateConfig(optionsThunderStorm);
                weatherConfigList.Add(weatherZoneThunderStorm);

                WeatherZoneConfigOptions optionsNoFogThunderStorm = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "nofogts",
                    prefabName = "Thunderstone",
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 4,
                };
                weatherZoneNoFogThunderStorm.GenerateConfig(optionsNoFogThunderStorm);
                weatherConfigList.Add(weatherZoneNoFogThunderStorm);

                WeatherZoneConfigOptions optionsMisty = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "Misty",
                    prefabName = "Crystal",
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 3,
                };
                weatherZoneMisty.GenerateConfig(optionsMisty);
                weatherConfigList.Add(weatherZoneMisty);

                WeatherZoneConfigOptions optionsSnow = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "Snow",
                    prefabName = "FreezeGland",
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 2,
                };
                weatherZoneSnow.GenerateConfig(optionsSnow);
                weatherConfigList.Add(weatherZoneSnow);

                WeatherZoneConfigOptions optionsSnowStorm = new WeatherZoneConfigOptions(SectorNameWeatherNormal)
                {
                    weatherName = "SnowStorm",
                    prefabName = ModularMagic_Utilities.Instance.prefabs.Lantern2.name,
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 1,
                };
                weatherZoneSnowStorm.GenerateConfig(optionsSnowStorm);
                weatherConfigList.Add(weatherZoneSnowStorm);



                WeatherZoneConfigOptions optionsMistlandsClear = new WeatherZoneConfigOptions(SectorNameWeatherMistlands)
                {
                    weatherName = "Mistlands_clear",
                    prefabName = "BlackCore",
                    lightColorPreset = LightColorPresetType.Pink,
                    order = 4,
                };
                weatherZoneMistlandsClear.GenerateConfig(optionsMistlandsClear);
                weatherConfigList.Add(weatherZoneMistlandsClear);

                WeatherZoneConfigOptions optionsMistlandsRain = new WeatherZoneConfigOptions(SectorNameWeatherMistlands)
                {
                    weatherName = "Mistlands_rain",
                    prefabName = "TrophySeekerBrute",
                    lightColorPreset = LightColorPresetType.Pink,
                    order = 3,
                };
                weatherZoneMistlandsRain.GenerateConfig(optionsMistlandsRain);
                weatherConfigList.Add(weatherZoneMistlandsRain);

                WeatherZoneConfigOptions optionsMistlandsStorm = new WeatherZoneConfigOptions(SectorNameWeatherMistlands)
                {
                    weatherName = "Mistlands_thunder",
                    prefabName = ModularMagic_Utilities.Instance.prefabs.Lantern3.name,
                    lightColorPreset = LightColorPresetType.Pink,
                    order = 2,
                };
                weatherZoneMistlandsThunder.GenerateConfig(optionsMistlandsStorm);
                weatherConfigList.Add(weatherZoneMistlandsThunder);

                WeatherZoneConfigOptions optionsDarklandsDark = new WeatherZoneConfigOptions(SectorNameWeatherMistlands)
                {
                    weatherName = "Darklands_Dark",
                    prefabName = "TrophyGrowth",
                    lightColorPreset = LightColorPresetType.Purple,
                    order = 1,
                };
                weatherZoneDarklandsDark.GenerateConfig(optionsDarklandsDark);
                weatherConfigList.Add(weatherZoneDarklandsDark);




                WeatherZoneConfigOptions optionsAshlandsAshRain = new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_ashrain_clear",
                    prefabName = "TrophyCharredMelee",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 6,
                };
                weatherZoneAshlandsAshRain.GenerateConfig(optionsAshlandsAshRain);
                weatherConfigList.Add(weatherZoneAshlandsAshRain);

                WeatherZoneConfigOptions optionsAshlandsMisty = new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_misty",
                    prefabName = "TrophyMorgen",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 5,
                };
                weatherZoneAshlandsMisty.GenerateConfig(optionsAshlandsMisty);
                weatherConfigList.Add(weatherZoneAshlandsMisty);

                WeatherZoneConfigOptions optionsAshlandsCinderRain = new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_CinderRain",
                    prefabName = "TrophyCharredArcher",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 4,
                };
                weatherZoneAshlandsCinderRain.GenerateConfig(optionsAshlandsCinderRain);
                weatherConfigList.Add(weatherZoneAshlandsCinderRain);

                WeatherZoneConfigOptions optionsAshlandsMeteorShower= new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_meteorshower",
                    prefabName = "TrophyCharredMage",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 3,
                };
                weatherZoneAshlandsMeteorShower.GenerateConfig(optionsAshlandsMeteorShower);
                weatherConfigList.Add(weatherZoneAshlandsMeteorShower);

                WeatherZoneConfigOptions optionsAshlandsSeaStorm = new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_SeaStorm",
                    prefabName = "TrophyBonemawSerpent",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 2,
                };
                weatherZoneAshlandsSeastorm.GenerateConfig(optionsAshlandsSeaStorm);
                weatherConfigList.Add(weatherZoneAshlandsSeastorm);

                WeatherZoneConfigOptions optionsAshlandsStorm = new WeatherZoneConfigOptions(SectorNameWeatherAshlands)
                {
                    weatherName = "Ashlands_storm",
                    prefabName = "TrophyFallenValkyrie",
                    lightColorPreset = LightColorPresetType.Red,
                    order = 1,
                };
                weatherZoneAshlandsStorm.GenerateConfig(optionsAshlandsStorm);
                weatherConfigList.Add(weatherZoneAshlandsStorm);



                WeatherZoneConfigOptions optionsTwilightClear = new WeatherZoneConfigOptions(SectorNameWeatherTwilight)
                {
                    weatherName = "Twilight_Clear",
                    prefabName = "DragonEgg",
                    lightColorPreset = LightColorPresetType.Yellow,
                    order = 3,
                };
                weatherZoneTwilightClear.GenerateConfig(optionsTwilightClear);
                weatherConfigList.Add(weatherZoneTwilightClear);

                WeatherZoneConfigOptions optionsTwilightSnow = new WeatherZoneConfigOptions(SectorNameWeatherTwilight)
                {
                    weatherName = "Twilight_Snow",
                    prefabName = "TrophyHatchling",
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 2,
                };
                weatherZoneTwilightSnow.GenerateConfig(optionsTwilightSnow);
                weatherConfigList.Add(weatherZoneTwilightSnow);

                WeatherZoneConfigOptions optionsTwilightSnowStorm = new WeatherZoneConfigOptions(SectorNameWeatherTwilight)
                {
                    weatherName = "Twilight_SnowStorm",
                    prefabName = "TrophySGolem",
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 1,
                };
                weatherZoneTwilightSnowStorm.GenerateConfig(optionsTwilightSnowStorm);
                weatherConfigList.Add(weatherZoneTwilightSnowStorm);




                WeatherZoneConfigOptions optionsEikthyr = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "Eikthyr",
                    prefabName = "TrophyEikthyr",
                    lightColorPreset = LightColorPresetType.Orange,
                    order = 7,
                };
                weatherZoneEikthyr.GenerateConfig(optionsEikthyr);
                weatherConfigList.Add(weatherZoneEikthyr);

                WeatherZoneConfigOptions optionsElder = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "GDKing",
                    prefabName = "TrophyTheElder",
                    lightColorPreset = LightColorPresetType.LemonGreen,
                    order = 6,
                };
                weatherZoneElder.GenerateConfig(optionsElder);
                weatherConfigList.Add(weatherZoneElder);

                WeatherZoneConfigOptions optionsBonemass = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "Bonemass",
                    prefabName = "TrophyBonemass",
                    lightColorPreset = LightColorPresetType.Green,
                    order = 5,
                };
                weatherZoneBonemass.GenerateConfig(optionsBonemass);
                weatherConfigList.Add(weatherZoneBonemass);

                WeatherZoneConfigOptions optionsModer = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "Moder",
                    prefabName = "TrophyDragonQueen",
                    lightColorPreset = LightColorPresetType.LightBlue,
                    order = 4,
                };
                weatherZoneModer.GenerateConfig(optionsModer);
                weatherConfigList.Add(weatherZoneModer);

                WeatherZoneConfigOptions optionsYagluth = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "GoblinKing",
                    prefabName = "TrophyGoblinKing",
                    lightColorPreset = LightColorPresetType.Orange,
                    order = 3,
                };
                weatherZoneYagluth.GenerateConfig(optionsYagluth);
                weatherConfigList.Add(weatherZoneYagluth);

                WeatherZoneConfigOptions optionsQueen = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "Queen",
                    prefabName = "TrophySeekerQueen",
                    lightColorPreset = LightColorPresetType.Pink,
                    order = 2,
                };
                weatherZoneQueen.GenerateConfig(optionsQueen);
                weatherConfigList.Add(weatherZoneQueen);

                WeatherZoneConfigOptions optionsFader = new WeatherZoneConfigOptions(SectorNameWeatherBosses)
                {
                    weatherName = "Fader",
                    prefabName = "TrophyFader",
                    lightColorPreset = LightColorPresetType.Pink,
                    order = 1,
                };
                weatherZoneFader.GenerateConfig(optionsFader);
                weatherConfigList.Add(weatherZoneFader);



                WeatherZoneConfigOptions optionsCrypt = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "Crypt",
                    prefabName = "TrophySkeleton",
                    lightColorPreset = LightColorPresetType.Green,
                    order = 6,
                };
                weatherZoneCrypt.GenerateConfig(optionsCrypt);
                weatherConfigList.Add(weatherZoneCrypt);

                WeatherZoneConfigOptions optionsSunkenCrypt = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "SunkenCrypt",
                    prefabName = "TrophyDraugr",
                    lightColorPreset = LightColorPresetType.Green,
                    order = 5,
                };
                weatherZoneSunkenCrypt.GenerateConfig(optionsSunkenCrypt);
                weatherConfigList.Add(weatherZoneSunkenCrypt);

                WeatherZoneConfigOptions optionsCavesHildir = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "CavesHildir",
                    prefabName = "TrophyCultist_Hildir",
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 4,
                };
                weatherZoneCavesHildir.GenerateConfig(optionsCavesHildir);
                weatherConfigList.Add(weatherZoneCavesHildir);

                WeatherZoneConfigOptions optionsCryptHildir = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "CryptHildir",
                    prefabName = "TrophySkeletonHildir",
                    lightColorPreset = LightColorPresetType.Orange,
                    order = 3,
                };
                weatherZoneCryptHildir.GenerateConfig(optionsCryptHildir);
                weatherConfigList.Add(weatherZoneCryptHildir);

                WeatherZoneConfigOptions optionsGhosts = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "Ghosts",
                    prefabName = "TrophyWraith",
                    lightColorPreset = LightColorPresetType.Blue,
                    order = 2,
                };
                weatherZoneGhosts.GenerateConfig(optionsGhosts);
                weatherConfigList.Add(weatherZoneGhosts);

                WeatherZoneConfigOptions optionsInfectedMine = new WeatherZoneConfigOptions(SectorNameWeatherDungeons)
                {
                    weatherName = "InfectedMine",
                    prefabName = "TrophyDvergr",
                    lightColorPreset = LightColorPresetType.LemonGreen,
                    order = 1,
                };
                weatherZoneInfectedMine.GenerateConfig(optionsInfectedMine);
                weatherConfigList.Add(weatherZoneInfectedMine);




            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise weather config: " + e);
            }
        }
    }
}
