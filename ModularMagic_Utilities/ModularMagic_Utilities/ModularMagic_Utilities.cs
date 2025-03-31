using BepInEx;
using Jotunn.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.helpers;
using ModularMagic_Utilities.Models;
using System;
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
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();

        private ButtonConfig utilityModeButton;

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            ConfigUtilities.Init();
            InitInputs();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddUtilities;
            Jotunn.Logger.LogInfo("ModularMagic_Utilities has been initialised");
            // ItemManager.OnItemsRegistered += LogRecipes;
        }

        //private void LogRecipes()
        //{
        //    ObjectDB.instance.m_recipes.ForEach(r =>
        //    {
        //        if (r.name.Contains("MMU_"))
        //        Jotunn.Logger.LogInfo(r.name);
        //    });

        //    ItemManager.OnItemsRegistered -= LogRecipes;
        //}

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
                            ItemDrop.ItemData itemData = Player.m_localPlayer.m_utilityItem;

                            if (itemData == null || itemData.m_shared == null)
                            {
                                Jotunn.Logger.LogWarning("Item Data is null");
                                return;
                            }

                            if (itemData.m_shared.m_name == ConfigUtilities.lantern1.name.Value)
                            {
                                UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.lantern1Mat, ModularMagic_Utilities.Instance.materials.lantern1OffMat);
                            }
                            else if (itemData.m_shared.m_name == ConfigUtilities.lantern2.name.Value)
                            {
                                UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.lantern2Mat, ModularMagic_Utilities.Instance.materials.lantern2OffMat);
                            }
                            else if (itemData.m_shared.m_name == ConfigUtilities.lantern3.name.Value)
                            {
                               UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.lantern3Mat, ModularMagic_Utilities.Instance.materials.lantern3OffMat);
                            }
                        }
                    }
                }
            }
        }

        private void AddUtilities()
        {
            try
            {
                ItemHelper.create(prefabs.spellbook1Prefab, ConfigUtilities.spellbook1);
                ItemHelper.create(prefabs.spellbook2Prefab, ConfigUtilities.spellbook2);
                ItemHelper.create(prefabs.spellbook3Prefab, ConfigUtilities.spellbook3);
                ItemHelper.create(prefabs.lantern1Prefab, ConfigUtilities.lantern1, true);
                ItemHelper.create(prefabs.lantern2Prefab, ConfigUtilities.lantern2, true);
                ItemHelper.create(prefabs.lantern3Prefab, ConfigUtilities.lantern3, true);

                PrefabManager.OnVanillaPrefabsAvailable -= AddUtilities;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not add utilities: " + error);
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
                    Name = "Weapon mode",
                    ShortcutConfig = ConfigUtilities.configUtilityModeKey,
                };

                InputManager.Instance.AddButton(PluginGUID, utilityModeButton);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + error);
            }
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_utilities_dw");

            // Materials
            materials.lantern1Mat = assetBundle.LoadAsset<Material>("MMU_MysticalLanternMat");
            materials.lantern1OffMat = assetBundle.LoadAsset<Material>("MMU_MysticalLanternMat_Off");
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

