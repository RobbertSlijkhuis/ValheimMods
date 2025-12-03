using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SeedCart.Components;
using SeedCart.Configs;
using SeedCart.Helpers;
using SeedCart.Models;
using SeedCart.Types;
using UnityEngine;

namespace SeedCart
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class SeedCart : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SeedCart";
        public const string PluginName = "SeedCart";
        public const string PluginVersion = "0.0.1";
        public static SeedCart Instance;

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public static string currentTrader;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();

            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddPieces()
        {
            ItemHelper.CreateMaterial(prefabs.Plow, PluginConfig.rustedPlow);

            prefabs.SeedCart.AddComponent<MovementChecker>();
            PieceHelper.Create(prefabs.SeedCart, PluginConfig.piece1);

            currentTrader = PluginConfig.rustedPlow.trader.Value;
            GameObject prefab = PrefabManager.Instance.GetPrefab(currentTrader);
            Trader comp = prefab.GetComponent<Trader>();
            comp.m_items.Add(UpdateHelper.CreateNewTradeItem());

            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("seedcart_dw");

            prefabs.SeedCart = assetBundle.LoadAsset<GameObject>("SeedCart_SC");
            prefabs.Plow = assetBundle.LoadAsset<GameObject>("Plow_SC");
        }
    }
}

