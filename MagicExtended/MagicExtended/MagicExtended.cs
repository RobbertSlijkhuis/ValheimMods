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

        private GameObject projectileLightningPrefab;
        private GameObject projectileEarthPrefab;
        private GameObject projectileRockPrefab;
        private GameObject projectileIceShardPrefab;
        private GameObject secondaryLightningPrefab;
        private GameObject stoneSpawnPrefab;
        private GameObject iceShardScriptPrefab;


        private string sectionGeneral = "1. General";
        private ConfigEntry<bool> configEnable;

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

        /**
         * Called when the mod is being initialised
         */
        private void Awake()
        {
            InitConfig();

            if (!configEnable.Value) return;

            InitAssetBundle();

            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaff;
            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaff;
            PrefabManager.OnVanillaPrefabsAvailable += AddMetalIceStaff;
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
            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaff;
        }

        private void AddMetalIceStaff()
        {
            var script = iceShardScriptPrefab.GetComponent<SpawnAbility>();
            script.m_spawnRadius = 15;
            script.m_spawnDelay = 0.05f;
            script.m_projectileAccuracy = 3;

            ItemConfig staffConfig = new ItemConfig();
            staffConfig.Name = "Metal Ice Staff";
            staffConfig.CraftingStation = "Workbench";
            staffConfig.AddRequirement(new RequirementConfig("FineWood", 10, 5));
            staffConfig.AddRequirement(new RequirementConfig("Thunderstone", 5, 3));
            staffConfig.AddRequirement(new RequirementConfig("Eitr", 15, 3));
            ItemManager.Instance.AddItem(new CustomItem(staffMetalIcePrefab, true, staffConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMetalIceStaff;
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
            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaff;
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

            // Lightning assets
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_aoe_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_hit_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_secondary_DW"), true));

            // Earth assets
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_spawn_stone_script_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_spawn_stone_projectile_DW"), true));
            projectileRockPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_earth_stone_projectile_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileRockPrefab, true));

            // Ice Metal assets
            projectileIceShardPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_metal_iceshard_projectile_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileIceShardPrefab, true));
            iceShardScriptPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_metal_spawn_iceshard_script_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(iceShardScriptPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_metal_spawn_iceshard_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_metal_iceshard_big_projectile_DW"), true));
        }
    }
}

