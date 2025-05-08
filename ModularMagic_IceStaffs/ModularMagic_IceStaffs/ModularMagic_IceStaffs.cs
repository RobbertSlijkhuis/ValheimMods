using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_IceStaffs.Configs;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Models;
using UnityEngine;

namespace ModularMagic_IceStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_IceStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_IceStaffs";
        public const string PluginName = "ModularMagic_IceStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_IceStaffs Instance;

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

            PrefabManager.OnVanillaPrefabsAvailable += AddIceStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_IceStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMIS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddIceStaffs()
        {
            ItemHelper.CreateStaff(prefabs.staffIce1Prefab, ConfigStaffs.staffIce1);
            ItemHelper.CreateStaff(prefabs.staffIce2Prefab, ConfigStaffs.staffIce2);
            ItemHelper.CreateStaff(prefabs.staffIce3Prefab, ConfigStaffs.staffIce3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddIceStaffs;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_icestaffs_dw");

            // Ice assets
            prefabs.staffIce1Prefab = assetBundle.LoadAsset<GameObject>("MMIS_staffIce1");
            prefabs.staffIce2Prefab = assetBundle.LoadAsset<GameObject>("MMIS_staffIce2");
            prefabs.staffIce3Prefab = assetBundle.LoadAsset<GameObject>("MMIS_staffIce3");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_ice_projectile_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_ice_spores_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_ice_spikes_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_smoke_MMIS"), true));
        }
    }
}

