using ModularMagic_IceStaffs.configs;
using ModularMagic_IceStaffs.Models;
using System;

namespace ModularMagic_IceStaffs.Configs
{
    internal static class PluginConfig
    {
        public static string staff1Name = "Staff of Frost";
        public static string staff1Recipe = "Bronze:6, RoundLog:20, Blueberries:10, GreydwarfEye:20";
        public static string staff1UpgradeRecipe = "Bronze:1, RoundLog:5, Blueberries:2, GreydwarfEye:4";
        public static StaffConfig staffIce1 = new StaffConfig();

        public static string staff2Name = "Staff of Permafrost";
        public static string staff2Recipe = $"{ModularMagic_IceStaffs.Instance.prefabs.StaffIce1Prefab.name}:1, FineWood:20, FreezeGland:10, Crystal:10";
        public static string staff2UpgradeRecipe = "FineWood:5, FreezeGland:4, Crystal:4";
        public static StaffConfig staffIce2 = new StaffConfig();

        public static string staff3Name = "Hrímkald, touch of Niflheim";
        public static string staff3Recipe = $"{ModularMagic_IceStaffs.Instance.prefabs.StaffIce2Prefab.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:2, Eitr:8";
        public static StaffConfig staffIce3 = new StaffConfig();

        public static string staff4Name = "Skull of Frost";
        public static string staff4Recipe = "YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff4UpgradeRecipe = "YggdrasilWood:10, Sap:2, Eitr:8";
        public static StaffConfig staffIce4 = new StaffConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffIce1Config();
            InitStaffIce2Config();
            InitStaffIce3Config();
            InitStaffIceAOEConfig();
        }

        private static void InitStaffIce1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_IceStaffs.Instance.prefabs.StaffIce1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe)
                {
                    description = "Ice staff 1",
                    craftingStation = "Forge",
                    minStationLevel = 2,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 12f,
                    damagePierce = 4f,
                    blockArmor = 12,
                    deflectionForce = 20,
                    attackForce = 10,
                    useEitr = 3,
                };
                staffIce1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff1Name  + " config: " + error);
            }
        }

        private static void InitStaffIce2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_IceStaffs.Instance.prefabs.StaffIce2Prefab, staff2Name, staff2Recipe, staff2UpgradeRecipe)
                {
                    description = "Ice staff 2",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 17f,
                    damagePierce = 6f,
                    blockArmor = 30,
                    deflectionForce = 20,
                    attackForce = 10,
                    useEitr = 5,
                };
                staffIce2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff2Name  + " config: " + error);
            }
        }

        private static void InitStaffIce3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_IceStaffs.Instance.prefabs.StaffIce3Prefab, staff3Name, staff3Recipe, staff3UpgradeRecipe)
                {
                    description = "Ice staff 3",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 23f,
                    damagePierce = 7f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 10,
                    useEitr = 7,
                };
                staffIce3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }


        private static void InitStaffIceAOEConfig()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_IceStaffs.Instance.prefabs.StaffIceAOEPrefab, staff4Name, staff4Recipe, staff4UpgradeRecipe)
                {
                    description = "Ice staff AOE",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 40f,
                    damagePierce = 0f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 10,
                };
                staffIce4.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff4Name + " config: " + error);
            }
        }
    }
}
