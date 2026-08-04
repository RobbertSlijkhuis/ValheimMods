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
        private ButtonConfig quickTestRedeemButton;
        public static bool useRedeemCommand = false;
        public static readonly string HumanoidGroupNoDamageStructure = "WBTI_HumanoidGroupNoDamageStructure";
        public static readonly string HumanoidGroupSpawnEnemy = "WBTI_HumanoidGroupSpawnEnemy";
        public static readonly string HumanoidGroupSpawnFriendly = "WBTI_HumanoidGroupSpawnFriendly";

        public static readonly string customConfigPath = "BepInEx/config/WizshBoneTwitchIntegration";
        public static readonly string redeemsConfigPath = customConfigPath + "/redeems.yaml";
        public static readonly string redeemsSchemaPath = customConfigPath + "/redeems-schema.json";
        public static readonly string bannedPath = customConfigPath + "/banned.txt";
        public static readonly string viewersPath = customConfigPath + "/viewers.yaml";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        public void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            ProfileSyncHelper.Init();
            InitStatusEffects();
            InitInputs();
            InitCommands();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += InitExtraConfigFiles;
            PrefabManager.OnVanillaPrefabsAvailable += SetupPieces;
            PrefabManager.OnVanillaPrefabsAvailable += AddEffectLists;
            PrefabManager.OnPrefabsRegistered += AddPersistentComponents;
            ItemManager.OnItemsRegistered += LogStatusEffects;
        }

        public void Update()
        {
            HandleWizshBoneWindowInput();
            HandleQuickTestRedeemInput();
        }

        private void HandleWizshBoneWindowInput()
        {
            try
            {
                if (ZInput.instance == null || wizshBoneWindowButton == null || !Player.m_localPlayer)
                    return;

                TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();
                bool guiVisible = authComp.wizshBoneGUI.IsAnyGUIVisible;

                // ZInput is blocked while the GUI is open, so fall back to raw Unity input
                // via KeyboardShortcut.IsDown() which bypasses the ZInput block entirely.
                bool togglePressed = guiVisible
                    ? PluginConfig.configWizshBoneWindow.Value.IsDown()
                    : ZInput.GetButtonDown(wizshBoneWindowButton.Name);

                if (!togglePressed)
                    return;

                if (guiVisible)
                    authComp.wizshBoneGUI.CloseGUI();
                else
                    authComp.wizshBoneGUI.ShowGUI();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not show WizshBone settings GUI: " + e);
            }
        }

        private void HandleQuickTestRedeemInput()
        {
            try
            {
                if (ZInput.instance == null || quickTestRedeemButton == null || !Player.m_localPlayer)
                    return;

                if (!ZInput.GetButtonDown(quickTestRedeemButton.Name))
                    return;

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                if (string.IsNullOrEmpty(customRewards.m_quickTestRedeem))
                {
                    Jotunn.Logger.LogWarning("[WBTI] No quick test redeem set. Use WBTISetQuickRedeem <title> first.");
                    return;
                }

                UseRedeemCommand.FireRedeem(customRewards.m_quickTestRedeem);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not fire quick test redeem: " + e);
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

        private void SetupPieces()
        {
            prefabs.ChestIron.AddComponent<TwitchSurpriseChest>();
            prefabs.ChestIron.transform.Find("chest_top").gameObject.AddComponent<TwitchSurpriseChestInteract>();
            prefabs.ChestGold.AddComponent<TwitchSurpriseChest>();
            prefabs.ChestGold.transform.Find("chest_top").gameObject.AddComponent<TwitchSurpriseChestInteract>();

            prefabs.GuardStone.transform.Find("safezone").gameObject.AddComponent<TwitchSafeZone>();
            prefabs.GuardStone.transform.Find("controls").gameObject.AddComponent<TwitchSafeZoneControls>();

            prefabs.EnvZone.AddComponent<TwitchPersistentDestruction>();

            prefabs.TimeStopZone.AddComponent<TwitchPersistentDestruction>();
            prefabs.TimeStopZone.AddComponent<TwitchTimeStopZone>();

            prefabs.TerrainEdit.AddComponent<TwitchTerrainReset>();

            prefabs.Windmill = PrefabManager.Instance.CreateClonedPrefab("Windmill_WBTI", "windmill");
            Windmill vanillaWindmill = prefabs.Windmill.GetComponent<Windmill>();
            Smelter vanillaSmelter = prefabs.Windmill.GetComponent<Smelter>();
            prefabs.Windmill.AddComponent<TwitchWindmillPersistentData>().ConfigurePrefab(vanillaWindmill);
            GameObject.DestroyImmediate(vanillaWindmill);
            if (vanillaSmelter != null)
                GameObject.DestroyImmediate(vanillaSmelter);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.Windmill, true));

            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Enabled = true;
            pieceConfig.Name = "Twitchy Ward";
            pieceConfig.Description = "A ward against mysterious and twitchy forces!";
            pieceConfig.PieceTable = PieceTables.Hammer;
            pieceConfig.Category = PieceCategories.Misc;
            pieceConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(ProfileSettingsHelper.Current.wardRecipe, null, null);
            PieceManager.Instance.AddPiece(new CustomPiece(prefabs.GuardStone, true, pieceConfig));

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

            PrefabManager.OnVanillaPrefabsAvailable -= SetupPieces;
        }

        private void AddPersistentComponents()
        {
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();

                prefab.AddComponent<TwitchBasePersistentData>();

                if (humanoid != null && monsterAI != null)
                {
                    Transform visualTrans = prefab.transform.Find("Visual");

                    if (visualTrans == null)
                        continue;

                    LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                    if (levelEffects == null)
                        continue;

                    LevelEffects.LevelSetup levelSetup = levelEffects.m_levelSetups[1];

                    for (int index = 0; index < 8; index++)
                    {
                        levelEffects.m_levelSetups.Add(levelSetup);
                    }

                    continue;
                }
            }

            string[] traders = new string[3] { "Vendor_BlackForest", "Hildir_camp", "BogWitch_Camp" };

            foreach (string name in traders)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
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

                Demister demister = forceFieldTransform.gameObject.AddComponent<Demister>();
                ParticleSystemForceField forceField = forceFieldTransform.gameObject.AddComponent<ParticleSystemForceField>();
                forceField.gravity = -0.08f;
                forceField.endRange = 15f;
                forceField.multiplyDragByParticleSize = false;
                forceField.multiplyDragByParticleVelocity = false;
                forceField.rotationAttraction = 1f;
                forceField.vectorFieldAttraction = 1f;
                forceField.vectorFieldSpeed = 1f;

                TwitchSafeZone safezone = forceFieldTransform.gameObject.AddComponent<TwitchSafeZone>();
            }

            PrefabManager.OnPrefabsRegistered -= AddPersistentComponents;
        }

        private void AddEffectLists()
        {
            // Spawn cloud effects
            EffectList.EffectData SpawnEffectSmallData = new EffectList.EffectData();
            SpawnEffectSmallData.m_enabled = true;
            SpawnEffectSmallData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_small");
            SpawnEffectSmallData.m_variant = -1;

            List<EffectList.EffectData> SpawnEffectSmallDataList = new List<EffectList.EffectData>();
            SpawnEffectSmallDataList.Add(SpawnEffectSmallData);

            effectLists.SpawnEffectSmall.m_effectPrefabs = SpawnEffectSmallDataList.ToArray();

            EffectList.EffectData SpawnEffectMediumData = new EffectList.EffectData();
            SpawnEffectMediumData.m_enabled = true;
            SpawnEffectMediumData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_medium");
            SpawnEffectMediumData.m_variant = -1;

            List<EffectList.EffectData> SpawnEffectMediumDataList = new List<EffectList.EffectData>();
            SpawnEffectMediumDataList.Add(SpawnEffectMediumData);

            effectLists.SpawnEffectMedium.m_effectPrefabs = SpawnEffectMediumDataList.ToArray();

            EffectList.EffectData SpawnEffectLargeData = new EffectList.EffectData();
            SpawnEffectLargeData.m_enabled = true;
            SpawnEffectLargeData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_large");
            SpawnEffectLargeData.m_variant = -1;

            List<EffectList.EffectData> SpawnEffectLargeDataList = new List<EffectList.EffectData>();
            SpawnEffectLargeDataList.Add(SpawnEffectLargeData);

            effectLists.SpawnEffectLarge.m_effectPrefabs = SpawnEffectLargeDataList.ToArray();

            // Spawn item smoke and sound effect
            EffectList.EffectData itemSpawnEffectData = new EffectList.EffectData();
            itemSpawnEffectData.m_enabled = true;
            itemSpawnEffectData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_Place_wood_pole");
            itemSpawnEffectData.m_variant = -1;

            EffectList.EffectData itemSpawnSoundEffectData = new EffectList.EffectData();
            itemSpawnSoundEffectData.m_enabled = true;
            itemSpawnSoundEffectData.m_prefab = PrefabManager.Instance.GetPrefab("sfx_cooking_station_take");
            itemSpawnSoundEffectData.m_variant = -1;

            List<EffectList.EffectData> itemSpawnList = new List<EffectList.EffectData>();
            itemSpawnList.Add(itemSpawnEffectData);
            itemSpawnList.Add(itemSpawnSoundEffectData);

            effectLists.SpawnItemEffect.m_effectPrefabs = itemSpawnList.ToArray();

            // Chest open glow effect
            EffectList.EffectData openingGlowEffectData = new EffectList.EffectData();
            openingGlowEffectData.m_enabled = true;
            openingGlowEffectData.m_prefab = PrefabManager.Instance.GetPrefab("fx_HildirChest_Unlock");
            openingGlowEffectData.m_variant = 0;

            List<EffectList.EffectData> openingGlowList = new List<EffectList.EffectData>();
            openingGlowList.Add(openingGlowEffectData);

            

            effectLists.ChestOpenEffect.m_effectPrefabs = openingGlowList.ToArray();

            PrefabManager.OnVanillaPrefabsAvailable -= AddEffectLists;
        }

        private void InitExtraConfigFiles()
        {
            ExtraConfigHelper.InitExtraConfigs();
            ExtraConfigHelper.ReadRedeemsConfig();
            ProfileSettingsHelper.Reload();
        }

        private void InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Burning, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Freezing, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Poison, fixReference: true));
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.NoFallDamage, fixReference: true));

            effects.PlayerScale = ScriptableObject.CreateInstance<Components.SE_PlayerScale>();
            effects.PlayerScale.name     = "PlayerScale";
            effects.PlayerScale.m_name   = "PlayerScale";
            effects.PlayerScale.m_icon   = sprites.MiniMeSprite;
            effects.PlayerScale.m_ttl    = 180f;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.PlayerScale, fixReference: false));
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

                quickTestRedeemButton = new ButtonConfig
                {
                    Name = "WBTI Quick Test Redeem",
                    ShortcutConfig = PluginConfig.configQuickTestRedeemKey,
                };

                InputManager.Instance.AddButton(PluginGUID, quickTestRedeemButton);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + e);
            }
        }

        private void InitCommands()
        {
            CommandManager.Instance.AddConsoleCommand(new RemoveChestsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveCreaturesCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveMistCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTrapsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveDoorsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveWindmillsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveLogsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveBoatsCommand());

            CommandManager.Instance.AddConsoleCommand(new UsersBanList());
            CommandManager.Instance.AddConsoleCommand(new UserBanCommand());
            CommandManager.Instance.AddConsoleCommand(new UserUnbanCommand());

            CommandManager.Instance.AddConsoleCommand(new ClearBossKeysCommand());
            CommandManager.Instance.AddConsoleCommand(new OpenConfigFolderCommand());
            CommandManager.Instance.AddConsoleCommand(new ReloadRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new ReloadViewersCommand());
            CommandManager.Instance.AddConsoleCommand(new SetAliasCommand());
            CommandManager.Instance.AddConsoleCommand(new SetQuickRedeemCommand());
            CommandManager.Instance.AddConsoleCommand(new TestCommand());
            CommandManager.Instance.AddConsoleCommand(new UseRedeemCommand());
            CommandManager.Instance.AddConsoleCommand(new ScanCommand());
            CommandManager.Instance.AddConsoleCommand(new ClaimCommand());
            CommandManager.Instance.AddConsoleCommand(new UnclaimCommand());
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("wbti_dw");

            prefabs.DetectFish = assetBundle.LoadAsset<GameObject>("Detect_Fish_WBTI");
            prefabs.FishRainScript = assetBundle.LoadAsset<GameObject>("spawn_fish_WBTI");
            prefabs.Flashbang = assetBundle.LoadAsset<GameObject>("Flashbang_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.FishRainScript, true));
            prefabs.GuardStone = assetBundle.LoadAsset<GameObject>("WBTI_guard_stone");
            prefabs.TerrainEdit = assetBundle.LoadAsset<GameObject>("TerrainEdit_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.TerrainEdit, true));
            prefabs.NeckBeard = assetBundle.LoadAsset<GameObject>("FeoTheHistorian_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.NeckBeard, true));
            prefabs.ChestIron = assetBundle.LoadAsset<GameObject>("ChestIron_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ChestIron, true));
            prefabs.ChestGold = assetBundle.LoadAsset<GameObject>("ChestGold_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ChestGold, true));
            prefabs.EnvZone = assetBundle.LoadAsset<GameObject>("EnvZone_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.EnvZone, true));
            prefabs.TimeStopZone = assetBundle.LoadAsset<GameObject>("TimeStopZone_WBTI");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.TimeStopZone, true));

            effects.Burning = assetBundle.LoadAsset<SE_Stats>("Burning_WBTI");
            effects.Freezing = assetBundle.LoadAsset<StatusEffect>("Freezing_WBTI");
            effects.Poison = assetBundle.LoadAsset<StatusEffect>("Poison_WBTI");
            effects.NoFallDamage = assetBundle.LoadAsset<StatusEffect>("NoFallDamage_WBTI");

            sprites.MiniMeSprite = assetBundle.LoadAsset<Sprite>("MiniMeSprite_WBTI");
            sprites.BigMeSprite = assetBundle.LoadAsset<Sprite>("BigMeSprite_WBTI");

            materials.RecolorAbomination = assetBundle.LoadAsset<Material>("Abomination_recolor_WBTI");
            materials.RecolorBat = assetBundle.LoadAsset<Material>("Bat_recolor_WBTI");
            materials.RecolorBjorn = assetBundle.LoadAsset<Material>("Bjorn_recolor_WBTI");
            materials.RecolorBjornZebra = assetBundle.LoadAsset<Material>("Bjorn_recolor_zebra_WBTI");
            materials.RecolorBlob = assetBundle.LoadAsset<Material>("Blob_recolor_WBTI");
            materials.RecolorBlobSlime = assetBundle.LoadAsset<Material>("BlobSlime_recolor_WBTI");
            materials.RecolorBoar = assetBundle.LoadAsset<Material>("Boar_recolor_WBTI");
            materials.RecolorBoarTusk = assetBundle.LoadAsset<Material>("BoarTusk_recolor_WBTI");
            materials.RecolorCultist = assetBundle.LoadAsset<Material>("Cultist_recolor_WBTI");
            materials.RecolorCultistCape = assetBundle.LoadAsset<Material>("CultistCape_recolor_WBTI");
            materials.RecolorDeathsquito = assetBundle.LoadAsset<Material>("Deathsquito_recolor_WBTI");
            materials.RecolorDeer = assetBundle.LoadAsset<Material>("Deer_recolor_WBTI");
            materials.RecolorDeerAntlers = assetBundle.LoadAsset<Material>("Deer_antlers_recolor_WBTI");
            materials.RecolorDraugr = assetBundle.LoadAsset<Material>("Draugr_recolor_WBTI");
            materials.RecolorDraugrFem = assetBundle.LoadAsset<Material>("Draugr_Ranged_recolor_WBTI");
            materials.RecolorDraugrElite = assetBundle.LoadAsset<Material>("Draugr_Elite_recolor_WBTI");
            materials.RecolorFenring = assetBundle.LoadAsset<Material>("Fenring_recolor_WBTI");
            materials.RecolorFuling = assetBundle.LoadAsset<Material>("Fuling_recolor_WBTI");
            materials.RecolorFulingArmor = assetBundle.LoadAsset<Material>("Fuling_armor_recolor_WBTI");
            materials.RecolorGhost = assetBundle.LoadAsset<Material>("Ghost_recolor_WBTI");
            materials.RecolorGreydwarf = assetBundle.LoadAsset<Material>("Greydwarf_recolor_WBTI");
            materials.RecolorGreydwarfShaman = assetBundle.LoadAsset<Material>("Greydwarf_Shaman_recolor_WBTI");
            materials.RecolorGreydwarfRootsword = assetBundle.LoadAsset<Material>("RootSword_recolor_WBTI");
            materials.RecolorHatchling = assetBundle.LoadAsset<Material>("Hatchling_recolor_WBTI");
            materials.RecolorLeech = assetBundle.LoadAsset<Material>("Leech_recolor_WBTI");
            materials.RecolorLox = assetBundle.LoadAsset<Material>("Lox_recolor_WBTI");
            materials.RecolorNeck = assetBundle.LoadAsset<Material>("Neck_recolor_WBTI");
            materials.RecolorSerpent = assetBundle.LoadAsset<Material>("Serpent_recolor_WBTI");
            materials.RecolorSkeleton = assetBundle.LoadAsset<Material>("Skeleton_recolor_WBTI");
            materials.RecolorStoneGolem = assetBundle.LoadAsset<Material>("StoneGolem_recolor_WBTI");
            materials.RecolorStoneGolemClubs = assetBundle.LoadAsset<Material>("StoneGolemClubs_recolor_WBTI");
            materials.RecolorSurtling = assetBundle.LoadAsset<Material>("Surtling_recolor_WBTI");
            materials.RecolorTroll = assetBundle.LoadAsset<Material>("Troll_recolor_WBTI");
            materials.RecolorUlv = assetBundle.LoadAsset<Material>("Ulv_recolor_WBTI");
            materials.RecolorWolf = assetBundle.LoadAsset<Material>("Wolf_recolor_WBTI");
            materials.RecolorWraith = assetBundle.LoadAsset<Material>("Wraith_recolor_WBTI");
            materials.RecolorWraithZebra = assetBundle.LoadAsset<Material>("Wraith_recolor_zebra_WBTI");

            // ====================================
            // TODO:
            // ====================================
            // Serpent, noodle detonate
            // Teleport allies to streamer emote
            // SPAWN MISILE (mistile) redeem!
            // Ppl specific monsters
            // - Xainty the Greydwarf
            // - Flow the Deathsquito
            // - Crys the Fueling
            // - NECK SQUAD (Lothren, Soma, Phenazo, 1mmun1tyy, DeathWizsh, Bonkerz)

            // ====================================
            // DONE
            // ====================================
            // DONE: Add claim to a tame by renaming it with claim:insertnamehere
            // DONE: Fix message showing for split second of creature claims
            // DONE: Allow permanent claiming with the chatting system
            // DONE: Add maximum limit of how many of the same spawned creatures are allowed
            // DONE: Add maximum limit of total spawned creaturs are allowed
            // DONE: Change weather/tod redeem
            // DONE: Save/load recolors from a file
            // DONE: Add new rains like meteors from Yagluth and Fader, Karve rain etc.
            // DONE: Add persistend data for pieces, like allow drops
            // DONE: Armor, shield, weapons, monsters don't get thrown out very far out of suprise chests
            // DONE: Fix noodles spawning in dungeons
            // DONE: Add option to block/push out enemies from Safezones
            // DONE: Greydwarfds friendlies do no attack enemy greyfwards
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
            // - Kassie (Kassandra) The god of chaos, makes map dissapear when pissed off?
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
            // - Troll pacify, give them something to become neutral and in that state you can backpack them
            // - square cultivator/hoe mod (no circle editing) with range scroll
            // - Mist control mod, add demister to shield gen and configurable range for wisp light/torches and maybe even remove all mist
            // - Beewax mod that allows you to put wax on wood to protect against water dammage. Mix it with other things to creat colors as well
        }
    }
}

