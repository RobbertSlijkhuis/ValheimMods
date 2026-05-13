using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_BloodMagic
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_BloodMagic : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_BloodMagic";
        public const string PluginName = "ModularMagic_BloodMagic";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_BloodMagic Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += SetupPrefabs;
            PrefabManager.OnVanillaPrefabsAvailable += AddItems;
        }

        private void SetupPrefabs()
        {
            GameObject ghost = PrefabManager.Instance.GetPrefab("Ghost");
            Tameable tameable = ghost.AddComponent<Tameable>();
            tameable.m_fedDuration = 0f;
            tameable.m_tamingTime = 0f;

            PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
        }

        private void AddItems()
        {
            ItemHelper.Create(prefabs.Scythe1, PluginConfig.scythe1);

            PrefabManager.OnVanillaPrefabsAvailable -= AddItems;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_bloodmagic_dw");

            prefabs.Scythe1 = assetBundle.LoadAsset<GameObject>("MMBM_Scythe1");
        }
    }
}

