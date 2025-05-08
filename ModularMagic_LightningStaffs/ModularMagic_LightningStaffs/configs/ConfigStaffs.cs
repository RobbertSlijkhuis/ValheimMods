using ModularMagic_LightningStaffs.Models;
using System;

namespace ModularMagic_LightningStaffs.Configs
{
    internal static class ConfigStaffs
    {
        public static string staff1Name = "Staff of Lightning";
        public static string staff1Recipe = "Iron:20, ElderBark:20, Chain:10, SurtlingCore:10";
        public static string staff1UpgradeRecipe = "ElderBark:10, Chain:5, SurtlingCore:4";
        public static StaffConfig staffLightning1 = new StaffConfig();

        public static string staff2Name = "Staff of Thunder";
        public static string staff2Recipe = $"{ModularMagic_LightningStaffs.Instance.prefabs.staffLightning1Prefab.name}:1, FineWood:20, SurtlingCore:10, Crystal:20";
        public static string staff2UpgradeRecipe = "FineWood:10, SurtlingCore:4, Crystal:8";
        public static StaffConfig staffLightning2 = new StaffConfig();

        public static string staff3Name = "Staff of Storms";
        public static string staff3Recipe = $"{ModularMagic_LightningStaffs.Instance.prefabs.staffLightning2Prefab.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:4, Eitr:8";
        public static StaffConfig staffLightning3 = new StaffConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffLightning1Config();
            InitStaffLightning2Config();
            InitStaffLightning3Config();
        }

        private static void InitStaffLightning1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_LightningStaffs.Instance.prefabs.staffLightning1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe, sectionIndex)
                {
                    description = "Staff lightning 1",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 40f,
                    damagePickaxe = 10,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 20,
                };
                staffLightning1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff1Name  + " config: " + error);
            }
        }

        private static void InitStaffLightning2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_LightningStaffs.Instance.prefabs.staffLightning2Prefab, staff2Name, staff2Recipe, staff2UpgradeRecipe, sectionIndex)
                {
                    description = "Staff lightning 2",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 80f,
                    damagePickaxe = 15,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 27,
                };
                staffLightning2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff2Name  + " config: " + error);
            }
        }

        private static void InitStaffLightning3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_LightningStaffs.Instance.prefabs.staffLightning3Prefab, staff3Name, staff3Recipe, staff3UpgradeRecipe, sectionIndex)
                {
                    description = "Staff lightning 3",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 120f,
                    damagePickaxe = 30,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 35,
                };
                staffLightning3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }
    }
}
