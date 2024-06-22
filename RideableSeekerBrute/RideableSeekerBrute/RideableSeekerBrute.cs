using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using UnityEngine;

namespace RideableSeekerBrute
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class RideableSeekerBrute : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.RideableBrute";
        public const string PluginName = "RideableSeekerBrute";
        public const string PluginVersion = "0.0.1";

        private static readonly Harmony harmony = new Harmony(PluginGUID);

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        AssetBundle assetBundle;

        GameObject saddleSimple;
        GameObject saddleNoCloth;
        GameObject saddleMedium;
        GameObject saddleHeavy;

        GameObject seekerBrute;
        GameObject bunny;
        GameObject deer;
        GameObject wolf;
        GameObject boar;
        GameObject abomination;
        GameObject warg;
        GameObject drake;

        private ConfigEntry<KeyboardShortcut> configAttackKey;
        private ConfigEntry<KeyboardShortcut> configAttackSecondaryKey;
        private ConfigEntry<bool> configDebug;

        private ButtonConfig AttackButton;
        private ButtonConfig AttackSecondaryButton;

        private void Awake()
        {
            InitConfig();

            
            ModQuery.Enable();
            harmony.PatchAll();

            InitAssetBundle();
            InitInputs();

            PrefabManager.OnVanillaPrefabsAvailable += InitMountable;
            PrefabManager.OnPrefabsRegistered += InitCustomMounts;
        }

        private void Update()
        {
            if (ZInput.instance == null)
                return;

            Player player = Player.m_localPlayer;

            if (!player)
                return;

            AttackButton.BlockOtherInputs = player.IsRiding();
            AttackSecondaryButton.BlockOtherInputs = player.IsRiding();

            if (!player.IsRiding())
                return;

            Humanoid character = (Humanoid)player.GetDoodadController().GetControlledComponent();

            if (ZInput.GetButton(AttackButton.Name) && !character.InAttack())
            {
                Jotunn.Logger.LogWarning(AttackButton.BlockOtherInputs);
                if (character.m_defaultItems.Length > 0 && character.m_defaultItems[0] != null)
                {
                    Jotunn.Logger.LogInfo("Attack!");
                    character.m_rightItem = character.m_defaultItems[0].GetComponent<ItemDrop>().m_itemData;
                    character.StartAttack(null, false);
                }
            }
            else if (ZInput.GetButton(AttackSecondaryButton.Name) && !character.InAttack())
            {
                if (character.m_defaultItems.Length > 1 && character.m_defaultItems[1] != null)
                {
                    Jotunn.Logger.LogInfo("Secondary attack!");
                    character.m_rightItem = character.m_defaultItems[1].GetComponent<ItemDrop>().m_itemData;
                    character.StartAttack(null, false);
                }
            }
        }

        private void InitCustomMounts()
        {
            // Jotunn.Logger.LogInfo($"Modded prefabs:");
            foreach (var moddedPrefab in ModQuery.GetPrefabs())
            {
                // Jotunn.Logger.LogInfo($"  {moddedPrefab.Prefab.name} added by {moddedPrefab.SourceMod.Name}");
                if (moddedPrefab.Prefab.name == "AshenWarg_TW")
                {
                    warg = moddedPrefab.Prefab;
                    Jotunn.Logger.LogInfo("Found Warg!");
                    break;
                }
            }

            if (warg != null)
            {
                TameableConfig tameableConfig = new TameableConfig();
                tameableConfig.feedDuration = 15;
                tameableConfig.tamingTime = 15;
                tameableConfig.commandable = true;
                tameableConfig.AddConsumeItem(PrefabManager.Instance.GetPrefab("RawMeat").GetComponent<ItemDrop>());
                tameableConfig.AddConsumeItem(PrefabManager.Instance.GetPrefab("DeerMeat").GetComponent<ItemDrop>());

                TameableManager tameable = new TameableManager();
                tameable.MakeTameable(warg, tameableConfig);

                // Mount stuff
                GameObject mediumSaddle = PrefabManager.Instance.GetPrefab("SaddleMedium_DW");
                Jotunn.Logger.LogWarning("Saddle prefab: "+ mediumSaddle?.name);

                SaddleConfig saddleConfigMedium = new SaddleConfig(mediumSaddle, "Visual");
                saddleConfigMedium.hoverText = "Medium Saddle";
                saddleConfigMedium.maxStamina = 240;
                saddleConfigMedium.attach.position = new Vector3(0, 1.6f, 0);
                saddleConfigMedium.attach.scale = new Vector3(1, 1, 1);
                saddleConfigMedium.sphereRadius = 0.6f;

                MountableConfig wargAttachConfig = new MountableConfig("BARGHEST_ Spine");
                wargAttachConfig.characterAttach.position = new Vector3(-0.6f, -0.4f, 0);
                wargAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 270, 180);
                wargAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
                wargAttachConfig.saddleAttach.position = new Vector3(0, -0.4f, -0.27f);
                wargAttachConfig.saddleAttach.scale = new Vector3(0.7f, 0.7f, 0.7f);

                MountableManager.Instance.MakeMountable(warg, saddleConfigMedium, wargAttachConfig);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= InitCustomMounts;
        }

        private void InitMountable()
        {
            seekerBrute = PrefabManager.Instance.GetPrefab("SeekerBrute");
            boar = PrefabManager.Instance.GetPrefab("Boar");
            wolf = PrefabManager.Instance.GetPrefab("Wolf");
            deer = PrefabManager.Instance.GetPrefab("Deer");
            bunny = PrefabManager.Instance.GetPrefab("Hare");
            abomination = PrefabManager.Instance.GetPrefab("Abomination");
            drake = PrefabManager.Instance.GetPrefab("Hatchling");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(drake, true));

            ItemConfig saddleConfig = new ItemConfig();
            saddleConfig.Name = "Saddle";
            saddleConfig.Description = "A saddle, used for mounts!";
            saddleConfig.CraftingStation = "Workbench";
            saddleConfig.AddRequirement(new RequirementConfig("LeatherScraps", 10));
            saddleConfig.AddRequirement(new RequirementConfig("DeerHide", 10));
            saddleConfig.AddRequirement(new RequirementConfig("Chain", 4));
            saddleConfig.AddRequirement(new RequirementConfig("Wood", 10));
            CustomItem saddle = new CustomItem(saddleNoCloth, true, saddleConfig);
            ItemManager.Instance.AddItem(saddle);

            ItemConfig simpleSaddleConfig = new ItemConfig();
            simpleSaddleConfig.Name = "Simple Saddle";
            simpleSaddleConfig.Description = "A saddle, used for mounts!";
            simpleSaddleConfig.CraftingStation = "Workbench";
            simpleSaddleConfig.AddRequirement(new RequirementConfig("LeatherScraps", 10));
            simpleSaddleConfig.AddRequirement(new RequirementConfig("DeerHide", 10));
            simpleSaddleConfig.AddRequirement(new RequirementConfig("Chain", 4));
            simpleSaddleConfig.AddRequirement(new RequirementConfig("Wood", 10));
            CustomItem simpleSaddle = new CustomItem(saddleSimple, true, simpleSaddleConfig);
            ItemManager.Instance.AddItem(simpleSaddle);

            ItemConfig mediumSaddleConfig = new ItemConfig();
            mediumSaddleConfig.Name = "Medium Saddle";
            mediumSaddleConfig.Description = "A comfy saddle, used for mounts!";
            mediumSaddleConfig.CraftingStation = "Workbench";
            mediumSaddleConfig.AddRequirement(new RequirementConfig("LeatherScraps", 10));
            mediumSaddleConfig.AddRequirement(new RequirementConfig("DeerHide", 10));
            mediumSaddleConfig.AddRequirement(new RequirementConfig("Chain", 4));
            mediumSaddleConfig.AddRequirement(new RequirementConfig("RoundLog", 10));
            CustomItem mediumSaddle = new CustomItem(saddleMedium, true, mediumSaddleConfig);
            ItemManager.Instance.AddItem(mediumSaddle);

            ItemConfig heavySaddleConfig = new ItemConfig();
            heavySaddleConfig.Name = "Heavy Saddle";
            heavySaddleConfig.Description = "A huge saddle, used for big mounts!";
            heavySaddleConfig.CraftingStation = "Workbench";
            heavySaddleConfig.AddRequirement(new RequirementConfig("LeatherScraps", 10));
            heavySaddleConfig.AddRequirement(new RequirementConfig("DeerHide", 10));
            heavySaddleConfig.AddRequirement(new RequirementConfig("Chain", 4));
            heavySaddleConfig.AddRequirement(new RequirementConfig("FineWood", 10));
            CustomItem heavySaddle = new CustomItem(saddleHeavy, true, heavySaddleConfig);
            ItemManager.Instance.AddItem(heavySaddle);

            // Tame stuff
            TameableConfig tameableConfig = new TameableConfig();
            tameableConfig.feedDuration = 15;
            tameableConfig.tamingTime = 15;
            tameableConfig.commandable = true;
            tameableConfig.AddConsumeItem(PrefabManager.Instance.GetPrefab("RawMeat").GetComponent<ItemDrop>());
            tameableConfig.AddConsumeItem(PrefabManager.Instance.GetPrefab("DeerMeat").GetComponent<ItemDrop>());

            TameableManager tameable = new TameableManager();
            tameable.MakeTameable(seekerBrute, tameableConfig);
            tameable.MakeTameable(wolf, tameableConfig);
            tameable.MakeTameable(boar, tameableConfig);
            tameable.MakeTameable(bunny, tameableConfig);
            tameable.MakeTameable(deer, tameableConfig);
            tameable.MakeTameable(abomination, tameableConfig);
            //tameable.MakeTameable(drake, tameableConfig);

            // Mount stuff
            SaddleConfig saddleConfigHeavy = new SaddleConfig(heavySaddle.ItemPrefab, "Visual");
            saddleConfigHeavy.hoverText = "Heavy Saddle";
            saddleConfigHeavy.maxStamina = 240;
            saddleConfigHeavy.attach.position = new Vector3(0, 2, 0);
            saddleConfigHeavy.attach.scale = new Vector3(1, 1, 1);
            saddleConfigHeavy.sphereRadius = 1;

            SaddleConfig saddleConfigMedium = new SaddleConfig(mediumSaddle.ItemPrefab, "Visual");
            saddleConfigMedium.hoverText = "Medium Saddle";
            saddleConfigMedium.maxStamina = 240;
            saddleConfigMedium.attach.position = new Vector3(0, 2, 0);
            saddleConfigMedium.attach.scale = new Vector3(1, 1, 1);
            saddleConfigMedium.sphereRadius = 1;

            SaddleConfig saddleConfigSimple = new SaddleConfig(simpleSaddle.ItemPrefab, "Visual");
            saddleConfigSimple.hoverText = "Simple Saddle";
            saddleConfigSimple.maxStamina = 240;
            saddleConfigSimple.attach.position = new Vector3(0, 2, 0);
            saddleConfigSimple.attach.scale = new Vector3(1, 1, 1);
            saddleConfigSimple.sphereRadius = 1;

            MountableConfig bruteAttachConfig = new MountableConfig("Visual/Armature/root/root2/hip");
            bruteAttachConfig.characterAttach.position = new Vector3(0, 0.013f, 0.004f);
            bruteAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(270, 180, 0);
            bruteAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            bruteAttachConfig.saddleAttach.position = new Vector3(0, -0.004f, -0.0037f);
            bruteAttachConfig.saddleAttach.scale = new Vector3(0.008f, 0.008f, 0.008f);

            MountableConfig wolfAttachConfig = new MountableConfig("Visual/WolfSmooth/CG/Pelvis/Spine/Spine1");
            wolfAttachConfig.characterAttach.position = new Vector3(-0.25f, -0.03f, 0);
            wolfAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(350, 270, 180);
            wolfAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            wolfAttachConfig.saddleAttach.position = new Vector3(0, -0.27f, -0.22f);
            wolfAttachConfig.saddleAttach.scale = new Vector3(0.52f, 0.52f, 0.52f);

            MountableConfig boarAttachConfig = new MountableConfig("Visual/CG");
            boarAttachConfig.characterAttach.position = new Vector3(-0.7f, 0, 0.1f);
            boarAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 270, 270);
            boarAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            boarAttachConfig.saddleAttach.position = new Vector3(0, -0.4f, -0.33f);
            boarAttachConfig.saddleAttach.scale = new Vector3(0.75f, 0.75f, 0.75f);

            MountableConfig deerAttachConfig = new MountableConfig("Visual/CG");
            deerAttachConfig.characterAttach.position = new Vector3(-0.75f, 0, 0.08f);
            deerAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 270, 270);
            deerAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            deerAttachConfig.saddleAttach.position = new Vector3(0, -0.45f, -0.3f);
            deerAttachConfig.saddleAttach.scale = new Vector3(0.8f, 0.8f, 0.8f);

            MountableConfig hareAttachConfig = new MountableConfig("Visual/Armature/Root");
            hareAttachConfig.characterAttach.position = new Vector3(0, 0.008f, -0.003f);
            hareAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 180, 0);
            hareAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            hareAttachConfig.saddleAttach.position = new Vector3(0, -0.0037f, -0.003f);
            hareAttachConfig.saddleAttach.scale = new Vector3(0.0075f, 0.0075f, 0.0075f);

            MountableConfig abomAttachConfig = new MountableConfig("Visual/Armature.001/root/hip");
            abomAttachConfig.characterAttach.position = new Vector3(-0.0013f, 0.011f, 0);
            abomAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(270, 90, 0);
            abomAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            abomAttachConfig.saddleAttach.position = new Vector3(0, -0.0033f, -0.0025f);
            abomAttachConfig.saddleAttach.scale = new Vector3(0.006f, 0.006f, 0.006f);

            //MountableConfig drakeAttachConfig = new MountableConfig("Neck2");
            //drakeAttachConfig.characterAttach.position = new Vector3(0, -0.001f, -0.005f);
            //drakeAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(285, 0, 180);
            //drakeAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            //drakeAttachConfig.saddleAttach.position = new Vector3(0, 0, 0);
            //// drakeAttachConfig.saddleAttach.scale = new Vector3(0.01f, 0.01f, 0.01f);
            //drakeAttachConfig.saddleAttach.scale = new Vector3(0, 0, 0);
            ////drakeAttachConfig.characterAttach.position = new Vector3(0, 0, 0);
            ////drakeAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(-90, 0, 0);
            ////drakeAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            ////drakeAttachConfig.saddleAttach.position = new Vector3(0, 0, 0);
            ////drakeAttachConfig.saddleAttach.scale = new Vector3(0.008f, 0.008f, 0.008f);

            MountableManager.Instance.MakeMountable(seekerBrute, saddleConfigHeavy, bruteAttachConfig);

            // saddleConfig.sphereRadius = 0.9f;
            saddleConfigSimple.sphereRadius = 0.45f;
            saddleConfigSimple.attach.position = new Vector3(0, 1.4f, 0.1f);
            MountableManager.Instance.MakeMountable(wolf, saddleConfigSimple, wolfAttachConfig);

            // saddleConfig.sphereRadius = 0.9f;
            saddleConfigSimple.sphereRadius = 0.45f;
            saddleConfigSimple.attach.position = new Vector3(0, 1.1f, 0);
            MountableManager.Instance.MakeMountable(boar, saddleConfigSimple, boarAttachConfig);

            // saddleConfig.sphereRadius = 0.7f;
            saddleConfigSimple.sphereRadius = 0.35f;
            saddleConfigSimple.attach.position = new Vector3(0, 1, -0.05f);
            MountableManager.Instance.MakeMountable(bunny, saddleConfigSimple, hareAttachConfig);

            // saddleConfig.sphereRadius = 1;
            saddleConfigSimple.sphereRadius = 0.5f;
            saddleConfigSimple.attach.position = new Vector3(0, 1.6f, 0);
            MountableManager.Instance.MakeMountable(deer, saddleConfigSimple, deerAttachConfig);

            saddleConfigHeavy.maxUseRange = 6;
            saddleConfigHeavy.sphereRadius = 0.7f;
            saddleConfigHeavy.attach.position = new Vector3(0, 2.5f, 1.5f);
            MountableManager.Instance.MakeMountable(abomination, saddleConfigHeavy, abomAttachConfig);

            //// saddleConfig.sphereRadius = 0.9f;
            //saddleConfigHeavy.maxUseRange = 20;
            //saddleConfigSimple.sphereRadius = 1;
            //saddleConfigSimple.attach.position = new Vector3(0, 2, 1);
            //MountableManager.Instance.MakeMountable(drake, saddleConfigSimple, drakeAttachConfig);

            PrefabManager.OnVanillaPrefabsAvailable -= InitMountable;
        }

        private void InitConfig()
        {
            Config.SaveOnConfigSet = true;

            configDebug = Config.Bind("General", "Debug", false,
                new ConfigDescription("Put mod in debug mode", null));
            // configDebug.SettingChanged += (obj, attr) => { };

            //configAttackKey = Config.Bind("General", "Attack", new KeyboardShortcut((KeyCode)323),
            //     new ConfigDescription("Attack with main attack", null));
            //configAttackSecondaryKey = Config.Bind("General", "Secondary attack", new KeyboardShortcut((KeyCode)325),
            //    new ConfigDescription("Attack with secondary attack", null));
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("saddles_dw");
            saddleHeavy = assetBundle.LoadAsset<GameObject>("SaddleHeavy_DW");
            saddleMedium = assetBundle.LoadAsset<GameObject>("SaddleMedium_DW");
            saddleSimple = assetBundle.LoadAsset<GameObject>("SaddleSimple_DW");
            saddleNoCloth = assetBundle.LoadAsset<GameObject>("SaddleNoCloth_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(saddleHeavy, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(saddleMedium, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(saddleSimple, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(saddleNoCloth, true));
        }

        /**
         * Initialise the inputs of this mod
         */
        private void InitInputs()
        {
            try
            {
                AttackButton = new ButtonConfig
                {
                    Name = "Attack",
                    Key = (KeyCode)323,
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(PluginGUID, AttackButton);

                AttackSecondaryButton = new ButtonConfig
                {
                    Name = "Secondary attack",
                    Key = (KeyCode)325,
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(PluginGUID, AttackSecondaryButton);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + error);
            }
        }
    }
}

