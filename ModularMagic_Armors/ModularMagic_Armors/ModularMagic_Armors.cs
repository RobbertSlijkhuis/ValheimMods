using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Armors.Models;
using UnityEngine;

namespace ModularMagic_Armors
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Armors : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.ModularMagic_Armors";
        public const string PluginName = "ModularMagic_Armors";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Armors Instance;

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            // ConfigStaffs.Init();

            PrefabManager.OnVanillaPrefabsAvailable += AddArmorsStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_Armor has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMA"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddArmorsStaffs()
        {
            //ItemHelper.CreateStaff(prefabs.staffLightning1Prefab, ConfigStaffs.staffLightning1);
            //ItemHelper.CreateStaff(prefabs.staffLightning2Prefab, ConfigStaffs.staffLightning2);
            //ItemHelper.CreateStaff(prefabs.staffLightning3Prefab, ConfigStaffs.staffLightning3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddArmorsStaffs;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_armor_dw");

            // Armor assets
            //prefabs.staffLightning1Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning1");
            //prefabs.staffLightning2Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning2");
            //prefabs.staffLightning3Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning3");
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_lightning_projectile_MMLS"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("cinder_MMLS"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_MMLS"), true));
        }
    }
}

