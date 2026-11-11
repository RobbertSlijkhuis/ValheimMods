using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Components;
using ModularMagic_Core.Types;
using ModularMagic_IceStaffs.Configs;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Locale;
using ModularMagic_IceStaffs.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CoreSecondaryAttackHelper = ModularMagic_Core.Helpers.SecondaryAttackHelper;
using ImbuedStaffHelper = ModularMagic_Core.Helpers.ImbuedStaffHelper;
using MMAoeHelper = ModularMagic_Core.Helpers.MMAoeHelper;
using SpawnedProjectileHelper = ModularMagic_Core.Helpers.SpawnedProjectileHelper;

namespace ModularMagic_IceStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_IceStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_IceStaffs";
        public const string PluginName = "ModularMagic_IceStaffs";
        public const string PluginVersion = "0.0.4";
        public static ModularMagic_IceStaffs Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle _assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();
        // What Core needs to know about the staffs of this mod, see _RegisterStaffHooks
        public StaffDefinition definition;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            LocaleEnglish.Init();
            _InitAssetBundle();
            _InitStatusEffects();
            PluginConfig.Init();
            // Every entry forwards its change to its own handler from this event in the order they were created,
            // so this runs after the handlers that update the prefabs, and the staff in the hand gets the new values with its runes
            Config.SettingChanged += (sender, args) => ImbuedStaffHelper.ReapplyOnHeldStaff();
            _RegisterStaffHooks();
            _RegisterSecondaryAttacks();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += _AddIceStaffs;
            ItemManager.OnItemsRegistered += _LogRecipes;
            // Not unsubscribed: the recipe is set again each time the items are registered, which is harmless
            ItemManager.OnItemsRegistered += _ApplyVanillaStaffRecipe;
        }

        // Core gives the staffs their rune slots and applies the runes when a staff is equipped. It only needs to know what belongs to this mod.
        // The Ice staffs are burst-timed, so the Attack Speed rune lowers their burst interval. The shards of the Rain rune get the damage of the
        // staff from this mod
        private void _RegisterStaffHooks()
        {
            definition = new StaffDefinition()
            {
                effectName = "Imbuements_MMIS",
                weaponType = WeaponType.MMIS,
                colorPreset = LightColorPresetType.IceBlue,
                allowedRunes = IceImbuementHelper.AllowedRunes,
                imbuementConfig = PluginConfig.imbuementConfig,
                attackSpeedAnimation = false,
                coneAttack = prefabs.MainAttackCone,
                coneConfig = PluginConfig.mainAttackCone,
                physicalDamageType = "Pierce",
            };
            definition.AddSecondaryAttack("Nova", prefabs.SecondaryAttackNova, PluginConfig.secondaryAttackNova);
            definition.AddSecondaryAttack("Rain", prefabs.SecondaryAttackRain, PluginConfig.secondaryAttackRain);

            StaffImbuementHelper.Register(definition);
            SpawnedProjectileHelper.Register(prefabs.ProjectileIceShard.name, IceImbuementHelper.OnProjectileSpawned);
        }

        private void _ApplyVanillaStaffRecipe()
        {
            VanillaStaffHelper.SetRecipeEnabled(PluginConfig.enableVanillaStaff.Value);
        }

        private void _LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMIS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= _LogRecipes;
        }

        private void _AddIceStaffs()
        {
            try
            {
                // The slots follow the Earth staffs: more slots and a higher tier (higher rune levels) per staff. The config is bound in Awake, so it is ready here
                definition.AddStaff(prefabs.StaffIce1Prefab, PluginConfig.staffIce1, 2, 1);
                definition.AddStaff(prefabs.StaffIce2Prefab, PluginConfig.staffIce2, 3, 2);
                definition.AddStaff(prefabs.StaffIce3Prefab, PluginConfig.staffIce3, 4, 4);
                // Skull of Frost disabled for now
                // StaffHelper.CreateStaff(prefabs.StaffIceAOEPrefab, PluginConfig.staffIce4);

                // After the staffs are registered, so a failing attack update can not take the staffs with it
                AttackHelper.UpdateNova(prefabs.AttackNova, PluginConfig.secondaryAttackNova);
                AttackHelper.UpdateRain(prefabs.ProjectileIceShard, prefabs.ScriptIceShards, PluginConfig.secondaryAttackRain);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in adding the ice staffs: " + e);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= _AddIceStaffs;
        }

        // Core gives a registered secondary attack its own eitr cost and the cooldown, the attack is recognised by the projectile it spawns.
        // The config is bound before this is called
        private void _RegisterSecondaryAttacks()
        {
            CoreSecondaryAttackHelper.Register(prefabs.AttackNova.name, PluginConfig.secondaryAttackNova.cooldownStatusEffectName, "Frost nova cooldown",
                () => PluginConfig.secondaryAttackNova.cooldown.Value);
            CoreSecondaryAttackHelper.Register(prefabs.ProjectileRainSpawn.name, PluginConfig.secondaryAttackRain.cooldownStatusEffectName, "Ice shards cooldown",
                () => PluginConfig.secondaryAttackRain.cooldown.Value);
        }

        private void _InitStatusEffects()
        {
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.WalkFastSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void _InitAssetBundle()
        {
            _assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_icestaffs_dw");

            // Ice assets
            prefabs.StaffIce1Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce1");
            prefabs.StaffIce2Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce2");
            prefabs.StaffIce3Prefab = _assetBundle.LoadAsset<GameObject>("MMIS_staffIce3");
            prefabs.StaffIceAOEPrefab = _assetBundle.LoadAsset<GameObject>("MMIS_StaffIceAOE");
            effects.WalkFastSE = _assetBundle.LoadAsset<StatusEffect>("FasterWalk_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("staff_ice_projectile_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_spores_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_spikes_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_iceshard_launch_smoke_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("IceSheet_MMIS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_IceSheetSpawn_MMIS"), true));
            // The nova AOE of the Skull of Frost, it takes its damage from the staff
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("attack_nova_SKULL_MMIS"), true));
            // The nova rune: the hit is a local object of the shooter (see MMAoe), so it is not registered. The wrapper holds the
            // secondary attack that is copied to the staff and is not registered
            prefabs.AttackNova = _assetBundle.LoadAsset<GameObject>("attack_nova_MMIS");
            MMAoeHelper.PrepareSphere(prefabs.AttackNova, hitProps: false);
            prefabs.SecondaryAttackNova = _assetBundle.LoadAsset<GameObject>("secondary_nova_MMIS");

            // The cone rune: the hit of a burst is a local object of the shooter (see MMAoe), so it is not registered. The visuals are a separate
            // effect (the burst effect of the wrapper) and it is registered. The wrapper holds the main attack that replaces the main attack
            // of the staff and is not registered
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_cone_MMIS"), true));
            prefabs.AttackCone = _assetBundle.LoadAsset<GameObject>("attack_cone_MMIS");
            MMAoeHelper.PrepareBox(prefabs.AttackCone, hitProps: true);
            prefabs.MainAttackCone = _assetBundle.LoadAsset<GameObject>("main_cone_attack_MMIS");

            // The rain rune (ice shards): the spawn projectile, its script and the shard are spawned in the world, so they are registered.
            // The wrapper holds the secondary attack that is copied to the staff and is not registered
            prefabs.ProjectileRainSpawn = _assetBundle.LoadAsset<GameObject>("projectile_spawn_rain_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileRainSpawn, true));
            prefabs.ScriptIceShards = _assetBundle.LoadAsset<GameObject>("staff_ice_script_iceshards_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ScriptIceShards, true));
            prefabs.ProjectileIceShard = _assetBundle.LoadAsset<GameObject>("staff_ice_projectile_secondary_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileIceShard, true));
            prefabs.ProjectileIceShardBlocker = _assetBundle.LoadAsset<GameObject>("staff_ice_blocker_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileIceShardBlocker, true));
            prefabs.SecondaryAttackRain = _assetBundle.LoadAsset<GameObject>("secondary_rain_MMIS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_nova_MMIS"), true));
            // The start effect of the rain wrapper. Only registered prefabs get their mocked references (like the mock mixer group) resolved
            PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>("fx_staff_ice_windup_MMIS"), true));
        }
    }
}

