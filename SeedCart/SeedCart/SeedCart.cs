using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SeedCart.Models;
using UnityEngine;

namespace SeedCart
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SeedCart : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SeedCart";
        public const string PluginName = "SeedCart";
        public const string PluginVersion = "0.0.1";
        public static SeedCart Instance;

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();

            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddPieces()
        {
            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Enabled = true;
            pieceConfig.Name = "Seed Cart";
            pieceConfig.Description = "A card that plants your seeds!";
            pieceConfig.PieceTable = PieceTables.Hammer;
            pieceConfig.Category = PieceCategories.Misc;
            pieceConfig.AddRequirement("Wood", 20);

            prefabs.SeedCart.AddComponent<MovementChecker>();

            PieceManager.Instance.AddPiece(new CustomPiece(prefabs.SeedCart, true, pieceConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("seedcart_dw");

            prefabs.SeedCart = assetBundle.LoadAsset<GameObject>("SeedCart_SC");
        }
    }
}

