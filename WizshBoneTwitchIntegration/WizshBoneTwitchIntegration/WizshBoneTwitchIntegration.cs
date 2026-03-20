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
        public CustomMaterials materials = new CustomMaterials();
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

            PrefabManager.OnVanillaPrefabsAvailable += InitExtraConfigFiles;
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
                authComp.wizshBoneGUI.ShowGUI();
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

            prefabs.GuardStone.transform.Find("safezone").gameObject.AddComponent<TwitchSafeZone>();
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

            // Temp
            GameObject shieldGen = PrefabManager.Instance.GetPrefab("piece_shieldgenerator");
            Demister demister = shieldGen.AddComponent<Demister>();
            ParticleSystemForceField forceField = shieldGen.AddComponent<ParticleSystemForceField>();
            forceField.gravity = -0.08f;
            forceField.endRange = 30f;
            forceField.multiplyDragByParticleSize = false;
            forceField.multiplyDragByParticleVelocity = false;
            forceField.rotationAttraction = 1f;
            forceField.vectorFieldAttraction = 1f;
            forceField.vectorFieldSpeed = 1f;
        }

        private void AddPersistentComponents()
        {
            Jotunn.Logger.LogWarning("AddPersistentComponents()");
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();
                Mister mister = prefab.GetComponent<Mister>();
                Trap trap = prefab.GetComponent<Trap>();
                Ship ship = prefab.GetComponent<Ship>();

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

                if (mister != null || trap != null)
                {
                    Jotunn.Logger.LogWarning($"Adding persistent destruction to {name}");
                    prefab.AddComponent<TwitchPersistentDestruction>();
                }

                if (ship != null)
                {
                    Jotunn.Logger.LogWarning($"Adding safezone to ship {name}");
                    Transform onboardTriggerTrans = prefab.transform.Find("OnboardTrigger");
                    BoxCollider boxCollider = onboardTriggerTrans.gameObject.GetComponent<BoxCollider>();
                    onboardTriggerTrans.gameObject.AddComponent<TwitchSafeZone>();
                    boxCollider.includeLayers = LayerMask.GetMask("piece");

                    if (prefab.name == "VikingShip_Ashlands")
                    {
                        boxCollider.center = new Vector3(0.01260833f, 4.1f, 0.0135176f);
                        boxCollider.size = new Vector3(4.536945f, 10f, 3.080277f);
                    }
                    else
                    {
                        boxCollider.center = new Vector3(0, 1.2f, 0);
                        boxCollider.size = new Vector3(1, 3.5f, 1);
                    }
                }

                if (prefab.name == "fuling_trap" || prefab.name == "piece_trap_troll")
                {
                    Jotunn.Logger.LogWarning("Found fuling_trap, applying persistent damage");
                    prefab.gameObject.AddComponent<TwitchPersistentDamage>();
                }
            }

            IndestructibleHelper.SetBoats(PluginConfig.configIndestructibleBoats.Value);
            IndestructibleHelper.SetChests(PluginConfig.configIndestructibleChests.Value);
            IndestructibleHelper.SetPortals(PluginConfig.configIndestructiblePortals.Value);
            IndestructibleHelper.SetVegetables(PluginConfig.configIndestructibleVegetables.Value);

            string[] traders = new string[3] { "Vendor_BlackForest", "Hildir_camp", "BogWitch_Camp" };

            foreach (string name in traders)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Jotunn.Logger.LogWarning($"Adding safezone to Traders {name}");
                Transform forceFieldTransform = prefab.transform.Find("ForceField");

                if (forceFieldTransform == null)
                {
                    Jotunn.Logger.LogError("Could not find force field to add safezone!");
                    continue;
                }

                forceFieldTransform.gameObject.layer = 14;
                CapsuleCollider capsuleCollider = forceFieldTransform.gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = 0.5f;
                capsuleCollider.height = 200f;
                capsuleCollider.isTrigger = true;
                capsuleCollider.includeLayers = LayerMask.GetMask("piece");

                TwitchSafeZone safezone = forceFieldTransform.gameObject.AddComponent<TwitchSafeZone>();
                safezone.m_burnCreatures = false;
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

        private void InitExtraConfigFiles()
        {
            ExtraConfigHelper.InitExtraConfigs();
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
            CommandManager.Instance.AddConsoleCommand(new BanTwitchUserCommand());
            CommandManager.Instance.AddConsoleCommand(new ClearBossKeysCommand());
            CommandManager.Instance.AddConsoleCommand(new ClearCustomStatusEffectsCommand());
            //CommandManager.Instance.AddConsoleCommand(new ClearFlashbangCommand());
            CommandManager.Instance.AddConsoleCommand(new ListBannedTwitchUsers());
            CommandManager.Instance.AddConsoleCommand(new OpenConfigFolderCommand());
            CommandManager.Instance.AddConsoleCommand(new ReloadRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveCreatureClaimCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveSurpriseChestsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTwitchCreaturesCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTwitchMistCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTwitchTrapsCommand());
            CommandManager.Instance.AddConsoleCommand(new SetRedeemAliasCommand());
            CommandManager.Instance.AddConsoleCommand(new TestCommand());
            CommandManager.Instance.AddConsoleCommand(new UpdateRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new UnbanTwitchUserCommand());
            CommandManager.Instance.AddConsoleCommand(new UseRedeemCommand());
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("wbti_dw");

            prefabs.DetectFish = assetBundle.LoadAsset<GameObject>("Detect_Fish_WBTI");
            prefabs.FishRainScript = assetBundle.LoadAsset<GameObject>("spawn_fish_WBTI");
            prefabs.Flashbang = assetBundle.LoadAsset<GameObject>("Flashbang_WBTI");
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

            materials.RecolorAbomination = assetBundle.LoadAsset<Material>("Abomination_recolor_WBTI");
            materials.RecolorBjorn = assetBundle.LoadAsset<Material>("Bjorn_recolor_WBTI");
            materials.RecolorBjornZebra = assetBundle.LoadAsset<Material>("Bjorn_recolor_zebra_WBTI");
            materials.RecolorBlob = assetBundle.LoadAsset<Material>("Blob_recolor_WBTI");
            materials.RecolorBlobSlime = assetBundle.LoadAsset<Material>("BlobSlime_recolor_WBTI");
            materials.RecolorBoar = assetBundle.LoadAsset<Material>("Boar_recolor_WBTI");
            materials.RecolorBoarTusk = assetBundle.LoadAsset<Material>("BoarTusk_recolor_WBTI");
            materials.RecolorDraugr = assetBundle.LoadAsset<Material>("Draugr_recolor_WBTI");
            materials.RecolorDraugrFem = assetBundle.LoadAsset<Material>("Draugr_Ranged_recolor_WBTI");
            materials.RecolorDraugrElite = assetBundle.LoadAsset<Material>("Draugr_Elite_recolor_WBTI");
            materials.RecolorGhost = assetBundle.LoadAsset<Material>("Ghost_recolor_WBTI");
            materials.RecolorGreydwarf = assetBundle.LoadAsset<Material>("Greydwarf_recolor_WBTI");
            materials.RecolorGreydwarfShaman = assetBundle.LoadAsset<Material>("Greydwarf_Shaman_recolor_WBTI");
            materials.RecolorGreydwarfRootsword = assetBundle.LoadAsset<Material>("RootSword_recolor_WBTI");
            materials.RecolorNeck = assetBundle.LoadAsset<Material>("Neck_recolor_WBTI");
            materials.RecolorSkeleton = assetBundle.LoadAsset<Material>("Skeleton_recolor_WBTI");
            materials.RecolorTroll = assetBundle.LoadAsset<Material>("Troll_recolor_WBTI");
            materials.RecolorWraith = assetBundle.LoadAsset<Material>("Wraith_recolor_WBTI");
            materials.RecolorWraithZebra = assetBundle.LoadAsset<Material>("Wraith_recolor_zebra_WBTI");

            // ====================================
            // ValCON:
            // ====================================
            // Setup Twitch integration for a tower defense game? Section of the stream.
            // Add a nuke all creatures command/redeem? Kills all creatures and refunds points
            // Setup different types of surprise chests liked food, creatures, materials? and streamer doesnt know content

            // ====================================
            // TODO:
            // ====================================
            // Redeems still re-enabling when supose to be disabled
            // Add options to allow monsters to fight each other
            // Greydwarfds friendlies do no attack enemy greyfwards
            // Add limit of how much the mob can be active on one time
            // Add name option to creatureData (ZDO)
            // Add size option to creatureData (ZDO)
            // Save/load recolors from a file
            // Shrink/Grow cancel each other out
            // Shrink/grow the boat when player is sailing
            // Reset redeem cooldown somehow when something went wrong? Update CustomRewards?
            // Add internal user/global cooldown functionality cause Twitch's is DODGY AF

            // Add CreatureData and ItemData to SpawnAbility
            // Make safezones square (option)
            // Armor and shield don't get thrown out very far out of suprise chests
            // Boat speed redeem, positive and negative
            // Roots (enemy/friendly) redeem
            // Reverse controlls redeem
            // Temp naked redeem
            // Spawn items redeem
            // Change weather/tod redeem
            // Drop all equipment/inventory redeem
            // Multiplayer execute redeem for all players
            // Add leader board of points spend, deaths caused, saves maybe? Other statistics?

            // TalkInteract always show Feo's history fact message, also does not properly show follow/rename creature
            // Log creatures dieing from safezone
            // Prevent wolfs/fenrings from howling all the time as helpers

            // ====================================
            // IDEAS:
            // ====================================
            // Allow redeemers to give specific item
            // Allow redeemers to choose the surprise for surprise chests
            // Timeout redeemers as a chance when redeeming surprise chests
            // Remove/add redeems when player leaves/enters a dungeon and check what kind of dungeon the player is in
            // Make certain creatures smaller in dungeons so they can be spawned
            // More loot if mob is grown?
            // LoyalBones: A red skeleton with normal damage but insane health pool
            // Chat loves this: Spawn chest with cheese (random food?) Make cheese wheel that gives random food?".
            // Add a way to be able to find spanwed creature. For the kill all spawned rule!
            // Drunk blur effect?
            // Disable all twitchy wards via bits (specific amount like 1000)
            // Add commands for claimed creatures to do things (flee, stop, attack player/base, activate specific attack, emote?)
            // Add options to super charge a spawned creature, more hp, damage, equip gear?

            // ====================================
            // IN PROGRESS:
            // ====================================
            // DONE: Add a mass follow/unfollow method
            // DONE: Sausage rain for Bearded
            // DONE: Fix starred mobs recolors
            // DONE: Add options to make portals/chests industructable
            // DONE: Safezones don't allow hoe actions
            // DONE: Allow messages to parse variables like RedeemerName
            // DONE: Bug: thou shall be smited used in dungeon
            // DONE: Remove redeems on logout/quit
            // DONE: Added CreatureData and ItemData to Surprise chests
            // DONE: Cancel redeem when player is teleporting
            // DONE: Add globalkey add/remove to creatures
            // DONE: Surprise chest map marker not dissapearing automaticly correctly
            // DONE: Don't refresh timer of buffs under 5 seconds to prevent endless buff bugg
            // DONE: Wake up mosnter programmaticly so they engange straight away
            // DONE: Follow status is resetted to follow when player moves to far away (portal)
            // DONE: Fixed timer for being logged out by Twitch warning
            // DONE: Fixed refreshing buffs and persist through death
            // DONE: Stop spawning things when streamer walks into a safezone
            // DONE: Add safe zones to traders
            // DONE: Make trap/log/smite damage health/armor base
            // DONE: Make trap's damage persistent
            // DONE: Add command or action to delete surprise chests / spawned creatures in the vicinity
            // DONE: Check for safezone when surprise chest actually spawns in
            // DONE: Prevent redeems when on a boat
            // DONE: Make friendly follow troops automaticly follow their owner
            // DONE: Friendly troops now attack bosses
            // DONE: Prevent AOE scripts from doing damage to boats (Thou shall be smited)
            // DONE: Prevent Impact scripts from doing damage to boats (Log rain)
            // DONE: Trap field redeem
            // DONE: Add recipe in config for twitch ward, also add option to turn of burning spawns to death
            // DONE: Add map markers where you spawned certain stuff (surprise chests)
            // DONE: Make surpise chests floatable in water, reduce mass to not sink ships XD, add map pin and auto remove it, make them persistent
            // DONE: Finish all types of statuseffects and make it decently configurable
            // DONE: Add configurable timers to status effect, persist through death, renew
            // DONE: Mod says it refunded stuff from a custom redeem, should not do that.
            // Wind in back (moder) and reverse redeem
            // Add halucinations, make player stunned/dazed when getting hit by Hallucinations? Or half damage?
            // Suprise chests, multiple chests to gamble, add a mimic to bite the opener (add legs like the luggage from terry pratchett's novel

            // Add references to friends, streamers, viewers in a seperate mod. Like I did with troll Betuti line
            // - DeathWizsh the Raven landing on streamers shoulder saying "There is a mod for that" every time someone says that in chat
            // - Feo the neckbeard (A neck with a beard) that randomly gives you history facts (spawned by redeem?)
            // - Sjoeky an actuall goat that gives you random combat/survial tips when you die to much in the most dry and sarcastic manner (spawn at your respawn location and redeem aswell?)
            // - Kimetsu a Dvergr mage, blessing?
            // - Soma_af a bear (reskinned as a white/pink teddybear with antlers as soma has a bear with antlers emote) Spawns when cooking? Drops random food when sneezing? Adds a 4th food slot?
            // - Make DurdyJay exactly like Odin, and spawns randomly in like Odin. But instead of dissappearing straight away he gets Googly Eyes and a stick out Tongue and says something nice/somewhat durdy. And his name changes to DurdyJay at that moment ofc (blessing?)
            // - Xxainty iets van een greydwarf ofzo
            // - Kassie The god of chaos, makes map dissapear when pissed off?
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

            // New mod ideas:
            // - Inventory in the Saddle
            // - Priortise spear slot
            // - lock inventory slots so putting all in inventory does not touch it
            // - square cultivator/hoe mod (no circle editing) with range scroll
            // - Mist control mod, add demister to shield gen and configurable range for wisp light/torches and maybe even remove all mist
            // - Beewax mod that allows you to put wax on wood to protect against water dammage. Mix it with other things to creat colors as well
        }
    }
}

