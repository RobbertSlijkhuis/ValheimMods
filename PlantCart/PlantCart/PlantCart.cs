using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using PlantCart.Components;
using PlantCart.Configs;
using PlantCart.Helpers;
using PlantCart.Models;
using UnityEngine;

namespace PlantCart
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class PlantCart : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.PlantCart";
        public const string PluginName = "PlantCart";
        public const string PluginVersion = "1.0.0";
        public static PlantCart Instance;

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public static string currentTrader;

        public void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();

            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddPieces()
        {
            ItemHelper.CreateMaterial(prefabs.Plow, PluginConfig.rustedPlow);

            prefabs.PlantCart.AddComponent<Planter>();
            PieceHelper.Create(prefabs.PlantCart, PluginConfig.piece1);

            currentTrader = PluginConfig.rustedPlow.trader.Value;
            GameObject prefab = PrefabManager.Instance.GetPrefab(currentTrader);
            Trader comp = prefab.GetComponent<Trader>();
            comp.m_items.Add(UpdateHelper.CreateNewTradeItem());

            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("plantcart_dw");

            prefabs.PlantCart = assetBundle.LoadAsset<GameObject>("PlantCart_PC");
            prefabs.Plow = assetBundle.LoadAsset<GameObject>("Plow_PC");
        }
    }
}

