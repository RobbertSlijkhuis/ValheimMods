using ModularMagic_Core.Types;
using ModularMagic_IceStaffs.configs;
using ModularMagic_IceStaffs.Models;
using System;

namespace ModularMagic_IceStaffs.Configs
{
    internal static class PluginConfig
    {
        public static string staff1Name = "Staff of Frost";
        public static string staff1Recipe = $"Bronze:6, RoundLog:20, GreydwarfEye:10, {MaterialType.EitrCrude}:12";
        public static string staff1UpgradeRecipe = $"Bronze:2, RoundLog:5, {MaterialType.EitrCrude}:4";
        public static StaffConfig staffIce1 = new StaffConfig();

        public static string staff2Name = "Staff of Permafrost";
        public static string staff2Recipe = $"FineWood:20, FreezeGland:10, Crystal:10, {MaterialType.EitrFine}:12";
        public static string staff2UpgradeRecipe = $"FineWood:5, FreezeGland:5, {MaterialType.EitrCrude}:4";
        public static StaffConfig staffIce2 = new StaffConfig();

        public static string staff3Name = "Hrímkald, touch of Niflheim";
        public static string staff3Recipe = $"YggdrasilWood:20, Sap:10, FreezeGland:20, Eitr:12";
        public static string staff3UpgradeRecipe = "YggdrasilWood:10, Sap:3, Eitr:4";
        public static StaffConfig staffIce3 = new StaffConfig();

        public static string staff4Name = "Skull of Frost";
        public static string staff4Recipe = "YggdrasilWood:20, Sap:10, FreezeGland:20, Eitr:12";
        public static string staff4UpgradeRecipe = "YggdrasilWood:10, Sap:3, Eitr:4";
        public static StaffConfig staffIce4 = new StaffConfig();

        public static string secondaryAttackNovaName = "Secondary attack Nova";
        public static SecondaryAttackConfig secondaryAttackNova = new SecondaryAttackConfig();

        public static string secondaryAttackRainName = "Secondary attack Ice shards";
        public static SecondaryAttackConfig secondaryAttackRain = new SecondaryAttackConfig();

        public static ImbuementConfig imbuementConfig = new ImbuementConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            imbuementConfig.GenerateConfig();
            InitStaffIce1Config();
            InitStaffIce2Config();
            InitStaffIce3Config();
            InitStaffIceAOEConfig();
            InitSecondaryAttackNovaConfig();
            InitSecondaryAttackRainConfig();
        }

        private static void InitSecondaryAttackNovaConfig()
        {
            try
            {
                SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                    ModularMagic_IceStaffs.Instance.prefabs.SecondaryAttackNova,
                    ModularMagic_IceStaffs.Instance.prefabs.AttackNova,
                    SecondaryAttackType.AOE,
                    secondaryAttackNovaName
                )
                {
                    useEitr = 100,
                    cooldown = 30f,
                    aoe = 6f,
                    damageFrost = 75f,
                    damagePierce = 0f,
                    attackForce = 40f,
                };
                secondaryAttackNova.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + secondaryAttackNovaName + " config: " + error);
            }
        }

        private static void InitSecondaryAttackRainConfig()
        {
            try
            {
                SecondaryAttackConfigOptions options = new SecondaryAttackConfigOptions(
                    ModularMagic_IceStaffs.Instance.prefabs.SecondaryAttackRain,
                    ModularMagic_IceStaffs.Instance.prefabs.ProjectileIceShard,
                    SecondaryAttackType.Projectile,
                    secondaryAttackRainName
                )
                {
                    useEitr = 100,
                    cooldown = 30f,
                    aoe = 1.5f,
                    damageFrost = 50f,
                    damagePierce = 40f,
                    attackForce = 100f,
                    launchAngle = -25f,
                    projectileVelocity = 20f,
                    projectileAccuracy = 1f,
                    spawnAbilityPrefab = ModularMagic_IceStaffs.Instance.prefabs.ScriptIceShards,
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
                    damagePiercePerLevel = 1f,
                    projectileVelocity = 20f,
                    projectileAccuracy = 5f,
                    projectileBurst = 0.35f,
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
                    damagePiercePerLevel = 1f,
                    projectileVelocity = 25f,
                    projectileAccuracy = 4f,
                    projectileBurst = 0.3f,
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
                    damagePiercePerLevel = 2f,
                    projectileVelocity = 30f,
                    projectileAccuracy = 3f,
                    projectileBurst = 0.25f,
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
