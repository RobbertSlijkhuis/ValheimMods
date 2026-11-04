using ModularMagic_LightningStaffs.Models;
using ModularMagic_LightningStaffs.Types;
using System;

namespace ModularMagic_LightningStaffs.Configs
{
    internal static class PluginConfig
    {
        public static string staff1Name = "Staff of Lightning";
        public static string staff1Recipe = "Bronze:6, BoneFragments:20, HardAntler:4, GreydwarfEye:20";
        public static string staff1UpgradeRecipe = "Bronze:1, BoneFragments:4, HardAntler:1, GreydwarfEye:4";
        public static StaffConfig staffLightning1 = new StaffConfig();

        public static string staff2Name = "Staff of Thunder";
        public static string staff2Recipe = $"{ModularMagic_LightningStaffs.Instance.prefabs.staffLightning1Prefab.name}:1, FineWood:20, Thunderstone:4, Crystal:10";
        public static string staff2UpgradeRecipe = "FineWood:5, Thunderstone:1, Crystal:4";
        public static StaffConfig staffLightning2 = new StaffConfig();

        public static string staff3Name = "Staff of Storms";
        public static string staff3Recipe = $"{ModularMagic_LightningStaffs.Instance.prefabs.staffLightning2Prefab.name}:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:2, Eitr:8";
        public static StaffConfig staffLightning3 = new StaffConfig();

        public static string secondaryAttackNovaName = "Secondary attack Nova";
        public static SecondaryAttackConfig secondaryAttackNova = new SecondaryAttackConfig();

        public static string mainAttackConeName = "Main attack Cone";
        public static MainAttackConfig mainAttackCone = new MainAttackConfig();

        public static string secondaryAttackRainName = "Secondary attack Lightning bolts";
        public static SecondaryAttackConfig secondaryAttackRain = new SecondaryAttackConfig();

        public static ImbuementConfig imbuementConfig = new ImbuementConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            imbuementConfig.GenerateConfig();
            InitStaffLightning1Config();
            InitStaffLightning2Config();
            InitStaffLightning3Config();
            InitSecondaryAttackNovaConfig();
            InitSecondaryAttackRainConfig();
            InitMainAttackConeConfig();
        }

        private static void InitMainAttackConeConfig()
        {
            try
            {
                MainAttackConfigOptions options = new MainAttackConfigOptions(
                    ModularMagic_LightningStaffs.Instance.prefabs.mainAttackCone,
                    ModularMagic_LightningStaffs.Instance.prefabs.attackCone,
                    mainAttackConeName
                )
                {
                    useEitr = 5,
                    burstInterval = 0.4f,
                    damageLightning = 10f,
                    attackForce = 0f,
                };
                mainAttackCone.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mainAttackConeName + " config: " + error);
            }
        }

        private static void InitSecondaryAttackRainConfig()
        {
            try
            {
                SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                    ModularMagic_LightningStaffs.Instance.prefabs.secondaryAttackRain,
                    ModularMagic_LightningStaffs.Instance.prefabs.projectileBolt,
                    SecondaryAttackType.Projectile,
                    secondaryAttackRainName
                )
                {
                    useEitr = 100,
                    cooldown = 30f,
                    aoe = 3f,
                    damageLightning = 70f,
                    damageChop = 20f,
                    damagePickaxe = 20f,
                    attackForce = 30f,
                    launchAngle = -25f,
                    projectileVelocity = 20f,
                    projectileAccuracy = 1f,
                    spawnAbilityPrefab = ModularMagic_LightningStaffs.Instance.prefabs.scriptBolts,
                    minToSpawn = 15,
                    maxToSpawn = 15,
                    spawnRadius = 20f,
                };
                secondaryAttackRain.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + secondaryAttackRainName + " config: " + error);
            }
        }

        private static void InitSecondaryAttackNovaConfig()
        {
            try
            {
                SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                    ModularMagic_LightningStaffs.Instance.prefabs.secondaryAttackNova,
                    ModularMagic_LightningStaffs.Instance.prefabs.attackNova,
                    SecondaryAttackType.AOE,
                    secondaryAttackNovaName
                )
                {
                    useEitr = 100,
                    cooldown = 30f,
                    aoe = 6f,
                    damageLightning = 75f,
                    attackForce = 40f,
                };
                secondaryAttackNova.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + secondaryAttackNovaName + " config: " + error);
            }
        }

        private static void InitStaffLightning1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_LightningStaffs.Instance.prefabs.staffLightning1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe, sectionIndex)
                {
                    description = "Staff lightning 1",
                    craftingStation = "Forge",
                    minStationLevel = 2,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 40f,
                    damagePickaxe = 10,
                    blockArmor = 12,
                    deflectionForce = 20,
                    attackForce = 25,
                    useEitr = 20,
                    projectileVelocity = 15f,
                    projectileAccuracy = 2f,
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
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 80f,
                    damagePickaxe = 15,
                    blockArmor = 30,
                    deflectionForce = 20,
                    attackForce = 30,
                    useEitr = 27,
                    projectileVelocity = 17.5f,
                    projectileAccuracy = 1.5f,
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
                    projectileVelocity = 20f,
                    projectileAccuracy = 1f,
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
