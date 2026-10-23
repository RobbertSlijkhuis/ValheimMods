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
using WizshBoneTwitchIntegration.Gui;
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
        public const string PluginVersion = "1.0.3";
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
        public static readonly string HumanoidGroupNoDamageStructure = "WBTI_HumanoidGroupNoDamageStructure";
        public static readonly string HumanoidGroupSpawnEnemy = "WBTI_HumanoidGroupSpawnEnemy";
        public static readonly string HumanoidGroupSpawnFriendly = "WBTI_HumanoidGroupSpawnFriendly";

        public static readonly string customConfigPath = "BepInEx/config/WizshBoneTwitchIntegration";
        // Computed lazily (rather than eagerly like customConfigPath above) so they pick up
        // ConfigPathHelper's VirtualStore fallback, which can only be decided once the game has
        // actually started - see ConfigPathHelper.GetEffectiveRoot().
        public static string bannedPath => ConfigPathHelper.GetEffectiveRoot() + "/banned.txt";
        public static string viewersPath => ConfigPathHelper.GetEffectiveRoot() + "/viewers.yaml";
        public static string newsSeenPath => ConfigPathHelper.GetEffectiveRoot() + "/newsSeen.yaml";

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
            // ZNetScene.instance.GetPrefabNames() (used by RedeemPrefabCatalog's scans) isn't
            // populated yet at OnVanillaPrefabsAvailable - OnPrefabsRegistered is the hook
            // AddPersistentComponents above already relies on for the same reason.
            PrefabManager.OnPrefabsRegistered += RedeemPrefabCatalog.BuildPrefabCatalogs;
            ItemManager.OnItemsRegistered += LogStatusEffects;
        }

        public void Update()
        {
            HandleNewUIWindowInput();
        }

        private void HandleNewUIWindowInput()
        {
            try
            {
                if (ZInput.instance == null || wizshBoneWindowButton == null || !Player.m_localPlayer)
                    return;

                WizshBoneGUI newUI = Game.instance.gameObject.GetComponent<WizshBoneGUI>();
                if (newUI == null)
                    return;

                // Rebinding the key on the Home tab: the key being pressed must not also toggle the window.
                if (WindowKeyHelper.SuppressToggle)
                    return;

                bool guiVisible = newUI.IsVisible;

                if (IsVanillaHideUiCombo())
                {
                    // Vanilla's hide-UI toggle never touches the shell (it lives on Jotunn's canvas and
                    // blocks game input), so close it on the combo instead of letting it stay up. A closed
                    // shell is left alone, so the combo never opens it.
                    if (guiVisible && Input.GetKeyDown(PluginConfig.configWizshBoneWindow.Value.MainKey))
                        newUI.CloseShell();

                    return;
                }

                // ZInput is blocked while the GUI is open, so fall back to raw Unity input
                // via KeyboardShortcut.IsDown() which bypasses the ZInput block entirely.
                bool togglePressed = guiVisible
                    ? PluginConfig.configWizshBoneWindow.Value.IsDown()
                    : ZInput.GetButtonDown(wizshBoneWindowButton.Name);

                if (!togglePressed)
                    return;

                if (guiVisible)
                    newUI.CloseShell();
                else
                    newUI.ShowShell();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not show new WizshBone UI: {e}");
            }
        }

        /// <summary>
        /// True while Ctrl is held and the window key is F3 (without Ctrl being part of the configured
        /// shortcut itself). Vanilla's Ctrl+F3 "hide UI" shares our F3 key, and neither
        /// KeyboardShortcut.IsDown() nor Jotunn's ZInput button check rejects extra modifiers, so
        /// without this the hide-UI combo would also toggle the shell. The caller handles the combo
        /// itself (closing an open shell) instead of falling through to the normal toggle.
        /// </summary>
        private static bool IsVanillaHideUiCombo()
        {
            var shortcut = PluginConfig.configWizshBoneWindow.Value;
            if (shortcut.MainKey != KeyCode.F3)
                return false;

            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (modifier == KeyCode.LeftControl || modifier == KeyCode.RightControl)
                    return false;
            }

            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private void LogStatusEffects()
        {
            //foreach (var effect in ObjectDB.m_instance.m_StatusEffects)
            //{
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
            prefabs.EnvZone.AddComponent<TwitchWeatherZone>();

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
            pieceConfig.Enabled = ProfileSettingsHelper.Current.wardEnabled;
            pieceConfig.Name = "Twitch Ward";
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

            // Only a marker: SlipperyGroundHelper / SlipperyGroundPatchesWBTI do the actual sliding
            // while the local player has it.
            effects.SlipperyGround = ScriptableObject.CreateInstance<SE_Stats>();
            effects.SlipperyGround.name      = Helpers.SlipperyGroundHelper.EffectName;
            effects.SlipperyGround.m_name    = "Slippery ground";
            effects.SlipperyGround.m_tooltip = "The ground is as slippery as ice.";
            effects.SlipperyGround.m_ttl     = 30f;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.SlipperyGround, fixReference: false));

            // Vanilla status effects only exist once ObjectDB is populated, so borrow the Frost icon then.
            ItemManager.OnItemsRegistered += () => effects.SlipperyGround.m_icon = ObjectDB.instance.GetStatusEffect("Frost".GetStableHashCode())?.m_icon;
        }

        private void InitInputs()
        {
            try
            {
                wizshBoneWindowButton = new ButtonConfig
                {
                    Name = "WizshBone Window",
                    ShortcutConfig = PluginConfig.configWizshBoneWindow,
                    // Without this Jötunn gates the button on Player.TakeInput(), which is false while
                    // the Esc menu (and so the single-player pause) or another vanilla GUI is open.
                    ActiveInGUI = true,
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
            CommandManager.Instance.AddConsoleCommand(new RemoveChestsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveCreaturesCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveMistCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTrapsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveWeatherCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveTerrainEditCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveDoorsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveWindmillsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveLogsCommand());
            CommandManager.Instance.AddConsoleCommand(new RemoveBoatsCommand());

            CommandManager.Instance.AddConsoleCommand(new LeaderboardSeedCommand());
            CommandManager.Instance.AddConsoleCommand(new LeaderboardClearCommand());
            CommandManager.Instance.AddConsoleCommand(new FakeRedeemerCommand());

            CommandManager.Instance.AddConsoleCommand(new UsersBanList());
            CommandManager.Instance.AddConsoleCommand(new UserBanCommand());
            CommandManager.Instance.AddConsoleCommand(new UserUnbanCommand());

            CommandManager.Instance.AddConsoleCommand(new ClearBossKeysCommand());
            CommandManager.Instance.AddConsoleCommand(new OpenConfigFolderCommand());
            CommandManager.Instance.AddConsoleCommand(new ReloadRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new ReloadViewersCommand());
            CommandManager.Instance.AddConsoleCommand(new SetAliasCommand());
            CommandManager.Instance.AddConsoleCommand(new TestCommand());
            CommandManager.Instance.AddConsoleCommand(new TestWeatherCommand());
            CommandManager.Instance.AddConsoleCommand(new UseRedeemCommand());
            CommandManager.Instance.AddConsoleCommand(new ScanCommand());
            CommandManager.Instance.AddConsoleCommand(new ClaimCommand());
            CommandManager.Instance.AddConsoleCommand(new UnclaimCommand());
            CommandManager.Instance.AddConsoleCommand(new TestProfilesCommand());
            CommandManager.Instance.AddConsoleCommand(new TestRedeemsCommand());
            CommandManager.Instance.AddConsoleCommand(new TestSettingsCommand());
        }

        // A prefab missing from the bundle (e.g. the bundle wasn't rebuilt alongside the plugin) would
        // make `new CustomPrefab(null, true)` throw and abort the rest of the load, so it is skipped
        // with a warning instead; the code that uses that prefab has to cope with it being null.
        private static void RegisterBundlePrefab(GameObject prefab, string name)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Prefab '{name}' not found in the asset bundle, not registering it");
                return;
            }

            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("wbti_dw");

            //prefabs.DetectFish = assetBundle.LoadAsset<GameObject>("Detect_Fish_WBTI");
            prefabs.FishRainScript = assetBundle.LoadAsset<GameObject>("spawn_fish_WBTI");
            RegisterBundlePrefab(prefabs.FishRainScript, "spawn_fish_WBTI");
            prefabs.Flashbang = assetBundle.LoadAsset<GameObject>("Flashbang_WBTI");
            RegisterBundlePrefab(prefabs.Flashbang, "Flashbang_WBTI");
            prefabs.GuardStone = assetBundle.LoadAsset<GameObject>("WBTI_guard_stone");
            prefabs.TerrainEdit = assetBundle.LoadAsset<GameObject>("TerrainEdit_WBTI");
            RegisterBundlePrefab(prefabs.TerrainEdit, "TerrainEdit_WBTI");
            //prefabs.NeckBeard = assetBundle.LoadAsset<GameObject>("FeoTheHistorian_WBTI");
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.NeckBeard, true));
            prefabs.ChestIron = assetBundle.LoadAsset<GameObject>("ChestIron_WBTI");
            RegisterBundlePrefab(prefabs.ChestIron, "ChestIron_WBTI");
            prefabs.ChestGold = assetBundle.LoadAsset<GameObject>("ChestGold_WBTI");
            RegisterBundlePrefab(prefabs.ChestGold, "ChestGold_WBTI");
            prefabs.EnvZone = assetBundle.LoadAsset<GameObject>("EnvZone_WBTI");
            RegisterBundlePrefab(prefabs.EnvZone, "EnvZone_WBTI");
            prefabs.TimeStopZone = assetBundle.LoadAsset<GameObject>("TimeStopZone_WBTI");
            RegisterBundlePrefab(prefabs.TimeStopZone, "TimeStopZone_WBTI");
            prefabs.DomeHit = assetBundle.LoadAsset<GameObject>("WBTI_dome_hit");
            RegisterBundlePrefab(prefabs.DomeHit, "WBTI_dome_hit");

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
            materials.RecolorBonemass = assetBundle.LoadAsset<Material>("Bonemass_recolor_WBTI");
            materials.RecolorBonemassBones = assetBundle.LoadAsset<Material>("Bonemass_bones_recolor_WBTI");
            materials.RecolorCultist = assetBundle.LoadAsset<Material>("Cultist_recolor_WBTI");
            materials.RecolorCultistCape = assetBundle.LoadAsset<Material>("CultistCape_recolor_WBTI");
            materials.RecolorDeathsquito = assetBundle.LoadAsset<Material>("Deathsquito_recolor_WBTI");
            materials.RecolorDeer = assetBundle.LoadAsset<Material>("Deer_recolor_WBTI");
            materials.RecolorDeerAntlers = assetBundle.LoadAsset<Material>("Deer_antlers_recolor_WBTI");
            materials.RecolorDraugr = assetBundle.LoadAsset<Material>("Draugr_recolor_WBTI");
            materials.RecolorDraugrFem = assetBundle.LoadAsset<Material>("Draugr_Ranged_recolor_WBTI");
            materials.RecolorDraugrElite = assetBundle.LoadAsset<Material>("Draugr_Elite_recolor_WBTI");
            materials.RecolorEikthyr = assetBundle.LoadAsset<Material>("Eikthyr_recolor_WBTI");
            materials.RecolorEikthyrHair = assetBundle.LoadAsset<Material>("Eikthyr_hair_recolor_WBTI");
            materials.RecolorFenring = assetBundle.LoadAsset<Material>("Fenring_recolor_WBTI");
            materials.RecolorFuling = assetBundle.LoadAsset<Material>("Fuling_recolor_WBTI");
            materials.RecolorFulingArmor = assetBundle.LoadAsset<Material>("Fuling_armor_recolor_WBTI");
            materials.RecolorFulingBrute = assetBundle.LoadAsset<Material>("FulingBrute_recolor_WBTI");
            materials.RecolorFulingShaman = assetBundle.LoadAsset<Material>("FulingShaman_recolor_WBTI");
            materials.RecolorFulingShamanCape = assetBundle.LoadAsset<Material>("FulingShaman_cape_recolor_WBTI");
            materials.RecolorGDKing = assetBundle.LoadAsset<Material>("Elder_recolor_WBTI");
            materials.RecolorGDKingEye = assetBundle.LoadAsset<Material>("Elder_eye_recolor_WBTI");
            materials.RecolorGhost = assetBundle.LoadAsset<Material>("Ghost_recolor_WBTI");
            materials.RecolorGjall = assetBundle.LoadAsset<Material>("Gjall_recolor_WBTI");
            materials.RecolorGreydwarf = assetBundle.LoadAsset<Material>("Greydwarf_recolor_WBTI");
            materials.RecolorGreydwarfShaman = assetBundle.LoadAsset<Material>("Greydwarf_Shaman_recolor_WBTI");
            materials.RecolorGreydwarfRootsword = assetBundle.LoadAsset<Material>("RootSword_recolor_WBTI");
            materials.RecolorHatchling = assetBundle.LoadAsset<Material>("Hatchling_recolor_WBTI");
            materials.RecolorLeech = assetBundle.LoadAsset<Material>("Leech_recolor_WBTI");
            materials.RecolorModer = assetBundle.LoadAsset<Material>("Moder_recolor_WBTI");
            materials.RecolorLox = assetBundle.LoadAsset<Material>("Lox_recolor_WBTI");
            materials.RecolorNeck = assetBundle.LoadAsset<Material>("Neck_recolor_WBTI");
            materials.RecolorQueen = assetBundle.LoadAsset<Material>("Queen_recolor_WBTI");
            materials.RecolorSeeker = assetBundle.LoadAsset<Material>("Seeker_recolor_WBTI");
            materials.RecolorSeekerBrute = assetBundle.LoadAsset<Material>("SeekerBrute_recolor_WBTI");
            materials.RecolorSeekerBrood = assetBundle.LoadAsset<Material>("SeekerBrood_recolor_WBTI");
            materials.RecolorSerpent = assetBundle.LoadAsset<Material>("Serpent_recolor_WBTI");
            materials.RecolorSkeleton = assetBundle.LoadAsset<Material>("Skeleton_recolor_WBTI");
            materials.RecolorStoneGolem = assetBundle.LoadAsset<Material>("StoneGolem_recolor_WBTI");
            materials.RecolorStoneGolemClubs = assetBundle.LoadAsset<Material>("StoneGolemClubs_recolor_WBTI");
            materials.RecolorSurtling = assetBundle.LoadAsset<Material>("Surtling_recolor_WBTI");
            materials.RecolorTick = assetBundle.LoadAsset<Material>("Tick_recolor_WBTI");
            materials.RecolorTroll = assetBundle.LoadAsset<Material>("Troll_recolor_WBTI");
            materials.RecolorUlv = assetBundle.LoadAsset<Material>("Ulv_recolor_WBTI");
            materials.RecolorVile = assetBundle.LoadAsset<Material>("Vile_recolor_WBTI");
            materials.RecolorWolf = assetBundle.LoadAsset<Material>("Wolf_recolor_WBTI");
            materials.RecolorWraith = assetBundle.LoadAsset<Material>("Wraith_recolor_WBTI");
            materials.RecolorWraithZebra = assetBundle.LoadAsset<Material>("Wraith_recolor_zebra_WBTI");
            materials.RecolorWrithan = assetBundle.LoadAsset<Material>("Writhan_recolor_WBTI");
            materials.RecolorYagluth = assetBundle.LoadAsset<Material>("Yagluth_recolor_WBTI");
            materials.RecolorYagluthHair = assetBundle.LoadAsset<Material>("Yagluth_hair_recolor_WBTI");
        }
    }
}

