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
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SplashMeads : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SplashMeads";
        public const string PluginName = "SplashMeads";
        public const string PluginVersion = "1.0.0";
        public static SplashMeads Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        public static readonly int barleyWineHash = 1458612846;
        public static readonly int frostResistHash = -1768438774;
        public static readonly int poisonResistHash = -568360536;
        public static readonly int ratatoskHash = 1965486703;
        public static int barleyWineSplashHash;
        public static int frostResistSplashHash;
        public static int poisonResistSplashHash;
        public static int ratatoskSplashHash;

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

            PrefabManager.OnVanillaPrefabsAvailable -= AddStuff;
        }


        private void InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.BarlyWineSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.FrostResistSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.PoisonResistSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.RatatoskSplash, true));

            barleyWineSplashHash = effects.BarlyWineSplash.name.GetStableHashCode();
            frostResistSplashHash = effects.FrostResistSplash.name.GetStableHashCode();
            poisonResistSplashHash = effects.PoisonResistSplash.name.GetStableHashCode();
            ratatoskSplashHash = effects.RatatoskSplash.name.GetStableHashCode();
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
            prefabs.RatatoskSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_Ratatosk_SM");
            prefabs.RatatoskSplashHudIcon = enemyHud.transform.Find("HudRoot/HudBase/SplashRatatosk").gameObject;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RatatoskSplashFX, true));
            effects.RatatoskSplash = assetBundle.LoadAsset<StatusEffect>("Potion_hasty_splash_SM");
        }
    }
}

