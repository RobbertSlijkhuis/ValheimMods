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

            prefabs.ChestIron.AddComponent<TwitchSurpriseChest>();
            prefabs.ChestGold.AddComponent<TwitchSurpriseChest>();

            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void AddPersistentDataToCreatures()
        {
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();

                if (humanoid != null && monsterAI != null)
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
            CommandManager.Instance.AddConsoleCommand(new UseRedeemCommand());
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
            prefabs.NeckBeard = assetBundle.LoadAsset<GameObject>("FeoTheHistorian_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.NeckBeard, true));
            prefabs.ChestIron = assetBundle.LoadAsset<GameObject>("ChestIron_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ChestIron, true));
            prefabs.ChestGold = assetBundle.LoadAsset<GameObject>("ChestGold_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ChestGold, true));

            effects.Burning = assetBundle.LoadAsset<SE_Stats>("Burning_WBTI");
            effects.Freezing = assetBundle.LoadAsset<StatusEffect>("Freezing_WBTI");
            effects.Poison = assetBundle.LoadAsset<StatusEffect>("Poison_WBTI");
            effects.NoFallDamage = assetBundle.LoadAsset<StatusEffect>("NoFallDamage_WBTI");

            sprites.MiniMeSprite = assetBundle.LoadAsset<Sprite>("MiniMeSprite_WBTI");
            sprites.BigMeSprite = assetBundle.LoadAsset<Sprite>("BigMeSprite_WBTI");

            // ====================================
            // TODO:
            // ====================================
            // Add extra stars to hud when level is higher then 3 (max level is 10)
            // Remove/add redeems when player leaves/enters a dungeon and check what kind of dungeon the player is in
            // Prevent redeems from being used when the game is paused
            // White/black list to block users from using the mod
            //
            // ====================================
            // IN PROGRESS:
            // ====================================
            // Remove redeems on game quit
            // Add halucinations, make player stunned/dazed when getting hit by Hallucinations? Or half damage?
            // Suprise chests, multiple chests to gamble, add a mimic to bite the opener
            //
            // ====================================
            // IDEAS:
            // ====================================
            // MORE POSITIVE EFFECTS
            // LoyalBones: A red skeleton with normal damage but insane health pool
            // Chat loves this: Spawn chest with cheese (random food?) Make cheese wheel that gives random food?".
            // Add a way to be able to find spanwed creature. For the kill all spawned rule!
            // Add temp mist fog to location
            // Drunk blur effect?
            // Flashbang?
            // Disable all twitchy wards via bits (specific amount like 1000)
            // Add commands for claimed creatures to do things (flee, stop, attack player/base, activate specific attack, emote?)
            // Add options to super charge a spawned creature, more hp, damage, equip gear?

            // Add references to friends, streamers, viewers in a seperate mod. Like I did with troll Betuti line
            // - DeathWizsh the Raven landing on streamers shoulder saying "There is a mod for that" every time someone says that in chat
            // - Feo the neckbeard (A neck with a beard) that randomly gives you history facts (spawned by redeem?)
            // - Sjoeky an actuall goat that gives you random combat/survial tips when you die to much in the most dry and sarcastic manner (spawn at your respawn location and redeem aswell?)
            // - Kimetsu a Dvergr mage, blessing?
            // - Soma_af a bear (reskinned as a white/pink teddybear with antlers as soma has a bear with antlers emote) Spawns when cooking? Drops random food when sneezing? Adds a 4th food slot?
            // - Make DurdyJay exactly like Odin, and spawns randomly in like Odin. But instead of dissappearing straight away he gets Googly Eyes and a stick out Tongue and says something nice/somewhat durdy. And his name changes to DurdyJay at that moment ofc (blessing?)
            // - Xxainty iets van een greydwarf ofzo
            // - itsnanobug?
            // - Azeriath? Blessing: Less fall damage he said
            // - jaqkEquips
            // - PithyPeaches?
            // - LoyalBones and KrzyMoogle?
            // References are kinda like gods? Allow them to give you blessings when encountered? Feo relaxing you, rested. Sjoeky "encouraging you", damage reduction
            // Some gods can be summoned via a ritual (new piece spawned in the world? and/or buildable one) Some gods can be angered and do something negative
            // - Feo summon alter spawns in meadows here and there, you can then make a food to summon him (includes neck tail as joke, he does not know)
            //   He can also get angry if you don't sit down to listen to his history fact or if you try to summon him when the summon is on cooldown
            //   3 summons to anger him, 3 summons while angered and then you get smiten by Obliterator lightning
            // - Soma_af can also be summoned and gives food to the player (back-up emergency food) according to highest boss kill (Global or player key)
            //   Will summon big teddybear version when angered that hunts you down! She has a temper
            // - When dieing to much Sjoeky will give tips and damage reduction blessing, during blessing timer he can also randomly help and fight.


            // What went wrong:
            // Enable/disable redeems did not work sometimes due to DUPLICATE redeem

            // New mod ideas:
            // - Inventory in the Saddle
            // - Priortise spear slot

        }
    }
}

