using System;
using ModularMagic_BloodMagic.Models;

namespace ModularMagic_BloodMagic.Configs
{
    internal static class PluginConfig
    {
        public static string scythe1Name = "Soul Reaper";
        public static string scythe1Recipe = "RoundLog:10, Iron:10, Ectoplasm:10";
        public static string scythe1UpgradeRecipe = "RoundLog:5, Iron:5, Ectoplasm:2";
        public static WeaponConfig scythe1 = new WeaponConfig();

        public static string scythe2Name = "Reaper of Souls";
        public static string scythe2Recipe = "RoundLog:10, Silver:10, Ectoplasm:10";
        public static string scythe2UpgradeRecipe = "RoundLog:5, Silver:5, Ectoplasm:2";
        public static WeaponConfig scythe2 = new WeaponConfig();

        public static void Init()
        {
            InitSythe1Config();
            InitSythe2Config();
        }

        private static void InitSythe1Config()
        {
            try
            {
                WeaponConfigOptions options = new WeaponConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe1, scythe1Name, scythe1Recipe, scythe1UpgradeRecipe)
                {
                    description = "A harvester of souls that can be used in your advantage!",
                    craftingStation = "Forge",
                    minStationLevel = 2,
                    damageSlash = 50f,
                    damageSlashPerLevel = 4f,
                    blockArmor = 28,
                    deflectionForce = 70,
                    attackForce = 70,
                    summonPrefab = "Skeleton"
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
                    description = "A harvester of souls that can be used in your advantage!",
                    craftingStation = "Forge",
                    minStationLevel = 2,
                    damageSlash = 70f,
                    damageSlashPerLevel = 4f,
                    blockArmor = 40,
                    deflectionForce = 70,
                    attackForce = 70,
                    summonPrefab = "BlobTar"
                };
                scythe2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe2Name + " config: " + error);
            }
        }
    }
}
