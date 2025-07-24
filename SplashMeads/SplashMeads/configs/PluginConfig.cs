using BepInEx.Configuration;
using SplashMeads.Helpers;
using SplashMeads.Models;
using SplashMeads.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

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
        private static string mead5Recipe = "MeadHasty:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead5 = new SplashMeadConfig();

        // Other
        public static string generalSectionname = "General";
        public static ConfigEntry<bool> showParticles;
        public static ConfigEntry<bool> showParticlesOnPlayers;
        public static ConfigEntry<bool> showHudIcons;

        public static void Init()
        {
            InitGeneralConfig();
            InitMeadsConfig();
        }

        public static void InitGeneralConfig()
        {
            showParticles = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show particles"), true,
               new ConfigDescription("Show particles when tames/players are affected by a splash mead", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 3 }));
            showParticles.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.BarlyWineSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.FrostResistSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.PoisonResistSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.RatatoskSplashFX, showParticles.Value);
                UpdateHelper.UpdateFXEnabled(SplashMeads.Instance.prefabs.VananidirSplashFX, showParticles.Value);
            };

            showParticlesOnPlayers = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show particles on players"), true,
               new ConfigDescription("Show particles on players affected by a splash mead", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 2 }));

            showHudIcons = SplashMeads.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Show hud icons"), true,
               new ConfigDescription("Show splash mead icons underneath healthbar of tames", null,
               new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1 }));
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
                    description = "Applies " + mead4Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                    craftingStation = CraftingStationType.Workbench,
                    minStationLevel = 3,
                    recipeAmount = 2,
                };
                mead5.GenerateConfig(options5);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise splash meads config: " + e);
            }
        }
    }
}
