using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_LightningStaffs.Configs;
using ModularMagic_LightningStaffs.Helpers;
using ModularMagic_LightningStaffs.Models;
using UnityEngine;

namespace ModularMagic_LightningStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_LightningStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_LightningStaffs";
        public const string PluginName = "ModularMagic_LightningStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_LightningStaffs Instance;

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            ConfigStaffs.Init();

            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_LightningStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMLS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddLightningStaffs()
        {
            ItemHelper.CreateStaff(prefabs.staffLightning1Prefab, ConfigStaffs.staffLightning1);
            ItemHelper.CreateStaff(prefabs.staffLightning2Prefab, ConfigStaffs.staffLightning2);
            ItemHelper.CreateStaff(prefabs.staffLightning3Prefab, ConfigStaffs.staffLightning3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaffs;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_lightningstaffs_dw");

            // Lightning assets
            prefabs.staffLightning1Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning1");
            prefabs.staffLightning2Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning2");
            prefabs.staffLightning3Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning3");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_lightning_projectile_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_lightning_nova_AOE_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("cinder_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_MMLS"), true));
        }
    }
}

