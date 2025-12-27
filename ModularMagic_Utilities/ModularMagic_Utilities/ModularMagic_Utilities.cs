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
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomRPC lanternRPC;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        private ButtonConfig utilityModeButton;

        public static readonly int lanternStatusHash = "LanternStatus_MMU".GetStableHashCode();
        public static readonly int weatherZoneLockedHash = "WeatherZoneLocked_MMU".GetStableHashCode();
        public static readonly int weatherZoneDomeHash = "WeatherZoneDome_MMU".GetStableHashCode();
        public static readonly int weatherZoneRadiusHash = "WeatherZoneRadius_MMU".GetStableHashCode();
        public static readonly int weatherZoneParticlesHash = "WeatherZoneDomeParticles_MMU".GetStableHashCode();
        public static readonly int weatherZoneLightColorPresetHash = "WeatherZoneLightColorPreset_MMU".GetStableHashCode();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            InitInputs();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
            PrefabManager.OnVanillaPrefabsAvailable += AddUtilities;
            // ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMU_"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
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

        private void AddUtilities()
        {
            try
            {
                ItemHelper.Create(prefabs.Spellbook1, PluginConfig.spellbook1);
                ItemHelper.Create(prefabs.Spellbook2, PluginConfig.spellbook2);
                ItemHelper.Create(prefabs.Spellbook3, PluginConfig.spellbook3);
                ItemHelper.Create(prefabs.Lantern1, PluginConfig.lantern1, true);
                ItemHelper.Create(prefabs.Lantern2, PluginConfig.lantern2, true);
                ItemHelper.Create(prefabs.Lantern3, PluginConfig.lantern3, true);

                PrefabManager.OnVanillaPrefabsAvailable -= AddUtilities;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add utilities: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= AddUtilities;
            }
        }

        private void AddPieces()
        {
            try
            {
                GameObject itemStand = PrefabManager.Instance.GetPrefab("itemstandh");
                ItemStand itemStandcomp = itemStand.gameObject.GetComponent<ItemStand>();
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook1.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook2.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Spellbook3.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern1.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern2.GetComponent<ItemDrop>());
                itemStandcomp.m_supportedItems.Add(prefabs.Lantern3.GetComponent<ItemDrop>());

                ZNetView netView = prefabs.MarbleItemstand.GetComponent<ZNetView>();
                ItemStand marbleItemStandcomp = prefabs.MarbleItemstand.transform.Find("itemstand").gameObject.GetComponent<ItemStand>();
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook1.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook2.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Spellbook3.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern1.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern2.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_supportedItems.Add(prefabs.Lantern3.GetComponent<ItemDrop>());
                marbleItemStandcomp.m_unsupportedItems.Add(PrefabManager.Instance.GetPrefab("Hammer").GetComponent<ItemDrop>());
                WeatherZoneMMU weatherZone = prefabs.MarbleItemstand.transform.Find("weatherzone").gameObject.AddComponent<WeatherZoneMMU>();
                prefabs.MarbleItemstand.transform.Find("controls/settings").gameObject.AddComponent<WeatherZoneSettingsControlsMMU>();
                prefabs.MarbleItemstand.transform.Find("controls/projector").gameObject.AddComponent<WeatherZoneProjectorControlsMMU>();

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

                //CustomPiece piece = PieceManager.Instance.GetPiece("MMU_MarbleItemstand");
                //Jotunn.Logger.LogWarning("Piece name: " + piece?.PiecePrefab?.name);
                //piece.Piece.m_resources
                
                PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add pieces: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
            }
        }

        /**
         * Initialise the inputs of this mod
         */
        private void InitInputs()
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
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_utilities_dw");

            // Materials
            materials.Lantern1 = assetBundle.LoadAsset<Material>("MythicalLanternMat_MMU");
            materials.Lantern1Off = assetBundle.LoadAsset<Material>("MythicalLanternMat_Off_MMU");
            materials.Lantern2 = assetBundle.LoadAsset<Material>("EverwinterLanternMat_MMU");
            materials.Lantern2Off = assetBundle.LoadAsset<Material>("EverwinterLanternMat_Off_MMU");
            materials.Lantern3 = assetBundle.LoadAsset<Material>("MistcallerLanternMat_MMU");
            materials.Lantern3Off = assetBundle.LoadAsset<Material>("MistcallerLanternMat_Off_MMU");
            materials.AltarParticles = assetBundle.LoadAsset<Material>("AltarParticals_MMU");

            // Books
            prefabs.Spellbook1 = assetBundle.LoadAsset<GameObject>("MMU_SpellbookOfTheHearth");
            prefabs.Spellbook2 = assetBundle.LoadAsset<GameObject>("MMU_GrimoireOfTheStorm");
            prefabs.Spellbook3 = assetBundle.LoadAsset<GameObject>("MMU_CodexOfTheAsgardianSorcerer");

            // Lanterns
            prefabs.Lantern1 = assetBundle.LoadAsset<GameObject>("MMU_MythicalLantern");
            prefabs.Lantern2 = assetBundle.LoadAsset<GameObject>("MMU_EverwinterLantern");
            prefabs.Lantern3 = assetBundle.LoadAsset<GameObject>("MMU_MistcallerLantern");

            // Pieces
            prefabs.MarbleItemstand = assetBundle.LoadAsset<GameObject>("MMU_MarbleItemstand");
            prefabs.ActivationFX = assetBundle.LoadAsset<GameObject>("fx_activation_MMU");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ActivationFX, true));
        }
    }
}

