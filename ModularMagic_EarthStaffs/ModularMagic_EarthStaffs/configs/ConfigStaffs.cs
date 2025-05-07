using ModularMagic_EarthStaffs.Models;
using System;

namespace ModularMagic_EarthStaffs.Configs
{
    internal static class ConfigStaffs
    {
        public static string staff0Name = "Staff of Mushrooms";
        public static string staff0Recipe = "Wood:20, Stone:20, Resin:20, DeerHide:6";
        public static string staff0UpgradeRecipe = "Wood:10, Stone:10, DeerHide:2";
        public static StaffConfig staffEarth0 = new StaffConfig();

        public static string staff1Name = "Staff of Stone";
        public static string staff1Recipe = "RoundLog:20, Stone:20, Resin:20, Thistle:10";
        public static string staff1UpgradeRecipe = "RoundLog:10, Stone:10, Thistle:4";
        public static StaffConfig staffEarth1 = new StaffConfig();

        public static string staff2Name = "Staff of Boulders";
        public static string staff2Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1Prefab.name}:1, FineWood:20, Root:10, Crystal:20";
        public static string staff2UpgradeRecipe = "FineWood:10, Root:4, Crystal:8";
        public static StaffConfig staffEarth2 = new StaffConfig();

        public static string staff3Name = "Staff of Earth";
        public static string staff3Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2Prefab.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:4, Eitr:8";
        // public static string staff3CooldownStatusEffectName = "StaffEarth3Cooldown_DW";
        public static StaffConfig staffEarth3 = new StaffConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffEarth0Config();
            InitStaffEarth1Config();
            InitStaffEarth2Config();
            InitStaffEarth3Config();
        }

        private static void InitStaffEarth0Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0Prefab, staff0Name, staff0Recipe, staff0UpgradeRecipe, sectionIndex)
                {
                    description = "You feel weird holding this staff... its as if everything seems different but you can put your finger on it. Finger! Haha!",
                    craftingStation = "Workbench",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 20f,
                    damageSpirit = 6f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 10,
                };
                staffEarth0.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff0Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe, sectionIndex)
                {
                    description = "Insert cringy description about getting stoned or something",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 15f,
                    damageSpirit = 1f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffEarth1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff1Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2Prefab, staff2Name, staff2Recipe, staff2UpgradeRecipe, sectionIndex)
                {
                    description = "Something with bing stones",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 21f,
                    damageSpirit = 2f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 7,
                };
                staffEarth2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff2Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3Prefab, staff3Name, staff3Recipe, staff3UpgradeRecipe, sectionIndex)
                {
                    description = "Your foes will bend to natures will... or they will get bend!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 27f,
                    damageSpirit = 3f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 10,
                };
                staffEarth3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }
    }
}
