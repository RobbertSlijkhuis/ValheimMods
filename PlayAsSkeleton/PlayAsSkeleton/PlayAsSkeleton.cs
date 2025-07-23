using BepInEx;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using PlayAsSkeleton.Components;
using PlayAsSkeleton.Configs;
using PlayAsSkeleton.Models;
using System;
using System.Reflection;
using UnityEngine;

namespace PlayAsSkeleton
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
    internal class PlayAsSkeleton : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.PlayAsSkeleton";
        public const string PluginName = "PlayAsSkeleton";
        public const string PluginVersion = "0.0.1";
        public static PlayAsSkeleton Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        public CustomMeshes meshes = new CustomMeshes();
        private ButtonConfig utilityModeButton;
        public static long playerID = -1;

        public static readonly string playerDataKey = "PlayerData_PAS";
        public static readonly int playerIDHash = "PlayerID_PAS".GetStableHashCode();
        public static readonly int isSkeletonHash = "IsSkeleton_PAS".GetStableHashCode();
        public static readonly int skinHash = "Skin_PAS".GetStableHashCode();
        public static readonly int canSwimHash = "CanSwim_PAS".GetStableHashCode();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            InitInputs();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += ResolveSkins;
        }

        private void Update()
        {
            try
            {
                if (ZInput.instance == null || utilityModeButton == null || !ZInput.GetButtonDown(utilityModeButton.Name) || !Player.m_localPlayer)
                    return;

                Player player = Player.GetPlayer(PlayAsSkeleton.playerID);
                SkeletonPAS comp = player.gameObject.GetComponent<SkeletonPAS>();
                comp.settingsGUI.ShowGUI();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not show settings GUI: " + e);
            }
        }

        private void ResolveSkins()
        {
            try
            {
                materials.Normal = PrefabManager.Instance.GetPrefab("Skeleton").GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
                materials.Poison = PrefabManager.Instance.GetPrefab("Skeleton_Poison").GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
                materials.Burned = PrefabManager.Instance.GetPrefab("Skeleton_Hildir").GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
                materials.Dark = PrefabManager.Instance.GetPrefab("Skeleton_Friendly").GetComponentInChildren<SkinnedMeshRenderer>().materials[0];

                materials.NormalEyes = PrefabManager.Instance.GetPrefab("Skeleton").GetComponentInChildren<MeshRenderer>().materials[0];
                materials.PoisonEyes = PrefabManager.Instance.GetPrefab("Skeleton_Poison").GetComponentInChildren<MeshRenderer>().materials[0];
                materials.BurnedEyes = PrefabManager.Instance.GetPrefab("Skeleton_Hildir").GetComponentInChildren<MeshRenderer>().materials[0];
                materials.DarkEyes = PrefabManager.Instance.GetPrefab("Skeleton_Friendly").GetComponentInChildren<MeshRenderer>().materials[0];

                PrefabManager.OnVanillaPrefabsAvailable -= ResolveSkins;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not resolve skins: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= ResolveSkins;
            }
        }

        private void InitInputs()
        {
            try
            {
                utilityModeButton = new ButtonConfig
                {
                    Name = "Settings",
                    ShortcutConfig = PluginConfig.configLanternModKey,
                };

                InputManager.Instance.AddButton(PluginGUID, utilityModeButton);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + e);
            }
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("playasskeleton_dw");

            prefabs.SkeletonBase = assetBundle.LoadAsset<GameObject>("skeleton_base_PAS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SkeletonBase, true));
            meshes.Skeleton = assetBundle.LoadAsset<Mesh>("Skeleton_PAS");
            materials.LoyalBones = assetBundle.LoadAsset<Material>("Skeleton_LoyalBones_PAS");
        }
    }
}

