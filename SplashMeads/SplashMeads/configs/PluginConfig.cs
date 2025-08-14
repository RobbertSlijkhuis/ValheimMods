using BepInEx.Configuration;
using SplashMeads.Helpers;
using SplashMeads.Models;
using SplashMeads.Types;
using System;

namespace SplashMeads.Configs
{
    internal static class PluginConfig
    {
        private static string mead1Name = "Splash Fire Resistance Barley Wine";
        private static string mead1Recipe = "BarleyWine:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead1 = new SplashMeadConfig();

        private static string mead2Name = "Splash Frost Resistance Mead";
        private static string mead2Recipe = "MeadFrostResist:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead2 = new SplashMeadConfig();

        private static string mead3Name = "Splash Poison Resistance Mead";
        private static string mead3Recipe = "MeadPoisonResist:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead3 = new SplashMeadConfig();

        private static string mead4Name = "Splash Ratatosk Tonic";
        private static string mead4Recipe = "MeadHasty:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead4 = new SplashMeadConfig();

        private static string mead5Name = "Splash Vananidir Mead";
        private static string mead5Recipe = "MeadSwimmer:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead5 = new SplashMeadConfig();

        private static string mead6Name = "Splash Anti-Sting Mead";
        private static string mead6Recipe = "MeadBugRepellent:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead6 = new SplashMeadConfig();

        private static string mead7Name = "Splash Major Health Mead";
        private static string mead7Recipe = "MeadHealthMajor:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead7 = new SplashMeadConfig();

        private static string mead8Name = "Splash Medium Health Mead";
        private static string mead8Recipe = "MeadHealthMedium:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead8 = new SplashMeadConfig();

        private static string mead9Name = "Splash Minor Health Mead";
        private static string mead9Recipe = "MeadHealthMinor:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead9 = new SplashMeadConfig();

        // Other
        public static string generalSectionname = "General";
        public static ConfigEntry<bool> showParticles;
        public static ConfigEntry<bool> showParticlesOnPlayers;
        public static ConfigEntry<bool> showHudIcons;
        public static ConfigEntry<float> HudIconSize;
        public static ConfigEntry<bool> showHudTimers;
        public static ConfigEntry<int> timersFontSize;
        public static ConfigEntry<string> timersFormat;
        public static ConfigEntry<string> timersAlignment;

        // General options
        public static string[] hudAlignmentOptions = new string[] {
            HudAlignmentType.TopLeft, HudAlignmentType.Top, HudAlignmentType.TopRight, HudAlignmentType.Left, HudAlignmentType.Center, HudAlignmentType.Right,
            HudAlignmentType.BottomLeft, HudAlignmentType.Bottom,  HudAlignmentType.BottomRight };

        public static string[] hudTimerFormatOptions = new string[] {
            HudTimerFormat.Minutes, HudTimerFormat.Seconds };

        public static void Init()
        {
            InitGeneralConfig();
            InitMeadsConfig();
        }

        public static void InitGeneralConfig()
        {
            showParticles = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show particles"), true,
               new ConfigDescription("Show particles when tames/players are affected by a splash mead", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 10 }));
            showParticles.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.BarlyWineSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.FrostResistSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.PoisonResistSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.RatatoskSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.VananidirSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.AntiStingSplashFX, showParticles.Value);
            };

            showParticlesOnPlayers = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show particles on players"), true,
               new ConfigDescription("Show particles on players affected by a splash mead", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 9 }));

            showHudIcons = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show hud icons"), true,
               new ConfigDescription("Show splash mead icons underneath healthbar of tames", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 8 }));

            HudIconSize = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Icon size"), 30f,
               new ConfigDescription("The hud icon size",
               new AcceptableValueRange<float>(30f, 50f),
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 7 }));

            showHudTimers = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show hud timers"), true,
               new ConfigDescription("Show a timer on the icons", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 6 }));

            timersFontSize = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Timer font size"), 16,
               new ConfigDescription("The font size of hud timers", 
               new AcceptableValueRange<int>(12, 24),
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 5 }));

            timersFormat = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Timer format"), HudTimerFormat.Minutes,
               new ConfigDescription("The format in which the timers are displayed (Minutes:Seconds or the entire duration in seconds)",
               new AcceptableValueList<string>(hudTimerFormatOptions),
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 4 }));

            timersAlignment = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Timer alignment"), HudAlignmentType.Bottom,
                new ConfigDescription("The vertical alignment of the hud timers",
                new AcceptableValueList<string>(hudAlignmentOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 3 }));
        }

        public static void InitMeadsConfig()
        {
            try
            {
                SplashMeadConfigOptions options = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.BarlyWineSplash, mead1Name, mead1Recipe)
                {
                    description = "Applies " + mead1Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead1.GenerateConfig(options);

                SplashMeadConfigOptions options2 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.FrostResistSplash, mead2Name, mead2Recipe)
                {
                    description = "Applies " + mead2Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead2.GenerateConfig(options2);

                SplashMeadConfigOptions options3 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.PoisonResistSplash, mead3Name, mead3Recipe)
                {
                    description = "Applies " + mead3Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead3.GenerateConfig(options3);

                SplashMeadConfigOptions options4 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.RatatoskSplash, mead4Name, mead4Recipe)
                {
                    description = "Applies " + mead4Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead4.GenerateConfig(options4);

                SplashMeadConfigOptions options5 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.VananidirSplash, mead5Name, mead5Recipe)
                {
                    description = "Applies " + mead5Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead5.GenerateConfig(options5);

                SplashMeadConfigOptions options6 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.AntiStingSplash, mead6Name, mead6Recipe)
                {
                    description = "Applies " + mead6Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead6.GenerateConfig(options6);

                SplashMeadConfigOptions options7 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.MajorHealthSplash, mead7Name, mead7Recipe)
                {
                    description = "Applies " + mead7Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                    duration = 120,
                    isCooldown = true
                };
                mead7.GenerateConfig(options7);

                SplashMeadConfigOptions options8 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.MediumHealthSplash, mead8Name, mead8Recipe)
                {
                    description = "Applies " + mead8Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                    duration = 120,
                    isCooldown = true
                };
                mead8.GenerateConfig(options8);

                SplashMeadConfigOptions options9 = new SplashMeadConfigOptions(SplashMeads.Instance.prefabs.MinorHealthSplash, mead9Name, mead9Recipe)
                {
                    description = "Applies " + mead9Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                    duration = 120,
                    isCooldown = true
                };
                mead9.GenerateConfig(options9);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise splash meads config: " + e);
            }
        }
    }
}
