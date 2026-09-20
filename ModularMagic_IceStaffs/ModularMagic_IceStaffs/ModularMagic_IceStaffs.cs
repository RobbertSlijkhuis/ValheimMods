using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_IceStaffs.Configs;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Models;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_IceStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_IceStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_IceStaffs";
        public const string PluginName = "ModularMagic_IceStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_IceStaffs Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle _assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            _InitAssetBundle();
            _InitStatusEffects();
            PluginConfig.Init();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += _AddIceStaffs;
            ItemManager.OnItemsRegistered += _LogRecipes;
        }

        private void _LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMIS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= _LogRecipes;
        }

        private void _AddIceStaffs()
        {
            ItemHelper.CreateStaff(prefabs.StaffIce1Prefab, PluginConfig.staffIce1);
            ItemHelper.CreateStaff(prefabs.StaffIce2Prefab, PluginConfig.staffIce2);
            ItemHelper.CreateStaff(prefabs.StaffIce3Prefab, PluginConfig.staffIce3);
            ItemHelper.CreateStaff(prefabs.StaffIceAOEPrefab, PluginConfig.staffIce4);

            PrefabManager.OnVanillaPrefabsAvailable -= _AddIceStaffs;
        }

        private void _InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.WalkFastSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void _InitAssetBundle()
        {
            _assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_icestaffs_dw");

            // Ice assets
            prefabs.StaffIce1Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce1");
            prefabs.StaffIce2Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce2");
            prefabs.StaffIce3Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce3");
            prefabs.StaffIceAOEPrefab = _assetBundle.LoadAsset<GameObject>("MMIS_StaffIceAOE");
            effects.WalkFastSE = _assetBundle.LoadAsset<StatusEffect>("FasterWalk_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("staff_ice_projectile_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_spores_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_spikes_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_smoke_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("IceSheet_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_IceSheetSpawn_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("staff_ice_nova_AOE_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_nova_MMIS"), true));
        }
    }
}

