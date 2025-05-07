using ModularMagic_FireStaffs.Models;
using System;

namespace ModularMagic_FireStaffs.Configs
{
    internal static class ConfigStaffs
    {
        public static string staff1Name = "Staff of Fire";
        public static string staff1Recipe = "Iron:20, ElderBark:20, Chain:10, SurtlingCore:10";
        public static string staff1UpgradeRecipe = "ElderBark:10, Chain:5, SurtlingCore:4";
        public static StaffConfig staffFire1 = new StaffConfig();

        public static string staff2Name = "Staff of Fireballs";
        public static string staff2Recipe = $"{ModularMagic_FireStaffs.Instance.prefabs.staffFire1Prefab.name}:1, FineWood:20, SurtlingCore:10, Crystal:20";
        public static string staff2UpgradeRecipe = "FineWood:10, SurtlingCore:4, Crystal:8";
        public static StaffConfig staffFire2 = new StaffConfig();

        public static string staff3Name = "Staff of Engulfing Flames";
        public static string staff3Recipe = $"{ModularMagic_FireStaffs.Instance.prefabs.staffFire2Prefab.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:4, Eitr:8";
        public static StaffConfig staffFire3 = new StaffConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffFire1Config();
            InitStaffFire2Config();
            InitStaffFire3Config();
        }

        private static void InitStaffFire1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_FireStaffs.Instance.prefabs.staffFire1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe, sectionIndex)
                {
                    description = "Spits fire",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 40f,
                    damageFire = 40f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 20,
                };
                staffFire1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff1Name  + " config: " + error);
            }
        }

        private static void InitStaffFire2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_FireStaffs.Instance.prefabs.staffFire2Prefab, staff2Name, staff2Recipe, staff2UpgradeRecipe, sectionIndex)
                {
                    description = "Throw fireballs to destroy your enemies!",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 80f,
                    damageFire = 80f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 27,
                };
                staffFire2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff2Name  + " config: " + error);
            }
        }

        private static void InitStaffFire3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_FireStaffs.Instance.prefabs.staffFire3Prefab, staff3Name, staff3Recipe, staff3UpgradeRecipe, sectionIndex)
                {
                    description = "Your foes BURN!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 120f,
                    damageFire = 120f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 35,
                };
                staffFire3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }
    }
}
