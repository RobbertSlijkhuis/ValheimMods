using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.helpers;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
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

                            Jotunn.Logger.LogWarning("Item name: " + itemData.m_shared.m_name);
                            if (itemData.m_shared.m_name == ConfigUtilities.blackCoreLantern.name.Value)
                            {
                               UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.blackCoreLanternMat, ModularMagic_Utilities.Instance.materials.blackCoreLanternOffMat);
                            }
                            else if (itemData.m_shared.m_name == ConfigUtilities.everWinterLantern.name.Value)
                            {
                                UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.everWinterLanternMat, ModularMagic_Utilities.Instance.materials.everWinterLanternOffMat);
                            }
                            else if (itemData.m_shared.m_name == ConfigUtilities.mysticLantern.name.Value)
                            {
                                UpdateHelper.updateLanternMode(ModularMagic_Utilities.Instance.materials.mysticLanternMat, ModularMagic_Utilities.Instance.materials.mysticLanternOffMat);
                            }
                            Jotunn.Logger.LogWarning("Pressing Lantern mode button!");
                        }
                    }
                }
            }
        }

        private void AddUtilities()
        {
            try
            {
                // Simple Spellbook
                ItemConfig simpleConfig = new ItemConfig();
                simpleConfig.Enabled = ConfigUtilities.simpleSpellbook.enable.Value;
                simpleConfig.CraftingStation = ConfigUtilities.simpleSpellbook.craftingStation.Value;
                simpleConfig.MinStationLevel = ConfigUtilities.simpleSpellbook.minStationLevel.Value;
                RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.simpleSpellbook.recipe.Value, null, null);

                if (simpleRequirements == null || simpleRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_SimpleSpellbook");
                else
                    simpleConfig.Requirements = simpleRequirements;

                ItemDrop simpleDrop = prefabs.simpleSpellbookPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr simpleStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                simpleStatusEffect.name = "MMU_SimpleEitrStatusEffect";
                simpleStatusEffect.m_name = simpleDrop.m_itemData.m_shared.m_name;
                simpleStatusEffect.m_icon = simpleDrop.m_itemData.GetIcon();
                simpleStatusEffect.SetEitr(ConfigUtilities.simpleSpellbook.eitr.Value);
                simpleDrop.m_itemData.m_shared.m_equipStatusEffect = simpleStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.simpleSpellbookPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.simpleSpellbook.name.Value,
                    description = ConfigUtilities.simpleSpellbook.description.Value,
                    eitrRegen = ConfigUtilities.simpleSpellbook.eitrRegen.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.simpleSpellbookPrefab, true, simpleConfig));

                // Advanced Spellbook
                ItemConfig advancedConfig = new ItemConfig();
                advancedConfig.Enabled = ConfigUtilities.advancedSpellbook.enable.Value;
                advancedConfig.CraftingStation = ConfigUtilities.advancedSpellbook.craftingStation.Value;
                advancedConfig.MinStationLevel = ConfigUtilities.advancedSpellbook.minStationLevel.Value;
                RequirementConfig[] advancedRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.advancedSpellbook.recipe.Value, null, null);

                if (advancedRequirements == null || advancedRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_AdvancedSpellbook");
                else
                    advancedConfig.Requirements = advancedRequirements;

                ItemDrop advancedDrop = prefabs.advancedSpellbookPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr advancedStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                advancedStatusEffect.name = "MMU_AdvancedEitrStatusEffect";
                advancedStatusEffect.m_name = advancedDrop.m_itemData.m_shared.m_name;
                advancedStatusEffect.m_icon = advancedDrop.m_itemData.GetIcon();
                advancedStatusEffect.SetEitr(ConfigUtilities.advancedSpellbook.eitr.Value);
                advancedDrop.m_itemData.m_shared.m_equipStatusEffect = advancedStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.advancedSpellbookPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.advancedSpellbook.name.Value,
                    description = ConfigUtilities.advancedSpellbook.description.Value,
                    eitrRegen = ConfigUtilities.advancedSpellbook.eitrRegen.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.advancedSpellbookPrefab, true, advancedConfig));

                // Master Spellbook
                ItemConfig masterConfig = new ItemConfig();
                masterConfig.Enabled = ConfigUtilities.masterSpellbook.enable.Value;
                masterConfig.CraftingStation = ConfigUtilities.masterSpellbook.craftingStation.Value;
                masterConfig.MinStationLevel = ConfigUtilities.masterSpellbook.minStationLevel.Value;
                RequirementConfig[] masterRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.masterSpellbook.recipe.Value, null, null);

                if (masterRequirements == null || masterRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_MasterSpellbook");
                else
                    masterConfig.Requirements = masterRequirements;

                ItemDrop masterDrop = prefabs.masterSpellbookPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr masterStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                masterStatusEffect.name = "MMU_MasterEitrStatusEffect";
                masterStatusEffect.m_name = masterDrop.m_itemData.m_shared.m_name;
                masterStatusEffect.m_icon = masterDrop.m_itemData.GetIcon();
                masterStatusEffect.SetEitr(ConfigUtilities.masterSpellbook.eitr.Value);
                masterDrop.m_itemData.m_shared.m_equipStatusEffect = masterStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.masterSpellbookPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.masterSpellbook.name.Value,
                    description = ConfigUtilities.masterSpellbook.description.Value,
                    eitrRegen = ConfigUtilities.masterSpellbook.eitrRegen.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.masterSpellbookPrefab, true, masterConfig));

                // Mystic Lantern
                ItemConfig mysticConfig = new ItemConfig();
                mysticConfig.Enabled = ConfigUtilities.mysticLantern.enable.Value;
                mysticConfig.CraftingStation = ConfigUtilities.mysticLantern.craftingStation.Value;
                mysticConfig.MinStationLevel = ConfigUtilities.mysticLantern.minStationLevel.Value;
                RequirementConfig[] mysticRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.mysticLantern.recipe.Value, null, null);

                if (mysticRequirements == null || mysticRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_MysticLantern");
                else
                    mysticConfig.Requirements = mysticRequirements;

                ItemDrop mysticDrop = prefabs.mysticLanternPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr mysticStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                mysticStatusEffect.name = "MMU_MysticEitrStatusEffect";
                mysticStatusEffect.m_name = mysticDrop.m_itemData.m_shared.m_name;
                mysticStatusEffect.m_icon = mysticDrop.m_itemData.GetIcon();
                mysticStatusEffect.SetEitr(ConfigUtilities.mysticLantern.eitr.Value);
                mysticDrop.m_itemData.m_shared.m_equipStatusEffect = mysticStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.mysticLanternPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.mysticLantern.name.Value,
                    description = ConfigUtilities.mysticLantern.description.Value,
                    eitrRegen = ConfigUtilities.mysticLantern.eitrRegen.Value,
                    demister = ConfigUtilities.mysticLantern.demister.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.mysticLanternPrefab, true, mysticConfig));

                // Ever Winter Lantern
                ItemConfig EverWinterConfig = new ItemConfig();
                EverWinterConfig.Enabled = ConfigUtilities.everWinterLantern.enable.Value;
                EverWinterConfig.CraftingStation = ConfigUtilities.everWinterLantern.craftingStation.Value;
                EverWinterConfig.MinStationLevel = ConfigUtilities.everWinterLantern.minStationLevel.Value;
                RequirementConfig[] EverWinterRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.everWinterLantern.recipe.Value, null, null);

                if (EverWinterRequirements == null || EverWinterRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_EverWinterLantern");
                else
                    EverWinterConfig.Requirements = EverWinterRequirements;

                ItemDrop EverWinterDrop = prefabs.everWinterLanternPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr EverWinterStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                EverWinterStatusEffect.name = "MMU_EverWinterEitrStatusEffect";
                EverWinterStatusEffect.m_name = EverWinterDrop.m_itemData.m_shared.m_name;
                EverWinterStatusEffect.m_icon = EverWinterDrop.m_itemData.GetIcon();
                EverWinterStatusEffect.SetEitr(ConfigUtilities.everWinterLantern.eitr.Value);
                EverWinterDrop.m_itemData.m_shared.m_equipStatusEffect = EverWinterStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.everWinterLanternPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.everWinterLantern.name.Value,
                    description = ConfigUtilities.everWinterLantern.description.Value,
                    eitrRegen = ConfigUtilities.everWinterLantern.eitrRegen.Value,
                    demister = ConfigUtilities.everWinterLantern.demister.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.everWinterLanternPrefab, true, EverWinterConfig));

                // Black Core Lantern
                ItemConfig BlackCoreConfig = new ItemConfig();
                BlackCoreConfig.Enabled = ConfigUtilities.blackCoreLantern.enable.Value;
                BlackCoreConfig.CraftingStation = ConfigUtilities.blackCoreLantern.craftingStation.Value;
                BlackCoreConfig.MinStationLevel = ConfigUtilities.blackCoreLantern.minStationLevel.Value;
                RequirementConfig[] BlackCoreRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigUtilities.blackCoreLantern.recipe.Value, null, null);

                if (BlackCoreRequirements == null || BlackCoreRequirements.Length == 0)
                    Jotunn.Logger.LogWarning("Could not resolve recipe for: MMU_BlackCoreLantern");
                else
                    BlackCoreConfig.Requirements = BlackCoreRequirements;

                ItemDrop BlackCoreDrop = prefabs.blackCoreLanternPrefab.GetComponent<ItemDrop>();
                StatusEffectWithEitr BlackCoreStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
                BlackCoreStatusEffect.name = "MMU_BlackCoreEitrStatusEffect";
                BlackCoreStatusEffect.m_name = BlackCoreDrop.m_itemData.m_shared.m_name;
                BlackCoreStatusEffect.m_icon = BlackCoreDrop.m_itemData.GetIcon();
                BlackCoreStatusEffect.SetEitr(ConfigUtilities.blackCoreLantern.eitr.Value);
                BlackCoreDrop.m_itemData.m_shared.m_equipStatusEffect = BlackCoreStatusEffect;

                ConfigHelper.UpdateItemDropStats(prefabs.blackCoreLanternPrefab, new UpdateItemDropStatsOptions()
                {
                    name = ConfigUtilities.blackCoreLantern.name.Value,
                    description = ConfigUtilities.blackCoreLantern.description.Value,
                    eitrRegen = ConfigUtilities.blackCoreLantern.eitrRegen.Value,
                    demister = ConfigUtilities.blackCoreLantern.demister.Value,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefabs.blackCoreLanternPrefab, true, BlackCoreConfig));

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
            materials.mysticLanternMat = assetBundle.LoadAsset<Material>("MysticLanternMat_MMU");
            materials.mysticLanternOffMat = assetBundle.LoadAsset<Material>("MysticLanternMat_Off_MMU");
            materials.everWinterLanternMat = assetBundle.LoadAsset<Material>("everWinterLanternMat_MMU");
            materials.everWinterLanternOffMat = assetBundle.LoadAsset<Material>("everWinterLanternMat_Off_MMU");
            materials.blackCoreLanternMat = assetBundle.LoadAsset<Material>("BlackCoreLanternMat_MMU");
            materials.blackCoreLanternOffMat = assetBundle.LoadAsset<Material>("BlackCoreLanternMat_Off_MMU");


            // Books
            prefabs.simpleSpellbookPrefab = assetBundle.LoadAsset<GameObject>("MMU_SimpleSpellbook");
            prefabs.advancedSpellbookPrefab = assetBundle.LoadAsset<GameObject>("MMU_AdvancedSpellbook");
            prefabs.masterSpellbookPrefab = assetBundle.LoadAsset<GameObject>("MMU_MasterSpellbook");

            // Lanterns
            prefabs.mysticLanternPrefab = assetBundle.LoadAsset<GameObject>("MMU_MysticLantern");
            prefabs.everWinterLanternPrefab = assetBundle.LoadAsset<GameObject>("MMU_EverWinterLantern");
            prefabs.blackCoreLanternPrefab = assetBundle.LoadAsset<GameObject>("MMU_BlackCoreLantern");
        }
    }
}

