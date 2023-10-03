using BepInEx;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static EffectList;

namespace MagicExtended
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class MagicExtended : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.MagicExtended";
        public const string PluginName = "MagicExtended";
        public const string PluginVersion = "0.0.1";
        private static string configFileName = PluginGUID + ".cfg";
        private static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;

        private AssetBundle magicExtendedBundle;
        private GameObject staffLightningPrefab;
        private GameObject staffEarthPrefab;
        private GameObject staffMetalIcePrefab;
        private GameObject staffMetalFirePrefab;
        private GameObject staffMetalLightningPrefab;
        private GameObject StaffMetalEarthPrefab;

        private GameObject projectileLightningPrefab;
        private GameObject projectileLightningAOEPrefab;
        private GameObject fxLightningAOEPrefab;
        private GameObject fxLightningHitPrefab;
        private GameObject fxLightningWindupPrefab;
        private GameObject projectileRockPrefab;
        private GameObject projectileIceShardPrefab;
        private GameObject projectileIceShardBigPrefab;
        private GameObject fxIceWindupPrefab;

        private string[] configModStyleOptions = new string[] { "Vanilla", "Kimetsu" };

        private string sectionGeneral = "1. General";
        private ConfigEntry<bool> configEnable;
        private ConfigEntry<string> configModStyle;

        private string sectionLightningStaff = "2. Lightning staff";
        private static ConfigEntry<bool> configLightningStaffEnable;
        private static ConfigEntry<string> configLightningStaffName;
        private static ConfigEntry<string> configLightningStaffDescription;
        private static ConfigEntry<string> configLightningStaffCraftingStation;
        private static ConfigEntry<int> configLightningStaffMinStationLevel;
        private static ConfigEntry<string> configLightningStaffRecipe;
        private static ConfigEntry<string> configLightningStaffRecipeUpgrade;
        private static ConfigEntry<int> configLightningStaffRecipeMultiplier;
        private static ConfigEntry<int> configLightningStaffMaxQuality;
        private static ConfigEntry<float> configLightningStaffMovementSpeed;
        private static ConfigEntry<float> configLightningStaffDamageMultiplier;
        private static ConfigEntry<int> configLightningStaffBlockArmor;
        private static ConfigEntry<int> configLightningStaffBlockForce;
        private static ConfigEntry<int> configLightningStaffKnockBack;
        private static ConfigEntry<int> configLightningStaffBackStab;

        private string sectionLightningStaffMetal = "3. Lightning staff";
        private static ConfigEntry<bool> configLightningStaffMetalEnable;
        private static ConfigEntry<string> configLightningStaffMetalName;
        private static ConfigEntry<string> configLightningStaffMetalDescription;
        private static ConfigEntry<string> configLightningStaffMetalCraftingStation;
        private static ConfigEntry<int> configLightningStaffMetalMinStationLevel;
        private static ConfigEntry<string> configLightningStaffMetalRecipe;
        private static ConfigEntry<string> configLightningStaffMetalRecipeUpgrade;
        private static ConfigEntry<int> configLightningStaffMetalRecipeMultiplier;
        private static ConfigEntry<int> configLightningStaffMetalMaxQuality;
        private static ConfigEntry<float> configLightningStaffMetalMovementSpeed;
        private static ConfigEntry<float> configLightningStaffMetalDamageMultiplier;
        private static ConfigEntry<int> configLightningStaffMetalBlockArmor;
        private static ConfigEntry<int> configLightningStaffMetalBlockForce;
        private static ConfigEntry<int> configLightningStaffMetalKnockBack;
        private static ConfigEntry<int> configLightningStaffMetalBackStab;

        private string sectionEarthStaff = "4. Earth staff";
        private static ConfigEntry<bool> configEarthStaffEnable;

        private string sectionEarthStaffMetal = "5. Earth staff";
        private static ConfigEntry<bool> configEarthStaffMetalEnable;

        private bool isModStyleInitialised = false;
        private string normalEffects = "normal_effects";
        private string normalCrystals = "normal_crystals";
        private string kimetsuEffects = "kimetsu_effects";
        private string kimetsuCrystals = "kimetsu_crystals";

        /**
         * Called when the mod is being initialised
         */
        private void Awake()
        {
            // TODO
            // 1. Add AOE to frost shower otherwise you need to hit them very precise.
            // 2. Check damage on frost shower
            // 3. Check damage on boulder shower
            // 4. Check damage on lightning ball & AOE range

            InitConfig();

            if (!configEnable.Value) return;

            InitAssetBundle();

            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaff;
            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaff;
            PrefabManager.OnVanillaPrefabsAvailable += AddMetalIceStaff;
            PrefabManager.OnVanillaPrefabsAvailable += AddMetalFireStaff;
            PrefabManager.OnVanillaPrefabsAvailable += PatchModStyle;
        }

        private void AddLightningStaff()
        {
            ItemConfig staffConfig = new ItemConfig();
            staffConfig.Name = "Thunderstone Staff";
            staffConfig.CraftingStation = "Workbench";
            staffConfig.AddRequirement(new RequirementConfig("FineWood", 10, 5));
            staffConfig.AddRequirement(new RequirementConfig("Thunderstone", 5, 3));
            staffConfig.AddRequirement(new RequirementConfig("Eitr", 15, 3));
            ItemManager.Instance.AddItem(new CustomItem(staffLightningPrefab, true, staffConfig));
            ItemManager.Instance.AddItem(new CustomItem(staffMetalLightningPrefab, true, staffConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaff;
        }

        private void AddEarthStaff()
        {
            GameObject earthSledgePrefab = PrefabManager.Instance.CreateClonedPrefab("fx_staff_earth_stone_impact", "fx_sledge_demolisher_hit");
            Transform bubble = earthSledgePrefab.transform.Find("bubble wave");
            Transform cloud = earthSledgePrefab.transform.Find("Cloud");
            Transform rocks = earthSledgePrefab.transform.Find("vfx_troll_rock_destroyed");
            Transform splosh = earthSledgePrefab.transform.Find("vfx_Splosh (1)");
            bubble.localScale = new Vector3(2, 2, 2);
            cloud.gameObject.SetActive(true);
            rocks.gameObject.SetActive(true);
            splosh.gameObject.SetActive(true);

            Projectile projComp = projectileRockPrefab.GetComponent<Projectile>();
            PrefabManager.Instance.AddPrefab(earthSledgePrefab);
            EffectData effect = new EffectData();
            effect.m_prefab = earthSledgePrefab;
            effect.m_enabled = true;
            effect.m_variant = -1;
            List<EffectData> effectList = new List<EffectData> { effect };
            projComp.m_hitEffects.m_effectPrefabs = effectList.ToArray();

            ItemConfig staffConfig = new ItemConfig();
            staffConfig.Name = "Earth Rocks Staff";
            staffConfig.CraftingStation = "Workbench";
            staffConfig.AddRequirement(new RequirementConfig("FineWood", 10, 5));
            staffConfig.AddRequirement(new RequirementConfig("Stone", 20, 3));
            staffConfig.AddRequirement(new RequirementConfig("Eitr", 15, 3));
            ItemManager.Instance.AddItem(new CustomItem(staffEarthPrefab, true, staffConfig));
            ItemManager.Instance.AddItem(new CustomItem(StaffMetalEarthPrefab, true, staffConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaff;
        }

        private void AddMetalIceStaff()
        {
            ItemConfig staffConfig = new ItemConfig();
            staffConfig.Name = "Metal Ice Staff";
            staffConfig.CraftingStation = "Workbench";
            staffConfig.AddRequirement(new RequirementConfig("FineWood", 10, 5));
            staffConfig.AddRequirement(new RequirementConfig("Thunderstone", 5, 3));
            staffConfig.AddRequirement(new RequirementConfig("Eitr", 15, 3));
            ItemManager.Instance.AddItem(new CustomItem(staffMetalIcePrefab, true, staffConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMetalIceStaff;
        }

        private void AddMetalFireStaff()
        {
            ItemConfig staffConfig = new ItemConfig();
            staffConfig.Name = "Metal Fire Staff";
            staffConfig.CraftingStation = "Workbench";
            staffConfig.AddRequirement(new RequirementConfig("FineWood", 10, 5));
            staffConfig.AddRequirement(new RequirementConfig("Thunderstone", 5, 3));
            staffConfig.AddRequirement(new RequirementConfig("Eitr", 15, 3));
            ItemManager.Instance.AddItem(new CustomItem(staffMetalFirePrefab, true, staffConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMetalFireStaff;
        }

        private void PatchModStyle()
        {
            // Ice
            Transform effectsIceNormal = staffMetalIcePrefab.transform.Find("attach/default/effects/" + normalEffects);
            Transform effectsIceKimetsu = staffMetalIcePrefab.transform.Find("attach/default/effects/" + kimetsuEffects);
            Transform crystalsIceNormal = staffMetalIcePrefab.transform.Find("attach/default/" + normalCrystals);
            Transform crystalsIceKimetsu = staffMetalIcePrefab.transform.Find("attach/default/" + kimetsuCrystals);
            Transform effectsIceProjectileNormal = projectileIceShardPrefab.transform.Find(normalEffects);
            Transform effectsIceProjectileKimetsu = projectileIceShardPrefab.transform.Find(kimetsuEffects);
            Transform effectsIceBigProjectileNormal = projectileIceShardBigPrefab.transform.Find("visual/" + normalEffects);
            Transform effectsIceBigProjectileKimetsu = projectileIceShardBigPrefab.transform.Find("visual/" + kimetsuEffects);
            Transform effectsIceWindupNormal = fxIceWindupPrefab.transform.Find(normalEffects);
            Transform effectsIceWindupNormal2 = fxIceWindupPrefab.transform.Find("parent/" + normalEffects + "_2");
            Transform effectsIceWindupKimetsu = fxIceWindupPrefab.transform.Find(kimetsuEffects);
            Transform effectsIceWindupKimetsu2 = fxIceWindupPrefab.transform.Find("parent/" + kimetsuEffects + "_2");

            // Lightning
            Transform effectsLightningNormal = staffMetalLightningPrefab.transform.Find("attach/default/effects/" + normalEffects);
            Transform effectsLightningKimetsu = staffMetalLightningPrefab.transform.Find("attach/default/effects/" + kimetsuEffects);
            Transform Thunderstone = staffMetalLightningPrefab.transform.Find("attach/default/thunderstone");
            Transform crystalsLightningKimetsu = staffMetalLightningPrefab.transform.Find("attach/default/" + kimetsuCrystals);
            Transform effectsLightningProjectileNormal = projectileLightningPrefab.transform.Find(normalEffects);
            Transform effectsLightningProjectileKimetsu = projectileLightningPrefab.transform.Find(kimetsuEffects);
            Transform effectsLightningAOENormal = fxLightningAOEPrefab.transform.Find(normalEffects);
            Transform effectsLightningAOEKimetsu = fxLightningAOEPrefab.transform.Find(kimetsuEffects);
            Transform effectsLightningHitnormal = fxLightningHitPrefab.transform.Find(normalEffects);
            Transform effectsLightningHitKimetsu = fxLightningHitPrefab.transform.Find(kimetsuEffects);
            Transform effectsLightningWindupNormal = fxLightningWindupPrefab.transform.Find(normalEffects);
            Transform effectsLightningWindupNormal2 = fxLightningWindupPrefab.transform.Find("parent/" + normalEffects + "_2");
            Transform effectsLightningWindupKimetsu = fxLightningWindupPrefab.transform.Find(kimetsuEffects);
            Transform effectsLightningWindupKimetsu2 = fxLightningWindupPrefab.transform.Find("parent/" + kimetsuEffects + "_2");

            switch (configModStyle.Value)
            {
                case "Vanilla":
                    // Ice
                    effectsIceNormal.gameObject.SetActive(true);
                    effectsIceKimetsu.gameObject.SetActive(false);
                    crystalsIceNormal.gameObject.SetActive(true);
                    crystalsIceKimetsu.gameObject.SetActive(false);
                    effectsIceProjectileNormal.gameObject.SetActive(true);
                    effectsIceProjectileKimetsu.gameObject.SetActive(false);
                    effectsIceBigProjectileNormal.gameObject.SetActive(true);
                    effectsIceBigProjectileKimetsu.gameObject.SetActive(false);
                    effectsIceWindupNormal.gameObject.SetActive(true);
                    effectsIceWindupKimetsu.gameObject.SetActive(false);
                    effectsIceWindupNormal2.gameObject.SetActive(true);
                    effectsIceWindupKimetsu2.gameObject.SetActive(false);

                    // Lightning
                    effectsLightningNormal.gameObject.SetActive(true);
                    effectsLightningKimetsu.gameObject.SetActive(false);
                    Thunderstone.gameObject.SetActive(true);
                    crystalsLightningKimetsu.gameObject.SetActive(false);
                    effectsLightningProjectileNormal.gameObject.SetActive(true);
                    effectsLightningProjectileKimetsu.gameObject.SetActive(false);
                    effectsLightningAOENormal.gameObject.SetActive(true);
                    effectsLightningAOEKimetsu.gameObject.SetActive(false);
                    effectsLightningHitnormal.gameObject.SetActive(true);
                    effectsLightningHitKimetsu.gameObject.SetActive(false);
                    effectsLightningWindupNormal.gameObject.SetActive(true);
                    effectsLightningWindupKimetsu.gameObject.SetActive(false);
                    effectsLightningWindupNormal2.gameObject.SetActive(true);
                    effectsLightningWindupKimetsu2.gameObject.SetActive(false);
                    break;
                case "Kimetsu":
                    // Ice
                    effectsIceNormal.gameObject.SetActive(false);
                    effectsIceKimetsu.gameObject.SetActive(true);
                    crystalsIceNormal.gameObject.SetActive(false);
                    crystalsIceKimetsu.gameObject.SetActive(true);
                    effectsIceProjectileNormal.gameObject.SetActive(false);
                    effectsIceProjectileKimetsu.gameObject.SetActive(true);
                    effectsIceBigProjectileNormal.gameObject.SetActive(false);
                    effectsIceBigProjectileKimetsu.gameObject.SetActive(true);
                    effectsIceWindupNormal.gameObject.SetActive(false);
                    effectsIceWindupKimetsu.gameObject.SetActive(true);
                    effectsIceWindupNormal2.gameObject.SetActive(false);
                    effectsIceWindupKimetsu2.gameObject.SetActive(true);

                    // Lightning
                    effectsLightningNormal.gameObject.SetActive(false);
                    effectsLightningKimetsu.gameObject.SetActive(true);
                    Thunderstone.gameObject.SetActive(false);
                    crystalsLightningKimetsu.gameObject.SetActive(true);
                    effectsLightningProjectileNormal.gameObject.SetActive(false);
                    effectsLightningProjectileKimetsu.gameObject.SetActive(true);
                    effectsLightningAOENormal.gameObject.SetActive(false);
                    effectsLightningAOEKimetsu.gameObject.SetActive(true);
                    effectsLightningHitnormal.gameObject.SetActive(false);
                    effectsLightningHitKimetsu.gameObject.SetActive(true);
                    effectsLightningWindupNormal.gameObject.SetActive(false);
                    effectsLightningWindupKimetsu.gameObject.SetActive(true);
                    effectsLightningWindupNormal2.gameObject.SetActive(false);
                    effectsLightningWindupKimetsu2.gameObject.SetActive(true);
                    break;
            }

            if (!isModStyleInitialised)
            {
                PrefabManager.OnVanillaPrefabsAvailable -= PatchModStyle;
                isModStyleInitialised = true;
            }
        }

        /**
         * Initialise config entries and add the necessary events
         */
        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = false;

                // General
                configEnable = Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                Config.SaveOnConfigSet = true;

                configModStyle = Config.Bind(new ConfigDefinition(sectionGeneral, "Mod style"), "Vanilla",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configModStyleOptions)));
                configModStyle.SettingChanged += (obj, attr) => { PatchModStyle(); };

                FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, configFileName);
                configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                configWatcher.IncludeSubdirectories = true;
                configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise config: " + error);
            }
        }

        /**
         * Event handler for when the config file changes
         */
        private void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(configFileFullPath))
                return;

            try
            {
                Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            magicExtendedBundle = AssetUtils.LoadAssetBundleFromResources("magicextended_dw");

            // Staffs
            staffLightningPrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning_DW");
            staffEarthPrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth_DW");
            staffMetalIcePrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffMetalIceShards_DW");
            staffMetalFirePrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffMetalFireBall_DW");
            staffMetalLightningPrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffMetalLightning_DW");
            StaffMetalEarthPrefab = magicExtendedBundle.LoadAsset<GameObject>("StaffMetalEarth_DW");

            // Lightning assets
            projectileLightningPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_projectile_DW");
            projectileLightningAOEPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_aoe_projectile_DW");
            fxLightningAOEPrefab = magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_aoe_DW");
            fxLightningHitPrefab = magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_hit_DW");
            fxLightningWindupPrefab = magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileLightningPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileLightningAOEPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(fxLightningAOEPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(fxLightningHitPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(fxLightningWindupPrefab, true));

            // Earth assets
            projectileRockPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_earth_stone_projectile_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileRockPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_spawn_stone_script_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_spawn_stone_projectile_DW"), true));
            
            // Ice Metal assets
            projectileIceShardPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_metal_iceshard_projectile_DW");
            projectileIceShardBigPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_metal_iceshard_big_projectile_DW");
            fxIceWindupPrefab = magicExtendedBundle.LoadAsset<GameObject>("fx_staff_ice_windup_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileIceShardPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileIceShardBigPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(fxIceWindupPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_metal_spawn_iceshard_script_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_metal_spawn_iceshard_projectile_DW"), true));

            // Fire Metal assets
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_metal_fireball_projectile"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fire_clusterbomb_aoe"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_fire_windup_DW"), true));
        }
    }
}
