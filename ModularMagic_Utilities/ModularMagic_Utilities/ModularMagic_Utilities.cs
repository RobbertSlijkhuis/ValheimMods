using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_Utilities
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Utilities : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Utilities";
        public const string PluginName = "ModularMagic_Utilities";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Utilities Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle _assetBundle;
        public CustomRPC lanternRPC;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        private ButtonConfig utilityModeButton;

        public static readonly int lanternStatusHashCode = "LanternStatus_MMU".GetStableHashCode();

        private void Awake()
        {
            Instance = this;
            _InitAssetBundle();
            PluginConfig.Init();
            _InitInputs();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += _AddUtilities;
            ItemManager.OnItemsRegistered += _LogRecipes;
        }

        private void _LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMU_"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= _LogRecipes;
        }
        
        /**
         * Called on every update
         */
        private void Update()
        {
            try
            {
                if (ZInput.instance == null || utilityModeButton == null || !ZInput.GetButtonDown(utilityModeButton.Name) || !Player.m_localPlayer)
                    return;

                ItemDrop.ItemData itemData = Player.m_localPlayer.m_utilityItem;

                if (itemData == null || itemData.m_shared == null)
                {
                    Jotunn.Logger.LogWarning("Item Data is null");
                    return;
                }

                // ZPackage package = new ZPackage();
                int type = LanternHelper.GetLanternType(itemData);

                if (type != 0)
                {
                    long playerId = Player.m_localPlayer.GetPlayerID();
                    LanternMMU comp = Player.m_localPlayer.GetComponent<LanternMMU>();
                    LanternStatus playerStatus = comp.GetPlayerStatus();
                    comp.SetPlayerStatus(playerId, playerStatus != null ? !playerStatus.status : false);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not change lantern mode: " + e);
            }
        }

        private void _AddUtilities()
        {
            try
            {
                ItemHelper.Create(prefabs.spellbook1Prefab, PluginConfig.spellbook1);
                ItemHelper.Create(prefabs.spellbook2Prefab, PluginConfig.spellbook2);
                ItemHelper.Create(prefabs.spellbook3Prefab, PluginConfig.spellbook3);
                ItemHelper.Create(prefabs.lantern1Prefab, PluginConfig.lantern1, true);
                ItemHelper.Create(prefabs.lantern2Prefab, PluginConfig.lantern2, true);
                ItemHelper.Create(prefabs.lantern3Prefab, PluginConfig.lantern3, true);

                GameObject itemStand = PrefabManager.Instance.GetPrefab("itemstandh");
                ItemStand comp = itemStand.GetComponent<ItemStand>();
                comp.m_supportedItems.Add(prefabs.spellbook1Prefab.GetComponent<ItemDrop>());
                comp.m_supportedItems.Add(prefabs.spellbook2Prefab.GetComponent<ItemDrop>());
                comp.m_supportedItems.Add(prefabs.spellbook3Prefab.GetComponent<ItemDrop>());
                comp.m_supportedItems.Add(prefabs.lantern1Prefab.GetComponent<ItemDrop>());
                comp.m_supportedItems.Add(prefabs.lantern2Prefab.GetComponent<ItemDrop>());
                comp.m_supportedItems.Add(prefabs.lantern3Prefab.GetComponent<ItemDrop>());

                PrefabManager.OnVanillaPrefabsAvailable -= _AddUtilities;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add utilities: " + e);
            }
        }

        /**
         * Initialise the inputs of this mod
         */
        private void _InitInputs()
        {
            try
            {
                utilityModeButton = new ButtonConfig
                {
                    Name = "Lantern mode",
                    ShortcutConfig = PluginConfig.configLanternModKey,
                };

                InputManager.Instance.AddButton(PluginGUID, utilityModeButton);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + e);
            }
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void _InitAssetBundle()
        {
            _assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_utilities_dw");

            // Materials
            materials.lantern1Mat = _assetBundle.LoadAsset<Material>("MMU_MythicalLanternMat");
            materials.lantern1OffMat = _assetBundle.LoadAsset<Material>("MMU_MythicalLanternMat_Off");
            materials.lantern2Mat = _assetBundle.LoadAsset<Material>("MMU_EverwinterLanternMat");
            materials.lantern2OffMat = _assetBundle.LoadAsset<Material>("MMU_EverwinterLanternMat_Off");
            materials.lantern3Mat = _assetBundle.LoadAsset<Material>("MMU_MistcallerLanternMat");
            materials.lantern3OffMat = _assetBundle.LoadAsset<Material>("MMU_MistcallerLanternMat_Off");

            // Books
            prefabs.spellbook1Prefab = _assetBundle.LoadAsset<GameObject>("MMU_SpellbookOfTheHearth");
            prefabs.spellbook2Prefab = _assetBundle.LoadAsset<GameObject>("MMU_GrimoireOfTheStorm");
            prefabs.spellbook3Prefab = _assetBundle.LoadAsset<GameObject>("MMU_CodexOfTheAsgardianSorcerer");

            // Lanterns
            prefabs.lantern1Prefab = _assetBundle.LoadAsset<GameObject>("MMU_MythicalLantern");
            prefabs.lantern2Prefab = _assetBundle.LoadAsset<GameObject>("MMU_EverwinterLantern");
            prefabs.lantern3Prefab = _assetBundle.LoadAsset<GameObject>("MMU_MistcallerLantern");
        }
    }
}

