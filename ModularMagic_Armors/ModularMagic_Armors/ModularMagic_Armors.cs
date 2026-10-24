using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using System;
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
            InitInputs();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddSpellbooks;
            PrefabManager.OnVanillaPrefabsAvailable += AddLanterns;
            PrefabManager.OnVanillaPrefabsAvailable += AddItemStandSupport;
            PrefabManager.OnVanillaPrefabsAvailable += AddShamanArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddWraithArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddFrostWolfArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddDarkWizardArmor;
            PrefabManager.OnVanillaPrefabsAvailable += ApplyEffectsEmblaArmor;
            PrefabManager.OnVanillaPrefabsAvailable += FindCallerArmor;
            ItemManager.OnItemsRegistered += AdjustEitrWeaveArmor;
            ItemManager.OnItemsRegistered += AdjustEmblaArmor;
            ItemManager.OnItemsRegistered += AdjustCallerArmor;
        }

        // Polled here and not in a Player.Update patch: Jotunn buttons are read from the plugin's own Update
        private void Update()
        {
            try
            {
                LanternHelper.HandleToggleInput(Player.m_localPlayer);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not handle lantern input: " + e);
            }
        }

        public void RefreshConfigManager()
        {
            configManager?.GetType().GetMethod("BuildSettingList")!.Invoke(configManager, Array.Empty<object>());
        }

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

        private void AddSpellbooks()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddSpellbooks;

            ItemHelper.Create(prefabs.SpellbookHearthPrefab, PluginConfig.spellbook1);
            ItemHelper.Create(prefabs.SpellbookStormPrefab, PluginConfig.spellbook2);
            ItemHelper.Create(prefabs.SpellbookCodexPrefab, PluginConfig.spellbook3);
        }

        private void AddLanterns()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddLanterns;

            ItemHelper.Create(prefabs.LanternMythicalPrefab, PluginConfig.lantern1);
            ItemHelper.Create(prefabs.LanternEverwinterPrefab, PluginConfig.lantern2);
            ItemHelper.Create(prefabs.LanternMistcallerPrefab, PluginConfig.lantern3);

            // Only the Mistcaller lantern keeps the demister
            LanternHelper.Register(prefabs.LanternMythicalPrefab, materials.LanternMythicalOn, materials.LanternMythicalOff, false);
            LanternHelper.Register(prefabs.LanternEverwinterPrefab, materials.LanternEverwinterOn, materials.LanternEverwinterOff, false);
            LanternHelper.Register(prefabs.LanternMistcallerPrefab, materials.LanternMistcallerOn, materials.LanternMistcallerOff, true);
        }

        private void AddItemStandSupport()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddItemStandSupport;

            ItemStandHelper.AddSupportedItems(new[] { "itemstand", "itemstandh" }, new[]
            {
                prefabs.SpellbookHearthPrefab,
                prefabs.SpellbookStormPrefab,
                prefabs.SpellbookCodexPrefab,
                prefabs.LanternMythicalPrefab,
                prefabs.LanternEverwinterPrefab,
                prefabs.LanternMistcallerPrefab,
            });
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

        private void FindCallerArmor()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= FindCallerArmor;

            prefabs.CallerHelmetPrefab = PrefabManager.Instance.GetPrefab("HelmetDNMage");
            prefabs.CallerCapePrefab = PrefabManager.Instance.GetPrefab("CapeDeepNorthMage");
            prefabs.CallerChestPrefab = PrefabManager.Instance.GetPrefab("ArmorDeepNorthMageChest");
            prefabs.CallerLegsPrefab = PrefabManager.Instance.GetPrefab("ArmorDeepNorthMagelegs");
        }

        private void AdjustCallerArmor()
        {
            ItemManager.OnItemsRegistered -= AdjustCallerArmor;

            if (prefabs.CallerHelmetPrefab == null || prefabs.CallerCapePrefab == null || prefabs.CallerChestPrefab == null || prefabs.CallerLegsPrefab == null)
            {
                Jotunn.Logger.LogWarning("[Caller] A prefab of the set was not found, the set is left as vanilla");
                return;
            }

            // The recipes (made with moulds) stay vanilla, so only the stats are snapshotted. Vanilla prefab edits persist for the whole session, so a snapshot must never be taken from an already adjusted prefab
            if (itemSnapShots.CallerHelmetStats == null)
            {
                itemSnapShots.CallerHelmetStats = UpdateHelper.TakeSnapShot(prefabs.CallerHelmetPrefab);
                itemSnapShots.CallerCapeStats = UpdateHelper.TakeSnapShot(prefabs.CallerCapePrefab);
                itemSnapShots.CallerChestStats = UpdateHelper.TakeSnapShot(prefabs.CallerChestPrefab);
                itemSnapShots.CallerLegsStats = UpdateHelper.TakeSnapShot(prefabs.CallerLegsPrefab);
            }

            PluginConfig.InitArmor7Config(PluginConfig.adjustCaller.Value);

            if (!PluginConfig.adjustCaller.Value)
                return;

            ArmorSetOptions armorSetOptions = new ArmorSetOptions()
            {
                name = "CallerSet_MMA",
                size = 4,
                statusEffect = effects.CallerArmorSetSE,
            };

            ItemHelper.Adjust(prefabs.CallerHelmetPrefab, PluginConfig.armor7Helmet, armorSetOptions);
            ItemHelper.Adjust(prefabs.CallerCapePrefab, PluginConfig.armor7Cape, armorSetOptions);
            ItemHelper.Adjust(prefabs.CallerChestPrefab, PluginConfig.armor7Chest, armorSetOptions);
            ItemHelper.Adjust(prefabs.CallerLegsPrefab, PluginConfig.armor7Legs, armorSetOptions);
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
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.CallerArmorSetSE, true));
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

            // The Caller armor
            effects.CallerArmorSetSE = assetBundle.LoadAsset<StatusEffect>("SetEffect_CallerArmor_MMA");

            // Spell books
            prefabs.SpellbookHearthPrefab = assetBundle.LoadAsset<GameObject>("MMA_SpellbookOfTheHearth");
            prefabs.SpellbookStormPrefab = assetBundle.LoadAsset<GameObject>("MMA_SpellbookGrimoireOfTheStorm");
            prefabs.SpellbookCodexPrefab = assetBundle.LoadAsset<GameObject>("MMA_SpellbookCodexOfTheAsgardianSorcerer");

            // Lanterns
            prefabs.LanternMythicalPrefab = assetBundle.LoadAsset<GameObject>("MMA_LanternMythical");
            prefabs.LanternEverwinterPrefab = assetBundle.LoadAsset<GameObject>("MMA_LanternEverWinter");
            prefabs.LanternMistcallerPrefab = assetBundle.LoadAsset<GameObject>("MMA_LanternMistcaller");
            materials.LanternMythicalOn = assetBundle.LoadAsset<Material>("MythicalLanternMat_MMA");
            materials.LanternMythicalOff = assetBundle.LoadAsset<Material>("MythicalLanternMat_Off_MMA");
            materials.LanternEverwinterOn = assetBundle.LoadAsset<Material>("EverwinterLanternMat_MMA");
            materials.LanternEverwinterOff = assetBundle.LoadAsset<Material>("EverwinterLanternMat_Off_MMA");
            materials.LanternMistcallerOn = assetBundle.LoadAsset<Material>("MistcallerLanternMat_MMA");
            materials.LanternMistcallerOff = assetBundle.LoadAsset<Material>("MistcallerLanternMat_Off_MMA");
        }

        private void InitInputs()
        {
            try
            {
                LanternHelper.InitInput(PluginConfig.lanternKey);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + e);
            }
        }
    }
}

