using BepInEx.Configuration;
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

        public static void Init()
        {
            InitMeadsConfig();
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
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise splash meads config: " + e);
            }
        }
    }
}
