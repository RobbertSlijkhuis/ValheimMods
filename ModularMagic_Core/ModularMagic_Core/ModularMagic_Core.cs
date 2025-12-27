using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Data;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.localization;
using ModularMagic_Core.Models;
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
        public static CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();

        public static readonly string imbuementDataKey = "Imbuements_MMC";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        public void Awake()
        {
            Instance = this;
            InitAssetBundle();
            LocaleEnglish.Init();
            RuneData.Init();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddItems;
            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddItems()
        {
            foreach (RuneEntry entry in RuneData.list)
            {
                ImbuementRune imbuementRune = entry.prefab.AddComponent<ImbuementRune>();
                imbuementRune.Init(entry.type, entry.value, entry.tier, entry.level, entry.allowedWeapons, entry.nameKey, entry.descriptionKey);
                ItemHelper.Create(entry.prefab, entry.config, true);
            }

            ItemHelper.Create(prefabs.EitrCrude, PluginConfig.crudeEitr);
            ItemHelper.Create(prefabs.EitrFine, PluginConfig.fineEitr);
            
            PrefabManager.OnVanillaPrefabsAvailable -= AddItems;
        }

        private void AddPieces()
        {
            ItemStand itemStandComp = prefabs.ImbuementTable.transform.Find("itemstand").gameObject.GetComponent<ItemStand>();
            Transform acceptTrans = prefabs.ImbuementTable.transform.Find("controls/accept_book");
            itemStandComp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());

            foreach (RuneEntry entry in RuneData.list)
            {
                itemStandComp.m_unsupportedItems.Add(entry.prefab.GetComponent<ItemDrop>());
            }

            acceptTrans.gameObject.AddComponent<ImbuementTableAccept>();
            prefabs.ImbuementTable.AddComponent<ImbuementTable>();

            PieceHelper.Create(prefabs.ImbuementTable, PluginConfig.piece1, true);
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_core_dw");

            prefabs.EitrCrude = assetBundle.LoadAsset<GameObject>("MMC_EitrCrude");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.EitrCrude, true));
            prefabs.EitrFine = assetBundle.LoadAsset<GameObject>("MMC_EitrFine");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.EitrFine, true));
            prefabs.ImbuementTable = assetBundle.LoadAsset<GameObject>("MMC_ImbuementTable");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ImbuementTable, true));

            prefabs.RuneAccuracyWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_Accuracy");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneAccuracyWood, true));
            prefabs.RuneAccuracyStone = PrefabHelper.CreateClonedVariant("MMC_Rune_Accuracy_Stone", prefabs.RuneAccuracyWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneAccuracyStone, true));
            prefabs.RuneAccuracyMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_Accuracy_Marble", prefabs.RuneAccuracyWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneAccuracyMarble, true));
            prefabs.RuneAccuracyGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_Accuracy_Grausten", prefabs.RuneAccuracyWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneAccuracyGrausten, true));

            prefabs.RuneDamageSlashWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_DamageSlash");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashWood, true));
            prefabs.RuneDamageSlashStone = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Stone", prefabs.RuneDamageSlashWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashStone, true));
            prefabs.RuneDamageSlashMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Marble", prefabs.RuneDamageSlashWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashMarble, true));
            prefabs.RuneDamageSlashGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Grausten", prefabs.RuneDamageSlashWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashGrausten, true));

            prefabs.RuneNovaStone = assetBundle.LoadAsset<GameObject>("MMC_Rune_Nova");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneNovaStone, true));
            prefabs.RuneNovaMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_Nova_Marble", prefabs.RuneNovaStone.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneNovaMarble, true));
            prefabs.RuneNovaGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_Nova_Grausten", prefabs.RuneNovaStone.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneNovaGrausten, true));

            prefabs.RuneRainStone = assetBundle.LoadAsset<GameObject>("MMC_Rune_Rain");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneRainStone, true));
            prefabs.RuneRainMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_Rain_Marble", prefabs.RuneRainStone.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneRainMarble, true));
            prefabs.RuneRainGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_Rain_Grausten", prefabs.RuneRainStone.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneRainGrausten, true));

            materials.ImbuementTable = assetBundle.LoadAsset<Material>("Altar_MMC");
            materials.SpellBook = assetBundle.LoadAsset<Material>("Spellbook2_1_1_MMC");
            materials.SpellBookOff = assetBundle.LoadAsset<Material>("Spellbook2_1_1_off_MMC");
            materials.RuneGhost = assetBundle.LoadAsset<Material>("Rune_Ghost_MMC");

            /*
             * Notes
             * 
             * Add global key to staff to prevent smurfing.
             * Add tier to runes and weapons to prevent high level runes from being put in lower weapons
             */
        }
    }
}

