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
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void InitRedeemsFile()
        {
            YAMLHelper.InitRedeemsConfig();
            YAMLHelper.ReadRedeemsConfig(true);
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
            // Chat loves this: Spawn chest with small healing meads in there and call it "cheese mead".
            // IN PROGRESS: Remove redeems on game quit
            // IN PROGRESS: Add halucinations, make player stunned/dazed when getting hit by Hallucinations? Or half damage?
            // IN PROGRESS: Load redeems from YAML file
            // ====================================
            // Ideas:
            // MORE POSITIVE EFFECTS
            // Add temp mist fog to location
            // Drunk blur effect?
            // Flashbang?
            // Disable all twitchy wards via bits (specific amount like 1000)
            // White/black list to block users from using the mod
        }
    }
}

