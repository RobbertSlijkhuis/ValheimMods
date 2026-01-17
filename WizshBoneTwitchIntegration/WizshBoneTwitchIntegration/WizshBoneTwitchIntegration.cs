using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
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
        public CustomEffectLists effectLists = new CustomEffectLists();
        private ButtonConfig wizshBoneWindowButton;
        public static bool useRedeemCommand = false;
        public static readonly string NoDamageStructureGroup = "WBTI_NoDamageStructure";

        public static readonly string customConfigPath = "BepInEx/config/WizshBoneTwitchIntegration";
        public static readonly string redeemsConfigPath = customConfigPath + "/redeems.yaml";
        public static readonly string redeemsSchemaPath = customConfigPath + "/redeems-schema.json";
        public static readonly string bannedPath = customConfigPath + "/banned.txt";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        public void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            InitStatusEffects();
            InitInputs();
            InitCommands();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += InitRedeemsFile;
            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
            PrefabManager.OnVanillaPrefabsAvailable += AddEffectLists;
            PrefabManager.OnPrefabsRegistered += AddPersistentComponents;
            ItemManager.OnItemsRegistered += LogStatusEffects;
        }

        public void Update()
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
            prefabs.ChestIron.AddComponent<TwitchSurpriseChest>();
            prefabs.ChestIron.transform.Find("chest_top").gameObject.AddComponent<TwitchSurpriseChestInteract>();
            prefabs.ChestGold.AddComponent<TwitchSurpriseChest>();

            prefabs.ChestGold.transform.Find("chest_top").gameObject.AddComponent<TwitchSurpriseChestInteract>();
            prefabs.GuardStone.transform.Find("AreaMarker").gameObject.AddComponent<TwitchSafeZone>();
            prefabs.GuardStone.transform.Find("controls").gameObject.AddComponent<TwitchSafeZoneControls>();

            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Enabled = true;
            pieceConfig.Name = "Twitchy Ward";
            pieceConfig.Description = "A ward against mysterious and twitchy forces!";
            pieceConfig.PieceTable = PieceTables.Hammer;
            pieceConfig.Category = PieceCategories.Misc;
            pieceConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(PluginConfig.configWardRecipe.Value, null, null);
            PieceManager.Instance.AddPiece(new CustomPiece(prefabs.GuardStone, true, pieceConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;
        }

        private void AddPersistentComponents()
        {
            Jotunn.Logger.LogWarning("AddPersistentComponents()");
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();

                if (humanoid != null && monsterAI != null)
                {
                    // Jotunn.Logger.LogWarning($"Adding persistent data to {name}");
                    prefab.AddComponent<TwitchCreaturePersistentData>();

                    Transform visualTrans = prefab.transform.Find("Visual");

                    if (visualTrans == null)
                        continue;

                    LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                    if (levelEffects == null)
                        continue;

                    // Jotunn.Logger.LogWarning($"Adding levelsetups to {name}");
                    LevelEffects.LevelSetup levelSetup = levelEffects.m_levelSetups[1];

                    for (int index = 0; index < 8; index++)
                    {
                        levelEffects.m_levelSetups.Add(levelSetup);
                    }

                    continue;
                }

                Mister mister = prefab.GetComponent<Mister>();
                Trap trap = prefab.GetComponent<Trap>();

                if (mister != null || trap != null)
                {
                    Jotunn.Logger.LogWarning($"Adding persistent destruction to {name}");
                    prefab.AddComponent<TwitchPersistentDestruction>();
                }
            }

            PrefabManager.OnPrefabsRegistered -= AddPersistentComponents;
        }

        private void AddEffectLists()
        {
            EffectList.EffectData SpawnEffectData = new EffectList.EffectData();
            SpawnEffectData.m_enabled = true;
            SpawnEffectData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_small");
            SpawnEffectData.m_variant = -1;

            List<EffectList.EffectData> SpawnEffectDataList = new List<EffectList.EffectData>();
            SpawnEffectDataList.Add(SpawnEffectData);

            effectLists.SpawnEffect.m_effectPrefabs = SpawnEffectDataList.ToArray();
            PrefabManager.OnVanillaPrefabsAvailable -= AddEffectLists;
        }

        private void InitRedeemsFile()
        {
            ExtraConfigHelper.InitRedeemsConfig();
            ExtraConfigHelper.ReadRedeemsConfig();
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
            CommandManager.Instance.AddConsoleCommand(new ClearMonsterClaimsCommand());
            CommandManager.Instance.AddConsoleCommand(new BanTwitchUser());
            CommandManager.Instance.AddConsoleCommand(new ListBannedTwitchUsers());
            CommandManager.Instance.AddConsoleCommand(new ReloadRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new SetRedeemAlias());
            CommandManager.Instance.AddConsoleCommand(new UpdateRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new UnbanTwitchUser());
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
            // Roots (enemy/friendly)
            // Timer met warning 10min voren om opnieuw in te loggen, to fix enable/disable redeems en auto-resolve
            // ALLOW PPL TO POST LINKS IN CHAT FOR A SMALL MOMENT
            // Add check for resource meads, if there is cooldown and prevent usage.
            // Trap field redeem
            // Shrink/Grow cancel each other out
            // TalkInteract always show Feo's history fact message, also does not properly show follow/rename creature
            // Armor and shield don't get thrown out very far out of suprise chests
            // Apply creature data settings to creates spawned from SpawnShower
            // Apply creature data settings to creates spawned from Suprise chests
            // Allow SpawnShower to be spawned from Suprise chests
            // Reverse controlls redeem
            // Temp naked redeem
            // Remove/add redeems when player leaves/enters a dungeon and check what kind of dungeon the player is in
            // Add leader board of points spend, deaths caused, saves maybe? Other statistics?
            //
            // ====================================
            // IN PROGRESS:
            // ====================================
            // Trap field redeem
            // DONE: Add recipe in config for twitch ward, also add option to turn of burning spawns to death
            // DONE: Add map markers where you spawned certain stuff (surprise chests)
            // DONE: Make surpise chests floatable in water, reduce mass to not sink ships XD, add map pin and auto remove it, make them persistent
            // DONE: Finish all types of statuseffects and make it decently configurable
            // DONE: Add configurable timers to status effect, persist through death, renew
            // DONE: Mod says it refunded stuff from a custom redeem, should not do that.
            // Wind in back (moder) and reverse redeem
            // Remove redeems on game quit
            // Add halucinations, make player stunned/dazed when getting hit by Hallucinations? Or half damage?
            // Suprise chests, multiple chests to gamble, add a mimic to bite the opener (add legs like the luggage from terry pratchett's novel
            //
            // ====================================
            // IDEAS:
            // ====================================
            // MORE POSITIVE EFFECTS
            // LoyalBones: A red skeleton with normal damage but insane health pool
            // Chat loves this: Spawn chest with cheese (random food?) Make cheese wheel that gives random food?".
            // Add a way to be able to find spanwed creature. For the kill all spawned rule!
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
            // Enable/disable redeems did not work sometimes due to DUPLICATE redeem and long time playing?
            // Poiunts somehow are not refunded...

            // New mod ideas:
            // - Inventory in the Saddle
            // - Priortise spear slot
            // - lock inventory slots so putting all in inventory does not touch it
            // - square cultivator/hoe mod (no circle editing)
        }
    }
}

