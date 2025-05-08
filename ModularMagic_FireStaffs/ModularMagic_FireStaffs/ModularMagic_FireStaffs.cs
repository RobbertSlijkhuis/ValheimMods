using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_FireStaffs.Configs;
using ModularMagic_FireStaffs.Helpers;
using ModularMagic_FireStaffs.Models;
using UnityEngine;

namespace ModularMagic_FireStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_FireStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_FireStaffs";
        public const string PluginName = "ModularMagic_FireStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_FireStaffs Instance;

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

            PrefabManager.OnVanillaPrefabsAvailable += AddFireStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_FireStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMFS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddFireStaffs()
        {
            ItemHelper.CreateStaff(prefabs.staffFire1Prefab, ConfigStaffs.staffFire1);
            ItemHelper.CreateStaff(prefabs.staffFire2Prefab, ConfigStaffs.staffFire2);
            ItemHelper.CreateStaff(prefabs.staffFire3Prefab, ConfigStaffs.staffFire3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddFireStaffs;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_firestaffs_dw");

            // Fire assets
            prefabs.staffFire1Prefab = assetBundle.LoadAsset<GameObject>("MMFS_EmberspireTheFlamebinder");
            prefabs.staffFire2Prefab = assetBundle.LoadAsset<GameObject>("MMFS_WyrmflareOfTheCindercoil");
            prefabs.staffFire3Prefab = assetBundle.LoadAsset<GameObject>("MMFS_IgnivarFangOfSurtur");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_fire_projectile_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("cinder_MMFS"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_windup_MMFS"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_nova_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_spores_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_spikes_MMFS"), true));
        }
    }
}

