using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Core.Components;
using ModularMagic_Core.Types;
using ModularMagic_LightningStaffs.Configs;
using ModularMagic_LightningStaffs.Helpers;
using ModularMagic_LightningStaffs.Locale;
using ModularMagic_LightningStaffs.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CoreSecondaryAttackHelper = ModularMagic_Core.Helpers.SecondaryAttackHelper;
using MMAoeHelper = ModularMagic_Core.Helpers.MMAoeHelper;

namespace ModularMagic_LightningStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_LightningStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_LightningStaffs";
        public const string PluginName = "ModularMagic_LightningStaffs";
        public const string PluginVersion = "0.0.3";
        public static ModularMagic_LightningStaffs Instance;
        private static readonly HarmonyLib.Harmony _harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public ItemDataSnapshots snapshots = new ItemDataSnapshots();

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
            Config.SettingChanged += (sender, args) => LightningImbuementHelper.ReapplyOnHeldStaff();
            RegisterSecondaryAttacks();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_LightningStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMLS"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddLightningStaffs()
        {
            try
            {
                // The slots follow the Fire staffs: more slots and a higher tier (higher rune levels) per staff
                AddImbuementSlots(prefabs.staffLightning1Prefab, 2, 1);
                AddImbuementSlots(prefabs.staffLightning2Prefab, 3, 2);
                AddImbuementSlots(prefabs.staffLightning3Prefab, 4, 4);

                ItemHelper.CreateStaff(prefabs.staffLightning1Prefab, PluginConfig.staffLightning1);
                ItemHelper.CreateStaff(prefabs.staffLightning2Prefab, PluginConfig.staffLightning2);
                ItemHelper.CreateStaff(prefabs.staffLightning3Prefab, PluginConfig.staffLightning3);

                // The config is bound in Awake, so it is ready here
                snapshots.staffLightning1 = new ItemDataSnapShot(prefabs.staffLightning1Prefab.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffLightning1);
                snapshots.staffLightning2 = new ItemDataSnapShot(prefabs.staffLightning2Prefab.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffLightning2);
                snapshots.staffLightning3 = new ItemDataSnapShot(prefabs.staffLightning3Prefab.GetComponent<ItemDrop>().m_itemData, PluginConfig.staffLightning3);

                // After the staffs are registered, so a failing attack update can not take the staffs with it
                AttackHelper.UpdateNova(prefabs.attackNova, PluginConfig.secondaryAttackNova);
                AttackHelper.UpdateRain(prefabs.projectileBolt, prefabs.scriptBolts, PluginConfig.secondaryAttackRain);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in adding the lightning staffs: " + e);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaffs;
        }

        private void AddImbuementSlots(GameObject prefab, int slots, int tier)
        {
            ImbuementSlots imbuementSlots = prefab.AddComponent<ImbuementSlots>();
            imbuementSlots.m_slots = slots;
            imbuementSlots.m_tier = tier;
            imbuementSlots.m_weaponType = WeaponType.MMLS;
            imbuementSlots.m_allowedRunes = new List<string>(LightningImbuementHelper.AllowedRunes);
            imbuementSlots.m_colorPreset = LightColorPresetType.LightBlue;
        }

        // Core gives a registered secondary attack its own eitr cost and the cooldown, the attack is recognised by the projectile it spawns.
        // The config is bound before this is called
        private void RegisterSecondaryAttacks()
        {
            CoreSecondaryAttackHelper.Register(prefabs.attackNova.name, PluginConfig.secondaryAttackNova.cooldownStatusEffectName, "Lightning nova cooldown",
                () => PluginConfig.secondaryAttackNova.cooldown.Value);
            CoreSecondaryAttackHelper.Register(prefabs.projectileRainSpawn.name, PluginConfig.secondaryAttackRain.cooldownStatusEffectName, "Lightning bolts cooldown",
                () => PluginConfig.secondaryAttackRain.cooldown.Value);
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_lightningstaffs_dw");

            // Lightning assets
            prefabs.staffLightning1Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning1");
            prefabs.staffLightning2Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning2");
            prefabs.staffLightning3Prefab = assetBundle.LoadAsset<GameObject>("MMLS_staffLightning3");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_lightning_projectile_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("cinder_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_MMLS"), true));
            // The effect of the nova. Only registered prefabs get their mocked references (like the mock mixer group) resolved
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_nova_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_spores_MMLS"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_spikes_MMLS"), true));
            // The nova rune: the hit is a local object of the shooter (see MMAoe), so it is not registered. The wrapper holds the
            // secondary attack that is copied to the staff and is not registered
            prefabs.attackNova = assetBundle.LoadAsset<GameObject>("attack_nova_MMLS");
            MMAoeHelper.PrepareSphere(prefabs.attackNova, hitProps: false);
            prefabs.secondaryAttackNova = assetBundle.LoadAsset<GameObject>("secondary_nova_MMLS");

            // The cone rune: the hit of a burst is a local object of the shooter (see MMAoe), so it is not registered. The wrapper holds the
            // main attack that replaces the main attack of the staff and is not registered. The effects of the cone are registered
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_lightning_cone_MMLS"), true));
            prefabs.attackCone = assetBundle.LoadAsset<GameObject>("attack_cone_MMLS");
            MMAoeHelper.PrepareBox(prefabs.attackCone, hitProps: true);
            prefabs.mainAttackCone = assetBundle.LoadAsset<GameObject>("main_cone_attack_MMLS");

            // The rain rune (lightning bolts): the spawn projectile, its script and the bolt are spawned in the world, so they are registered.
            // The wrapper holds the secondary attack that is copied to the staff and is not registered
            prefabs.projectileRainSpawn = assetBundle.LoadAsset<GameObject>("projectile_spawn_rain_MMLS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileRainSpawn, true));
            prefabs.scriptBolts = assetBundle.LoadAsset<GameObject>("staff_lightning_script_bolts_MMLS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.scriptBolts, true));
            prefabs.projectileBolt = assetBundle.LoadAsset<GameObject>("projectile_bolt_MMLS");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileBolt, true));
            prefabs.secondaryAttackRain = assetBundle.LoadAsset<GameObject>("secondary_rain_MMLS");
        }
    }
}
