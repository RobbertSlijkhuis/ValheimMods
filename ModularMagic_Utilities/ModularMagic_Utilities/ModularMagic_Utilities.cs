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
    [BepInDependency("Azumatt.AzuExtendedPlayerInventory", BepInDependency.DependencyFlags.SoftDependency)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Utilities : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Utilities";
        public const string PluginName = "ModularMagic_Utilities";
        public const string PluginVersion = "0.0.1";
        public static readonly int lanternStatusHashCode = "LanternStatus_MMU".GetStableHashCode();
        public static ModularMagic_Utilities Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomRPC lanternRPC;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        private ButtonConfig utilityModeButton;
        public Dictionary<long, bool> lanternStatusDict = new Dictionary<long, bool>();

        private void Awake()
        {
            Instance = this;
            _InitAssetBundle();
            PluginConfig.Init();
            _InitInputs();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // lanternRPC = RPCHelper.Init();

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
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // KeyboardShortcuts are also injected into the ZInput system
                if (utilityModeButton != null && MessageHud.instance != null)
                {
                    if (ZInput.GetButtonDown(utilityModeButton.Name) && MessageHud.instance.m_msgQeue.Count == 0)
                    {
                        if (Player.m_localPlayer)
                        {
                            try
                            {
                                ItemDrop.ItemData itemData = Player.m_localPlayer.m_utilityItem;

                                if (itemData == null || itemData.m_shared == null)
                                {
                                    Jotunn.Logger.LogWarning("Item Data is null");
                                    return;
                                }

                                // ZPackage package = new ZPackage();
                                int type = UpdateHelper.GetLanternType(itemData);

                                Jotunn.Logger.LogWarning("Type: " + type);

                                if (type != 0)
                                {
                                    //long playerId = Player.m_localPlayer.GetPlayerID();
                                    //package.Write($"{playerId},{type},{!lanternStatusDictionary[playerId]},true");
                                    //lanternRPC.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);

                                    LanternMMU comp = Player.m_localPlayer.GetComponent<LanternMMU>();
                                    long playerId = Player.m_localPlayer.GetPlayerID();
                                    LanternStatus playerStatus = comp.GetPlayerStatus(playerId);

                                    comp.SetPlayerStatus(playerId, !playerStatus.status);
                                }
                            }
                            catch (Exception e)
                            {
                                Jotunn.Logger.LogError(e);
                            }
                        }
                    }
                }
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
                    ShortcutConfig = Configs.PluginConfig.configLanternModKey,
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
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_utilities_dw");

            // Materials
            materials.lantern1Mat = assetBundle.LoadAsset<Material>("MMU_MythicalLanternMat");
            materials.lantern1OffMat = assetBundle.LoadAsset<Material>("MMU_MythicalLanternMat_Off");
            materials.lantern2Mat = assetBundle.LoadAsset<Material>("MMU_EverwinterLanternMat");
            materials.lantern2OffMat = assetBundle.LoadAsset<Material>("MMU_EverwinterLanternMat_Off");
            materials.lantern3Mat = assetBundle.LoadAsset<Material>("MMU_MistcallerLanternMat");
            materials.lantern3OffMat = assetBundle.LoadAsset<Material>("MMU_MistcallerLanternMat_Off");

            // Books
            prefabs.spellbook1Prefab = assetBundle.LoadAsset<GameObject>("MMU_SpellbookOfTheHearth");
            prefabs.spellbook2Prefab = assetBundle.LoadAsset<GameObject>("MMU_GrimoireOfTheStorm");
            prefabs.spellbook3Prefab = assetBundle.LoadAsset<GameObject>("MMU_CodexOfTheAsgardianSorcerer");

            // Lanterns
            prefabs.lantern1Prefab = assetBundle.LoadAsset<GameObject>("MMU_MythicalLantern");
            prefabs.lantern2Prefab = assetBundle.LoadAsset<GameObject>("MMU_EverwinterLantern");
            prefabs.lantern3Prefab = assetBundle.LoadAsset<GameObject>("MMU_MistcallerLantern");
        }
    }
}

