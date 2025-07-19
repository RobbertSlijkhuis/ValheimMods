using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_Core
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Core : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Core";
        public const string PluginName = "ModularMagic_Core";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Core Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

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
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMC"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddMaterials()
        {
            ItemHelper.CreateMaterial(prefabs.EitrCrude, PluginConfig.crudeEitr);
            ItemHelper.CreateMaterial(prefabs.EitrFine, PluginConfig.fineEitr);

            PrefabManager.OnVanillaPrefabsAvailable -= AddMaterials;
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_core_dw");

            prefabs.EitrCrude = assetBundle.LoadAsset<GameObject>("MMC_EitrCrude");
            prefabs.EitrFine = assetBundle.LoadAsset<GameObject>("MMC_EitrFine");
        }
    }
}

