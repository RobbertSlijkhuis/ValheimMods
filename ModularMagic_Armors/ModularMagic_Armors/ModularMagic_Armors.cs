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
        public PathHelper pathHelper;
        public ItemSnapShots itemSnapShots = new ItemSnapShots();
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        public CustomStatusEffects effects = new CustomStatusEffects();
        public CustomSprites sprites = new CustomSprites();

        public bool gameIsReady = false;
        public string playerBeard;
        public bool updatePlayerBeard = true;

        public static readonly int armorStatusHashCode = "ArmorStatus_MMA".GetStableHashCode();
        public static int ShamanArmorSetHashCode;
        public static int WraithArmorSetHashCode;
        public static int FrostWolfArmorSetHashCode;
        public static int DarkWizardArmorSetHashCode;
        public static int EitrWeaveArmorSetHashCode;
        public static int EmblaArmorSetHashCode;

        public static int WraithHelmetHashCode;
        public static int WraithChestHashCode;
        public static int WraithLegsHashCode;
        public static int DarkWizardHelmetHashCode;
        public static int EmblaHelmetHashCode;
        public static int EmblaChestHashCode;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            pathHelper = new PathHelper();

            Assembly? bepinexConfigManager = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");
            Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
            configManager = configManagerType == null ? null : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);

            InitAssetBundle();
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
            ItemManager.OnItemsRegistered += InitHashes;
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
            ItemHelper.Create(prefabs.ShamanHelmetPrefab, PluginConfig.armor1Helmet);
            ItemHelper.Create(prefabs.ShamanCapePrefab, PluginConfig.armor1Cape);
            ItemHelper.Create(prefabs.ShamanChestPrefab, PluginConfig.armor1Chest);
            ItemHelper.Create(prefabs.ShamanLegsPrefab, PluginConfig.armor1Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(pathHelper.headPath).gameObject;
            GameObject playerWristLeft = player.transform.Find(pathHelper.wristLeftPath).gameObject;
            GameObject playerWristRight = player.transform.Find(pathHelper.wristRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(pathHelper.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(pathHelper.kneeRightPath).gameObject;

            GameObject helmetEffectHead = prefabs.ShamanHelmetPrefab.transform.Find(pathHelper.ShamanHeadEffectPath).gameObject;
            GameObject HelmetEffectAntlerLeft = prefabs.ShamanHelmetPrefab.transform.Find(pathHelper.ShamanAntlerLefEffectPath).gameObject;
            GameObject helmetEffectAntlerRight = prefabs.ShamanHelmetPrefab.transform.Find(pathHelper.ShamanAntlerRightEffectPath).gameObject;
            GameObject chestEffectWristLeft = prefabs.ShamanChestPrefab.transform.Find(pathHelper.ShamanWristLeftEffectPath).gameObject;
            GameObject chestEffectWristRight = prefabs.ShamanChestPrefab.transform.Find(pathHelper.ShamanWristRightEffectPath).gameObject;
            GameObject legsEffectKneeLeft = prefabs.ShamanLegsPrefab.transform.Find(pathHelper.ShamanKneeLeftEffectPath).gameObject;
            GameObject legsEffectKneeRight = prefabs.ShamanLegsPrefab.transform.Find(pathHelper.ShamanKneeRightEffectPath).gameObject;

            helmetEffectHead.FixReferences();
            HelmetEffectAntlerLeft.FixReferences();
            helmetEffectAntlerRight.FixReferences();
            chestEffectWristLeft.FixReferences();
            chestEffectWristRight.FixReferences();
            legsEffectKneeLeft.FixReferences();
            legsEffectKneeRight.FixReferences();

            helmetEffectHead.transform.parent = playerHead.transform;
            helmetEffectHead.transform.localPosition = new Vector3(-0.0017f, -0.0008f, 0f);
            helmetEffectHead.transform.localRotation = TransformHelper.generateRotation(new Vector3(90f, 0f, 0f));
            helmetEffectHead.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            helmetEffectHead.SetActive(false);

            HelmetEffectAntlerLeft.transform.parent = playerHead.transform;
            HelmetEffectAntlerLeft.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            HelmetEffectAntlerLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            HelmetEffectAntlerLeft.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            HelmetEffectAntlerLeft.SetActive(false);

            helmetEffectAntlerRight.transform.parent = playerHead.transform;
            helmetEffectAntlerRight.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            helmetEffectAntlerRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            helmetEffectAntlerRight.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            helmetEffectAntlerRight.SetActive(false);

            chestEffectWristLeft.transform.parent = playerWristLeft.transform;
            chestEffectWristLeft.transform.localPosition = new Vector3(-0.0012f, -0.001f, -0.00081f);
            chestEffectWristLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            chestEffectWristLeft.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            chestEffectWristLeft.SetActive(false);

            chestEffectWristRight.transform.parent = playerWristRight.transform;
            chestEffectWristRight.transform.localPosition = new Vector3(-0.0012f, -0.001f, 0f);
            chestEffectWristRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            chestEffectWristRight.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            chestEffectWristRight.SetActive(false);

            legsEffectKneeLeft.transform.parent = playerKneeLeft.transform;
            legsEffectKneeLeft.transform.localPosition = new Vector3(-0.0012f, 0.003f, 0f);
            legsEffectKneeLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            legsEffectKneeLeft.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            legsEffectKneeLeft.SetActive(false);

            legsEffectKneeRight.transform.parent = playerKneeRight.transform;
            legsEffectKneeRight.transform.localPosition = new Vector3(-0.0012f, 0.003f, 0f);
            legsEffectKneeRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            legsEffectKneeRight.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            legsEffectKneeRight.SetActive(false);

            PrefabManager.OnVanillaPrefabsAvailable -= AddShamanArmor;
        }

        private void AddWraithArmor()
        {
            ItemHelper.Create(prefabs.WraithHelmetPrefab, PluginConfig.armor2Helmet);
            ItemHelper.Create(prefabs.WraithCapePrefab, PluginConfig.armor2Cape);
            ItemHelper.Create(prefabs.WraithChestPrefab, PluginConfig.armor2Chest);
            ItemHelper.Create(prefabs.WraithLegsPrefab, PluginConfig.armor2Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(pathHelper.headPath).gameObject;
            GameObject playerSpine1 = player.transform.Find(pathHelper.spine1Path).gameObject;
            GameObject playerHandLeft = player.transform.Find(pathHelper.handLeftPath).gameObject;
            GameObject playerHandRight = player.transform.Find(pathHelper.handRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(pathHelper.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(pathHelper.kneeRightPath).gameObject;

            GameObject eyeLeft = prefabs.WraithHelmetPrefab.transform.Find(pathHelper.EyeLeftPath).gameObject;
            GameObject eyeRight = prefabs.WraithHelmetPrefab.transform.Find(pathHelper.EyeRightPath).gameObject;
            GameObject helmetEffectHead = prefabs.WraithHelmetPrefab.transform.Find(pathHelper.WraithHeadEffectPath).gameObject;
            GameObject helmetEffectHeadFace = prefabs.WraithHelmetPrefab.transform.Find(pathHelper.WraithFaceEffectPath).gameObject;
            GameObject chestEffectSpine1 = prefabs.WraithChestPrefab.transform.Find(pathHelper.WraithChestEffectPath).gameObject;
            GameObject chestEffectHandLeft = prefabs.WraithChestPrefab.transform.Find(pathHelper.WraithHandLeftEffectPath).gameObject;
            GameObject chestEffectHandRight = prefabs.WraithChestPrefab.transform.Find(pathHelper.WraithHandRightEffectPath).gameObject;
            GameObject legsEffectKneeLeft = prefabs.WraithLegsPrefab.transform.Find(pathHelper.WraithKneeLeftEffectPath).gameObject;
            GameObject legsEffectKneeRight = prefabs.WraithLegsPrefab.transform.Find(pathHelper.WraithKneeRightEffectPath).gameObject;

            eyeLeft.FixReferences();
            eyeRight.FixReferences();
            helmetEffectHead.FixReferences();
            helmetEffectHeadFace.FixReferences();
            chestEffectSpine1.FixReferences();
            chestEffectHandLeft.FixReferences();
            chestEffectHandRight.FixReferences();
            legsEffectKneeLeft.FixReferences();
            legsEffectKneeRight.FixReferences();

            eyeLeft.transform.parent = playerHead.transform;
            eyeLeft.transform.localPosition = new Vector3(-0.00087f, 0.00178f, -0.00035f);
            eyeLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 340.544f, 0f));
            eyeLeft.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeLeft.SetActive(false);

            eyeRight.transform.parent = playerHead.transform;
            eyeRight.transform.localPosition = new Vector3(-0.00087f, 0.00178f, 0.00044f);
            eyeRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 27.096f, 0f));
            eyeRight.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeRight.SetActive(false);

            helmetEffectHead.transform.parent = playerHead.transform;
            helmetEffectHead.transform.localPosition = new Vector3(0f, 0f, 0f);
            helmetEffectHead.SetActive(false);

            helmetEffectHeadFace.transform.parent = playerHead.transform;
            helmetEffectHeadFace.transform.localPosition = new Vector3(-0.0015f, 0.001f, 0f);
            helmetEffectHeadFace.SetActive(false);

            chestEffectSpine1.transform.parent = playerSpine1.transform;
            chestEffectSpine1.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectSpine1.SetActive(false);

            chestEffectHandLeft.transform.parent = playerHandLeft.transform;
            chestEffectHandLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectHandLeft.SetActive(false);

            chestEffectHandRight.transform.parent = playerHandRight.transform;
            chestEffectHandRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectHandRight.SetActive(false);

            legsEffectKneeLeft.transform.parent = playerKneeLeft.transform;
            legsEffectKneeLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            legsEffectKneeLeft.SetActive(false);

            legsEffectKneeRight.transform.parent = playerKneeRight.transform;
            legsEffectKneeRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            legsEffectKneeRight.SetActive(false);

            PrefabManager.OnVanillaPrefabsAvailable -= AddWraithArmor;
        }

        private void AddFrostWolfArmor()
        {
            ItemHelper.Create(prefabs.FrostWolfHelmetPrefab, PluginConfig.armor3Helmet);
            ItemHelper.Create(prefabs.FrostWolfCapePrefab, PluginConfig.armor3Cape);
            ItemHelper.Create(prefabs.FrostWolfChestPrefab, PluginConfig.armor3Chest);
            ItemHelper.Create(prefabs.FrostWolfLegsPrefab, PluginConfig.armor3Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(pathHelper.headPath).gameObject;
            GameObject playerSpine2 = player.transform.Find(pathHelper.spine2Path).gameObject;
            GameObject playerHandLeft = player.transform.Find(pathHelper.handLeftPath).gameObject;
            GameObject playerHandRight = player.transform.Find(pathHelper.handRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(pathHelper.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(pathHelper.kneeRightPath).gameObject;

            GameObject helmetEffectHead = prefabs.FrostWolfHelmetPrefab.transform.Find(pathHelper.FrostwolfHeadEffectPath).gameObject;
            GameObject chestEffectSpine2 = prefabs.FrostWolfChestPrefab.transform.Find(pathHelper.FrostwolfChestEffectPath).gameObject;
            GameObject chestEffectHandLeft = prefabs.FrostWolfChestPrefab.transform.Find(pathHelper.FrostwolfHandLeftEffectPath).gameObject;
            GameObject chestEffectHandRight = prefabs.FrostWolfChestPrefab.transform.Find(pathHelper.FrostwolfHandRightEffectPath).gameObject;
            GameObject legsEffectKneeLeft = prefabs.FrostWolfLegsPrefab.transform.Find(pathHelper.FrostwolfKneeLeftEffectPath).gameObject;
            GameObject legsEffectKneeRight = prefabs.FrostWolfLegsPrefab.transform.Find(pathHelper.FrostwolfKneeRightEffectPath).gameObject;

            helmetEffectHead.FixReferences();
            chestEffectSpine2.FixReferences();
            chestEffectHandLeft.FixReferences();
            chestEffectHandRight.FixReferences();
            legsEffectKneeLeft.FixReferences();
            legsEffectKneeRight.FixReferences();

            helmetEffectHead.transform.parent = playerHead.transform;
            helmetEffectHead.transform.localPosition = new Vector3(0f, 0.0025f, 0f);
            helmetEffectHead.transform.localScale = new Vector3(1f, 1f, 1f);
            helmetEffectHead.SetActive(false);

            chestEffectSpine2.transform.parent = playerSpine2.transform;
            chestEffectSpine2.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectSpine2.transform.localScale = new Vector3(1f, 1f, 1f);
            chestEffectSpine2.SetActive(false);

            chestEffectHandLeft.transform.parent = playerHandLeft.transform;
            chestEffectHandLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectHandLeft.transform.localScale = new Vector3(1f, 1f, 1f);
            chestEffectHandLeft.SetActive(false);

            chestEffectHandRight.transform.parent = playerHandRight.transform;
            chestEffectHandRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            chestEffectHandRight.transform.localScale = new Vector3(1f, 1f, 1f);
            chestEffectHandRight.SetActive(false);

            legsEffectKneeLeft.transform.parent = playerKneeLeft.transform;
            legsEffectKneeLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            legsEffectKneeLeft.transform.localScale = new Vector3(1f, 1f, 1f);
            legsEffectKneeLeft.SetActive(false);

            legsEffectKneeRight.transform.parent = playerKneeRight.transform;
            legsEffectKneeRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            legsEffectKneeRight.transform.localScale = new Vector3(1f, 1f, 1f);
            legsEffectKneeRight.SetActive(false);

            PrefabManager.OnVanillaPrefabsAvailable -= AddFrostWolfArmor;
        }

        private void AddDarkWizardArmor()
        {
            ItemHelper.Create(prefabs.DarkWizardHelmetPrefab, PluginConfig.armor4Helmet);
            ItemHelper.Create(prefabs.DarkWizardCapePrefab, PluginConfig.armor4Cape);
            ItemHelper.Create(prefabs.DarkWizardChestPrefab, PluginConfig.armor4Chest);
            ItemHelper.Create(prefabs.DarkWizardLegsPrefab, PluginConfig.armor4Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerShoulderLeft = player.transform.Find(pathHelper.shoulderLeftPath).gameObject;
            GameObject playerShoulderRight = player.transform.Find(pathHelper.shoulderRightPath).gameObject;

            GameObject chestEffectShoulderLeft = prefabs.DarkWizardChestPrefab.transform.Find(pathHelper.DarkWizardShoulderLeftEffectPath).gameObject;
            GameObject chestEffectShoulderRight = prefabs.DarkWizardChestPrefab.transform.Find(pathHelper.DarkWizardShoulderRightEffectPath).gameObject;

            chestEffectShoulderLeft.FixReferences();
            chestEffectShoulderRight.FixReferences();

            chestEffectShoulderLeft.transform.parent = playerShoulderLeft.transform;
            chestEffectShoulderLeft.transform.localPosition = new Vector3(0f, 0f, -0.0001f);
            // chestEffectShoulderLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            chestEffectShoulderLeft.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            chestEffectShoulderLeft.SetActive(false);

            chestEffectShoulderRight.transform.parent = playerShoulderRight.transform;
            chestEffectShoulderRight.transform.localPosition = new Vector3(0f, 0f, -0.0001f); // 0 -0,0007 -0,0005
            // chestEffectShoulderRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f)); // 58, 130, 90
            chestEffectShoulderRight.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            chestEffectShoulderRight.SetActive(false);

            PrefabManager.OnVanillaPrefabsAvailable -= AddDarkWizardArmor;
        }

        private void AdjustEitrWeaveArmor()
        {
            prefabs.EitrWeaveHelmetPrefab = PrefabManager.Instance.GetPrefab("HelmetMage");
            prefabs.EitrWeaveChestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest");
            prefabs.EitrWeaveLegsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs");

            itemSnapShots.EitrWeaveHelmetStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveHelmetPrefab);
            itemSnapShots.EitrWeaveChestStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveChestPrefab);
            itemSnapShots.EitrWeaveLegsStats = UpdateHelper.TakeSnapShot(prefabs.EitrWeaveLegsPrefab);

            itemSnapShots.EitrWeaveHelmetRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveHelmetPrefab);
            itemSnapShots.EitrWeaveChestRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveChestPrefab);
            itemSnapShots.EitrWeaveLegsRecipe = RecipeHelper.TakeSnapShot(prefabs.EitrWeaveLegsPrefab);

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

            PrefabManager.OnVanillaPrefabsAvailable -= AdjustEitrWeaveArmor;
        }

        private void AdjustEmblaArmor()
        {
            itemSnapShots.EmblaHelmetStats = UpdateHelper.TakeSnapShot(prefabs.EmblaHelmetPrefab);
            itemSnapShots.EmblaChestStats = UpdateHelper.TakeSnapShot(prefabs.EmblaChestPrefab);
            itemSnapShots.EmblaLegsStats = UpdateHelper.TakeSnapShot(prefabs.EmblaLegsPrefab);

            itemSnapShots.EmblaHelmetRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaHelmetPrefab);
            itemSnapShots.EmblaChestRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaChestPrefab);
            itemSnapShots.EmblaLegsRecipe = RecipeHelper.TakeSnapShot(prefabs.EmblaLegsPrefab);

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

            ItemManager.OnItemsRegistered -= AdjustEmblaArmor;
        }

        private void ApplyEffectsEmblaArmor()
        {
            prefabs.EmblaHelmetPrefab = PrefabManager.Instance.GetPrefab("HelmetMage_Ashlands");
            prefabs.EmblaChestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest_Ashlands");
            prefabs.EmblaLegsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs_Ashlands");

            if (!PluginConfig.reskinEmbla.Value)
                return;

            materials.EmblaArmor.FixReferences();
            materials.EmblaChest.FixReferences();
            materials.EmblaLegs.FixReferences();

            List<Material> armorMaterials = new List<Material>();
            armorMaterials.Add(materials.EmblaArmor);

            ItemDrop helmetItemDrop = prefabs.EmblaHelmetPrefab.GetComponent<ItemDrop>();
            ItemDrop chestItemDrop = prefabs.EmblaChestPrefab.GetComponent<ItemDrop>();
            ItemDrop legsItemDrop = prefabs.EmblaLegsPrefab.GetComponent<ItemDrop>();

            Transform helmet = prefabs.EmblaHelmetPrefab.transform.Find("attach_skin/AshlandsHood");
            Transform helmetFlat = prefabs.EmblaHelmetPrefab.transform.Find("hood");
            Transform chest = prefabs.EmblaChestPrefab.transform.Find("attach_skin/AshlandsMageChest");
            Transform chestFlat = prefabs.EmblaChestPrefab.transform.Find("model");
            Transform legs = prefabs.EmblaLegsPrefab.transform.Find("attach_skin/AshlandsMageLegs");
            Transform legsFlat = prefabs.EmblaLegsPrefab.transform.Find("log");

            SkinnedMeshRenderer helmetMesh = helmet.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer helmetFlatMesh = helmetFlat.gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer chestMesh = chest.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer chestFlatMesh = chestFlat.gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer legsMesh = legs.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer legsFlatMesh = legsFlat.gameObject.GetComponent<MeshRenderer>();

            List<Sprite> helmetSpriteList = new List<Sprite>();
            helmetSpriteList.Add(sprites.EmblaHood);
            helmetItemDrop.m_itemData.m_shared.m_icons = helmetSpriteList.ToArray();
            helmetMesh.materials = armorMaterials.ToArray();
            helmetFlatMesh.materials = armorMaterials.ToArray();

            List<Sprite> chestSpriteList = new List<Sprite>();
            chestSpriteList.Add(sprites.EmblaChest);
            chestItemDrop.m_itemData.m_shared.m_icons = chestSpriteList.ToArray();
            chestItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaChest;
            chestMesh.materials = armorMaterials.ToArray();
            chestFlatMesh.materials = armorMaterials.ToArray();

            List<Sprite> legsSpriteList = new List<Sprite>();
            legsSpriteList.Add(sprites.EmblaLegs);
            legsItemDrop.m_itemData.m_shared.m_icons = legsSpriteList.ToArray();
            legsItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaLegs;
            legsMesh.materials = armorMaterials.ToArray();
            legsFlatMesh.materials = armorMaterials.ToArray();

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHelmetAttach = player.transform.Find(pathHelper.helmetAttachPath).gameObject;
            GameObject playerShoulderLeft = player.transform.Find(pathHelper.shoulderLeftPath).gameObject;
            GameObject playerShoulderRight = player.transform.Find(pathHelper.shoulderRightPath).gameObject;

            GameObject shoulderLeftEffect = PrefabManager.Instance.CreateClonedPrefab(pathHelper.EmblaShoulderLeftEffectPath, prefabs.EmblaEffects.name);
            GameObject shoulderRightEffect = PrefabManager.Instance.CreateClonedPrefab(pathHelper.EmblaShoulderRightEffectPath, prefabs.EmblaEffects.name);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(shoulderLeftEffect, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(shoulderRightEffect, true));
            Transform flare = prefabs.EmblaEffects.transform.Find("equiped/flare");

            flare.gameObject.SetActive(true);
            prefabs.EmblaEffects.transform.parent = playerHelmetAttach.transform;
            prefabs.EmblaEffects.transform.localPosition = new Vector3(0f, -0.1f, -0.15f);
            prefabs.EmblaEffects.SetActive(false);

            shoulderLeftEffect.transform.parent = playerShoulderLeft.transform;
            shoulderLeftEffect.transform.localPosition = new Vector3(-0.002f, 0f, -0.0001f);
            shoulderLeftEffect.SetActive(false);

            shoulderRightEffect.transform.parent = playerShoulderRight.transform;
            shoulderRightEffect.transform.localPosition = new Vector3(0.002f, 0f, -0.0001f);
            shoulderRightEffect.SetActive(false);

            PrefabManager.OnVanillaPrefabsAvailable -= ApplyEffectsEmblaArmor;
        }

        private void InitHashes()
        {
            ShamanArmorSetHashCode = effects.ShamanArmorSetSE.name.GetStableHashCode();
            WraithArmorSetHashCode = effects.WraithArmorSetSE.name.GetStableHashCode();
            FrostWolfArmorSetHashCode = effects.FrostWolfArmorSetSE.name.GetStableHashCode();
            DarkWizardArmorSetHashCode = effects.DarkWizardArmorSetSE.name.GetStableHashCode();
            EitrWeaveArmorSetHashCode = effects.EitrWeaveArmorSetSE.name.GetStableHashCode();
            EmblaArmorSetHashCode = effects.EmblaArmorSetSE.name.GetStableHashCode();

            WraithHelmetHashCode = PluginConfig.armor2Helmet.name.Value.GetStableHashCode();
            WraithChestHashCode = PluginConfig.armor2Chest.name.Value.GetStableHashCode();
            WraithLegsHashCode = PluginConfig.armor2Legs.name.Value.GetStableHashCode();
            DarkWizardHelmetHashCode = PluginConfig.armor4Helmet.name.Value.GetStableHashCode();
            EmblaHelmetHashCode = PluginConfig.armor6Helmet.name.Value.GetStableHashCode();
            EmblaChestHashCode = PluginConfig.armor6Chest.name.Value.GetStableHashCode();

            ItemManager.OnItemsRegistered -= InitHashes;
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
            materials.WraithArmorEye = assetBundle.LoadAsset<Material>("WraithEye_MMA");
            materials.DarkWizardArmorEye = assetBundle.LoadAsset<Material>("DarkWizardEye_MMA");
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

