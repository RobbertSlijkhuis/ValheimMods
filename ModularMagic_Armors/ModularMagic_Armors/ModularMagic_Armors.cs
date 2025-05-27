using BepInEx;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModularMagic_Armors
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Armors : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Armors";
        public const string PluginName = "ModularMagic_Armors";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Armors Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle _assetBundle;
        public PlayerArmatureHelper playerArmature;
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

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            ModQuery.Enable();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            playerArmature = new PlayerArmatureHelper();
            _InitAssetBundle();
            _InitStatusEffects();
            PluginConfig.Init();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += _AddShamanArmor;
            PrefabManager.OnVanillaPrefabsAvailable += _AddWraithArmor;
            PrefabManager.OnVanillaPrefabsAvailable += _AddFrostWolfArmor;
            PrefabManager.OnVanillaPrefabsAvailable += _AddDarkWizardArmor;
            PrefabManager.OnVanillaPrefabsAvailable += _AdjustEitrWeaveArmor;
            PrefabManager.OnVanillaPrefabsAvailable += _AdjustEmblaArmor;
            ItemManager.OnItemsRegistered += _LogRecipes;
        }

        private void _LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMA"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            Jotunn.Logger.LogInfo($"Modded prefabs:");
            foreach (var moddedPrefab in ModQuery.GetPrefabs())
            {
                Jotunn.Logger.LogInfo($"  {moddedPrefab.Prefab.name} added by {moddedPrefab.SourceMod.Name}");

                if (moddedPrefab.Prefab.name == "MMES_TheForestFlinger")
                {
                    ItemDrop itemDrop = moddedPrefab.Prefab.GetComponent<ItemDrop>();
                }
            }

            //var helmet = ItemManager.Instance.GetItem("DeathWizshHelmet");
            //var chest = ItemManager.Instance.GetItem("DeathWizshChest");
            //var legs = ItemManager.Instance.GetItem("DeathWizshLegs");
            //var axe = ItemManager.Instance.GetItem("DeathWizshAxe");
            //var shield = ItemManager.Instance.GetItem("DeathWizshShield");

            //var helmetShared = helmet.ItemDrop.m_itemData.m_shared;
            //helmetShared.m_sneakStaminaModifier = -2f;
            //helmetShared.m_runStaminaModifier = -2f;
            //helmetShared.m_swimStaminaModifier = -2f;
            //helmetShared.m_jumpStaminaModifier = -2f;
            //helmetShared.m_attackStaminaModifier = -2f;
            //helmetShared.m_dodgeStaminaModifier = -2f;
            //helmetShared.m_blockStaminaModifier = -2f;
            //helmetShared.m_maxDurability = 10000f;

            //var chestShared = chest.ItemDrop.m_itemData.m_shared;
            //chestShared.m_armor = 130;
            //chestShared.m_maxDurability = 10000f;

            //var legsShared = legs.ItemDrop.m_itemData.m_shared;
            //// legsShared.m_movementModifier = 1f;
            //legsShared.m_maxDurability = 10000f;
            //legsShared.m_equipStatusEffect = effects.NoFallDamage;

            //var axeShared = axe.ItemDrop.m_itemData.m_shared;
            //axeShared.m_damages.m_slash = 115;
            //axeShared.m_damages.m_chop = 60;
            //axeShared.m_attackForce = 60;
            //axeShared.m_backstabBonus = 3;
            //axeShared.m_maxDurability = 10000f;

            //var shieldShared = shield.ItemDrop.m_itemData.m_shared;
            //shieldShared.m_blockPower = 126;
            //shieldShared.m_deflectionForce = 60;
            //shieldShared.m_maxDurability = 10000f;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ItemManager.OnItemsRegistered -= _LogRecipes;
        }

        private void _AddShamanArmor()
        {
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //ItemConfig DWHelmetConfig = new ItemConfig();
            //DWHelmetConfig.Name = "DeathWizsh's Helmet";
            //DWHelmetConfig.Description = "Hacler Chet!";
            //CustomItem DWHelmet = new CustomItem("DeathWizshHelmet", "HelmetLeather", DWHelmetConfig);
            //ItemManager.Instance.AddItem(DWHelmet);

            //ItemConfig DWChestConfig = new ItemConfig();
            //DWChestConfig.Name = "DeathWizsh's Chest";
            //DWChestConfig.Description = "Hacler Chet!";
            //CustomItem DWChest = new CustomItem("DeathWizshChest", "ArmorLeatherChest", DWChestConfig);
            //ItemManager.Instance.AddItem(DWChest);

            //ItemConfig DWLegsConfig = new ItemConfig();
            //DWLegsConfig.Name = "DeathWizsh's Legs";
            //DWLegsConfig.Description = "Hacler Chet!";
            //CustomItem DWLegs = new CustomItem("DeathWizshLegs", "ArmorLeatherLegs", DWLegsConfig);
            //ItemManager.Instance.AddItem(DWLegs);

            //ItemConfig DWFlintAxeConfig = new ItemConfig();
            //DWFlintAxeConfig.Name = "DeathWizsh's Axe";
            //DWFlintAxeConfig.Description = "Hacler Chet!";
            //CustomItem DWFlintAxe = new CustomItem("DeathWizshAxe", "AxeFlint", DWFlintAxeConfig);
            //ItemManager.Instance.AddItem(DWFlintAxe);

            //ItemConfig DWShieldConfig = new ItemConfig();
            //DWShieldConfig.Name = "DeathWizsh's Shield";
            //DWShieldConfig.Description = "Hacler Chet!";
            //CustomItem DWShield = new CustomItem("DeathWizshShield", "ShieldWood", DWShieldConfig);
            //ItemManager.Instance.AddItem(DWShield);
            ArmorConfig tiara = PluginConfig.armor1Helmet;
            ArmorConfig tutu = PluginConfig.armor1Chest;
            tiara.name.Value = "Audacious Tiara";
            tutu.name.Value = "Audacious Tutu";
            ItemHelper.Create(prefabs.AudaciousTiara, tiara);
            ItemHelper.Create(prefabs.AudaciousTutu, tutu);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            ItemHelper.Create(prefabs.shamanHelmetPrefab, PluginConfig.armor1Helmet);
            ItemHelper.Create(prefabs.shamanCapePrefab, PluginConfig.armor1Cape);
            ItemHelper.Create(prefabs.shamanChestPrefab, PluginConfig.armor1Chest);
            ItemHelper.Create(prefabs.shamanLegsPrefab, PluginConfig.armor1Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(playerArmature.headPath).gameObject;
            GameObject playerWristLeft = player.transform.Find(playerArmature.wristLeftPath).gameObject;
            GameObject playerWristRight = player.transform.Find(playerArmature.wristRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(playerArmature.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(playerArmature.kneeRightPath).gameObject;

            GameObject helmetEffectHead = prefabs.shamanHelmetPrefab.transform.Find("ME_blackforest_effect_head").gameObject;
            GameObject HelmetEffectAntlerLeft = prefabs.shamanHelmetPrefab.transform.Find("ME_blackforest_effect_antler_left").gameObject;
            GameObject helmetEffectAntlerRight = prefabs.shamanHelmetPrefab.transform.Find("ME_blackforest_effect_antler_right").gameObject;
            GameObject chestEffectWristLeft = prefabs.shamanChestPrefab.transform.Find("ME_blackforest_effect_wrist_left").gameObject;
            GameObject chestEffectWristRight = prefabs.shamanChestPrefab.transform.Find("ME_blackforest_effect_wrist_right").gameObject;
            GameObject legsEffectKneeLeft = prefabs.shamanLegsPrefab.transform.Find("ME_blackforest_effect_knee_left").gameObject;
            GameObject legsEffectKneeRight = prefabs.shamanLegsPrefab.transform.Find("ME_blackforest_effect_knee_right").gameObject;

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

            PrefabManager.OnVanillaPrefabsAvailable -= _AddShamanArmor;
        }

        private void _AddWraithArmor()
        {
            ItemHelper.Create(prefabs.wraithHelmetPrefab, PluginConfig.armor2Helmet);
            ItemHelper.Create(prefabs.wraithCapePrefab, PluginConfig.armor2Cape);
            ItemHelper.Create(prefabs.wraithChestPrefab, PluginConfig.armor2Chest);
            ItemHelper.Create(prefabs.wraithLegsPrefab, PluginConfig.armor2Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(playerArmature.headPath).gameObject;
            GameObject playerSpine1 = player.transform.Find(playerArmature.spine1Path).gameObject;
            GameObject playerHandLeft = player.transform.Find(playerArmature.handLeftPath).gameObject;
            GameObject playerHandRight = player.transform.Find(playerArmature.handRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(playerArmature.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(playerArmature.kneeRightPath).gameObject;

            GameObject eyeLeft = prefabs.wraithHelmetPrefab.transform.Find("ME_eye_left").gameObject;
            GameObject eyeRight = prefabs.wraithHelmetPrefab.transform.Find("ME_eye_right").gameObject;
            GameObject helmetEffectHead = prefabs.wraithHelmetPrefab.transform.Find("ME_swamp_effect_head").gameObject;
            GameObject helmetEffectHeadFace = prefabs.wraithHelmetPrefab.transform.Find("ME_swamp_effect_face").gameObject;
            GameObject chestEffectSpine1 = prefabs.wraithChestPrefab.transform.Find("ME_swamp_effect_spine1").gameObject;
            GameObject chestEffectHandLeft = prefabs.wraithChestPrefab.transform.Find("ME_swamp_effect_hand_left").gameObject;
            GameObject chestEffectHandRight = prefabs.wraithChestPrefab.transform.Find("ME_swamp_effect_hand_right").gameObject;
            GameObject legsEffectKneeLeft = prefabs.wraithLegsPrefab.transform.Find("ME_swamp_effect_knee_left").gameObject;
            GameObject legsEffectKneeRight = prefabs.wraithLegsPrefab.transform.Find("ME_swamp_effect_knee_right").gameObject;

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

            PrefabManager.OnVanillaPrefabsAvailable -= _AddWraithArmor;
        }

        private void _AddFrostWolfArmor()
        {
            ItemHelper.Create(prefabs.wolfHelmetPrefab, PluginConfig.armor3Helmet);
            ItemHelper.Create(prefabs.wolfCapePrefab, PluginConfig.armor3Cape);
            ItemHelper.Create(prefabs.wolfChestPrefab, PluginConfig.armor3Chest);
            ItemHelper.Create(prefabs.wolfLegsPrefab, PluginConfig.armor3Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(playerArmature.headPath).gameObject;
            GameObject playerSpine2 = player.transform.Find(playerArmature.spine2Path).gameObject;
            GameObject playerHandLeft = player.transform.Find(playerArmature.handLeftPath).gameObject;
            GameObject playerHandRight = player.transform.Find(playerArmature.handRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(playerArmature.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(playerArmature.kneeRightPath).gameObject;

            GameObject helmetEffectHead = prefabs.wolfHelmetPrefab.transform.Find("ME_mountain_effect_head").gameObject;
            GameObject chestEffectSpine2 = prefabs.wolfChestPrefab.transform.Find("ME_mountain_effect_spine2").gameObject;
            GameObject chestEffectHandLeft = prefabs.wolfChestPrefab.transform.Find("ME_mountain_effect_hand_left").gameObject;
            GameObject chestEffectHandRight = prefabs.wolfChestPrefab.transform.Find("ME_mountain_effect_hand_right").gameObject;
            GameObject legsEffectKneeLeft = prefabs.wolfLegsPrefab.transform.Find("ME_mountain_effect_knee_left").gameObject;
            GameObject legsEffectKneeRight = prefabs.wolfLegsPrefab.transform.Find("ME_mountain_effect_knee_right").gameObject;

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

            PrefabManager.OnVanillaPrefabsAvailable -= _AddFrostWolfArmor;
        }

        private void _AddDarkWizardArmor()
        {
            ItemHelper.Create(prefabs.darkWizardHelmetPrefab, PluginConfig.armor4Helmet);
            ItemHelper.Create(prefabs.darkWizardCapePrefab, PluginConfig.armor4Cape);
            ItemHelper.Create(prefabs.darkWizardChestPrefab, PluginConfig.armor4Chest);
            ItemHelper.Create(prefabs.darkWizardLegsPrefab, PluginConfig.armor4Legs);

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerShoulderLeft = player.transform.Find(playerArmature.shoulderLeftPath).gameObject;
            GameObject playerShoulderRight = player.transform.Find(playerArmature.shoulderRightPath).gameObject;

            GameObject chestEffectShoulderLeft = prefabs.darkWizardChestPrefab.transform.Find("ME_plains_effect_shoulder_left").gameObject;
            GameObject chestEffectShoulderRight = prefabs.darkWizardChestPrefab.transform.Find("ME_plains_effect_shoulder_right").gameObject;

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

            PrefabManager.OnVanillaPrefabsAvailable -= _AddDarkWizardArmor;
        }

        private void _AdjustEitrWeaveArmor()
        {
            string setName = "EitrWeaveSet_MMA";
            int setSize = 3;
            GameObject helmet = PrefabManager.Instance.GetPrefab("HelmetMage");
            GameObject chestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest");
            GameObject legsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs");
            ItemDrop helmetItemDrop = helmet.GetComponent<ItemDrop>();
            ItemDrop chestItemDrop = chestPrefab.GetComponent<ItemDrop>();
            ItemDrop legsItemDrop = legsPrefab.GetComponent<ItemDrop>();

            helmetItemDrop.m_itemData.m_shared.m_setName = setName;
            helmetItemDrop.m_itemData.m_shared.m_setSize = setSize;
            helmetItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EitrWeaveArmorSetSE;

            // chestItemDrop.m_itemData.m_shared.m_eitrRegenModifier = 0.30f;
            chestItemDrop.m_itemData.m_shared.m_setName = setName;
            chestItemDrop.m_itemData.m_shared.m_setSize = setSize;
            chestItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EitrWeaveArmorSetSE;

            // legsItemDrop.m_itemData.m_shared.m_eitrRegenModifier = 0.30f;
            legsItemDrop.m_itemData.m_shared.m_setName = setName;
            legsItemDrop.m_itemData.m_shared.m_setSize = setSize;
            legsItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EitrWeaveArmorSetSE;

            PrefabManager.OnVanillaPrefabsAvailable -= _AdjustEitrWeaveArmor;
        }

        private void _AdjustEmblaArmor()
        {
            materials.EmblaArmor.FixReferences();
            materials.EmblaChest.FixReferences();
            materials.EmblaLegs.FixReferences();

            ItemConfig emblaCapeConfig = new ItemConfig();
            emblaCapeConfig.Name = "Embla Cape";
            CustomItem emblaCape = new CustomItem(prefabs.EmblaCape, true, emblaCapeConfig);
            ItemManager.Instance.AddItem(emblaCape);

            string setName = "EmblaSet_MMA";
            int setSize = 3;
            List<Material> armorMaterials = new List<Material>();
            armorMaterials.Add(materials.EmblaArmor);

            GameObject helmetPrefab = PrefabManager.Instance.GetPrefab("HelmetMage_Ashlands");
            ItemDrop helmetItemDrop = helmetPrefab.GetComponent<ItemDrop>();
            Transform helmet = helmetPrefab.transform.Find("attach_skin/AshlandsHood");
            Transform helmetFlat = helmetPrefab.transform.Find("hood");

            GameObject chestPrefab = PrefabManager.Instance.GetPrefab("ArmorMageChest_Ashlands");
            ItemDrop chestItemDrop = chestPrefab.GetComponent<ItemDrop>();
            Transform chest = chestPrefab.transform.Find("attach_skin/AshlandsMageChest");
            Transform chestFlat = chestPrefab.transform.Find("model");

            GameObject legsPrefab = PrefabManager.Instance.GetPrefab("ArmorMageLegs_Ashlands");
            ItemDrop legsItemDrop = legsPrefab.GetComponent<ItemDrop>();
            Transform legs = legsPrefab.transform.Find("attach_skin/AshlandsMageLegs");
            Transform legsFlat = legsPrefab.transform.Find("log");

            SkinnedMeshRenderer helmetMesh = helmet.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer helmetFlatMesh = helmetFlat.gameObject.GetComponent<MeshRenderer>();

            List<Sprite> helmetSpriteList = new List<Sprite>();
            helmetSpriteList.Add(sprites.EmblaHood);
            helmetItemDrop.m_itemData.m_shared.m_icons = helmetSpriteList.ToArray();
            helmetItemDrop.m_itemData.m_shared.m_setName = setName;
            helmetItemDrop.m_itemData.m_shared.m_setSize = setSize;
            helmetItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EmblaArmorSetSE;
            helmetMesh.materials = armorMaterials.ToArray();
            helmetFlatMesh.materials = armorMaterials.ToArray();

            SkinnedMeshRenderer chestMesh = chest.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer chestFlatMesh = chestFlat.gameObject.GetComponent<MeshRenderer>();

            List<Sprite> chestSpriteList = new List<Sprite>();
            chestSpriteList.Add(sprites.EmblaChest);
            chestItemDrop.m_itemData.m_shared.m_icons = chestSpriteList.ToArray();
            chestItemDrop.m_itemData.m_shared.m_setName = setName;
            chestItemDrop.m_itemData.m_shared.m_setSize = setSize;
            chestItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EmblaArmorSetSE;
            // chestItemDrop.m_itemData.m_shared.m_eitrRegenModifier = 0.40f;
            chestItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaChest;
            chestMesh.materials = armorMaterials.ToArray();
            chestFlatMesh.materials = armorMaterials.ToArray();

            SkinnedMeshRenderer legsMesh = legs.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer legsFlatMesh = legsFlat.gameObject.GetComponent<MeshRenderer>();

            List<Sprite> legsSpriteList = new List<Sprite>();
            legsSpriteList.Add(sprites.EmblaLegs);
            legsItemDrop.m_itemData.m_shared.m_icons = legsSpriteList.ToArray();
            legsItemDrop.m_itemData.m_shared.m_setName = setName;
            legsItemDrop.m_itemData.m_shared.m_setSize = setSize;
            legsItemDrop.m_itemData.m_shared.m_setStatusEffect = effects.EmblaArmorSetSE;
            // legsItemDrop.m_itemData.m_shared.m_eitrRegenModifier = 0.40f;
            legsItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaLegs;
            legsMesh.materials = armorMaterials.ToArray();
            legsFlatMesh.materials = armorMaterials.ToArray();

            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHelmetAttach = player.transform.Find(playerArmature.helmetAttachPath).gameObject;
            GameObject playerShoulderLeft = player.transform.Find(playerArmature.shoulderLeftPath).gameObject;
            GameObject playerShoulderRight = player.transform.Find(playerArmature.shoulderRightPath).gameObject;

            GameObject shoulderLeftEffect = PrefabManager.Instance.CreateClonedPrefab("EmblaChest_Left_Effects_MMA", prefabs.EmblaEffects.name);
            GameObject shoulderRightEffect = PrefabManager.Instance.CreateClonedPrefab("EmblaChest_Right_Effects_MMA", prefabs.EmblaEffects.name);
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

            PrefabManager.OnVanillaPrefabsAvailable -= _AdjustEmblaArmor;
        }

        private void _InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.shamanArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.wraithArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.wolfArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.darkWizardArmorSetSE, true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.EitrWeaveArmorSetSE, true));
            effects.EmblaArmorSetSE.m_icon = sprites.EmblaHood;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.EmblaArmorSetSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void _InitAssetBundle()
        {
            _assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_armors_dw");

            // Sprites
            sprites.EmblaHood = _assetBundle.LoadAsset<Sprite>("EmblaHelmet_MMA");
            sprites.EmblaChest = _assetBundle.LoadAsset<Sprite>("EmblaChest_MMA");
            sprites.EmblaLegs = _assetBundle.LoadAsset<Sprite>("EmblaLegs_MMA");

            // Materials
            materials.WraithArmorEye = _assetBundle.LoadAsset<Material>("WraithEye_MMA");
            materials.DarkWizardArmorEye = _assetBundle.LoadAsset<Material>("DarkWizardEye_MMA");
            materials.EmblaArmor = _assetBundle.LoadAsset<Material>("AshlandsMageArmor_red_MMA");
            materials.EmblaChest = _assetBundle.LoadAsset<Material>("AshlandsMageArmorChest_red_MMA");
            materials.EmblaLegs = _assetBundle.LoadAsset<Material>("AshlandsMageArmorLegs_red_MMA");

            // Shaman armor
            prefabs.shamanHelmetPrefab = _assetBundle.LoadAsset<GameObject>("MMA_ShamanHelmet");
            prefabs.shamanCapePrefab = _assetBundle.LoadAsset<GameObject>("MMA_ShamanCape");
            prefabs.shamanChestPrefab = _assetBundle.LoadAsset<GameObject>("MMA_ShamanChest");
            prefabs.shamanLegsPrefab = _assetBundle.LoadAsset<GameObject>("MMA_ShamanLegs");
            // PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("shaman_armor_set_effect_MMA"), true));
            effects.shamanArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_ShamanArmor_MMA");

            // Wraith armor
            prefabs.wraithHelmetPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WraithHelmet");
            prefabs.wraithCapePrefab = _assetBundle.LoadAsset<GameObject>("MMA_WraithCape");
            prefabs.wraithChestPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WraithChest");
            prefabs.wraithLegsPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WraithLegs");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("wraith_armor_set_effect_MMA"), true));
            effects.wraithArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_WraithArmor_MMA");

            // Wolf armor
            prefabs.wolfHelmetPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WolfHelmet");
            prefabs.wolfCapePrefab = _assetBundle.LoadAsset<GameObject>("MMA_WolfCape");
            prefabs.wolfChestPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WolfChest");
            prefabs.wolfLegsPrefab = _assetBundle.LoadAsset<GameObject>("MMA_WolfLegs");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("wolf_armor_set_effect_MMA"), true));
            effects.wolfArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_WolfArmor_MMA");

            // DarkWizard armor
            prefabs.darkWizardHelmetPrefab = _assetBundle.LoadAsset<GameObject>("MMA_DarkWizardHelmet");
            prefabs.darkWizardCapePrefab = _assetBundle.LoadAsset<GameObject>("MMA_DarkWizardCape");
            prefabs.darkWizardChestPrefab = _assetBundle.LoadAsset<GameObject>("MMA_DarkWizardChest");
            prefabs.darkWizardLegsPrefab = _assetBundle.LoadAsset<GameObject>("MMA_DarkWizardLegs");
            effects.darkWizardArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_DarkWizardArmor_MMA");

            // Eitr-weave armor
            effects.EitrWeaveArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_EitrWeaveArmor_MMA");

            // Embla armor
            prefabs.EmblaCape = _assetBundle.LoadAsset<GameObject>("MMA_EmblaCape");
            prefabs.EmblaEffects = _assetBundle.LoadAsset<GameObject>("EmblaHood_Effects_MMA");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.EmblaEffects, true));
            effects.EmblaArmorSetSE = _assetBundle.LoadAsset<StatusEffect>("SetEffect_EmblaArmor_MMA");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //effects.NoFallDamage = _assetBundle.LoadAsset<StatusEffect>("NoFallDamage");
            //effects.SlowFallAndNoFallDamage = _assetBundle.LoadAsset<StatusEffect>("SlowFallAndNoFallDamage");
            //prefabs.AudaciousMaskPrefab = _assetBundle.LoadAsset<GameObject>("MMA_AudacityMask");
            prefabs.AudaciousTiara = _assetBundle.LoadAsset<GameObject>("AudaciousTiara_MMA");
            prefabs.AudaciousTutu = _assetBundle.LoadAsset<GameObject>("AudaciousTutu_MMA");
            prefabs.AudaciousSFX = _assetBundle.LoadAsset<GameObject>("sfx_audacity_MMA");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
    }
}

