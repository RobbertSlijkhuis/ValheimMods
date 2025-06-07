using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Utilities.components;
using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static EffectList;

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

            PrefabManager.OnVanillaPrefabsAvailable += _AddPieces;
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
                ItemHelper.Create(prefabs.Spellbook1, PluginConfig.spellbook1);
                ItemHelper.Create(prefabs.Spellbook2, PluginConfig.spellbook2);
                ItemHelper.Create(prefabs.Spellbook3, PluginConfig.spellbook3);
                ItemHelper.Create(prefabs.Lantern1, PluginConfig.lantern1, true);
                ItemHelper.Create(prefabs.Lantern2, PluginConfig.lantern2, true);
                ItemHelper.Create(prefabs.Lantern3, PluginConfig.lantern3, true);

                PrefabManager.OnVanillaPrefabsAvailable -= _AddUtilities;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add utilities: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= _AddUtilities;
            }
        }

        private void _AddPieces()
        {
            try
            {
                GameObject itemStand = PrefabManager.Instance.GetPrefab("itemstandh");
                ItemStand itemStandcomp = itemStand.GetComponent<ItemStand>();
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook1.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook2.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook3.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern1.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern2.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern3.GetComponent<ItemDrop>());

                ZNetView netView = prefabs.MarbleItemstand.GetComponent<ZNetView>();
                ItemStand marbleItemStandcomp = prefabs.MarbleItemstand.transform.Find("itemstand").gameObject.GetComponent<ItemStand>();
                marbleItemStandcomp.m_netViewOverride = netView;
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook1.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook2.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook3.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern1.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern2.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern3.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());
                WeatherZone weatherZone = prefabs.MarbleItemstand.transform.Find("weatherzone").gameObject.AddComponent<WeatherZone>();
                prefabs.MarbleItemstand.transform.Find("radius_controls").gameObject.AddComponent<WeatherZoneRadiusControls>();
                prefabs.MarbleItemstand.transform.Find("projector_controls").gameObject.AddComponent<WeatherZoneProjectorControls>();
                prefabs.MarbleItemstand.transform.Find("dome_controls").gameObject.AddComponent<WeatherZoneDomeControls>();


                List<EffectData> startList = new List<EffectData>();
                EffectData startSFX = new EffectData();
                startSFX.m_prefab = PrefabManager.Instance.GetPrefab("sfx_shieldgenerator_startup");
                startSFX.m_enabled = true;
                startSFX.m_variant = -1;

                EffectData startVFX = new EffectData();
                startVFX.m_prefab = prefabs.ActivationFX;
                startVFX.m_enabled = true;
                startVFX.m_variant = -1;
                startList.Add(startSFX);
                startList.Add(startVFX);

                List<EffectData> stopList = new List<EffectData>();
                EffectData stopSFX = new EffectData();
                stopSFX.m_prefab = PrefabManager.Instance.GetPrefab("sfx_shieldgenerator_shutdown");
                stopSFX.m_enabled = true;
                stopSFX.m_variant = -1;
                stopList.Add(stopSFX);

                weatherZone.startEffects.m_effectPrefabs = startList.ToArray();
                weatherZone.stopEffects.m_effectPrefabs = stopList.ToArray();

                PieceHelper.Create(prefabs.MarbleItemstand, PluginConfig.piece1);
                
                PrefabManager.OnVanillaPrefabsAvailable -= _AddPieces;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add pieces: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= _AddPieces;
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
            materials.Lantern1 = _assetBundle.LoadAsset<Material>("MythicalLanternMat_MMU");
            materials.Lantern1Off = _assetBundle.LoadAsset<Material>("MythicalLanternMat_Off_MMU");
            materials.Lantern2 = _assetBundle.LoadAsset<Material>("EverwinterLanternMat_MMU");
            materials.Lantern2Off = _assetBundle.LoadAsset<Material>("EverwinterLanternMat_Off_MMU");
            materials.Lantern3 = _assetBundle.LoadAsset<Material>("MistcallerLanternMat_MMU");
            materials.Lantern3Off = _assetBundle.LoadAsset<Material>("MistcallerLanternMat_Off_MMU");
            materials.AltarParticles = _assetBundle.LoadAsset<Material>("AltarParticals_MMU");

            // Books
            prefabs.Spellbook1 = _assetBundle.LoadAsset<GameObject>("MMU_SpellbookOfTheHearth");
            prefabs.Spellbook2 = _assetBundle.LoadAsset<GameObject>("MMU_GrimoireOfTheStorm");
            prefabs.Spellbook3 = _assetBundle.LoadAsset<GameObject>("MMU_CodexOfTheAsgardianSorcerer");

            // Lanterns
            prefabs.Lantern1 = _assetBundle.LoadAsset<GameObject>("MMU_MythicalLantern");
            prefabs.Lantern2 = _assetBundle.LoadAsset<GameObject>("MMU_EverwinterLantern");
            prefabs.Lantern3 = _assetBundle.LoadAsset<GameObject>("MMU_MistcallerLantern");

            // Pieces
            prefabs.MarbleItemstand = _assetBundle.LoadAsset<GameObject>("MMU_MarbleItemstand");
            prefabs.ActivationFX = _assetBundle.LoadAsset<GameObject>("fx_activation_MMU");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ActivationFX, true));
        }
    }
}

