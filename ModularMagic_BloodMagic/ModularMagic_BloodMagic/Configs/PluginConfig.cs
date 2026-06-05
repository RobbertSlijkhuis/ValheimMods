using BepInEx.Configuration;
using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
using System;
using System.Collections.Generic;

namespace ModularMagic_BloodMagic.Configs
{
    internal static class PluginConfig
    {
        public static string scythe1Name          = "Death's Embrace";
        public static string scythe1Recipe        = "RoundLog:10, Iron:10, Ectoplasm:10";
        public static string scythe1UpgradeRecipe = "RoundLog:5, Iron:5, Ectoplasm:2";
        public static WeaponConfig scythe1        = new WeaponConfig();

        public static string scythe2Name          = "Reaper of Souls";
        public static string scythe2Recipe        = "FineWood:10, BlackMetal:10, Tar:10";
        public static string scythe2UpgradeRecipe = "FineWood:5, BlackMetal:5, Tar:2";
        public static WeaponConfig scythe2        = new WeaponConfig();

        public static string scythe3Name          = "Eternal Reaper";
        public static string scythe3Recipe        = "YggdrasilWood:10, FlametalNew:10, MoltenCore:10";
        public static string scythe3UpgradeRecipe = "YggdrasilWood:5, FlametalNew:5, MoltenCore:2";
        public static WeaponConfig scythe3        = new WeaponConfig();

        public static string scythe4Name          = "Simple Reaper";
        public static string scythe4Recipe        = "Wood:10, Stone:10, Resin:10";
        public static string scythe4UpgradeRecipe = "Wood:5, Stone:5, Resin:2";
        public static WeaponConfig scythe4        = new WeaponConfig();

        // Apparition configs
        public static ConfigEntry<int>   scythe1ApparitionHealth;
        public static ConfigEntry<float> scythe1ApparitionDamage;

        public static ConfigEntry<int>   scythe2ApparitionHealth;
        public static ConfigEntry<float> scythe2ApparitionDamage;

        public static ConfigEntry<int>   scythe3ApparitionHealth;
        public static ConfigEntry<float> scythe3ApparitionDamage;

        public static ConfigEntry<int>   scythe4ApparitionHealth;
        public static ConfigEntry<float> scythe4ApparitionDamage;

        public static void Init()
        {
            InitSythe1Config();
            InitSythe2Config();
            InitSythe3Config();
            InitSythe4Config();
            InitApparitionConfigs();
        }

        private static void InitApparitionConfigs()
        {
            ConfigFile Config = ModularMagic_BloodMagic.Instance.Config;

            scythe1ApparitionHealth = Config.Bind(new ConfigDefinition("Deaths Embrace - Apparition", "Health"), 140,
                new ConfigDescription("The health of apparitions spawned by Death's Embrace.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe1ApparitionDamage = Config.Bind(new ConfigDefinition("Deaths Embrace - Apparition", "Damage"), 30f,
                new ConfigDescription("The actual damage dealt by apparitions spawned by Death's Embrace. Stored internally as a multiplier against the base damage of 150.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe2ApparitionHealth = Config.Bind(new ConfigDefinition("Reaper of Souls - Apparition", "Health"), 490,
                new ConfigDescription("The health of apparitions spawned by Reaper of Souls.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe2ApparitionDamage = Config.Bind(new ConfigDefinition("Reaper of Souls - Apparition", "Damage"), 60f,
                new ConfigDescription("The actual damage dealt by apparitions spawned by Reaper of Souls. Stored internally as a multiplier against the base damage of 150.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe3ApparitionHealth = Config.Bind(new ConfigDefinition("Eternal Reaper - Apparition", "Health"), 840,
                new ConfigDescription("The health of apparitions spawned by Eternal Reaper.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe3ApparitionDamage = Config.Bind(new ConfigDefinition("Eternal Reaper - Apparition", "Damage"), 90f,
                new ConfigDescription("The actual damage dealt by apparitions spawned by Eternal Reaper. Stored internally as a multiplier against the base damage of 150.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe4ApparitionHealth = Config.Bind(new ConfigDefinition("Simple Reaper - Apparition", "Health"), 40,
                new ConfigDescription("The health of apparitions spawned by Simple Reaper.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe4ApparitionDamage = Config.Bind(new ConfigDefinition("Simple Reaper - Apparition", "Damage"), 15f,
                new ConfigDescription("The actual damage dealt by apparitions spawned by Simple Reaper. Stored internally as a multiplier against the base damage of 150.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private static void InitSythe1Config()
        {
            try
            {
                WeaponConfigOptions options = new WeaponConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe1, scythe1Name, scythe1Recipe, scythe1UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    damageSlash         = 50f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 28,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    attackStamina       = 16,
                };
                scythe1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe1Name + " config: " + error);
            }
        }

        private static void InitSythe2Config()
        {
            try
            {
                WeaponConfigOptions options = new WeaponConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe2, scythe2Name, scythe2Recipe, scythe2UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    damageSlash         = 90f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 52,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    attackStamina       = 20,
                };
                scythe2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe2Name + " config: " + error);
            }
        }

        private static void InitSythe3Config()
        {
            try
            {
                WeaponConfigOptions options = new WeaponConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe3, scythe3Name, scythe3Recipe, scythe3UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    damageSlash         = 130f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 64,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    attackStamina       = 20,
                };
                scythe3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe3Name + " config: " + error);
            }
        }

        private static void InitSythe4Config()
        {
            try
            {
                WeaponConfigOptions options = new WeaponConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe4, scythe4Name, scythe4Recipe, scythe4UpgradeRecipe)
                {
                    description         = "Temporary placeholder",
                    craftingStation     = "Workbench",
                    minStationLevel     = 1,
                    damageSlash         = 15f,
                    damageSlashPerLevel = 5f,
                    blockArmor          = 8,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    attackStamina       = 12,
                };
                scythe4.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe4Name + " config: " + error);
            }
        }
    }
}
