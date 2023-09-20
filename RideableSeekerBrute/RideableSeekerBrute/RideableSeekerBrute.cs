using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace RideableSeekerBrute
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class RideableSeekerBrute : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Test1";
        public const string PluginName = "RideableSeekerBrute";
        public const string PluginVersion = "0.0.1";

        private static readonly Harmony harmony = new Harmony(PluginGUID);

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        AssetBundle assetBundle;

        GameObject saddleSimple;
        GameObject saddleHeavy;

        GameObject seekerBrute;
        GameObject bunny;
        GameObject deer;
        GameObject wolf;
        GameObject boar;

        private void Awake()
        {
            harmony.PatchAll();

            InitAssetBundle();

            PrefabManager.OnVanillaPrefabsAvailable += InitMountable;
        }

        private void InitMountable()
        {
            seekerBrute = PrefabManager.Instance.GetPrefab("SeekerBrute");
            boar = PrefabManager.Instance.GetPrefab("Boar");
            wolf = PrefabManager.Instance.GetPrefab("Wolf");
            deer = PrefabManager.Instance.GetPrefab("Deer");
            bunny = PrefabManager.Instance.GetPrefab("Hare");

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

            // Mount stuff
            SaddleConfig saddleConfigHeavy = new SaddleConfig(heavySaddle.ItemPrefab, "Visual");
            saddleConfigHeavy.hoverText = "Heavy Saddle";
            saddleConfigHeavy.maxStamina = 240;
            saddleConfigHeavy.attach.position = new Vector3(0, 2, 0);
            saddleConfigHeavy.attach.scale = new Vector3(1, 1, 1);
            saddleConfigHeavy.sphereRadius = 1;

            SaddleConfig saddleConfigSimple = new SaddleConfig(simpleSaddle.ItemPrefab, "Visual");
            saddleConfigSimple.hoverText = "Simple Saddle";
            saddleConfigSimple.maxStamina = 240;
            saddleConfigSimple.attach.position = new Vector3(0, 2, 0);
            saddleConfigSimple.attach.scale = new Vector3(1, 1, 1);
            saddleConfigSimple.sphereRadius = 1;

            MountableConfig bruteAttachConfig = new MountableConfig("hip");
            bruteAttachConfig.characterAttach.position = new Vector3(0, 0.013f, 0.004f);
            bruteAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(270, 180, 0);
            bruteAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            bruteAttachConfig.saddleAttach.position = new Vector3(0, -0.004f, -0.0037f);
            bruteAttachConfig.saddleAttach.scale = new Vector3(0.008f, 0.008f, 0.008f);

            MountableConfig wolfAttachConfig = new MountableConfig("Spine1");
            wolfAttachConfig.characterAttach.position = new Vector3(-0.25f, -0.03f, 0);
            wolfAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(350, 270, 180);
            wolfAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            wolfAttachConfig.saddleAttach.position = new Vector3(0, -0.27f, -0.22f);
            wolfAttachConfig.saddleAttach.scale = new Vector3(0.52f, 0.52f, 0.52f);

            MountableConfig boarAttachConfig = new MountableConfig("CG");
            boarAttachConfig.characterAttach.position = new Vector3(-0.7f, 0, 0.1f);
            boarAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 270, 270);
            boarAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            boarAttachConfig.saddleAttach.position = new Vector3(0, -0.4f, -0.33f);
            boarAttachConfig.saddleAttach.scale = new Vector3(0.75f, 0.75f, 0.75f);

            MountableConfig deerAttachConfig = new MountableConfig("CG");
            deerAttachConfig.characterAttach.position = new Vector3(-0.75f, 0, 0.08f);
            deerAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 270, 270);
            deerAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            deerAttachConfig.saddleAttach.position = new Vector3(0 , -0.45f, -0.3f);
            deerAttachConfig.saddleAttach.scale = new Vector3(0.8f, 0.8f, 0.8f);

            MountableConfig hareAttachConfig = new MountableConfig("Root");
            hareAttachConfig.characterAttach.position = new Vector3(0, 0.008f, -0.003f);
            hareAttachConfig.characterAttach.rotation.eulerAngles = new Vector3(0, 180, 0);
            hareAttachConfig.characterAttach.scale = new Vector3(1, 1, 1);
            hareAttachConfig.saddleAttach.position = new Vector3(0, -0.0037f, -0.003f);
            hareAttachConfig.saddleAttach.scale = new Vector3(0.0075f, 0.0075f, 0.0075f);

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
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("customsaddles");
            saddleHeavy = assetBundle.LoadAsset<GameObject>("SaddleHeavy");
            saddleSimple = assetBundle.LoadAsset<GameObject>("SaddleSimple");
        }
    }
}

