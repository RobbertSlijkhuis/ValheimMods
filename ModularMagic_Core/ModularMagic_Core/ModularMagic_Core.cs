using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Components;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_Core
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Core : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Core";
        public const string PluginName = "ModularMagic_Core";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Core Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();

        public static readonly string imbuementMMESDataKey = "Imbuements_MMES";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddMaterials;
            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddMaterials()
        {
            ItemHelper.CreateMaterial(prefabs.EitrCrude, PluginConfig.crudeEitr);
            ItemHelper.CreateMaterial(prefabs.EitrFine, PluginConfig.fineEitr);
            
            PrefabManager.OnVanillaPrefabsAvailable -= AddMaterials;
        }

        private void AddPieces()
        {
            ItemStand itemStandComp = prefabs.ImbuementTable.transform.Find("itemstand").gameObject.GetComponent<ItemStand>();
            Transform acceptTrans = prefabs.ImbuementTable.transform.Find("controls/accept");
            itemStandComp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());
            acceptTrans.gameObject.AddComponent<ImbuementTableAccept>();
            prefabs.ImbuementTable.AddComponent<ImbuementTable>();

            PieceHelper.Create(prefabs.ImbuementTable, PluginConfig.piece1);
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_core_dw");

            prefabs.EitrCrude = assetBundle.LoadAsset<GameObject>("MMC_EitrCrude");
            prefabs.EitrFine = assetBundle.LoadAsset<GameObject>("MMC_EitrFine");
            prefabs.ImbuementTable = assetBundle.LoadAsset<GameObject>("MMC_ImbuementTable");

            materials.RuneA = assetBundle.LoadAsset<Material>("Rune_A_MMC");
            materials.RuneB = assetBundle.LoadAsset<Material>("Rune_B_MMC");
            materials.RuneC = assetBundle.LoadAsset<Material>("Rune_C_MMC");
            materials.RuneD = assetBundle.LoadAsset<Material>("Rune_D_MMC");
            materials.RuneE = assetBundle.LoadAsset<Material>("Rune_E_MMC");
            materials.RuneF = assetBundle.LoadAsset<Material>("Rune_F_MMC");

            materials.RuneEmissiveA = assetBundle.LoadAsset<Material>("RuneEmissive_A_MMC");
            materials.RuneEmissiveB = assetBundle.LoadAsset<Material>("RuneEmissive_B_MMC");
            materials.RuneEmissiveC = assetBundle.LoadAsset<Material>("RuneEmissive_C_MMC");
            materials.RuneEmissiveD = assetBundle.LoadAsset<Material>("RuneEmissive_D_MMC");
            materials.RuneEmissiveE = assetBundle.LoadAsset<Material>("RuneEmissive_E_MMC");
            materials.RuneEmissiveF = assetBundle.LoadAsset<Material>("RuneEmissive_F_MMC");
        }
    }
}

