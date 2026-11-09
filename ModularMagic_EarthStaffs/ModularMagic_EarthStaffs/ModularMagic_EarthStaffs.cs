using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Components;
using ModularMagic_Core.Types;
using ModularMagic_EarthStaffs.Components;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Locale;
using ModularMagic_EarthStaffs.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CoreSecondaryAttackHelper = ModularMagic_Core.Helpers.SecondaryAttackHelper;
using MMAoeHelper = ModularMagic_Core.Helpers.MMAoeHelper;

namespace ModularMagic_EarthStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [BepInDependency(OptionalModHelper.FoodGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_EarthStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_EarthStaffs";
        public const string PluginName = "ModularMagic_EarthStaffs";
        public const string PluginVersion = "0.0.3";
        public static ModularMagic_EarthStaffs Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        public static CustomPrefabs prefabs = new CustomPrefabs();
        public static ItemDataSnapshots snapshots = new ItemDataSnapshots();

        public void Awake()
        {
            Instance = this;
            LocaleEnglish.Init();
            InitAssetBundle();
            PluginConfig.Init();
            // Every entry forwards its change to its own handler from this event in the order they were created,
            // so this runs after the handlers that update the prefabs, and the staff in the hand gets the new values with its runes
            Config.SettingChanged += (sender, args) => EarthImbuementHelper.ReapplyOnHeldStaff();
            RegisterSecondaryAttacks();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaffs;
        }

        private void AddEarthStaffs()
        {
            try
            {
                AddImbuementSlots(prefabs.StaffEarth0, 1, 0);
                AddImbuementSlots(prefabs.StaffEarth1, 2, 1);
                AddImbuementSlots(prefabs.StaffEarth2, 3, 2);
                AddImbuementSlots(prefabs.StaffEarth3, 4, 4);

                ItemHelper.CreateStaff(prefabs.StaffEarth0, PluginConfig.staffEarth0);
                ItemHelper.CreateStaff(prefabs.StaffEarth1, PluginConfig.staffEarth1);
                ItemHelper.CreateStaff(prefabs.StaffEarth2, PluginConfig.staffEarth2);
                ItemHelper.CreateStaff(prefabs.StaffEarth3, PluginConfig.staffEarth3);

                AttackHelper.UpdateNova(prefabs.Nova, PluginConfig.secondaryAttackNova);
                AttackHelper.UpdateRain(prefabs.ProjectileBoulder, PluginConfig.secondaryAttackRain);
                AttackHelper.UpdateSummon(prefabs.SecondaryAttackRoots, prefabs.Root, PluginConfig.secondaryAttackSummon);

                // The config is bound in Awake, so it is ready here
                snapshots.staffEarth0 = new ItemDataSnapShot(prefabs.StaffEarth0.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffEarth0);
                snapshots.staffEarth1 = new ItemDataSnapShot(prefabs.StaffEarth1.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffEarth1);
                snapshots.staffEarth2 = new ItemDataSnapShot(prefabs.StaffEarth2.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffEarth2);
                snapshots.staffEarth3 = new ItemDataSnapShot(prefabs.StaffEarth3.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffEarth3);

                PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in adding the earth staffs: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
            }
        }

        private void AddImbuementSlots(GameObject prefab, int slots, int tier)
        {
            ImbuementSlots imbuementSlots = prefab.AddComponent<ImbuementSlots>();
            imbuementSlots.m_slots = slots;
            imbuementSlots.m_tier = tier;
            imbuementSlots.m_weaponType = WeaponType.MMES;
            imbuementSlots.m_allowedRunes = new List<string>(EarthImbuementHelper.AllowedRunes);
            imbuementSlots.m_colorPreset = LightColorPresetType.Green;
        }

        // Core gives a registered secondary attack its own eitr cost and the cooldown, the attack is recognised by the projectile it spawns.
        // The config is bound before this is called
        private void RegisterSecondaryAttacks()
        {
            CoreSecondaryAttackHelper.Register("attack_nova_MMES", PluginConfig.secondaryAttackNova.cooldownStatusEffectName, "Nova cooldown",
                () => PluginConfig.secondaryAttackNova.cooldown.Value);
            CoreSecondaryAttackHelper.Register("projectile_spawn_boulder_MMES", PluginConfig.secondaryAttackRain.cooldownStatusEffectName, "Boulder cooldown",
                () => PluginConfig.secondaryAttackRain.cooldown.Value);
            CoreSecondaryAttackHelper.Register("script_roots_MMES", PluginConfig.secondaryAttackSummon.cooldownStatusEffectName, "Summon roots cooldown",
                () => PluginConfig.secondaryAttackSummon.cooldown.Value);

            //    StatusEffect exhaustAndFhoulMagicEffect = ScriptableObject.CreateInstance<StatusEffect>();
            //    exhaustAndFhoulMagicEffect.name = "ExhaustAndFoulMagicEffect_DW";
            //    exhaustAndFhoulMagicEffect.m_name = "Exhausted by foul magic";
            //    exhaustAndFhoulMagicEffect.m_icon = assetBundle.LoadAsset<Sprite>("staffEarth3.Sprite");
            //    exhaustAndFhoulMagicEffect.m_startMessageType = MessageHud.MessageType.Center;
            //    exhaustAndFhoulMagicEffect.m_startMessage = "";
            //    exhaustAndFhoulMagicEffect.m_stopMessageType = MessageHud.MessageType.Center;
            //    exhaustAndFhoulMagicEffect.m_stopMessage = "";
            //    exhaustAndFhoulMagicEffect.m_tooltip = "You are exhausted by the use of foul magic, reducing your strength and magic effectiveness";
            //    exhaustAndFhoulMagicEffect.m_ttl = 300;
            //    ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(exhaustAndFhoulMagicEffect, fixReference: false));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_earthstaffs_dw");

            prefabs.StaffEarth0 = assetBundle.LoadAsset<GameObject>("MMES_TheForestFlinger");
            prefabs.StaffEarth1 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth1");
            prefabs.StaffEarth2 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth2");
            prefabs.StaffEarth3 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth3");
            // The default projectile is the blunt one, slash and pierce are copies of it with their own visual
            prefabs.ProjectileDefault = assetBundle.LoadAsset<GameObject>("projectile_MMES");
            ProjectileHelper.SetBlunt(prefabs.ProjectileDefault);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileDefault, true));
            prefabs.ProjectileSlash = ProjectileHelper.CreateVariant("projectile_MMES_slash", prefabs.ProjectileDefault, ProjectileHelper.SetSlash);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileSlash, true));
            prefabs.ProjectilePierce = ProjectileHelper.CreateVariant("projectile_MMES_pierce", prefabs.ProjectileDefault, ProjectileHelper.SetPierce);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectilePierce, true));

            prefabs.ProjectileMushroom = assetBundle.LoadAsset<GameObject>("projectile_mushroom_MMES");
            prefabs.ProjectileMushroom.AddComponent<MushroomProjectileVisual>();
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileMushroom, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_MMES"), true));

            // The effect of the cone wrapper. It is registered so its mocked references are resolved, the wrapper itself is not registered
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_cone_MMES"), true));
            prefabs.mainAttackCone = assetBundle.LoadAsset<GameObject>("main_cone_attack_MMES");
            // The hit of a burst is a local object of the shooter (see MMAoe), so it is not registered
            prefabs.Cone = assetBundle.LoadAsset<GameObject>("attack_cone_MMES");
            MMAoeHelper.PrepareBox(prefabs.Cone, hitProps: true);
            prefabs.Cone.AddComponent<ConeDamage>();

            prefabs.SecondaryAttackNova = assetBundle.LoadAsset<GameObject>("secondary_nova_MMES");
            // The visuals of the nova (the spikes) are an effect of the wrapper attack, so they are registered. The effect plays on every client,
            // so the terrain edit of the nova is on it: the terrain modifier it creates is local to each client
            GameObject fxNova = assetBundle.LoadAsset<GameObject>("fx_staff_earth_nova_MMES");
            fxNova.AddComponent<NovaTerrainEdit>();
            PrefabManager.Instance.AddPrefab(new CustomPrefab(fxNova, true));
            // The hit is a local object of the shooter (see MMAoe), so it is not registered. The main damage of the nova is always pierce
            prefabs.Nova = assetBundle.LoadAsset<GameObject>("attack_nova_MMES");
            MMAoeHelper.PrepareSphere(prefabs.Nova, hitProps: true);
            prefabs.Nova.AddComponent<NovaDamage>();
            prefabs.LevelTerrain = assetBundle.LoadAsset<GameObject>("script_levelTerrain_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.LevelTerrain, false));

            prefabs.SecondaryAttackBoulder = assetBundle.LoadAsset<GameObject>("secondary_boulder_MMES");
            prefabs.ProjectileBoulder = assetBundle.LoadAsset<GameObject>("projectile_boulder_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileBoulder, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("projectile_spawn_boulder_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_boulder_MMES"), true));

            prefabs.SecondaryAttackRoots = assetBundle.LoadAsset<GameObject>("secondary_root_MMES");
            prefabs.Root = assetBundle.LoadAsset<GameObject>("Root_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.Root, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("attack_root_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_roots_MMES"), true));

            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_small_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_large_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spores_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spikes_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_windup_MMES"), true));
        }
    }
}

