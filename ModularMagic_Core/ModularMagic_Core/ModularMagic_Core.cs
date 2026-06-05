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
            PieceManager.OnPiecesRegistered += CreatePieceIcons;
        }

        private void AddItems()
        {
            ItemHelper.Create(prefabs.EitrCrude, PluginConfig.crudeEitr);
            ItemHelper.Create(prefabs.EitrFine, PluginConfig.fineEitr);

            Jotunn.Configs.ItemConfig healStaff = new Jotunn.Configs.ItemConfig();
            healStaff.Name = "Healing staff";
            healStaff.Description = "Staff to heal";
            healStaff.CraftingStation = CraftingStations.Workbench;
            healStaff.AddRequirement("Wood", 10);

            ItemManager.Instance.AddItem(new CustomItem(prefabs.HealStaff, true, healStaff));

            foreach (RuneEntry entry in RuneData.list)
            {
                ImbuementRune imbuementRune = entry.prefab.AddComponent<ImbuementRune>();
                imbuementRune.Init(entry.type, entry.value, entry.tier, entry.level, entry.allowedWeapons, entry.nameKey, entry.descriptionKey);
                ItemHelper.Create(entry.prefab, entry.config, true);
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

            prefabs.RuneDamageBluntWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_DamageBlunt");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageBluntWood, true));
            prefabs.RuneDamageBluntStone = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageBlunt_Stone", prefabs.RuneDamageBluntWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageBluntStone, true));
            prefabs.RuneDamageBluntMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageBlunt_Marble", prefabs.RuneDamageBluntWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageBluntMarble, true));
            prefabs.RuneDamageBluntGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageBlunt_Grausten", prefabs.RuneDamageBluntWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageBluntGrausten, true));

            prefabs.RuneDamagePierceWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_DamagePierce");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamagePierceWood, true));
            prefabs.RuneDamagePierceStone = PrefabHelper.CreateClonedVariant("MMC_Rune_DamagePierce_Stone", prefabs.RuneDamagePierceWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamagePierceStone, true));
            prefabs.RuneDamagePierceMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_DamagePierce_Marble", prefabs.RuneDamagePierceWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamagePierceMarble, true));
            prefabs.RuneDamagePierceGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_DamagePierce_Grausten", prefabs.RuneDamagePierceWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamagePierceGrausten, true));

            prefabs.RuneDamageSlashWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_DamageSlash");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashWood, true));
            prefabs.RuneDamageSlashStone = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Stone", prefabs.RuneDamageSlashWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashStone, true));
            prefabs.RuneDamageSlashMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Marble", prefabs.RuneDamageSlashWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashMarble, true));
            prefabs.RuneDamageSlashGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_DamageSlash_Grausten", prefabs.RuneDamageSlashWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneDamageSlashGrausten, true));

            prefabs.RuneEitrCostWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_EitrCost");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneEitrCostWood, true));
            prefabs.RuneEitrCostStone = PrefabHelper.CreateClonedVariant("MMC_Rune_EitrCost_Stone", prefabs.RuneEitrCostWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneEitrCostStone, true));
            prefabs.RuneEitrCostMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_EitrCost_Marble", prefabs.RuneEitrCostWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneEitrCostMarble, true));
            prefabs.RuneEitrCostGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_EitrCost_Grausten", prefabs.RuneEitrCostWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneEitrCostGrausten, true));

            prefabs.RuneProjectileAccuracyWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_ProjectileAccuracy");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileAccuracyWood, true));
            prefabs.RuneProjectileAccuracyStone = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileAccuracy_Stone", prefabs.RuneProjectileAccuracyWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileAccuracyStone, true));
            prefabs.RuneProjectileAccuracyMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileAccuracy_Marble", prefabs.RuneProjectileAccuracyWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileAccuracyMarble, true));
            prefabs.RuneProjectileAccuracyGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileAccuracy_Grausten", prefabs.RuneProjectileAccuracyWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileAccuracyGrausten, true));

            prefabs.RuneProjectileBurstWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_ProjectileBurst");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileBurstWood, true));
            prefabs.RuneProjectileBurstStone = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileBurst_Stone", prefabs.RuneProjectileBurstWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileBurstStone, true));
            prefabs.RuneProjectileBurstMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileBurst_Marble", prefabs.RuneProjectileBurstWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileBurstMarble, true));
            prefabs.RuneProjectileBurstGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileBurst_Grausten", prefabs.RuneProjectileBurstWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileBurstGrausten, true));

            prefabs.RuneProjectileSpeedWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_ProjectileSpeed");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileSpeedWood, true));
            prefabs.RuneProjectileSpeedStone = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileSpeed_Stone", prefabs.RuneProjectileSpeedWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileSpeedStone, true));
            prefabs.RuneProjectileSpeedMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileSpeed_Marble", prefabs.RuneProjectileSpeedWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileSpeedMarble, true));
            prefabs.RuneProjectileSpeedGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_ProjectileSpeed_Grausten", prefabs.RuneProjectileSpeedWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneProjectileSpeedGrausten, true));

            prefabs.RuneConeWood = assetBundle.LoadAsset<GameObject>("MMC_Rune_Cone");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneConeWood, true));
            prefabs.RuneConeStone = PrefabHelper.CreateClonedVariant("MMC_Rune_Cone_Stone", prefabs.RuneConeWood.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneConeStone, true));
            prefabs.RuneConeMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_Cone_Marble", prefabs.RuneConeWood.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneConeMarble, true));
            prefabs.RuneConeGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_Cone_Grausten", prefabs.RuneConeWood.name, 4);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneConeGrausten, true));

            prefabs.RuneCreaturesStone = assetBundle.LoadAsset<GameObject>("MMC_Rune_Creatures");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneCreaturesStone, true));
            prefabs.RuneCreaturesMarble = PrefabHelper.CreateClonedVariant("MMC_Rune_Creatures_Marble", prefabs.RuneCreaturesStone.name, 2);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneCreaturesMarble, true));
            prefabs.RuneCreaturesGrausten = PrefabHelper.CreateClonedVariant("MMC_Rune_Creatures_Grausten", prefabs.RuneCreaturesStone.name, 3);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RuneCreaturesGrausten, true));

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

            prefabs.HealStaff = assetBundle.LoadAsset<GameObject>("MMC_StaffHealing");
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(assetBundle.LoadAsset<SE_Stats>("Staff_healing_MMC"), true));

            /*
             * Notes:
             * Add global key to staff to prevent smurfing.
             */
        }
    }
}

