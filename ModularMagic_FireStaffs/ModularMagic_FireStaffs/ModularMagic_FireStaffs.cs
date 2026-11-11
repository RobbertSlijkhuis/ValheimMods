using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Components;
using ModularMagic_Core.Types;
using ModularMagic_FireStaffs.Configs;
using ModularMagic_FireStaffs.Helpers;
using ModularMagic_FireStaffs.Locale;
using ModularMagic_FireStaffs.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CoreSecondaryAttackHelper = ModularMagic_Core.Helpers.SecondaryAttackHelper;
using ImbuedStaffHelper = ModularMagic_Core.Helpers.ImbuedStaffHelper;
using MMAoeHelper = ModularMagic_Core.Helpers.MMAoeHelper;
using SpawnedProjectileHelper = ModularMagic_Core.Helpers.SpawnedProjectileHelper;

namespace ModularMagic_FireStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_FireStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_FireStaffs";
        public const string PluginName = "ModularMagic_FireStaffs";
        public const string PluginVersion = "0.0.4";
        public static ModularMagic_FireStaffs Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        // What Core needs to know about the staffs of this mod, see RegisterStaffHooks
        public StaffDefinition definition;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            LocaleEnglish.Init();
            InitAssetBundle();
            PluginConfig.Init();
            // Every entry forwards its change to its own handler from this event in the order they were created,
            // so this runs after the handlers that update the prefabs, and the staff in the hand gets the new values with its runes
            Config.SettingChanged += (sender, args) => ImbuedStaffHelper.ReapplyOnHeldStaff();
            RegisterStaffHooks();
            RegisterSecondaryAttacks();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddFireStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_FireStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
            // Not unsubscribed: the recipe is set again each time the items are registered, which is harmless
            ItemManager.OnItemsRegistered += ApplyVanillaStaffRecipe;
        }

        // Core gives the staffs their rune slots, applies the runes when a staff is equipped and speeds up the attack animation for the Attack Speed
        // rune. It only needs to know what belongs to this mod. The meteors of the Rain rune get the damage of the staff from this mod
        private void RegisterStaffHooks()
        {
            definition = new StaffDefinition()
            {
                effectName = "Imbuements_MMFS",
                weaponType = WeaponType.MMFS,
                colorPreset = LightColorPresetType.Orange,
                allowedRunes = FireImbuementHelper.AllowedRunes,
                imbuementConfig = PluginConfig.imbuementConfig,
                attackSpeedAnimation = true,
                coneAttack = prefabs.mainAttackCone,
                coneConfig = PluginConfig.mainAttackCone,
            };
            definition.AddSecondaryAttack("Nova", prefabs.secondaryAttackNova, PluginConfig.secondaryAttackNova);
            definition.AddSecondaryAttack("Rain", prefabs.secondaryAttackRain, PluginConfig.secondaryAttackRain);

            StaffImbuementHelper.Register(definition);
            SpawnedProjectileHelper.Register(prefabs.projectileMeteor.name, FireImbuementHelper.OnProjectileSpawned);
        }

        private void ApplyVanillaStaffRecipe()
        {
            VanillaStaffHelper.SetRecipeEnabled(PluginConfig.enableVanillaStaff.Value);
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMFS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddFireStaffs()
        {
            try
            {
                // The slots follow the Ice staffs: more slots and a higher tier (higher rune levels) per staff. The config is bound in Awake, so it is ready here
                definition.AddStaff(prefabs.staffFire1Prefab, PluginConfig.staffFire1, 2, 1);
                definition.AddStaff(prefabs.staffFire2Prefab, PluginConfig.staffFire2, 3, 2);
                definition.AddStaff(prefabs.staffFire3Prefab, PluginConfig.staffFire3, 4, 4);

                // After the staffs are registered, so a failing attack update can not take the staffs with it
                AttackHelper.UpdateNova(prefabs.attackNova, PluginConfig.secondaryAttackNova);
                AttackHelper.UpdateRain(prefabs.secondaryAttackRain, prefabs.projectileMeteor, prefabs.scriptMeteors, PluginConfig.secondaryAttackRain);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in adding the fire staffs: " + e);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddFireStaffs;
        }

        // Core gives a registered secondary attack its own eitr cost and the cooldown, the attack is recognised by the projectile it spawns.
        // The config is bound before this is called
        private void RegisterSecondaryAttacks()
        {
            CoreSecondaryAttackHelper.Register(prefabs.attackNova.name, PluginConfig.secondaryAttackNova.cooldownStatusEffectName, "Fire nova cooldown",
                () => PluginConfig.secondaryAttackNova.cooldown.Value);
            CoreSecondaryAttackHelper.Register(prefabs.projectileRainSpawn.name, PluginConfig.secondaryAttackRain.cooldownStatusEffectName, "Meteors cooldown",
                () => PluginConfig.secondaryAttackRain.cooldown.Value);
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_firestaffs_dw");

            // Fire assets
            prefabs.staffFire1Prefab = assetBundle.LoadAsset<GameObject>("MMFS_StaffFire1");
            prefabs.staffFire2Prefab = assetBundle.LoadAsset<GameObject>("MMFS_StaffFire2");
            prefabs.staffFire3Prefab = assetBundle.LoadAsset<GameObject>("MMFS_StaffFire3");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_fire_projectile_aoe_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_fire_projectile_aoe_small_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_fire_projectile_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_fireball_staff_explosion_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_fireball_staff_explosion_small_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_fireball_staff_impact_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("cinder_MMFS"), true));
            // The effects of the nova. Only registered prefabs get their mocked references (like the mock mixer group) resolved
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_windup_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_nova_NEW_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_spores_MMFS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_spikes_MMFS"), true));
            // The cone rune: the hit of a burst is a local object of the shooter (see MMAoe), so it is not registered. The wrapper holds the
            // main attack that replaces the main attack of the staff and is not registered. The effects of the cone are registered
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_fire_cone_MMFS"), true));
            prefabs.attackCone = assetBundle.LoadAsset<GameObject>("attack_cone_MMFS");
            MMAoeHelper.PrepareBox(prefabs.attackCone, hitProps: true);
            prefabs.mainAttackCone = assetBundle.LoadAsset<GameObject>("main_cone_attack_MMFS");

            // The nova rune: the hit is a local object of the shooter (see MMAoe), so it is not registered. The wrapper holds the
            // secondary attack that is copied to the staff and is not registered
            prefabs.attackNova = assetBundle.LoadAsset<GameObject>("attack_nova_MMFS");
            MMAoeHelper.PrepareSphere(prefabs.attackNova, hitProps: false);
            prefabs.secondaryAttackNova = assetBundle.LoadAsset<GameObject>("secondary_nova_MMFS");

            // The rain rune (meteors): the spawn projectile, its script and the meteor are spawned in the world, so they are registered.
            // The wrapper holds the secondary attack that is copied to the staff and is not registered
            prefabs.projectileRainSpawn = assetBundle.LoadAsset<GameObject>("projectile_spawn_rain_MMFS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileRainSpawn, true));
            prefabs.scriptMeteors = assetBundle.LoadAsset<GameObject>("staff_fire_script_meteors_MMFS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.scriptMeteors, true));
            prefabs.projectileMeteor = assetBundle.LoadAsset<GameObject>("projectile_meteor_MMFS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileMeteor, true));
            prefabs.secondaryAttackRain = assetBundle.LoadAsset<GameObject>("secondary_rain_MMFS");
        }
    }
}
