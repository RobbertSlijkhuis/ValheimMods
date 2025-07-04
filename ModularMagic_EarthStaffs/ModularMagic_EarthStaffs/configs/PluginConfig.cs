using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Configs
{
    internal static class PluginConfig
    {
        public static string staff0Name = "The Forest Flinger";
        public static string staff0Recipe = "Wood:10, Stone:10, Resin:10, LeatherScraps:6";
        public static string staff0UpgradeRecipe = "Wood:5, Stone:5, LeatherScraps:2";
        public static StaffConfig staffEarth0 = new StaffConfig();

        public static string staff1Name = "Staff of Rocks";
        public static string staff1Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0.name}:1, RoundLog:10, Stone:10, Thistle:10";
        public static string staff1UpgradeRecipe = "RoundLog:5, Stone:5, Thistle:3";
        public static StaffConfig staffEarth1 = new StaffConfig();

        public static string staff2Name = "Staff of the Avalanche";
        public static string staff2Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1.name}:1, FineWood:10, Root:10, Crystal:10";
        public static string staff2UpgradeRecipe = "FineWood:5, Root:3, Crystal:3";
        public static StaffConfig staffEarth2 = new StaffConfig();

        public static string staff3Name = "Oakrend the Earthcaller";
        public static string staff3Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:5, Sap:2, Eitr:8";
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
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0, staff0Name, staff0Recipe, staff0UpgradeRecipe)
                {
                    description = "Held together by nothing but wishful thinking, this staff hurls whatever the forest has lying around. Mushrooms? Sure. Berrie bushes? Why not!",
                    craftingStation = "Workbench",
                    minStationLevel = 1,
                    damageBlunt = 17f,
                    damageBluntPerLevel = 2f,
                    damageSpirit = 3f,
                    damageSpiritPerLevel = 2f,
                    projectileVelocity = 15f,
                    projectileAccuracy = 2f,
                    blockArmor = 4,
                    deflectionForce = 20,
                    attackForce = 20,
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
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1, staff1Name, staff1Recipe, staff1UpgradeRecipe)
                {
                    description = "The staff equivalent of shouting ‘rock!’ and hoping for the best! Chance for broken limbs: optimal",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    damageBlunt = 10f,
                    damageBluntPerLevel = 1f,
                    damageChop = 3f,
                    damageChopPerLevel = 1f,
                    damagePickaxe = 2f,
                    damagePickaxePerLevel = 1f,
                    damageSpirit = 3f,
                    damageSpiritPerLevel = 1f,
                    projectileVelocity = 20f,
                    projectileAccuracy = 4f,
                    projectileBurst = 0.4f,
                    blockArmor = 12,
                    deflectionForce = 20,
                    attackForce = 20,
                    useEitr = 3,
                };
                staffEarth1.GenerateConfig(options);

                //if (staffEarth1.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Boulder)
                //    staffEarth1.secondaryAttackConfig = InitSecondaryAttackBoulderConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1, staff1Name);

                //if (staffEarth1.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Roots)
                //    staffEarth1.secondaryAttackConfig = InitSecondaryAttackRootsConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1, staff1Name);
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
                
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2, staff2Name, staff2Recipe, staff2UpgradeRecipe)
                {
                    description = "This staff flings boulders with all the subtlety of an avalanche. Not great for diplomacy. Excellent for everything else!",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    damageBlunt = 14f,
                    damageBluntPerLevel = 1f,
                    damageChop = 8f,
                    damageChopPerLevel = 1f,
                    damagePickaxe = 5f,
                    damagePickaxePerLevel = 1f,
                    damageSpirit = 6f,
                    damageSpiritPerLevel = 1f,
                    projectileVelocity = 25f,
                    projectileAccuracy = 3f,
                    projectileBurst = 0.35f,
                    blockArmor = 30,
                    deflectionForce = 20,
                    attackForce = 20,
                    useEitr = 5,
                };
                staffEarth2.GenerateConfig(options);
                //staffEarth2.secondaryAttackConfig = InitSecondaryAttackBoulderConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2, staff2Name);

                //if (staffEarth2.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Boulder)
                //    staffEarth2.secondaryAttackConfig = InitSecondaryAttackBoulderConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2, staff2Name);

                //if (staffEarth2.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Roots)
                //    staffEarth2.secondaryAttackConfig = InitSecondaryAttackRootsConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2, staff2Name);
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
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3, staff3Name, staff3Recipe, staff3UpgradeRecipe)
                {
                    description = "Side effects may include dizziness, confusion, and being buried under several metric tons of stone. Nature just doesn’t negotiate. It bends, breaks, and buries!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    damageBlunt = 21f,
                    damageBluntPerLevel = 1f,
                    damageChop = 12f,
                    damageChopPerLevel = 1f,
                    damagePickaxe = 8f,
                    damagePickaxePerLevel = 1f,
                    damageSpirit = 9f,
                    damageSpiritPerLevel = 1f,
                    projectileVelocity = 30f,
                    projectileAccuracy = 2f,
                    projectileBurst = 0.3f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 7,
                };
                staffEarth3.GenerateConfig(options);
                //staffEarth3.secondaryAttackConfig = InitSecondaryAttackRootsConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3, staff3Name);

                //if (staffEarth3.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Boulder)
                //    staffEarth3.secondaryAttackConfig = InitSecondaryAttackBoulderConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3, staff3Name);

                //if (staffEarth3.selectedSecondaryAttack.Value == SelectSecondaryAttackType.Roots)
                //    staffEarth3.secondaryAttackConfig = InitSecondaryAttackRootsConfig(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3, staff3Name);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }

        private static SecondaryAttackConfig InitSecondaryAttackBoulderConfig(GameObject staffPrefab, string staffName)
        {
            SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                ModularMagic_EarthStaffs.Instance.prefabs.projectileBoulder,
                SecondaryAttackType.PROJECTILE,
                staffName + " (secondary attack)"
            )
            {
                aoe = 4.5f,
                damageBlunt = 90f,
                damageChop = 30f,
                damagePickaxe = 30f,
                damagePoison = 0f,
                damageSpirit = 0f,
                attackForce = 100f,
                launchAngle = -25f,
                projectileVelocity = 20f,
                projectileAccuracy = 1f,
            };
            SecondaryAttackConfig config = new SecondaryAttackConfig();
            config.GenerateConfig(staffPrefab, options);
            return config;
        }

        private static SecondaryAttackConfig InitSecondaryAttackRootsConfig(GameObject staffPrefab, string staffName)
        {
            SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                ModularMagic_EarthStaffs.Instance.prefabs.Root,
                SecondaryAttackType.HUMANOID,
                staffName + " (secondary attack)"
            )
            {
                health = 150f,
                minToSpawn = 7,
                maxToSpawn = 7,
                maxSpawns = 7,
                spawnRadius = 15f,
                damageBlunt = 70f,
                damageChop = 20f,
                damagePickaxe = 20f,
                damagePoison = 0f,
                damageSpirit = 20f,
                attackForce = 40f,
            };
            SecondaryAttackConfig config = new SecondaryAttackConfig();
            config.GenerateConfig(staffPrefab, options);
            return config;
        }
    }
}
