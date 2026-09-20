using BepInEx;
using Jotunn.Configs;
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
        public static CustomMaterials materials = new CustomMaterials();

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
            PieceManager.OnPiecesRegistered += CreatePieceIcons;
        }

        private void AddItems()
        {
            ItemHelper.Create(prefabs.EitrCrude, PluginConfig.crudeEitr);
            ItemHelper.Create(prefabs.EitrFine, PluginConfig.fineEitr);

            foreach (RuneEntry entry in RuneData.list)
            {
                // The rune has a model per level, that is how many levels (qualities) the item has
                int maxLevel = RuneModelHelper.CountModels(entry.prefab);

                if (maxLevel < 1)
                {
                    Jotunn.Logger.LogError($"[Imbuements] Rune '{entry.id}' has no models (attach/rune_1), skipping it");
                    continue;
                }

                if (entry.tiers.Length != maxLevel)
                    Jotunn.Logger.LogWarning($"[Imbuements] Rune '{entry.id}' has {maxLevel} models but {entry.tiers.Length} tiers");

                // Above 1 the game shows the quality and lets the item be upgraded
                entry.prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality = maxLevel;

                ImbuementRune imbuementRune = entry.prefab.AddComponent<ImbuementRune>();
                imbuementRune.Init(entry.id, entry.type, entry.value, maxLevel, entry.tiers, entry.allowedWeapons, entry.nameKey, entry.descriptionKey);
                ImbuementHelper.RegisterRune(imbuementRune);

                Sprite[] icons = RuneIconHelper.Render(entry.prefab, maxLevel);
                ItemHelper.Create(entry.prefab, entry.config, icons[0]);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddItems;
        }

        private void AddPieces()
        {
            ItemStand itemStandComp = prefabs.RuneTable.transform.Find("itemstand").gameObject.GetComponent<ItemStand>();
            Transform acceptTrans = prefabs.RuneTable.transform.Find("controls/accept_book");
            itemStandComp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());

            foreach (RuneEntry entry in RuneData.list)
            {
                itemStandComp.m_unsupportedItems.Add(entry.prefab.GetComponent<ItemDrop>());
            }

            acceptTrans.gameObject.AddComponent<RuneTableAccept>();
            prefabs.RuneTable.AddComponent<RuneTable>();

            PieceHelper.Create(prefabs.RuneTable, PluginConfig.piece1, true);
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void CreatePieceIcons()
        {
            CraftingStation craftingStation = prefabs.RuneTable.GetComponent<CraftingStation>();
            Piece piece = prefabs.RuneTable.GetComponent<Piece>();

            RenderManager.RenderRequest request = new RenderManager.RenderRequest(prefabs.RuneTable);
            request.Rotation = RenderManager.IsometricRotation;

            Sprite icon = RenderManager.Instance.Render(request);
            piece.m_icon = icon;
            craftingStation.m_icon = icon;

            PieceManager.OnPiecesRegistered -= CreatePieceIcons;
        }

        /**
         * Loads and registers the prefab of a rune. The name is fixed and the same on every client, the level of the rune is the quality of the item
         */
        private GameObject AddRunePrefab(string name)
        {
            GameObject prefab = assetBundle.LoadAsset<GameObject>(name);
            prefab.AddComponent<RuneItemModel>();
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
            return prefab;
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
            prefabs.RuneTable = assetBundle.LoadAsset<GameObject>("MMC_RuneTable");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneTable, true));
            prefabs.SaveFX = assetBundle.LoadAsset<GameObject>("fx_save_MMC");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SaveFX, true));

            prefabs.RuneDamageBlunt = AddRunePrefab("MMC_Rune_DamageBlunt");
            prefabs.RuneDamagePierce = AddRunePrefab("MMC_Rune_DamagePierce");
            prefabs.RuneDamageSlash = AddRunePrefab("MMC_Rune_DamageSlash");
            prefabs.RuneEitrCost = AddRunePrefab("MMC_Rune_EitrCost");
            prefabs.RuneProjectileAccuracy = AddRunePrefab("MMC_Rune_ProjectileAccuracy");
            prefabs.RuneProjectileBurst = AddRunePrefab("MMC_Rune_ProjectileBurst");
            prefabs.RuneProjectileSpeed = AddRunePrefab("MMC_Rune_ProjectileSpeed");
            prefabs.RuneCone = AddRunePrefab("MMC_Rune_Cone");
            prefabs.RuneCreatures = AddRunePrefab("MMC_Rune_Creatures");
            prefabs.RuneNova = AddRunePrefab("MMC_Rune_Nova");
            prefabs.RuneRain = AddRunePrefab("MMC_Rune_Rain");

            materials.ImbuementTable = assetBundle.LoadAsset<Material>("Altar_MMC");
            materials.SpellBook = assetBundle.LoadAsset<Material>("Spellbook2_1_1_MMC");
            materials.SpellBookOff = assetBundle.LoadAsset<Material>("Spellbook2_1_1_off_MMC");
            materials.RuneGhost = assetBundle.LoadAsset<Material>("Rune_Ghost_MMC");

            /*
             * Notes:
             * Add global key to staff to prevent smurfing.
             */
        }
    }
}

