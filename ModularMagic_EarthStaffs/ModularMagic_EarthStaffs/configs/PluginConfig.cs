using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System;

namespace ModularMagic_EarthStaffs.Configs
{
    internal static class PluginConfig
    {
        public static string staff0Name = "The Forest Flinger";
        public static string staff0Recipe = "Wood:10, Stone:10, Resin:10, LeatherScraps:6";
        public static string staff0UpgradeRecipe = "Wood:5, Stone:5, LeatherScraps:2";
        public static StaffConfig staffEarth0 = new StaffConfig();

        public static string staff1Name = "Staff of Rocks";
        public static string staff1Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth0.name}:1, RoundLog:10, Stone:10, Thistle:10";
        public static string staff1UpgradeRecipe = "RoundLog:5, Stone:5, Thistle:3";
        public static StaffConfig staffEarth1 = new StaffConfig();

        public static string staff2Name = "Staff of the Avalanche";
        public static string staff2Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth1.name}:1, FineWood:10, Root:10, Crystal:10";
        public static string staff2UpgradeRecipe = "FineWood:5, Root:3, Crystal:3";
        public static StaffConfig staffEarth2 = new StaffConfig();

        public static string staff3Name = "Oakrend the Earthcaller";
        public static string staff3Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth2.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:5, Sap:2, Eitr:8";
        public static StaffConfig staffEarth3 = new StaffConfig();

        public static string secondaryAttack1Name = "Secondary attack Boulder";
        public static SecondaryAttackConfig secondaryAttackBoulder = new SecondaryAttackConfig();

        public static string secondaryAttack2Name = "Secondary attack Roots";
        public static SecondaryAttackConfig secondaryAttackRoots = new SecondaryAttackConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffEarth0Config();
            InitStaffEarth1Config();
            InitStaffEarth2Config();
            InitStaffEarth3Config();
            InitSecondaryAttackBoulderConfig();
            InitSecondaryAttackRootsConfig();
        }

        private static void InitStaffEarth0Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth0, staff0Name, staff0Recipe, staff0UpgradeRecipe)
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
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth1, staff1Name, staff1Recipe, staff1UpgradeRecipe)
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
                
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth2, staff2Name, staff2Recipe, staff2UpgradeRecipe)
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
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth3, staff3Name, staff3Recipe, staff3UpgradeRecipe)
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
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }

        private static void InitSecondaryAttackBoulderConfig()
        {
            try
            {
                SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                    ModularMagic_EarthStaffs.Instance.prefabs.SecondaryAttackBoulder,
                    ModularMagic_EarthStaffs.Instance.prefabs.ProjectileBoulder,
                    SecondaryAttackType.Projectile,
                    secondaryAttack1Name
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
                secondaryAttackBoulder.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name + " config: " + error);
            }
        }

        private static void InitSecondaryAttackRootsConfig()
        {
            SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                ModularMagic_EarthStaffs.Instance.prefabs.SecondaryAttackRoots,
                ModularMagic_EarthStaffs.Instance.prefabs.Root,
                SecondaryAttackType.Humanoid,
                secondaryAttack2Name
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
            secondaryAttackRoots.GenerateConfig(options);
        }
    }
}
