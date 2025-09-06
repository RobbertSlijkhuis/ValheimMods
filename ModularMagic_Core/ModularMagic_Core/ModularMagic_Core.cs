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
            Transform acceptTrans = prefabs.ImbuementTable.transform.Find("controls/accept_book");
            itemStandComp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());
            acceptTrans.gameObject.AddComponent<ImbuementTableAccept>();
            prefabs.ImbuementTable.AddComponent<ImbuementTable>();

            PieceHelper.Create(prefabs.ImbuementTable, PluginConfig.piece1);
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;

            foreach (var button in ZInput.instance.m_buttons)
            {
                Jotunn.Logger.LogWarning("============================================");
                Jotunn.Logger.LogWarning("Key: " + button.Key);
                Jotunn.Logger.LogWarning("Name: " + button.Value.Name);
            }
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

            materials.SpellBook = assetBundle.LoadAsset<Material>("Spellbook2_1_1_MMC");
            materials.SpellBookOff = assetBundle.LoadAsset<Material>("Spellbook2_1_1_off_MMC");

            materials.RuneWoodOffA = assetBundle.LoadAsset<Material>("Rune_WoodOff_A_MMC");
            materials.RuneWoodOffB = assetBundle.LoadAsset<Material>("Rune_WoodOff_B_MMC");
            materials.RuneWoodOffC = assetBundle.LoadAsset<Material>("Rune_WoodOff_C_MMC");
            materials.RuneWoodOffD = assetBundle.LoadAsset<Material>("Rune_WoodOff_D_MMC");
            materials.RuneWoodOffE = assetBundle.LoadAsset<Material>("Rune_WoodOff_E_MMC");
            materials.RuneWoodOffF = assetBundle.LoadAsset<Material>("Rune_WoodOff_F_MMC");

            materials.RuneWoodA = assetBundle.LoadAsset<Material>("Rune_Wood_A_MMC");
            materials.RuneWoodB = assetBundle.LoadAsset<Material>("Rune_Wood_B_MMC");
            materials.RuneWoodC = assetBundle.LoadAsset<Material>("Rune_Wood_C_MMC");
            materials.RuneWoodD = assetBundle.LoadAsset<Material>("Rune_Wood_D_MMC");
            materials.RuneWoodE = assetBundle.LoadAsset<Material>("Rune_Wood_E_MMC");
            materials.RuneWoodF = assetBundle.LoadAsset<Material>("Rune_Wood_F_MMC");

            materials.RuneStoneA = assetBundle.LoadAsset<Material>("Rune_Stone_A_MMC");
            materials.RuneStoneB = assetBundle.LoadAsset<Material>("Rune_Stone_B_MMC");
            materials.RuneStoneC = assetBundle.LoadAsset<Material>("Rune_Stone_C_MMC");
            materials.RuneStoneD = assetBundle.LoadAsset<Material>("Rune_Stone_D_MMC");
            materials.RuneStoneE = assetBundle.LoadAsset<Material>("Rune_Stone_E_MMC");
            materials.RuneStoneF = assetBundle.LoadAsset<Material>("Rune_Stone_F_MMC");

            materials.RuneMarbleA = assetBundle.LoadAsset<Material>("Rune_Marble_A_MMC");
            materials.RuneMarbleB = assetBundle.LoadAsset<Material>("Rune_Marble_B_MMC");
            materials.RuneMarbleC = assetBundle.LoadAsset<Material>("Rune_Marble_C_MMC");
            materials.RuneMarbleD = assetBundle.LoadAsset<Material>("Rune_Marble_D_MMC");
            materials.RuneMarbleE = assetBundle.LoadAsset<Material>("Rune_Marble_E_MMC");
            materials.RuneMarbleF = assetBundle.LoadAsset<Material>("Rune_Marble_F_MMC");

            materials.RuneGraustenA = assetBundle.LoadAsset<Material>("Rune_Grausten_A_MMC");
            materials.RuneGraustenB = assetBundle.LoadAsset<Material>("Rune_Grausten_B_MMC");
            materials.RuneGraustenC = assetBundle.LoadAsset<Material>("Rune_Grausten_C_MMC");
            materials.RuneGraustenD = assetBundle.LoadAsset<Material>("Rune_Grausten_D_MMC");
            materials.RuneGraustenE = assetBundle.LoadAsset<Material>("Rune_Grausten_E_MMC");
            materials.RuneGraustenF = assetBundle.LoadAsset<Material>("Rune_Grausten_F_MMC");
        }
    }
}

