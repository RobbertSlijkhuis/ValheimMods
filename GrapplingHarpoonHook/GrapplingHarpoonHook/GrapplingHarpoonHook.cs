using BepInEx;
using GrapplingHarpoonHook.Helpers;
using GrapplingHarpoonHook.Models;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace GrapplingHarpoonHook
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class GrapplingHarpoonHook : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.GrapplingHarpoonHook";
        public const string PluginName = "GrapplingHarpoonHook";
        public const string PluginVersion = "0.0.1";
        public static GrapplingHarpoonHook Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PrefabManager.OnVanillaPrefabsAvailable += AttackHelper.ConfigureGrapplingHookSecondaryAttack;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("grapplingharpoonhook_dw");

            prefabs.HarpoonProjectile = assetBundle.LoadAsset<GameObject>("HarpoonHook_Projectile_GHH");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.HarpoonProjectile, true));
            prefabs.HarpoonVfx = assetBundle.LoadAsset<GameObject>("vfx_Harpooned_GHH");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.HarpoonVfx, true));

            effects.Harpoon = assetBundle.LoadAsset<StatusEffect>("Harpooned_GHH");
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Harpoon, true));
        }
    }
}
