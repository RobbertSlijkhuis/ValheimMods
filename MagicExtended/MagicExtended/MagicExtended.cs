using BepInEx;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using MagicExtended.Configs;
using MagicExtended.Helpers;
using MagicExtended.Models;
using MagicExtended.StatusEffects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);
        public static MagicExtended Instance;

        public PlayerArmatureHelper playerArmature;

        private AssetBundle magicExtendedBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects statusEffects = new CustomStatusEffects();

        public Material SwampMageArmor_eye_DW;
        public Material PlainsMageArmor_eye_DW;

        //// Use this class to add your own localization to the game
        //// https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        // TODO: Disable certain abilities in dungeons because of hitting through walls or being completely ineffective

        private void Awake()
        {
            Instance = this;
            playerArmature = new PlayerArmatureHelper();

            InitAssetBundle();
            InitConfig();
            InitStatusEffects();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddMaterials;
            PrefabManager.OnVanillaPrefabsAvailable += AddFood;
            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddFireStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddFrostStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddSpellbooks;
            PrefabManager.OnVanillaPrefabsAvailable += AddArmor;
            PrefabManager.OnVanillaPrefabsAvailable += AddFenringArmor;
            ZoneManager.OnVanillaVegetationAvailable += AddLocations;

            //ZoneManager.OnVegetationRegistered += CheckLocations;
            //ItemManager.OnItemsRegistered += LogRecipes;
        }

        //private void LogRecipes()
        //{
        //    ObjectDB.instance.m_recipes.ForEach(r =>
        //    {
        //        Jotunn.Logger.LogInfo(r.name);
        //    });

        //    ItemManager.OnItemsRegistered -= LogRecipes;
        //}

        //private void CheckLocations()
        //{
        //    var pm = ZoneManager.Instance.GetZoneVegetation("Pickable_Thistle");
        //    Jotunn.Logger.LogInfo(pm.m_biome);
        //    Jotunn.Logger.LogInfo(pm.m_biomeArea);
        //    Jotunn.Logger.LogInfo("Block Check: " + pm.m_blockCheck);
        //    Jotunn.Logger.LogInfo("Force placement: " + pm.m_forcePlacement);
        //    Jotunn.Logger.LogInfo("Group Radius: " + pm.m_groupRadius);
        //    Jotunn.Logger.LogInfo("Group Size: " + pm.m_groupSizeMin + " - " + pm.m_groupSizeMax);
        //    Jotunn.Logger.LogInfo("Random Scale: " + pm.m_scaleMin + " - " + pm.m_scaleMax);
        //    Jotunn.Logger.LogInfo("In Forest: " + pm.m_inForest);
        //    Jotunn.Logger.LogInfo("Forest Threshold: " + pm.m_forestTresholdMin + " - " + pm.m_forestTresholdMax);
        //    Jotunn.Logger.LogInfo("Distance form similar: " + pm.m_surroundCheckDistance);
        //    Jotunn.Logger.LogInfo("Min/max allowed in zone: " + pm.m_min + " - " + pm.m_max);

        //    Jotunn.Logger.LogInfo("Altitude: " + pm.m_minAltitude + " - " + pm.m_maxAltitude);
        //    Jotunn.Logger.LogInfo("Terrain Delta: " + pm.m_minTerrainDelta + " - " + pm.m_maxTerrainDelta);
        //    Jotunn.Logger.LogInfo("Terrain Delta Radius: " + pm.m_terrainDeltaRadius);
        //    Jotunn.Logger.LogInfo("Ocean Depth: " + pm.m_minOceanDepth + " - " + pm.m_maxOceanDepth);
        //    Jotunn.Logger.LogInfo("Tilt: " + pm.m_minTilt + " - " + pm.m_maxTilt);
        //}

        private void AddEarthStaffs()
        {
            // Create a copy of the demolisher hit fx and make it bigger and more awesome
            GameObject earthSledgePrefab = PrefabManager.Instance.CreateClonedPrefab("fx_staff_earth_impact_DW", "fx_sledge_demolisher_hit");
            Transform bubble = earthSledgePrefab.transform.Find("bubble wave");
            Transform cloud = earthSledgePrefab.transform.Find("Cloud");
            Transform rocks = earthSledgePrefab.transform.Find("vfx_troll_rock_destroyed");
            Transform splosh = earthSledgePrefab.transform.Find("vfx_Splosh (1)");
            bubble.localScale = new Vector3(2, 2, 2);
            cloud.gameObject.SetActive(true);
            rocks.gameObject.SetActive(true);
            splosh.gameObject.SetActive(true);
            PrefabManager.Instance.AddPrefab(earthSledgePrefab);

            // Get the prefab used for the big rock (secondary attack) and apply our fx
            GameObject secondaryProjectilePrefab = PrefabManager.Instance.GetPrefab("staff_earth_projectile_secondary_DW");
            Projectile projComp = secondaryProjectilePrefab.GetComponent<Projectile>();
            EffectData effect = new EffectData();
            effect.m_prefab = earthSledgePrefab;
            effect.m_enabled = true;
            effect.m_variant = -1;
            List<EffectData> effectList = new List<EffectData> { effect };
            projComp.m_hitEffects.m_effectPrefabs = effectList.ToArray();

            // Earth0 staff
            ItemConfig earth0Config = new ItemConfig();
            earth0Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth0.enable.Value : false;
            earth0Config.CraftingStation = ConfigStaffs.staffEarth0.craftingStation.Value;
            earth0Config.MinStationLevel = ConfigStaffs.staffEarth0.minStationLevel.Value;
            RequirementConfig[] earth0Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth0.recipe.Value, ConfigStaffs.staffEarth0.recipeUpgrade.Value, ConfigStaffs.staffEarth0.recipeMultiplier.Value);

            if (earth0Requirements == null || earth0Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth0_DW");
            else
                earth0Config.Requirements = earth0Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffEarth0Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffEarth0.name.Value,
                description = ConfigStaffs.staffEarth0.description.Value,
                maxQuality = ConfigStaffs.staffEarth0.maxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth0.movementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth0.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth0.deflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth0.attackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth0.damageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth0.damageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth0.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffEarth0Prefab, true, earth0Config));

            // Earth1 staff
            ItemConfig earth1Config = new ItemConfig();
            earth1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth1.enable.Value : false;
            earth1Config.CraftingStation = ConfigStaffs.staffEarth1.craftingStation.Value;
            earth1Config.MinStationLevel = ConfigStaffs.staffEarth1.minStationLevel.Value;
            RequirementConfig[] earth1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth1.recipe.Value, ConfigStaffs.staffEarth1.recipeUpgrade.Value, ConfigStaffs.staffEarth1.recipeMultiplier.Value);

            if (earth1Requirements == null || earth1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth1_DW");
            else
                earth1Config.Requirements = earth1Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffEarth1Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffEarth1.name.Value,
                description = ConfigStaffs.staffEarth1.description.Value,
                maxQuality = ConfigStaffs.staffEarth1.maxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth1.movementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth1.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth1.deflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth1.attackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth1.damageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth1.damageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth1.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffEarth1Prefab, true, earth1Config));

            // Earth2 staff
            ItemConfig earth2Config = new ItemConfig();
            earth2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth2.enable.Value : false;
            earth2Config.CraftingStation = ConfigStaffs.staffEarth2.craftingStation.Value;
            earth2Config.MinStationLevel = ConfigStaffs.staffEarth2.minStationLevel.Value;
            RequirementConfig[] earth2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth2.recipe.Value, ConfigStaffs.staffEarth2.recipeUpgrade.Value, ConfigStaffs.staffEarth2.recipeMultiplier.Value);

            if (earth2Requirements == null || earth2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth2_DW");
            else
                earth2Config.Requirements = earth2Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffEarth2Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffEarth2.name.Value,
                description = ConfigStaffs.staffEarth2.description.Value,
                maxQuality = ConfigStaffs.staffEarth2.maxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth2.movementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth2.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth2.deflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth2.attackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth2.damageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth2.damageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth2.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffEarth2.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffEarth2Prefab, true, earth2Config));

            // Earth3 staff
            ItemConfig earth3Config = new ItemConfig();
            earth3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth3.enable.Value : false;
            earth3Config.CraftingStation = ConfigStaffs.staffEarth3.craftingStation.Value;
            earth3Config.MinStationLevel = ConfigStaffs.staffEarth3.minStationLevel.Value;
            RequirementConfig[] earth3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth3.recipe.Value, ConfigStaffs.staffEarth3.recipeUpgrade.Value, ConfigStaffs.staffEarth3.recipeMultiplier.Value);

            if (earth3Requirements == null || earth3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth3_DW");
            else
                earth3Config.Requirements = earth3Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffEarth3Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffEarth3.name.Value,
                description = ConfigStaffs.staffEarth3.description.Value,
                maxQuality = ConfigStaffs.staffEarth3.maxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth3.movementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth3.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth3.deflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth3.attackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth3.damageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth3.damageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth3.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffEarth3.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffEarth3Prefab, true, earth3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
        }

        private void AddFireStaffs()
        {
            // Fire1 staff
            ItemConfig fire1Config = new ItemConfig();
            fire1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire1.enable.Value : false;
            fire1Config.CraftingStation = ConfigStaffs.staffFire1.craftingStation.Value;
            fire1Config.MinStationLevel = ConfigStaffs.staffFire1.minStationLevel.Value;
            RequirementConfig[] fire1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire1.recipe.Value, ConfigStaffs.staffFire1.recipeUpgrade.Value, ConfigStaffs.staffFire1.recipeMultiplier.Value);

            if (fire1Requirements == null || fire1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire1_DW");
            else
                fire1Config.Requirements = fire1Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFire1Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFire1.name.Value,
                description = ConfigStaffs.staffFire1.description.Value,
                maxQuality = ConfigStaffs.staffFire1.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFire1.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFire1.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire1.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFire1.attackForce.Value,
                damageBlunt = ConfigStaffs.staffFire1.damageBlunt.Value,
                damageFire = ConfigStaffs.staffFire1.damageFire.Value,
                attackEitr = ConfigStaffs.staffFire1.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFire1Prefab, true, fire1Config));

            // Fire2 staff
            ItemConfig fire2Config = new ItemConfig();
            fire2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire2.enable.Value : false;
            fire2Config.CraftingStation = ConfigStaffs.staffFire2.craftingStation.Value;
            fire2Config.MinStationLevel = ConfigStaffs.staffFire2.minStationLevel.Value;
            RequirementConfig[] fire2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire2.recipe.Value, ConfigStaffs.staffFire2.recipeUpgrade.Value, ConfigStaffs.staffFire2.recipeMultiplier.Value);

            if (fire2Requirements == null || fire2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire2_DW");
            else
                fire2Config.Requirements = fire2Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFire2Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFire2.name.Value,
                description = ConfigStaffs.staffFire2.description.Value,
                maxQuality = ConfigStaffs.staffFire2.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFire2.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFire2.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire2.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFire2.attackForce.Value,
                damageBlunt = ConfigStaffs.staffFire2.damageBlunt.Value,
                damageFire = ConfigStaffs.staffFire2.damageFire.Value,
                attackEitr = ConfigStaffs.staffFire2.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFire2.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFire2Prefab, true, fire2Config));

            // Fire3
            ItemConfig fire3Config = new ItemConfig();
            fire3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire3.enable.Value : false;
            fire3Config.CraftingStation = ConfigStaffs.staffFire3.craftingStation.Value;
            fire3Config.MinStationLevel = ConfigStaffs.staffFire3.minStationLevel.Value;
            RequirementConfig[] fire3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire3.recipe.Value, ConfigStaffs.staffFire3.recipeUpgrade.Value, ConfigStaffs.staffFire3.recipeMultiplier.Value);

            if (fire3Requirements == null || fire3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire3_DW");
            else
                fire3Config.Requirements = fire3Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFire3Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFire3.name.Value,
                description = ConfigStaffs.staffFire3.description.Value,
                maxQuality = ConfigStaffs.staffFire3.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFire3.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFire3.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire3.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFire3.attackForce.Value,
                damageBlunt = ConfigStaffs.staffFire3.damageBlunt.Value,
                damageFire = ConfigStaffs.staffFire3.damageFire.Value,
                attackEitr = ConfigStaffs.staffFire3.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFire3.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFire3Prefab, true, fire3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFireStaffs;
        }

        private void AddFrostStaffs()
        {
            // Frost1 staff
            ItemConfig frost1Config = new ItemConfig();
            frost1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost1.enable.Value : false;
            frost1Config.CraftingStation = ConfigStaffs.staffFrost1.craftingStation.Value;
            frost1Config.MinStationLevel = ConfigStaffs.staffFrost1.minStationLevel.Value;
            RequirementConfig[] frost1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost1.recipe.Value, ConfigStaffs.staffFrost1.recipeUpgrade.Value, ConfigStaffs.staffFrost1.recipeMultiplier.Value);

            if (frost1Requirements == null || frost1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost1_DW");
            else
                frost1Config.Requirements = frost1Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFrost1Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFrost1.name.Value,
                description = ConfigStaffs.staffFrost1.description.Value,
                maxQuality = ConfigStaffs.staffFrost1.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost1.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost1.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost1.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost1.attackForce.Value,
                damagePierce = ConfigStaffs.staffFrost1.damagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost1.damageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost1.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFrost1Prefab, true, frost1Config));

            // Frost2 staff
            ItemConfig frost2Config = new ItemConfig();
            frost2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost2.enable.Value : false;
            frost2Config.CraftingStation = ConfigStaffs.staffFrost2.craftingStation.Value;
            frost2Config.MinStationLevel = ConfigStaffs.staffFrost2.minStationLevel.Value;
            RequirementConfig[] frost2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost2.recipe.Value, ConfigStaffs.staffFrost2.recipeUpgrade.Value, ConfigStaffs.staffFrost2.recipeMultiplier.Value);

            if (frost2Requirements == null || frost2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost2_DW");
            else
                frost2Config.Requirements = frost2Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFrost2Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFrost2.name.Value,
                description = ConfigStaffs.staffFrost2.description.Value,
                maxQuality = ConfigStaffs.staffFrost2.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost2.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost2.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost2.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost2.attackForce.Value,
                damagePierce = ConfigStaffs.staffFrost2.damagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost2.damageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost2.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFrost2.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFrost2Prefab, true, frost2Config));

            // Frost3 staff
            ItemConfig frost3Config = new ItemConfig();
            frost3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost3.enable.Value : false;
            frost3Config.CraftingStation = ConfigStaffs.staffFrost3.craftingStation.Value;
            frost3Config.MinStationLevel = ConfigStaffs.staffFrost3.minStationLevel.Value;
            RequirementConfig[] frost3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost3.recipe.Value, ConfigStaffs.staffFrost3.recipeUpgrade.Value, ConfigStaffs.staffFrost3.recipeMultiplier.Value);

            if (frost3Requirements == null || frost3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost3_DW");
            else
                frost3Config.Requirements = frost3Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffFrost3Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffFrost3.name.Value,
                description = ConfigStaffs.staffFrost3.description.Value,
                maxQuality = ConfigStaffs.staffFrost3.maxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost3.movementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost3.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost3.deflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost3.attackForce.Value,
                damagePierce = ConfigStaffs.staffFrost3.damagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost3.damageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost3.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFrost3.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffFrost3Prefab, true, frost3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFrostStaffs;
        }

        private void AddLightningStaffs()
        {
            // Lightning1 staff
            ItemConfig lightning1Config = new ItemConfig();
            lightning1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning1.enable.Value : false;
            lightning1Config.CraftingStation = ConfigStaffs.staffLightning1.craftingStation.Value;
            lightning1Config.MinStationLevel = ConfigStaffs.staffLightning1.minStationLevel.Value;
            RequirementConfig[] lightning1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning1.recipe.Value, ConfigStaffs.staffLightning1.recipeUpgrade.Value, ConfigStaffs.staffLightning1.recipeMultiplier.Value);

            if (lightning1Requirements == null || lightning1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning1_DW");
            else
                lightning1Config.Requirements = lightning1Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffLightning1Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffLightning1.name.Value,
                description = ConfigStaffs.staffLightning1.description.Value,
                maxQuality = ConfigStaffs.staffLightning1.maxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning1.movementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning1.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning1.deflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning1.attackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning1.damagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning1.damagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning1.damageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning1.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffLightning1Prefab, true, lightning1Config));

            // Lightning2 staff
            ItemConfig lightning2Config = new ItemConfig();
            lightning2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning2.enable.Value : false;
            lightning2Config.CraftingStation = ConfigStaffs.staffLightning2.craftingStation.Value;
            lightning2Config.MinStationLevel = ConfigStaffs.staffLightning2.minStationLevel.Value;
            RequirementConfig[] lightning2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning2.recipe.Value, ConfigStaffs.staffLightning2.recipeUpgrade.Value, ConfigStaffs.staffLightning2.recipeMultiplier.Value);

            if (lightning2Requirements == null || lightning2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning2_DW");
            else
                lightning2Config.Requirements = lightning2Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffLightning2Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffLightning2.name.Value,
                description = ConfigStaffs.staffLightning2.description.Value,
                maxQuality = ConfigStaffs.staffLightning2.maxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning2.movementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning2.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning2.deflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning2.attackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning2.damagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning2.damagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning2.damageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning2.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffLightning2.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffLightning2Prefab, true, lightning2Config));

            // Lightning3 staff
            ItemConfig lightning3Config = new ItemConfig();
            lightning3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning3.enable.Value : false;
            lightning3Config.CraftingStation = ConfigStaffs.staffLightning3.craftingStation.Value;
            lightning3Config.MinStationLevel = ConfigStaffs.staffLightning3.minStationLevel.Value;
            RequirementConfig[] lightning3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning3.recipe.Value, ConfigStaffs.staffLightning3.recipeUpgrade.Value, ConfigStaffs.staffLightning3.recipeMultiplier.Value);

            if (lightning3Requirements == null || lightning3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning3_DW");
            else
                lightning3Config.Requirements = lightning3Requirements;

            ConfigHelper.UpdateItemDropStats(prefabs.staffLightning3Prefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigStaffs.staffLightning3.name.Value,
                description = ConfigStaffs.staffLightning3.description.Value,
                maxQuality = ConfigStaffs.staffLightning3.maxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning3.movementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning3.blockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning3.deflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning3.attackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning3.damagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning3.damagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning3.damageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning3.useEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffLightning3.useEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.staffLightning3Prefab, true, lightning3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaffs;
        }

        private void AddArmor()
        {
            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find(playerArmature.headPath).gameObject;
            GameObject playerSpine2 = player.transform.Find(playerArmature.spine2Path).gameObject;
            GameObject playerSpine1 = player.transform.Find(playerArmature.spine1Path).gameObject;
            GameObject playerShoulderLeft = player.transform.Find(playerArmature.shoulderLeftPath).gameObject;
            GameObject playerShoulderRight = player.transform.Find(playerArmature.shoulderRightPath).gameObject;
            GameObject playerElbowLeft = player.transform.Find(playerArmature.elbowLeftPath).gameObject;
            GameObject playerElbowRight = player.transform.Find(playerArmature.elbowRightPath).gameObject;
            GameObject playerWristLeft = player.transform.Find(playerArmature.wristLeftPath).gameObject;
            GameObject playerWristRight = player.transform.Find(playerArmature.wristRightPath).gameObject;
            GameObject playerHandLeft = player.transform.Find(playerArmature.handLeftPath).gameObject;
            GameObject playerHandRight = player.transform.Find(playerArmature.handRightPath).gameObject;
            GameObject playerKneeLeft = player.transform.Find(playerArmature.kneeLeftPath).gameObject;
            GameObject playerKneeRight = player.transform.Find(playerArmature.kneeRightPath).gameObject;
            GameObject playerAnkleLeft = player.transform.Find(playerArmature.ankleLeftPath).gameObject;
            GameObject playerAnkleRight = player.transform.Find(playerArmature.ankleRightPath).gameObject;

            ItemConfig shamanConfig = new ItemConfig();
            shamanConfig.Enabled = true;
            shamanConfig.Name = "Shaman Hood";
            shamanConfig.Description = "New armor!";
            shamanConfig.CraftingStation = "Workbench";
            shamanConfig.AddRequirement(new RequirementConfig("WolfHairBundle", 1, 1));
            shamanConfig.AddRequirement(new RequirementConfig("WolfPelt", 1, 1));
            shamanConfig.AddRequirement(new RequirementConfig("LeatherScraps", 1, 1));

            prefabs.helmetMageBlackForestPrefab = magicExtendedBundle.LoadAsset<GameObject>("HelmetShaman_DW");
            prefabs.armorMageBlackForestChestPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorShamanChest_DW");
            prefabs.armorMageBlackForestLegsPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorShamanLegs_DW");

            ItemManager.Instance.AddItem(new CustomItem(prefabs.helmetMageBlackForestPrefab, true, shamanConfig));
            shamanConfig.Name = "Shaman Cape";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeShaman_DW"), true, shamanConfig));
            shamanConfig.Name = "Shaman Chest";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageBlackForestChestPrefab, true, shamanConfig));
            shamanConfig.Name = "Shaman Legs";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageBlackForestLegsPrefab, true, shamanConfig));

            GameObject blackForestEffectHead = prefabs.helmetMageBlackForestPrefab.transform.Find("ME_blackforest_effect_head").gameObject;
            GameObject blackForestEffectAntlerLeft = prefabs.helmetMageBlackForestPrefab.transform.Find("ME_blackforest_effect_antler_left").gameObject;
            GameObject blackForestEffectAntlerRight = prefabs.helmetMageBlackForestPrefab.transform.Find("ME_blackforest_effect_antler_right").gameObject;
            GameObject blackForestEffectWristLeft = prefabs.armorMageBlackForestChestPrefab.transform.Find("ME_blackforest_effect_wrist_left").gameObject;
            GameObject blackForestEffectWristRight = prefabs.armorMageBlackForestChestPrefab.transform.Find("ME_blackforest_effect_wrist_right").gameObject;
            GameObject blackForestEffectShinLeft = prefabs.armorMageBlackForestLegsPrefab.transform.Find("ME_blackforest_effect_knee_left").gameObject;
            GameObject blackForestEffectShinRight = prefabs.armorMageBlackForestLegsPrefab.transform.Find("ME_blackforest_effect_knee_right").gameObject;

            blackForestEffectHead.FixReferences();
            blackForestEffectAntlerLeft.FixReferences();
            blackForestEffectAntlerRight.FixReferences();
            blackForestEffectWristLeft.FixReferences();
            blackForestEffectWristRight.FixReferences();
            blackForestEffectShinLeft.FixReferences();
            blackForestEffectShinRight.FixReferences();

            blackForestEffectHead.transform.parent = playerHead.transform;
            //blackForestEffectHead.transform.localPosition = new Vector3(0.001f, 0.004f, 0f);
            //blackForestEffectShinLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(270f, 0f, 0f));
            //blackForestEffectHead.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            blackForestEffectHead.transform.localPosition = new Vector3(-0.0017f, -0.0008f, 0f);
            blackForestEffectHead.transform.localRotation = TransformHelper.generateRotation(new Vector3(90f, 0f, 0f));
            blackForestEffectHead.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            blackForestEffectHead.SetActive(false);

            blackForestEffectAntlerLeft.transform.parent = playerHead.transform;
            //blackForestEffectAntlerLeft.transform.localPosition = new Vector3(0f, 0.0035f, 0.002f);
            //blackForestEffectAntlerLeft.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectAntlerLeft.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            blackForestEffectAntlerLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectAntlerLeft.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            blackForestEffectAntlerLeft.SetActive(false);

            blackForestEffectAntlerRight.transform.parent = playerHead.transform;
            //blackForestEffectAntlerRight.transform.localPosition = new Vector3(0f, 0.0035f, -0.004f);
            //blackForestEffectAntlerRight.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectAntlerRight.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            blackForestEffectAntlerRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectAntlerRight.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            blackForestEffectAntlerRight.SetActive(false);

            blackForestEffectWristLeft.transform.parent = playerWristLeft.transform;
            blackForestEffectWristLeft.transform.localPosition = new Vector3(-0.0012f, -0.001f, -0.00081f);
            blackForestEffectWristLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectWristLeft.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectWristLeft.SetActive(false);

            blackForestEffectWristRight.transform.parent = playerWristRight.transform;
            blackForestEffectWristRight.transform.localPosition = new Vector3(-0.0012f, -0.001f, 0f);
            blackForestEffectWristRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectWristRight.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectWristRight.SetActive(false);

            blackForestEffectShinLeft.transform.parent = playerKneeLeft.transform;
            blackForestEffectShinLeft.transform.localPosition = new Vector3(-0.0012f, 0.003f, 0f);
            blackForestEffectShinLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectShinLeft.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectShinLeft.SetActive(false);

            blackForestEffectShinRight.transform.parent = playerKneeRight.transform;
            blackForestEffectShinRight.transform.localPosition = new Vector3(-0.0012f, 0.003f, 0f);
            blackForestEffectShinRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            blackForestEffectShinRight.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            blackForestEffectShinRight.SetActive(false);

            //shamanConfig.Name = "Charred Mask";
            //ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetCharredRoot_DW"), true, shamanConfig));
            //shamanConfig.Name = "Charred Chest";
            //ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorCharredRootChest_DW"), true, shamanConfig));
            //shamanConfig.Name = "Charred Legs";
            //ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorCharredRootLegs_DW"), true, shamanConfig));

            prefabs.helmetMageSwampPrefab = magicExtendedBundle.LoadAsset<GameObject>("HelmetMageSwamp_DW");
            prefabs.armorMageSwampChestPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorMageSwampChest_DW");
            prefabs.armorMageSwampLegsPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorMageSwampLegs_DW");

            shamanConfig.Name = "Swamp Hood";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.helmetMageSwampPrefab, true, shamanConfig));
            shamanConfig.Name = "Swamp Cloak";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeMageSwamp_DW"), true, shamanConfig));
            shamanConfig.Name = "Swamp Chest";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageSwampChestPrefab, true, shamanConfig));
            shamanConfig.Name = "Swamp Legs";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageSwampLegsPrefab, true, shamanConfig));

            GameObject eyeLeft = prefabs.helmetMageSwampPrefab.transform.Find("ME_eye_left").gameObject;
            GameObject eyeRight = prefabs.helmetMageSwampPrefab.transform.Find("ME_eye_right").gameObject;
            GameObject swampEffectHead = prefabs.helmetMageSwampPrefab.transform.Find("ME_swamp_effect_head").gameObject;
            GameObject swampEffectHeadFace = prefabs.helmetMageSwampPrefab.transform.Find("ME_swamp_effect_face").gameObject;
            GameObject swampEffectSpine1 = prefabs.armorMageSwampChestPrefab.transform.Find("ME_swamp_effect_spine1").gameObject;
            GameObject swampEffectHandLeft = prefabs.armorMageSwampChestPrefab.transform.Find("ME_swamp_effect_hand_left").gameObject;
            GameObject swampEffectHandRight = prefabs.armorMageSwampChestPrefab.transform.Find("ME_swamp_effect_hand_right").gameObject;
            GameObject swampEffectKneeLeft = prefabs.armorMageSwampLegsPrefab.transform.Find("ME_swamp_effect_knee_left").gameObject;
            GameObject swampEffectKneeRight = prefabs.armorMageSwampLegsPrefab.transform.Find("ME_swamp_effect_knee_right").gameObject;

            eyeLeft.FixReferences();
            eyeRight.FixReferences();
            swampEffectHead.FixReferences();
            swampEffectHeadFace.FixReferences();
            swampEffectSpine1.FixReferences();
            swampEffectHandLeft.FixReferences();
            swampEffectHandRight.FixReferences();
            swampEffectKneeLeft.FixReferences();
            swampEffectKneeRight.FixReferences();

            eyeLeft.transform.parent = playerHead.transform;
            eyeLeft.transform.localPosition = new Vector3(-0.00087f, 0.00178f, -0.00035f);
            eyeLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 340.544f, 0f));
            eyeLeft.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeLeft.SetActive(false);

            eyeRight.transform.parent = playerHead.transform;
            eyeRight.transform.localPosition = new Vector3(-0.00087f, 0.00178f, 0.00044f);
            eyeRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 27.096f, 0f));
            eyeRight.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeRight.SetActive(false);

            swampEffectHead.transform.parent = playerHead.transform;
            swampEffectHead.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectHead.SetActive(false);

            swampEffectHeadFace.transform.parent = playerHead.transform;
            swampEffectHeadFace.transform.localPosition = new Vector3(-0.0015f, 0.001f, 0f);
            swampEffectHeadFace.SetActive(false);

            swampEffectSpine1.transform.parent = playerSpine1.transform;
            swampEffectSpine1.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectSpine1.SetActive(false);

            swampEffectHandLeft.transform.parent = playerHandLeft.transform;
            swampEffectHandLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectHandLeft.SetActive(false);

            swampEffectHandRight.transform.parent = playerHandRight.transform;
            swampEffectHandRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectHandRight.SetActive(false);

            swampEffectKneeLeft.transform.parent = playerKneeLeft.transform;
            swampEffectKneeLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectKneeLeft.SetActive(false);

            swampEffectKneeRight.transform.parent = playerKneeRight.transform;
            swampEffectKneeRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            swampEffectKneeRight.SetActive(false);

            prefabs.helmetMageMountainPrefab = magicExtendedBundle.LoadAsset<GameObject>("HelmetMountain_DW");
            prefabs.armorMageMountainChestPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorMountainChest_DW");
            prefabs.armorMageMountainLegsPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorMountainLegs_DW");

            shamanConfig.Name = "Mountain Hood";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.helmetMageMountainPrefab, true, shamanConfig));
            shamanConfig.Name = "Mountain Cape";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeMountain_DW"), true, shamanConfig));
            shamanConfig.Name = "Mountain Chest";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageMountainChestPrefab, true, shamanConfig));
            shamanConfig.Name = "Mountain Legs";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorMageMountainLegsPrefab, true, shamanConfig));

            GameObject mountainEffectHead = prefabs.helmetMageMountainPrefab.transform.Find("ME_mountain_effect_head").gameObject;
            GameObject mountainEffectSpine2 = prefabs.armorMageMountainChestPrefab.transform.Find("ME_mountain_effect_spine2").gameObject;
            GameObject mountainEffectHandLeft = prefabs.armorMageMountainChestPrefab.transform.Find("ME_mountain_effect_hand_left").gameObject;
            GameObject mountainEffectHandRight = prefabs.armorMageMountainChestPrefab.transform.Find("ME_mountain_effect_hand_right").gameObject;
            GameObject mountainEffectKneeLeft = prefabs.armorMageMountainLegsPrefab.transform.Find("ME_mountain_effect_knee_left").gameObject;
            GameObject mountainEffectKneeRight = prefabs.armorMageMountainLegsPrefab.transform.Find("ME_mountain_effect_knee_right").gameObject;

            mountainEffectHead.FixReferences();
            mountainEffectSpine2.FixReferences();
            mountainEffectHandLeft.FixReferences();
            mountainEffectHandRight.FixReferences();
            mountainEffectKneeLeft.FixReferences();
            mountainEffectKneeRight.FixReferences();

            mountainEffectHead.transform.parent = playerHead.transform;
            mountainEffectHead.transform.localPosition = new Vector3(0f, 0.0025f, 0f);
            mountainEffectHead.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectHead.SetActive(false);

            mountainEffectSpine2.transform.parent = playerSpine2.transform;
            mountainEffectSpine2.transform.localPosition = new Vector3(0f, 0f, 0f);
            mountainEffectSpine2.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectSpine2.SetActive(false);

            mountainEffectHandLeft.transform.parent = playerHandLeft.transform;
            mountainEffectHandLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            mountainEffectHandLeft.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectHandLeft.SetActive(false);

            mountainEffectHandRight.transform.parent = playerHandRight.transform;
            mountainEffectHandRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            mountainEffectHandRight.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectHandRight.SetActive(false);

            mountainEffectKneeLeft.transform.parent = playerKneeLeft.transform;
            mountainEffectKneeLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            mountainEffectKneeLeft.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectKneeLeft.SetActive(false);

            mountainEffectKneeRight.transform.parent = playerKneeRight.transform;
            mountainEffectKneeRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            mountainEffectKneeRight.transform.localScale = new Vector3(1f, 1f, 1f);
            mountainEffectKneeRight.SetActive(false);

            prefabs.helmetPlainsMagePrefab = magicExtendedBundle.LoadAsset<GameObject>("HelmetPlainsMage_DW");
            prefabs.armorPlainsMageChestPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorPlainsMageChest_DW");
            prefabs.armorPlainsMageLegsPrefab = magicExtendedBundle.LoadAsset<GameObject>("ArmorPlainsMageLegs_DW");

            shamanConfig.Name = "Plains Hood";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.helmetPlainsMagePrefab, true, shamanConfig));
            shamanConfig.Name = "Plains Cape";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapePlainsMage_DW"), true, shamanConfig));
            shamanConfig.Name = "Plains Chest";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorPlainsMageChestPrefab, true, shamanConfig));
            shamanConfig.Name = "Plains Legs";
            ItemManager.Instance.AddItem(new CustomItem(prefabs.armorPlainsMageLegsPrefab, true, shamanConfig));

            GameObject plainsEffectShoulderLeft = prefabs.armorPlainsMageChestPrefab.transform.Find("ME_plains_effect_shoulder_left").gameObject;
            GameObject plainsEffectShoulderRight = prefabs.armorPlainsMageChestPrefab.transform.Find("ME_plains_effect_shoulder_right").gameObject;

            plainsEffectShoulderLeft.FixReferences();
            plainsEffectShoulderRight.FixReferences();

            plainsEffectShoulderLeft.transform.parent = playerShoulderLeft.transform;
            plainsEffectShoulderLeft.transform.localPosition = new Vector3(0f, 0f, -0.0001f);
            // plainsEffectShoulderLeft.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f));
            plainsEffectShoulderLeft.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            plainsEffectShoulderLeft.SetActive(false);

            plainsEffectShoulderRight.transform.parent = playerShoulderRight.transform;
            plainsEffectShoulderRight.transform.localPosition = new Vector3(0f, 0f, -0.0001f); // 0 -0,0007 -0,0005
            // plainsEffectShoulderRight.transform.localRotation = TransformHelper.generateRotation(new Vector3(0f, 0f, 0f)); // 58, 130, 90
            plainsEffectShoulderRight.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            plainsEffectShoulderRight.SetActive(false);


            // TODO: Als extra effect on verschillende footstep entries hangen
            List<GameObject> footstepEffectList = new List<GameObject>();
            footstepEffectList.Add(prefabs.PlainsMageFootStepsPrefab);
            FootStep.StepEffect footstep = new FootStep.StepEffect();
            footstep.m_name = "PlainsMage Fire";
            footstep.m_motionType = FootStep.MotionType.Run;
            footstep.m_material = FootStep.GroundMaterial.Everything;
            footstep.m_effectPrefabs = footstepEffectList.ToArray();

            FootStep footstepComp = player.GetComponent<FootStep>();
            footstepComp.m_effects.Add(footstep);

            //GameObject fxFootStepGrassRun = PrefabManager.Instance.GetPrefab("fx_footstep_run");
            //prefabs.PlainsMageFootStepsPrefab.transform.parent = fxFootStepGrassRun.transform;

            // TODO: init somewhere else
            shamanConfig.Name = "Mystic Lantern";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("Lantern_DW"), true, shamanConfig));

            // Prepare materials for Embla set
            Material ashlandsMageArmorMat = magicExtendedBundle.LoadAsset<Material>("AshlandsMageArmor_red_DW");
            Material ashlandsMageArmorChestMat = magicExtendedBundle.LoadAsset<Material>("AshlandsMageArmorChest_red_DW");
            Material ashlandsMageArmorLegsMat = magicExtendedBundle.LoadAsset<Material>("AshlandsMageArmorLegs_red_DW");
            ashlandsMageArmorMat.FixReferences();
            ashlandsMageArmorChestMat.FixReferences();
            ashlandsMageArmorLegsMat.FixReferences();
            List<Material> ashlandsMageArmorMaterials = new List<Material>();
            ashlandsMageArmorMaterials.Add(ashlandsMageArmorMat);

            // Override materials on Embla Hood
            GameObject emblaHood = PrefabManager.Instance.GetPrefab("HelmetMage_Ashlands");
            Transform ashlandsHood = emblaHood.transform.Find("attach_skin/AshlandsHood");
            Transform hoodFlat = emblaHood.transform.Find("hood");

            SkinnedMeshRenderer ashlandsHoodMesh = ashlandsHood.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer hoodFlatMesh = hoodFlat.gameObject.GetComponent<MeshRenderer>();

            ashlandsHoodMesh.materials = ashlandsMageArmorMaterials.ToArray();
            hoodFlatMesh.materials = ashlandsMageArmorMaterials.ToArray();

            // Override materials on Embla Chest
            GameObject emblaChest = PrefabManager.Instance.GetPrefab("ArmorMageChest_Ashlands");
            ItemDrop emblaChestItemDrop = emblaChest.GetComponent<ItemDrop>();
            Transform chest = emblaChest.transform.Find("attach_skin/AshlandsMageChest");
            Transform modelFlat = emblaChest.transform.Find("model");

            SkinnedMeshRenderer chestMesh = chest.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer modelFlatMesh = modelFlat.gameObject.GetComponent<MeshRenderer>();

            emblaChestItemDrop.m_itemData.m_shared.m_armorMaterial = ashlandsMageArmorChestMat; // Chest mat
            chestMesh.materials = ashlandsMageArmorMaterials.ToArray();
            modelFlatMesh.materials = ashlandsMageArmorMaterials.ToArray();

            // Override Materials on Embla Leggs
            GameObject emblaLegs = PrefabManager.Instance.GetPrefab("ArmorMageLegs_Ashlands");
            ItemDrop emblaLegsItemDrop = emblaLegs.GetComponent<ItemDrop>();
            Transform Legs = emblaLegs.transform.Find("attach_skin/AshlandsMageLegs");
            Transform log = emblaLegs.transform.Find("log");

            SkinnedMeshRenderer legsMesh = Legs.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer logMesh = log.gameObject.GetComponent<MeshRenderer>();

            emblaLegsItemDrop.m_itemData.m_shared.m_armorMaterial = ashlandsMageArmorLegsMat; // Legs mat
            legsMesh.materials = ashlandsMageArmorMaterials.ToArray();
            logMesh.materials = ashlandsMageArmorMaterials.ToArray();

            PrefabManager.OnVanillaPrefabsAvailable -= AddArmor;
        }

        private void AddSpellbooks()
        {
            // Simple Spellbook
            ItemConfig simpleConfig = new ItemConfig();
            simpleConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.simpleSpellbook.enable.Value : false;
            simpleConfig.CraftingStation = ConfigSpellbooks.simpleSpellbook.craftingStation.Value;
            simpleConfig.MinStationLevel = ConfigSpellbooks.simpleSpellbook.minStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.simpleSpellbook.recipe.Value, null, null);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: SimpleSpellbook_DW");
            else
                simpleConfig.Requirements = simpleRequirements;

            ItemDrop simpleDrop = prefabs.simpleSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr simpleStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            simpleStatusEffect.name = "SimpleEitrStatusEffect_DW";
            simpleStatusEffect.m_name = simpleDrop.m_itemData.m_shared.m_name;
            simpleStatusEffect.m_icon = simpleDrop.m_itemData.GetIcon();
            simpleStatusEffect.SetEitr(ConfigSpellbooks.simpleSpellbook.eitr.Value);
            simpleDrop.m_itemData.m_shared.m_equipStatusEffect = simpleStatusEffect;

            ConfigHelper.UpdateItemDropStats(prefabs.simpleSpellbookPrefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigSpellbooks.simpleSpellbook.name.Value,
                description = ConfigSpellbooks.simpleSpellbook.description.Value,
                eitrRegen = ConfigSpellbooks.simpleSpellbook.eitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.simpleSpellbookPrefab, true, simpleConfig));

            // Advanced Spellbook
            ItemConfig advancedConfig = new ItemConfig();
            advancedConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.advancedSpellbook.enable.Value : false;
            advancedConfig.CraftingStation = ConfigSpellbooks.advancedSpellbook.craftingStation.Value;
            advancedConfig.MinStationLevel = ConfigSpellbooks.advancedSpellbook.minStationLevel.Value;
            RequirementConfig[] advancedRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.advancedSpellbook.recipe.Value, null, null);

            if (advancedRequirements == null || advancedRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: AdvancedSpellbook_DW");
            else
                advancedConfig.Requirements = advancedRequirements;

            ItemDrop advancedDrop = prefabs.advancedSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr advancedStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            advancedStatusEffect.name = "AdvancedEitrStatusEffect_DW";
            advancedStatusEffect.m_name = advancedDrop.m_itemData.m_shared.m_name;
            advancedStatusEffect.m_icon = advancedDrop.m_itemData.GetIcon();
            advancedStatusEffect.SetEitr(ConfigSpellbooks.advancedSpellbook.eitr.Value);
            advancedDrop.m_itemData.m_shared.m_equipStatusEffect = advancedStatusEffect;

            ConfigHelper.UpdateItemDropStats(prefabs.advancedSpellbookPrefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigSpellbooks.advancedSpellbook.name.Value,
                description = ConfigSpellbooks.advancedSpellbook.description.Value,
                eitrRegen = ConfigSpellbooks.advancedSpellbook.eitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.advancedSpellbookPrefab, true, advancedConfig));

            // Master Spellbook
            ItemConfig masterConfig = new ItemConfig();
            masterConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.masterSpellbook.enable.Value : false;
            masterConfig.CraftingStation = ConfigSpellbooks.masterSpellbook.craftingStation.Value;
            masterConfig.MinStationLevel = ConfigSpellbooks.masterSpellbook.minStationLevel.Value;
            RequirementConfig[] masterRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.masterSpellbook.recipe.Value, null, null);

            if (masterRequirements == null || masterRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: MasterSpellbook_DW");
            else
                masterConfig.Requirements = masterRequirements;

            ItemDrop masterDrop = prefabs.masterSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr masterStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            masterStatusEffect.name = "MasterEitrStatusEffect_DW";
            masterStatusEffect.m_name = masterDrop.m_itemData.m_shared.m_name;
            masterStatusEffect.m_icon = masterDrop.m_itemData.GetIcon();
            masterStatusEffect.SetEitr(ConfigSpellbooks.masterSpellbook.eitr.Value);
            masterDrop.m_itemData.m_shared.m_equipStatusEffect = masterStatusEffect;

            ConfigHelper.UpdateItemDropStats(prefabs.masterSpellbookPrefab, new UpdateItemDropStatsOptions()
            {
                name = ConfigSpellbooks.masterSpellbook.name.Value,
                description = ConfigSpellbooks.masterSpellbook.description.Value,
                eitrRegen = ConfigSpellbooks.masterSpellbook.eitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefabs.masterSpellbookPrefab, true, masterConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddSpellbooks;
        }

        private void AddMaterials()
        {
            // Refines Eitr
            GameObject eitr = PrefabManager.Instance.GetPrefab("Eitr");
            Transform sparcs = eitr.transform.Find("attach/sparcs_world");
            ParticleSystem particles = sparcs.GetComponent<ParticleSystem>();
            particles.maxParticles = 9;
            particles.startLifetime = 8;
            particles.playbackSpeed = 1.2f;
            particles.startColor = new Color(255, 0, 0);

            // Crude Eitr
            ItemConfig crudeConfig = new ItemConfig();
            crudeConfig.Name = ConfigMaterials.crudeEitr.name.Value;
            crudeConfig.Description = ConfigMaterials.crudeEitr.description.Value;
            crudeConfig.CraftingStation = ConfigMaterials.crudeEitr.craftingStation.Value;
            //crudeConfig.AddRequirement(new RequirementConfig("GreydwarfEye", 5));
            //crudeConfig.AddRequirement(new RequirementConfig("Resin", 5));

            RequirementConfig[] crudeRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigMaterials.crudeEitr.recipe.Value, null, null);

            if (crudeRequirements == null || crudeRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: CrudeEitr_DW");
            else
                crudeConfig.Requirements = crudeRequirements;

            ItemManager.Instance.AddItem(new CustomItem(prefabs.crudeEitrPrefab, true, crudeConfig));

            // Fine Eitr
            ItemConfig fineConfig = new ItemConfig();
            fineConfig.Name = ConfigMaterials.fineEitr.name.Value;
            fineConfig.Description = ConfigMaterials.fineEitr.description.Value;
            fineConfig.CraftingStation = ConfigMaterials.fineEitr.craftingStation.Value;
            fineConfig.MinStationLevel = ConfigMaterials.fineEitr.minStationLevel.Value;
            //fineConfig.AddRequirement(new RequirementConfig("Crystal", 5));
            //fineConfig.AddRequirement(new RequirementConfig("Coal", 5));

            RequirementConfig[] fineRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigMaterials.fineEitr.recipe.Value, null, null);

            if (fineRequirements == null || fineRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: FineEitr_DW");
            else
                fineConfig.Requirements = fineRequirements;


            ItemManager.Instance.AddItem(new CustomItem(prefabs.fineEitrPrefab, true, fineConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMaterials;
        }

        private void AddFood()
        {
            //// Increase size to find them better
            //Transform visual = magicalMushroomPickablePrefab.transform.Find("visual");
            //visual.localScale = new Vector3(3, 3, 3);

            //Transform visualGribSnow = gribSnowMushroomPickablePrefab.transform.Find("visual");
            //visualGribSnow.localScale = new Vector3(3, 3, 3);

            //Transform visualBog = bogMushroomPickablePrefab.transform.Find("visual");
            //Transform visualBogFoot = bogMushroomPickablePrefab.transform.Find("bogfoot");
            //visualBog.localScale = new Vector3(3, 3, 3);
            //visualBogFoot.localScale = new Vector3(3, 3, 3);

            ItemConfig magicMushroomConfig = new ItemConfig();
            magicMushroomConfig.Name = "Magical Mushroom";
            magicMushroomConfig.Description = "An oddly blue colored mushroom with a soft glow.";
            magicMushroomConfig.Weight = 0.1f;

            ItemConfig cookedMagicMushroomConfig = new ItemConfig();
            cookedMagicMushroomConfig.Name = "Cooked Magical Mushroom";
            cookedMagicMushroomConfig.Description = "They say you should be carefull eating unknown mushrooms... but it looks so delicious!";
            cookedMagicMushroomConfig.Weight = 0.1f;

            ItemConfig gribsnowConfig = new ItemConfig();
            gribsnowConfig.Name = "Gribsnow";
            gribsnowConfig.Description = "The name completely does not imply its effect... does it?";
            gribsnowConfig.Weight = 0.1f;

            ItemConfig bogMushroomConfig = new ItemConfig();
            bogMushroomConfig.Name = "Bog Mushroom";
            bogMushroomConfig.Description = "They taste as vile as they look, you better cook these first!";
            bogMushroomConfig.Weight = 0.1f;

            ItemConfig pircedMushroomConfig = new ItemConfig();
            pircedMushroomConfig.Name = "Pirced Mushroom";
            pircedMushroomConfig.Description = "They have a nice crunch and taste oddly sweet.";
            pircedMushroomConfig.Weight = 0.1f;

            ItemConfig cloudMushroomConfig = new ItemConfig();
            cloudMushroomConfig.Name = "Violet Cloud Mushroom";
            cloudMushroomConfig.Description = "A distinct violet mushroom with swirls!";
            cloudMushroomConfig.Weight = 0.1f;

            CookingConversionConfig cookedMushroomConfig = new CookingConversionConfig();
            cookedMushroomConfig.FromItem = "MagicalMushroom_DW";
            cookedMushroomConfig.ToItem = "CookedMagicalMushroom_DW";
            cookedMushroomConfig.Station = CookingStations.CookingStation;
            cookedMushroomConfig.CookTime = 20f;

            ItemManager.Instance.AddItem(new CustomItem(prefabs.magicalMushroomPrefab, true, magicMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(prefabs.magicalCookedMushroomPrefab, true, cookedMagicMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(prefabs.gribSnowMushroomPrefab, true, gribsnowConfig));
            ItemManager.Instance.AddItem(new CustomItem(prefabs.bogMushroomPrefab, true, bogMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(prefabs.pircedMushroomPrefab, true, pircedMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(prefabs.cloudMushroomPrefab, true, cloudMushroomConfig));
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(cookedMushroomConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFood;
        }

        private void AddLocations()
        {
            // Magical Mushroom
            List<Heightmap.Biome> magicMushroomBiomeList = new List<Heightmap.Biome>();
            magicMushroomBiomeList.Add(Heightmap.Biome.Meadows);

            VegetationConfig magicMushroomVegetationConfig = new VegetationConfig();
            magicMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(magicMushroomBiomeList.ToArray());
            magicMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
            magicMushroomVegetationConfig.BlockCheck = true;
            magicMushroomVegetationConfig.GroupRadius = 5;
            magicMushroomVegetationConfig.GroupSizeMin = 3;
            magicMushroomVegetationConfig.GroupSizeMax = 6;
            magicMushroomVegetationConfig.ScaleMin = 1f;
            magicMushroomVegetationConfig.ScaleMax = 1.5f;
            magicMushroomVegetationConfig.InForest = true;
            magicMushroomVegetationConfig.ForestThresholdMin = 0;
            magicMushroomVegetationConfig.ForestThresholdMax = 1;
            magicMushroomVegetationConfig.Min = 1;
            magicMushroomVegetationConfig.Max = 2;
            magicMushroomVegetationConfig.MinAltitude = 1f;
            magicMushroomVegetationConfig.MaxAltitude = 1000f;
            magicMushroomVegetationConfig.MinTerrainDelta = 0f;
            magicMushroomVegetationConfig.MaxTerrainDelta = 2f;
            magicMushroomVegetationConfig.TerrainDeltaRadius = 0f;
            magicMushroomVegetationConfig.MinOceanDepth = 0f;
            magicMushroomVegetationConfig.MaxOceanDepth = 2f;
            magicMushroomVegetationConfig.MinTilt = 0f;
            magicMushroomVegetationConfig.MaxTilt = 25;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.magicalMushroomPickablePrefab, true, magicMushroomVegetationConfig));

            // GribSnow
            List<Heightmap.Biome> gribSnowBiomeList = new List<Heightmap.Biome>();
            gribSnowBiomeList.Add(Heightmap.Biome.BlackForest);

            VegetationConfig gribSnowVegetationConfig = new VegetationConfig();
            gribSnowVegetationConfig.Biome = ZoneManager.AnyBiomeOf(gribSnowBiomeList.ToArray());
            gribSnowVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
            gribSnowVegetationConfig.BlockCheck = true;
            gribSnowVegetationConfig.GroupRadius = 5;
            gribSnowVegetationConfig.GroupSizeMin = 2;
            gribSnowVegetationConfig.GroupSizeMax = 4;
            gribSnowVegetationConfig.ScaleMin = 1f;
            gribSnowVegetationConfig.ScaleMax = 1.5f;
            gribSnowVegetationConfig.InForest = true;
            gribSnowVegetationConfig.ForestThresholdMin = 0;
            gribSnowVegetationConfig.ForestThresholdMax = 1;
            gribSnowVegetationConfig.Min = 1;
            gribSnowVegetationConfig.Max = 2;
            gribSnowVegetationConfig.MinAltitude = 1f;
            gribSnowVegetationConfig.MaxAltitude = 1000f;
            gribSnowVegetationConfig.MinTerrainDelta = 0f;
            gribSnowVegetationConfig.MaxTerrainDelta = 2f;
            gribSnowVegetationConfig.TerrainDeltaRadius = 0f;
            gribSnowVegetationConfig.MinOceanDepth = 0f;
            gribSnowVegetationConfig.MaxOceanDepth = 2f;
            gribSnowVegetationConfig.MinTilt = 0f;
            gribSnowVegetationConfig.MaxTilt = 25;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.gribSnowMushroomPickablePrefab, true, gribSnowVegetationConfig));

            // Bog Mushroom
            List<Heightmap.Biome> bogMushroomBiomeList = new List<Heightmap.Biome>();
            bogMushroomBiomeList.Add(Heightmap.Biome.Swamp);

            VegetationConfig bogMushroomVegetationConfig = new VegetationConfig();
            bogMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(bogMushroomBiomeList.ToArray());
            bogMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
            bogMushroomVegetationConfig.BlockCheck = true;
            bogMushroomVegetationConfig.GroupRadius = 4;
            bogMushroomVegetationConfig.GroupSizeMin = 2;
            bogMushroomVegetationConfig.GroupSizeMax = 5;
            bogMushroomVegetationConfig.ScaleMin = 0.7f;
            bogMushroomVegetationConfig.ScaleMax = 1f;
            bogMushroomVegetationConfig.InForest = false;
            bogMushroomVegetationConfig.ForestThresholdMin = 1f;
            bogMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            bogMushroomVegetationConfig.Min = 1;
            bogMushroomVegetationConfig.Max = 2;
            bogMushroomVegetationConfig.MinAltitude = 0f;
            bogMushroomVegetationConfig.MaxAltitude = 1000f;
            bogMushroomVegetationConfig.MinTerrainDelta = 0f;
            bogMushroomVegetationConfig.MaxTerrainDelta = 2f;
            bogMushroomVegetationConfig.TerrainDeltaRadius = 0f;
            bogMushroomVegetationConfig.MinOceanDepth = 0f;
            bogMushroomVegetationConfig.MaxOceanDepth = 0f;
            bogMushroomVegetationConfig.MinTilt = 0f;
            bogMushroomVegetationConfig.MaxTilt = 20;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.bogMushroomPickablePrefab, true, bogMushroomVegetationConfig));

            // Pirced Mushroom
            List<Heightmap.Biome> pircedMushroomBiomeList = new List<Heightmap.Biome>();
            pircedMushroomBiomeList.Add(Heightmap.Biome.Mountain);

            VegetationConfig pircedMushroomVegetationConfig = new VegetationConfig();
            pircedMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(pircedMushroomBiomeList.ToArray());
            pircedMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
            pircedMushroomVegetationConfig.BlockCheck = true;
            pircedMushroomVegetationConfig.GroupRadius = 4;
            pircedMushroomVegetationConfig.GroupSizeMin = 2;
            pircedMushroomVegetationConfig.GroupSizeMax = 5;
            pircedMushroomVegetationConfig.ScaleMin = 0.8f;
            pircedMushroomVegetationConfig.ScaleMax = 1f;
            pircedMushroomVegetationConfig.InForest = false;
            pircedMushroomVegetationConfig.ForestThresholdMin = 1f;
            pircedMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            pircedMushroomVegetationConfig.Min = 1;
            pircedMushroomVegetationConfig.Max = 2;
            pircedMushroomVegetationConfig.MinAltitude = 0f;
            pircedMushroomVegetationConfig.MaxAltitude = 1000f;
            pircedMushroomVegetationConfig.MinTerrainDelta = 0f;
            pircedMushroomVegetationConfig.MaxTerrainDelta = 2f;
            pircedMushroomVegetationConfig.TerrainDeltaRadius = 0f;
            pircedMushroomVegetationConfig.MinOceanDepth = 0f;
            pircedMushroomVegetationConfig.MaxOceanDepth = 0f;
            pircedMushroomVegetationConfig.MinTilt = 0f;
            pircedMushroomVegetationConfig.MaxTilt = 20;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.pircedMushroomPickablePrefab, true, pircedMushroomVegetationConfig));

            // Violet Cloud Mushroom
            List<Heightmap.Biome> cloudCloudMushroomBiomeList = new List<Heightmap.Biome>();
            cloudCloudMushroomBiomeList.Add(Heightmap.Biome.Plains);

            VegetationConfig cloudMushroomVegetationConfig = new VegetationConfig();
            cloudMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(cloudCloudMushroomBiomeList.ToArray());
            cloudMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
            cloudMushroomVegetationConfig.BlockCheck = true;
            cloudMushroomVegetationConfig.GroupRadius = 4;
            cloudMushroomVegetationConfig.GroupSizeMin = 2;
            cloudMushroomVegetationConfig.GroupSizeMax = 5;
            cloudMushroomVegetationConfig.ScaleMin = 0.7f;
            cloudMushroomVegetationConfig.ScaleMax = 1f;
            cloudMushroomVegetationConfig.InForest = false;
            cloudMushroomVegetationConfig.ForestThresholdMin = 1f;
            cloudMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            cloudMushroomVegetationConfig.Min = 1;
            cloudMushroomVegetationConfig.Max = 2;
            cloudMushroomVegetationConfig.MinAltitude = 0f;
            cloudMushroomVegetationConfig.MaxAltitude = 1000f;
            cloudMushroomVegetationConfig.MinTerrainDelta = 0f;
            cloudMushroomVegetationConfig.MaxTerrainDelta = 2f;
            cloudMushroomVegetationConfig.TerrainDeltaRadius = 0f;
            cloudMushroomVegetationConfig.MinOceanDepth = 0f;
            cloudMushroomVegetationConfig.MaxOceanDepth = 0f;
            cloudMushroomVegetationConfig.MinTilt = 0f;
            cloudMushroomVegetationConfig.MaxTilt = 20;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.cloudMushroomPickablePrefab, true, cloudMushroomVegetationConfig));
            ZoneManager.OnVanillaVegetationAvailable -= AddLocations;
        }

        private void AddFenringArmor()
        {
            // Test
            ItemConfig testConfig = new ItemConfig();
            testConfig.Enabled = true;
            testConfig.Name = "Fenring Red Hood";
            testConfig.Description = "New armor!";
            testConfig.CraftingStation = "Workbench";
            testConfig.AddRequirement(new RequirementConfig("WolfHairBundle", 1, 1));
            testConfig.AddRequirement(new RequirementConfig("WolfPelt", 1, 1));
            testConfig.AddRequirement(new RequirementConfig("LeatherScraps", 1, 1));

            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetFenringAshlands_DW"), true, testConfig));
            testConfig.Name = "Fenring Red Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringChestAshlands_DW"), true, testConfig));
            testConfig.Name = "Fenring Red Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringLegsAshlands_DW"), true, testConfig));

            testConfig.Name = "Fenring Purple Hood";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetFenringMistlands_DW"), true, testConfig));
            testConfig.Name = "Fenring Purple Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringChestMistlands_DW"), true, testConfig));
            testConfig.Name = "Fenring Purple Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringLegsMistlands_DW"), true, testConfig));

            testConfig.Name = "Fenring Green Hood";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetFenringPlains_DW"), true, testConfig));
            testConfig.Name = "Fenring Green Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringChestPlains_DW"), true, testConfig));
            testConfig.Name = "Fenring Green Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorFenringLegsPlains_DW"), true, testConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFenringArmor;
        }

        /**
         * Initialise config entries and add the necessary events
         */
        private void InitConfig()
        {
            try
            {
                ConfigPlugin.Init();
                Config.Save();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise config: " + error);
            }
        }

        private void InitStatusEffects()
        {
            StatusEffect cooldownEffect = ScriptableObject.CreateInstance<StatusEffect>();
            cooldownEffect.name = ConfigStaffs.staffEarth3CooldownStatusEffectName;
            cooldownEffect.m_name = "Summon roots cooldown";
            cooldownEffect.m_icon = magicExtendedBundle.LoadAsset<Sprite>("StaffEarth3Sprite_DW");
            cooldownEffect.m_startMessageType = MessageHud.MessageType.Center;
            cooldownEffect.m_startMessage = "";
            cooldownEffect.m_stopMessageType = MessageHud.MessageType.Center;
            cooldownEffect.m_stopMessage = "";
            cooldownEffect.m_tooltip = "Be patient!";
            cooldownEffect.m_ttl = ConfigStaffs.staffEarth3.secondaryCooldown.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(cooldownEffect, fixReference: false));

            StatusEffect exhaustAndFhoulMagicEffect = ScriptableObject.CreateInstance<StatusEffect>();
            exhaustAndFhoulMagicEffect.name = "ExhaustAndFoulMagicEffect_DW";
            exhaustAndFhoulMagicEffect.m_name = "Exhausted by foul magic";
            exhaustAndFhoulMagicEffect.m_icon = magicExtendedBundle.LoadAsset<Sprite>("staffEarth3.Sprite");
            exhaustAndFhoulMagicEffect.m_startMessageType = MessageHud.MessageType.Center;
            exhaustAndFhoulMagicEffect.m_startMessage = "";
            exhaustAndFhoulMagicEffect.m_stopMessageType = MessageHud.MessageType.Center;
            exhaustAndFhoulMagicEffect.m_stopMessage = "";
            exhaustAndFhoulMagicEffect.m_tooltip = "You are exhausted by the use of foul magic, reducing your strength and magic effectiveness";
            exhaustAndFhoulMagicEffect.m_ttl = 300;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(exhaustAndFhoulMagicEffect, fixReference: false));

            statusEffects.BlackForestMageArmorSetSE.name = "BlackForestMageArmorSet_DW";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(statusEffects.BlackForestMageArmorSetSE, true));

            statusEffects.SwampMageArmorSetSE.name = "SwampMageArmorSet_DW";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(statusEffects.SwampMageArmorSetSE, true));

            statusEffects.MountainMageArmorSetSE.name = "MountainMageArmorSet_DW";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(statusEffects.MountainMageArmorSetSE, true));

            statusEffects.PlainsMageArmorSetSE.name = "PlainsMageArmorSet_DW";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(statusEffects.PlainsMageArmorSetSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            magicExtendedBundle = AssetUtils.LoadAssetBundleFromResources("magicextended_dw");

            // Materials
            prefabs.crudeEitrPrefab = magicExtendedBundle.LoadAsset<GameObject>("CrudeEitr_DW");
            prefabs.fineEitrPrefab = magicExtendedBundle.LoadAsset<GameObject>("FineEitr_DW");
            SwampMageArmor_eye_DW = magicExtendedBundle.LoadAsset<Material>("SwampMageArmor_eye_DW");
            PlainsMageArmor_eye_DW = magicExtendedBundle.LoadAsset<Material>("PlainsMageArmor_eye_DW");

            // Food
            prefabs.magicalMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("MagicalMushroom_DW");
            prefabs.magicalCookedMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("CookedMagicalMushroom_DW");
            prefabs.magicalMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_MagicalMushroom_DW");
            prefabs.gribSnowMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("GribSnow_DW");
            prefabs.gribSnowMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_GribSnow_DW");
            prefabs.bogMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("BogMushroom_DW");
            prefabs.bogMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_BogMushroom_DW");
            prefabs.pircedMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("PircedMushroom_DW");
            prefabs.pircedMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_PircedMushroom_DW");
            prefabs.cloudMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("VioletCloudMushroom_DW");
            prefabs.cloudMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_VioletCloudMushroom_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.magicalMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.gribSnowMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.bogMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.pircedMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.cloudMushroomPickablePrefab, true));

            // Earth assets
            prefabs.staffEarth0Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth0_DW");
            prefabs.staffEarth1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth1_DW");
            prefabs.staffEarth2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth2_DW");
            prefabs.staffEarth3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth3_DW");
            prefabs.projectileMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_mushroom_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileMushroomPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_spawn_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_big_stone_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_roots_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("TentaRoot_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_spores_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_spikes_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_DW"), true));

            // Fire assets
            prefabs.staffFire1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire1_DW");
            prefabs.staffFire2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire2_DW");
            prefabs.staffFire3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_fire_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("cinder_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_fire_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_fire_nova_DW"), true));

            // Frost assets
            prefabs.staffFrost1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost1_DW");
            prefabs.staffFrost2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost2_DW");
            prefabs.staffFrost3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_spawn_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_script_iceshards_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_iceshard_launch_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_iceshard_launch_smoke_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_nova_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_nova_AOE_DW"), true));

            // Lightning
            prefabs.staffLightning1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning1_DW");
            prefabs.staffLightning2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning2_DW");
            prefabs.staffLightning3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_nova_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_DW"), true));

            // Books
            prefabs.simpleSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("SimpleSpellbook_DW");
            prefabs.advancedSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("AdvancedSpellbook_DW");
            prefabs.masterSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("MasterSpellbook_DW");

            // Effects
            statusEffects.BlackForestMageArmorSetSE = magicExtendedBundle.LoadAsset<StatusEffect>("SetEffect_BlackForestMageArmor_DW");
            statusEffects.SwampMageArmorSetSE = magicExtendedBundle.LoadAsset<StatusEffect>("SetEffect_SwampMageArmor_DW");
            statusEffects.MountainMageArmorSetSE = magicExtendedBundle.LoadAsset<StatusEffect>("SetEffect_MountainMageArmor_DW");
            statusEffects.PlainsMageArmorSetSE = magicExtendedBundle.LoadAsset<StatusEffect>("SetEffect_PlainsMageArmor_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("BlackForestMageArmorSetSEEffect_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("SwampMageArmorSetSEEffect_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("MountainMageArmorSetSEEffect_DW"), true));
            prefabs.PlainsMageFootStepsPrefab = magicExtendedBundle.LoadAsset<GameObject>("PlainsMageFootSteps_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.PlainsMageFootStepsPrefab, true));
        }
    }
}

