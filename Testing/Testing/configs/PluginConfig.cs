using Testing.Models;
using Testing.Types;
using System;

namespace Testing.Configs
{
    internal static class PluginConfig
    {
        private static string mead1Name = "Splash Fire Resistance Barley Wine";
        private static string mead1Recipe = "BarleyWine:6, LeatherScraps:3, Resin:2";
        public static SplashMeadConfig mead1 = new SplashMeadConfig();

        public static void Init()
        {
            InitMeadsConfig();
        }

        public static void InitMeadsConfig()
        {
            try
            {
                //SplashMeadConfigOptions options = new SplashMeadConfigOptions(Testing.Instance.prefabs.BarlyWineSplash, mead1Name, mead1Recipe)
                //{
                //    description = "Applies " + mead1Name.Replace("Splash ", "") + " to friendlies hit or in the splash radius",
                //    craftingStation = CraftingStationType.Workbench,
                //    minStationLevel = 3,
                //    recipeAmount = 2,
                //};
                //mead1.GenerateConfig(options);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise splash meads config: " + e);
            }
        }
    }
}
