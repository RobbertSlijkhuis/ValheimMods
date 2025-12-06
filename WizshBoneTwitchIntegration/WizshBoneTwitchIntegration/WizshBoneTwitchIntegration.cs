using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.Commands;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class WizshBoneTwitchIntegration : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.WizshBoneTwitchIntegration";
        public const string PluginName = "WizshBoneTwitchIntegration";
        public const string PluginVersion = "0.0.1";
        public static WizshBoneTwitchIntegration Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();
        public CustomSprites sprites = new CustomSprites();
        private ButtonConfig wizshBoneWindowButton;
        public static string customConfigPath = "BepInEx/config/WizshBoneTwitchIntegration";
        public static string redeemsConfigPath = customConfigPath + "/redeems.yaml";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            InitRedeemsFile();
            InitStatusEffects();
            InitInputs();
            InitCommands();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
            PrefabManager.OnPrefabsRegistered += AddPersistentDataToCreatures;
            ItemManager.OnItemsRegistered += LogStatusEffects;
        }

        private void Update()
        {
            try
            {
                if (ZInput.instance == null || wizshBoneWindowButton == null || !ZInput.GetButtonDown(wizshBoneWindowButton.Name) || !Player.m_localPlayer)
                    return;

                TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();
                authComp.loginGUI.ShowGUI();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not show WizshBone settings GUI: " + e);
            }
        }

        private void LogStatusEffects()
        {
            //Jotunn.Logger.LogWarning("LogStatusEffects");
            //foreach (var effect in ObjectDB.m_instance.m_StatusEffects)
            //{
            //    Jotunn.Logger.LogWarning("========================================");
            //    Jotunn.Logger.LogWarning("Name: " + effect.name);
            //    Jotunn.Logger.LogWarning("NameHash(): " + effect.NameHash());
            //}
            ItemManager.OnItemsRegistered -= LogStatusEffects;
        }

        private void AddPieces()
        {
            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Enabled = true;
            pieceConfig.Name = "Twitchy Ward";
            pieceConfig.Description = "A ward against mysterious and twitchy forces!";
            pieceConfig.PieceTable = PieceTables.Hammer;
            pieceConfig.Category = PieceCategories.Misc;
            pieceConfig.AddRequirement("FineWood", 5);
            pieceConfig.AddRequirement("GreydwarfEye", 5);
            pieceConfig.AddRequirement("SurtlingCore", 1);
            prefabs.GuardStone.transform.Find("AreaMarker").gameObject.AddComponent<TwitchSafeZone>();
            prefabs.GuardStone.transform.Find("controls").gameObject.AddComponent<TwitchSafeZoneControls>();
            PieceManager.Instance.AddPiece(new CustomPiece(prefabs.GuardStone, true, pieceConfig));

            prefabs.NeckBeard = PrefabManager.Instance.CreateClonedPrefab("NeckBeard", "Neck");
            Transform beardTransform = prefabs.NeckBeard.transform.Find("Visual/Armature/Hips/Spine/Spine1/Neck/Head/Jaw/Jaw_end");
            Humanoid humanoid = prefabs.NeckBeard.GetComponent<Humanoid>();
            GameObject beardPrefab = PrefabManager.Instance.GetPrefab("Beard_06");
            GameObject beardCopy = Instantiate(beardPrefab, beardTransform);
            SkinnedMeshRenderer skinnedMeshRenderer = beardCopy.transform.Find("beard6").gameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.materials[0].SetColor("_SkinColor", new Color(0.4392157f, 0.3803921f, 0.3647059f));

            beardCopy.transform.localPosition = new Vector3(0f, -0.0119f, -0.0594f);
            beardCopy.transform.localRotation = TransformHelper.GenerateRotation(new Vector3(-90, 180f, 0f));
            beardCopy.transform.localScale = new Vector3(0.08f, 0.0333333f, 0.08f);
            humanoid.m_name = "Feo, the Historian";

            Transform lillies = prefabs.NeckBeard.transform.Find("Visual/Lillies");
            lillies.gameObject.SetActive(false);

            PrefabManager.Instance.AddPrefab(prefabs.NeckBeard);
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void AddPersistentDataToCreatures()
        {
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();

                if (humanoid != null && humanoid.m_faction != Character.Faction.Boss && monsterAI != null)
                {
                    Jotunn.Logger.LogWarning($"Adding persistent data to {name}");
                    prefab.AddComponent<TwitchCreaturePersistentData>();
                }
            }

            PrefabManager.OnPrefabsRegistered -= AddPersistentDataToCreatures;
        }

        private void InitRedeemsFile()
        {
            YAMLHelper.InitRedeemsConfig();
            YAMLHelper.ReadRedeemsConfig();
        }

        private void InitStatusEffects()
        {
            effects.MiniMe = ScriptableObject.CreateInstance<StatusEffect>();
            effects.MiniMe.name = "MiniMe";
            effects.MiniMe.m_name = "MiniMe";
            effects.MiniMe.m_icon = sprites.MiniMeSprite;
            effects.MiniMe.m_ttl = PluginConfig.configMiniMeDuration.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.MiniMe, fixReference: false));

            effects.BigMe = ScriptableObject.CreateInstance<StatusEffect>();
            effects.BigMe.name = "BigMe";
            effects.BigMe.m_name = "BigMe";
            effects.BigMe.m_icon = sprites.BigMeSprite;
            effects.BigMe.m_ttl = PluginConfig.configMiniMeDuration.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.BigMe, fixReference: false));

            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Burning, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Freezing, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Poison, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.NoFallDamage, fixReference: true));
        }

        private void InitInputs()
        {
            try
            {
                wizshBoneWindowButton = new ButtonConfig
                {
                    Name = "WizshBone Window",
                    ShortcutConfig = PluginConfig.configWizshBoneWindow,
                };

                InputManager.Instance.AddButton(PluginGUID, wizshBoneWindowButton);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + e);
            }
        }

        private void InitCommands()
        {
            CommandManager.Instance.AddConsoleCommand(new ReloadRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new ClearMonsterClaimsCommand());
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("wbti_dw");

            prefabs.DetectFish = assetBundle.LoadAsset<GameObject>("Detect_Fish_WBTI");
            prefabs.FishRainScript = assetBundle.LoadAsset<GameObject>("spawn_fish_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FishRainScript, true));
            prefabs.GuardStone = assetBundle.LoadAsset<GameObject>("WBTI_guard_stone");
            prefabs.RemoveTheCountry = assetBundle.LoadAsset<GameObject>("RemoveTheCountry_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.RemoveTheCountry, true));

            effects.Burning = assetBundle.LoadAsset<SE_Stats>("Burning_WBTI");
            effects.Freezing = assetBundle.LoadAsset<StatusEffect>("Freezing_WBTI");
            effects.Poison = assetBundle.LoadAsset<StatusEffect>("Poison_WBTI");
            effects.NoFallDamage = assetBundle.LoadAsset<StatusEffect>("NoFallDamage_WBTI");

            sprites.MiniMeSprite = assetBundle.LoadAsset<Sprite>("MiniMeSprite_WBTI");
            sprites.BigMeSprite = assetBundle.LoadAsset<Sprite>("BigMeSprite_WBTI");

            // ====================================
            // TODO:
            // LoyalBones: A red skeleton with normal damage but insane health pool
            // Chat loves this: Spawn chest with cheese (random food?) Make cheese wheel that gives random food?".
            // IN PROGRESS: Remove redeems on game quit
            // IN PROGRESS: Add halucinations, make player stunned/dazed when getting hit by Hallucinations? Or half damage?
            // White/black list to block users from using the mod
            // ====================================
            // Ideas:
            // MORE POSITIVE EFFECTS
            // Add temp mist fog to location
            // Drunk blur effect?
            // Flashbang?
            // Disable all twitchy wards via bits (specific amount like 1000)
            // Add commands for claimed creatures to do things (flee, stop, attack player/base, activate specific attack, emote?)
            // Add options to super charge a spawned creature, more hp, damage, equip gear?
            // Add references to friends, streamers in the mod. Like I did with troll Betuti line
            // - DeathWizsh the Raven landing on streamers shoulder saying "There is a mod for that" every time someone says that in chat
            // - Feo the neckbeard (A neck with a beard) that randomly gives you history facts (Or spanwed by redeem aswell?)
            // - Sjoeky an actuall goat that gives you random combat/survial tips when you die to much in the most dry and sarcastic manner (spawn at your respawn location and redeem aswell?)
            // - Make DurdyJay exactly like Odin, and spawns randomly in like Odin. But instead of dissappearing straight away he gets Googly Eyes and a stick out Tongue and says something nice/somewhat durdy. And his name changes to DurdyJay at that moment ofc
            // - itsnanobug?
            // - Azeriath?
            // - jaqkEquips
            // - PithyPeaches?
            // - LoyalBones and KrzyMoogle?
            // References are kinda like gods? Allow them to give you blessings when encountered? Feo relaxing you, rested. Sjoeky "encouraging you", damage reduction
        }
    }
}

