using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using Testing.Configs;
using Testing.Helpers;
using Testing.Models;
using Testing.Types;
using UnityEngine;

namespace Testing
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Testing : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.Testing.SplashMeads";
        public const string PluginName = "Testing.SplashMeads";
        public const string PluginVersion = "0.0.1";
        public static Testing Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        public static int barlyWineSplashHash;
        public static int frostResistSplashHash;
        public static int poisonResistSplashHash;

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

            PrefabManager.OnVanillaPrefabsAvailable -= AddStuff;
        }

        private void InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.BarlyWineSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.FrostResistSplash, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.PoisonResistSplash, true));

            barlyWineSplashHash = effects.BarlyWineSplash.name.GetStableHashCode();
            frostResistSplashHash = effects.FrostResistSplash.name.GetStableHashCode();
            poisonResistSplashHash = effects.PoisonResistSplash.name.GetStableHashCode();
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("testing_dw");

            prefabs.BarlyWineSplash = assetBundle.LoadAsset<GameObject>("SM_BarleyWineSplash");
            prefabs.BarlyWineSplashProjectile = assetBundle.LoadAsset<GameObject>("barlywine_projectile_SM");
            prefabs.BarlyWineSplashExplosion = assetBundle.LoadAsset<GameObject>("barlywine_explosion_SM");
            prefabs.BarlyWineSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_fireresist_SM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.BarlyWineSplashFX, true));
            effects.BarlyWineSplash = assetBundle.LoadAsset<StatusEffect>("Potion_barleywine_splash_SM");

            prefabs.FrostResistSplash = assetBundle.LoadAsset<GameObject>("SM_FrostResistSplash");
            prefabs.FrostResistSplashProjectile = assetBundle.LoadAsset<GameObject>("frostresist_projectile_SM");
            prefabs.FrostResistSplashExplosion = assetBundle.LoadAsset<GameObject>("frostresist_explosion_SM");
            prefabs.FrostResistSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_frostresist_SM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FrostResistSplashFX, true));
            effects.FrostResistSplash = assetBundle.LoadAsset<StatusEffect>("Potion_frostresist_splash_SM");

            prefabs.PoisonResistSplash = assetBundle.LoadAsset<GameObject>("SM_PoisonResistSplash");
            prefabs.PoisonResistSplashProjectile = assetBundle.LoadAsset<GameObject>("poisonresist_projectile_SM");
            prefabs.PoisonResistSplashExplosion = assetBundle.LoadAsset<GameObject>("poisonresist_explosion_SM");
            prefabs.PoisonResistSplashFX = assetBundle.LoadAsset<GameObject>("fx_Potion_poisonresist_SM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashProjectile, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashExplosion, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PoisonResistSplashFX, true));
            effects.PoisonResistSplash = assetBundle.LoadAsset<StatusEffect>("Potion_poisonresist_splash_SM");
        }
    }
}

