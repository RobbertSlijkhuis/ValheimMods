using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SplashMeads.Configs;
using SplashMeads.Helpers;
using SplashMeads.Models;
using System.Reflection;
using UnityEngine;

namespace SplashMeads
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SplashMeads : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SplashMeads";
        public const string PluginName = "SplashMeads";
        public const string PluginVersion = "1.0.3";
        public static SplashMeads Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        public static readonly int barleyWineHash = 1458612846;
        public static readonly int frostResistHash = -1768438774;
        public static readonly int poisonResistHash = -568360536;
        public static readonly int ratatoskHash = 1965486703;
        public static readonly int vananidirHash = -1907265002;
        public static readonly int antiStingHash = -1157133715;
        public static readonly int majorHealthHash = 1251702474;
        public static readonly int mediumHealthHash = -67041294;
        public static readonly int minorHealthHash = -590058386;
        public static int barleyWineSplashHash;
        public static int frostResistSplashHash;
        public static int poisonResistSplashHash;
        public static int ratatoskSplashHash;
        public static int vananidirSplashHash;
        public static int antiStingSplashHash;
        public static int majorHealthSplashHash;
        public static int mediumHealthSplashHash;
        public static int minorHealthSplashHash;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            InitStatusEffects();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddStuff;
        }

        private void AddStuff()
        {
            ItemHelper.Create(prefabs.BarlyWineSplash, PluginConfig.mead1);
            ItemHelper.Create(prefabs.FrostResistSplash, PluginConfig.mead2);
            ItemHelper.Create(prefabs.PoisonResistSplash, PluginConfig.mead3);
            ItemHelper.Create(prefabs.RatatoskSplash, PluginConfig.mead4);
            ItemHelper.Create(prefabs.VananidirSplash, PluginConfig.mead5);
            ItemHelper.Create(prefabs.AntiStingSplash, PluginConfig.mead6);
            ItemHelper.Create(prefabs.MajorHealthSplash, PluginConfig.mead7);
            ItemHelper.Create(prefabs.MediumHealthSplash, PluginConfig.mead8);
            ItemHelper.Create(prefabs.MinorHealthSplash, PluginConfig.mead9);

            PrefabManager.OnVanillaPrefabsAvailable -= AddStuff;
        }

        private void InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.BarlyWineSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.FrostResistSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.PoisonResistSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.RatatoskSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.VananidirSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.AntiStingSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.MajorHealthSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.MediumHealthSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.MinorHealthSplash, true));

            barleyWineSplashHash = effects.BarlyWineSplash.name.GetStableHashCode();
            frostResistSplashHash = effects.FrostResistSplash.name.GetStableHashCode();
            poisonResistSplashHash = effects.PoisonResistSplash.name.GetStableHashCode();
            ratatoskSplashHash = effects.RatatoskSplash.name.GetStableHashCode();
            vananidirSplashHash = effects.VananidirSplash.name.GetStableHashCode();
            antiStingSplashHash = effects.AntiStingSplash.name.GetStableHashCode();
            majorHealthSplashHash = effects.MajorHealthSplash.name.GetStableHashCode();
            mediumHealthSplashHash = effects.MediumHealthSplash.name.GetStableHashCode();
            minorHealthSplashHash = effects.MinorHealthSplash.name.GetStableHashCode();
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("splashmeads_dw");
            GameObject enemyHud = assetBundle.LoadAsset<GameObject>("EnemyHud_SM");

            prefabs.BarlyWineSplash = assetBundle.LoadAsset<GameObject>("SM_SplashBarleyWine");
            prefabs.BarlyWineSplashProjectile = assetBundle.LoadAsset<GameObject>("barlywine_projectile_SM");
            prefabs.BarlyWineSplashExplosion = assetBundle.LoadAsset<GameObject>("barlywine_explosion_SM");
            prefabs.BarlyWineSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_fireresist_SM");
            prefabs.BarlyWineSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashBarleyWine").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashFX, true));
            effects.BarlyWineSplash = assetBundle.LoadAsset<StatusEffect>("Potion_barleywine_splash_SM");

            prefabs.FrostResistSplash = assetBundle.LoadAsset<GameObject>("SM_SplashFrostResist");
            prefabs.FrostResistSplashProjectile = assetBundle.LoadAsset<GameObject>("frostresist_projectile_SM");
            prefabs.FrostResistSplashExplosion = assetBundle.LoadAsset<GameObject>("frostresist_explosion_SM");
            prefabs.FrostResistSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_frostresist_SM");
            prefabs.FrostResistSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashFrostResist").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashFX, true));
            effects.FrostResistSplash = assetBundle.LoadAsset<StatusEffect>("Potion_frostresist_splash_SM");

            prefabs.PoisonResistSplash = assetBundle.LoadAsset<GameObject>("SM_SplashPoisonResist");
            prefabs.PoisonResistSplashProjectile = assetBundle.LoadAsset<GameObject>("poisonresist_projectile_SM");
            prefabs.PoisonResistSplashExplosion = assetBundle.LoadAsset<GameObject>("poisonresist_explosion_SM");
            prefabs.PoisonResistSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_poisonresist_SM");
            prefabs.PoisonResistSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashPoisonResist").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashFX, true));
            effects.PoisonResistSplash = assetBundle.LoadAsset<StatusEffect>("Potion_poisonresist_splash_SM");

            prefabs.RatatoskSplash = assetBundle.LoadAsset<GameObject>("SM_SplashRatatosk");
            prefabs.RatatoskSplashProjectile = assetBundle.LoadAsset<GameObject>("ratatosk_projectile_SM");
            prefabs.RatatoskSplashExplosion = assetBundle.LoadAsset<GameObject>("ratatosk_explosion_SM");
            prefabs.RatatoskSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_ratatosk_SM");
            prefabs.RatatoskSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashRatatosk").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashFX, true));
            effects.RatatoskSplash = assetBundle.LoadAsset<StatusEffect>("Potion_hasty_splash_SM");

            prefabs.VananidirSplash = assetBundle.LoadAsset<GameObject>("SM_SplashVananidir");
            prefabs.VananidirSplashProjectile = assetBundle.LoadAsset<GameObject>("vananidir_projectile_SM");
            prefabs.VananidirSplashExplosion = assetBundle.LoadAsset<GameObject>("vananidir_explosion_SM");
            prefabs.VananidirSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_vananidir_SM");
            prefabs.VananidirSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashVananidir").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.VananidirSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.VananidirSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.VananidirSplashFX, true));
            effects.VananidirSplash = assetBundle.LoadAsset<StatusEffect>("Potion_swimmer_splash_SM");

            prefabs.AntiStingSplash = assetBundle.LoadAsset<GameObject>("SM_SplashAntiSting");
            prefabs.AntiStingSplashProjectile = assetBundle.LoadAsset<GameObject>("antisting_projectile_SM");
            prefabs.AntiStingSplashExplosion = assetBundle.LoadAsset<GameObject>("antisting_explosion_SM");
            prefabs.AntiStingSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_antisting_SM");
            prefabs.AntiStingSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashAntiSting").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.AntiStingSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.AntiStingSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.AntiStingSplashFX, true));
            effects.AntiStingSplash = assetBundle.LoadAsset<StatusEffect>("Potion_antisting_SM");

            prefabs.MajorHealthSplash = assetBundle.LoadAsset<GameObject>("SM_SplashMajorHealth");
            prefabs.MajorHealthSplashProjectile = assetBundle.LoadAsset<GameObject>("majorhealth_projectile_SM");
            prefabs.MajorHealthSplashExplosion = assetBundle.LoadAsset<GameObject>("majorhealth_explosion_SM");
            prefabs.MajorHealthSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_majorhealth_SM");
            prefabs.MajorHealthSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashMajorHealth").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MajorHealthSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MajorHealthSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MajorHealthSplashFX, true));
            effects.MajorHealthSplash = assetBundle.LoadAsset<StatusEffect>("Potion_health_major_SM");

            prefabs.MediumHealthSplash = assetBundle.LoadAsset<GameObject>("SM_SplashMediumHealth");
            prefabs.MediumHealthSplashProjectile = assetBundle.LoadAsset<GameObject>("mediumhealth_projectile_SM");
            prefabs.MediumHealthSplashExplosion = assetBundle.LoadAsset<GameObject>("mediumhealth_explosion_SM");
            prefabs.MediumHealthSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_mediumhealth_SM");
            prefabs.MediumHealthSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashMediumHealth").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MediumHealthSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MediumHealthSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MediumHealthSplashFX, true));
            effects.MediumHealthSplash = assetBundle.LoadAsset<StatusEffect>("Potion_health_medium_SM");

            prefabs.MinorHealthSplash = assetBundle.LoadAsset<GameObject>("SM_SplashMinorHealth");
            prefabs.MinorHealthSplashProjectile = assetBundle.LoadAsset<GameObject>("minorhealth_projectile_SM");
            prefabs.MinorHealthSplashExplosion = assetBundle.LoadAsset<GameObject>("minorhealth_explosion_SM");
            prefabs.MinorHealthSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_minorhealth_SM");
            prefabs.MinorHealthSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashMinorHealth").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MinorHealthSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MinorHealthSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.MinorHealthSplashFX, true));
            effects.MinorHealthSplash = assetBundle.LoadAsset<StatusEffect>("Potion_health_minor_SM");
        }
    }
}

