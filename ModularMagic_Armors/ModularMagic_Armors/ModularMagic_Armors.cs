using BepInEx;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_Armors
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Armors : BaseUnityPlugin
    {
        // ADD FROST RESITANCE TO DARKWIZARD CLOAK, OR FREEZE TO DEATH IN MOUNTAINS

        public const string PluginGUID = "DeathWizsh.ModularMagic_Armors";
        public const string PluginName = "ModularMagic_Armors";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Armors Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);
        public object configManager;

        private AssetBundle assetBundle;
        public ItemSnapShots itemSnapShots = new ItemSnapShots();
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        public CustomStatusEffects effects = new CustomStatusEffects();
        public CustomSprites sprites = new CustomSprites();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;

            Assembly? bepinexConfigManager = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");
            Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
            configManager = configManagerType == null ? null : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);

            InitAssetBundle();
            ArmorSetHelper.Init();
            InitStatusEffects();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddShamanArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddWraithArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddFrostWolfArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddDarkWizardArmor;
            PrefabManager.OnVanillaPrefabsAvailable += ApplyEffectsEmblaArmor;
            ItemManager.OnItemsRegistered += AdjustEitrWeaveArmor;
            ItemManager.OnItemsRegistered += AdjustEmblaArmor;
            // ItemManager.OnItemsRegistered += LogRecipes;
        }

        public void RefreshConfigManager()
        {
            configManager?.GetType().GetMethod("BuildSettingList")!.Invoke(configManager, Array.Empty<object>());
        }

        //private void LogRecipes()
        //{
        //    ObjectDB.instance.m_recipes.ForEach(r =>
        //    {
        //        if (r.name.Contains("MMA"))
        //            Jotunn.Logger.LogInfo(r.name);
        //    });

        //    ItemManager.OnItemsRegistered -= LogRecipes;
        //}

        private void AddShamanArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddShamanArmor;

            ItemHelper.Create(prefabs.ShamanHelmetPrefab, PluginConfig.armor1Helmet);
            ItemHelper.Create(prefabs.ShamanCapePrefab, PluginConfig.armor1Cape);
            ItemHelper.Create(prefabs.ShamanChestPrefab, PluginConfig.armor1Chest);
            ItemHelper.Create(prefabs.ShamanLegsPrefab, PluginConfig.armor1Legs);

            ArmorEffectSetup.ApplyShaman(prefabs);
        }

        private void AddWraithArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddWraithArmor;

            ItemHelper.Create(prefabs.WraithHelmetPrefab, PluginConfig.armor2Helmet);
            ItemHelper.Create(prefabs.WraithCapePrefab, PluginConfig.armor2Cape);
            ItemHelper.Create(prefabs.WraithChestPrefab, PluginConfig.armor2Chest);
            ItemHelper.Create(prefabs.WraithLegsPrefab, PluginConfig.armor2Legs);

            ArmorEffectSetup.ApplyWraith(prefabs);
        }
        
        private void AddFrostWolfArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddFrostWolfArmor;

            ItemHelper.Create(prefabs.FrostWolfHelmetPrefab, PluginConfig.armor3Helmet);
            ItemHelper.Create(prefabs.FrostWolfCapePrefab, PluginConfig.armor3Cape);
            ItemHelper.Create(prefabs.FrostWolfChestPrefab, PluginConfig.armor3Chest);
            ItemHelper.Create(prefabs.FrostWolfLegsPrefab, PluginConfig.armor3Legs);

            ArmorEffectSetup.ApplyFrostWolf(prefabs);
        }

        private void AddDarkWizardArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddDarkWizardArmor;

            ItemHelper.Create(prefabs.DarkWizardHelmetPrefab, PluginConfig.armor4Helmet);
            ItemHelper.Create(prefabs.DarkWizardCapePrefab, PluginConfig.armor4Cape);
            ItemHelper.Create(prefabs.DarkWizardChestPrefab, PluginConfig.armor4Chest);
            ItemHelper.Create(prefabs.DarkWizardLegsPrefab, PluginConfig.armor4Legs);

            ArmorEffectSetup.ApplyDarkWizard(prefabs);
        }

        private void AdjustEitrWeaveArmor()
        {
            ItemManager.OnItemsRegistered -= AdjustEitrWeaveArmor;

            prefabs.EitrWeaveHelmetPrefab = PrefabManager.Instance.GetPrefab("HelmetMage");
            prefabs.EitrWeaveChestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest");
            prefabs.EitrWeaveLegsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs");

            // Vanilla prefab edits persist for the whole session, so a snapshot must never be taken from an already adjusted prefab
            if (itemSnapShots.EitrWeaveHelmetStats == null)
            {
                itemSnapShots.EitrWeaveHelmetStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveHelmetPrefab);
                itemSnapShots.EitrWeaveChestStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveChestPrefab);
                itemSnapShots.EitrWeaveLegsStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveLegsPrefab);

                itemSnapShots.EitrWeaveHelmetRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveHelmetPrefab);
                itemSnapShots.EitrWeaveChestRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveChestPrefab);
                itemSnapShots.EitrWeaveLegsRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveLegsPrefab);
            }

            PluginConfig.InitArmor5Config(PluginConfig.adjustEitrWeave.Value);

            if (!PluginConfig.adjustEitrWeave.Value)
                return;

            ArmorSetOptions armorSetOptions = new ArmorSetOptions()
            {
                name = "EitrWeaveSet_MMA",
                size = 3,
                statusEffect = effects.EitrWeaveArmorSetSE,
            };

            ItemHelper.Adjust(prefabs.EitrWeaveHelmetPrefab, PluginConfig.armor5Helmet, armorSetOptions);
            ItemHelper.Adjust(prefabs.EitrWeaveChestPrefab, PluginConfig.armor5Chest, armorSetOptions);
            ItemHelper.Adjust(prefabs.EitrWeaveLegsPrefab, PluginConfig.armor5Legs, armorSetOptions);
        }

        private void AdjustEmblaArmor()
        {
            ItemManager.OnItemsRegistered -= AdjustEmblaArmor;

            // Vanilla prefab edits persist for the whole session, so a snapshot must never be taken from an already adjusted prefab
            if (itemSnapShots.EmblaHelmetStats == null)
            {
                itemSnapShots.EmblaHelmetStats = UpdateHelper.TakeSnapShot(prefabs.EmblaHelmetPrefab);
                itemSnapShots.EmblaChestStats = UpdateHelper.TakeSnapShot(prefabs.EmblaChestPrefab);
                itemSnapShots.EmblaLegsStats = UpdateHelper.TakeSnapShot(prefabs.EmblaLegsPrefab);

                itemSnapShots.EmblaHelmetRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaHelmetPrefab);
                itemSnapShots.EmblaChestRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaChestPrefab);
                itemSnapShots.EmblaLegsRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaLegsPrefab);
            }

            PluginConfig.InitArmor6Config(PluginConfig.adjustEmbla.Value);

            if (!PluginConfig.adjustEmbla.Value)
                return;

            ArmorSetOptions armorSetOptions = new ArmorSetOptions()
            {
                name = "EmblaSet_MMA",
                size = 3,
                statusEffect = effects.EmblaArmorSetSE,
            };

            ItemHelper.Adjust(prefabs.EmblaHelmetPrefab, PluginConfig.armor6Helmet, armorSetOptions);
            ItemHelper.Adjust(prefabs.EmblaChestPrefab, PluginConfig.armor6Chest, armorSetOptions);
            ItemHelper.Adjust(prefabs.EmblaLegsPrefab, PluginConfig.armor6Legs, armorSetOptions);
        }

        private void ApplyEffectsEmblaArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= ApplyEffectsEmblaArmor;

            prefabs.EmblaHelmetPrefab = PrefabManager.Instance.GetPrefab("HelmetMage_Ashlands");
            prefabs.EmblaChestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest_Ashlands");
            prefabs.EmblaLegsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs_Ashlands");

            // One option for the stats, the retexture and the effects of the set. Retexture and effects need a restart, the stats follow the setting live
            if (!PluginConfig.adjustEmbla.Value)
                return;

            EmblaHelper.ApplyReskin(prefabs, materials, sprites);
            EmblaHelper.InjectEffects(prefabs);
        }

        private void InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.ShamanArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.WraithArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.FrostWolfArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.DarkWizardArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.EitrWeaveArmorSetSE, true));
            effects.EmblaArmorSetSE.m_icon = sprites.EmblaHood;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.EmblaArmorSetSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_armors_dw");

            // Sprites
            sprites.EmblaHood = assetBundle.LoadAsset<Sprite>("EmblaHelmet_MMA");
            sprites.EmblaChest = assetBundle.LoadAsset<Sprite>("EmblaChest_MMA");
            sprites.EmblaLegs = assetBundle.LoadAsset<Sprite>("EmblaLegs_MMA");

            // Materials
            materials.EmblaArmor = assetBundle.LoadAsset<Material>("AshlandsMageArmor_red_MMA");
            materials.EmblaChest = assetBundle.LoadAsset<Material>("AshlandsMageArmorChest_red_MMA");
            materials.EmblaLegs = assetBundle.LoadAsset<Material>("AshlandsMageArmorLegs_red_MMA");

            // Shaman armor
            prefabs.ShamanHelmetPrefab = assetBundle.LoadAsset<GameObject>("MMA_ShamanHelmet");
            prefabs.ShamanCapePrefab = assetBundle.LoadAsset<GameObject>("MMA_ShamanCape");
            prefabs.ShamanChestPrefab = assetBundle.LoadAsset<GameObject>("MMA_ShamanChest");
            prefabs.ShamanLegsPrefab = assetBundle.LoadAsset<GameObject>("MMA_ShamanLegs");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("Shaman_armor_set_effect_MMA"), true));
            effects.ShamanArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_ShamanArmor_MMA");

            // Wraith armor
            prefabs.WraithHelmetPrefab = assetBundle.LoadAsset<GameObject>("MMA_WraithHelmet");
            prefabs.WraithCapePrefab = assetBundle.LoadAsset<GameObject>("MMA_WraithCape");
            prefabs.WraithChestPrefab = assetBundle.LoadAsset<GameObject>("MMA_WraithChest");
            prefabs.WraithLegsPrefab = assetBundle.LoadAsset<GameObject>("MMA_WraithLegs");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("Wraith_armor_set_effect_MMA"), true));
            effects.WraithArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_WraithArmor_MMA");

            // Wolf armor
            prefabs.FrostWolfHelmetPrefab = assetBundle.LoadAsset<GameObject>("MMA_FrostWolfHelmet");
            prefabs.FrostWolfCapePrefab = assetBundle.LoadAsset<GameObject>("MMA_FrostWolfCape");
            prefabs.FrostWolfChestPrefab = assetBundle.LoadAsset<GameObject>("MMA_FrostWolfChest");
            prefabs.FrostWolfLegsPrefab = assetBundle.LoadAsset<GameObject>("MMA_FrostWolfLegs");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("FrostWolf_armor_set_effect_MMA"), true));
            effects.FrostWolfArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_FrostWolfArmor_MMA");

            // DarkWizard armor
            prefabs.DarkWizardHelmetPrefab = assetBundle.LoadAsset<GameObject>("MMA_DarkWizardHelmet");
            prefabs.DarkWizardCapePrefab = assetBundle.LoadAsset<GameObject>("MMA_DarkWizardCape");
            prefabs.DarkWizardChestPrefab = assetBundle.LoadAsset<GameObject>("MMA_DarkWizardChest");
            prefabs.DarkWizardLegsPrefab = assetBundle.LoadAsset<GameObject>("MMA_DarkWizardLegs");
            prefabs.DarkWizardFootstepPrefab = assetBundle.LoadAsset<GameObject>("DarkWizard_Footstep_MMA");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.DarkWizardFootstepPrefab, true));
            effects.DarkWizardArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_DarkWizardArmor_MMA");

            // Eitr-weave armor
            effects.EitrWeaveArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_EitrWeaveArmor_MMA");

            // Embla armor
            prefabs.EmblaEffects = assetBundle.LoadAsset<GameObject>("EmblaHood_Effects_MMA");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.EmblaEffects, true));
            effects.EmblaArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_EmblaArmor_MMA");
        }
    }
}

