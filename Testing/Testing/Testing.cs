using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using Testing.Configs;
using Testing.Models;
using UnityEngine;

namespace Testing
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Testing : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.Testing.TwitchIntergration";
        public const string PluginName = "Testing.TwitchIntergration";
        public const string PluginVersion = "0.0.1";
        public static Testing Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void InitAssetBundle()
        {
            //assetBundle = AssetUtils.LoadAssetBundleFromResources("testing_dw");
        }
    }
}

