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

        private AssetBundle magicExtendedBundle;

        // Materials
        public GameObject crudeEitrPrefab;
        public GameObject fineEitrPrefab;

        // Food
        public GameObject magicalMushroomPrefab;
        public GameObject magicalCookedMushroomPrefab;
        public GameObject magicalMushroomPickablePrefab;
        public GameObject gribSnowMushroomPrefab;
        public GameObject gribSnowMushroomPickablePrefab;
        public GameObject bogMushroomPrefab;
        public GameObject bogMushroomPickablePrefab;
        public GameObject pircedMushroomPrefab;
        public GameObject pircedMushroomPickablePrefab;

        // Staffs
        public GameObject staffEarth0Prefab;
        public GameObject staffEarth1Prefab;
        public GameObject staffEarth2Prefab;
        public GameObject staffEarth3Prefab;
        public GameObject staffFire1Prefab;
        public GameObject staffFire2Prefab;
        public GameObject staffFire3Prefab;
        public GameObject staffFrost1Prefab;
        public GameObject staffFrost2Prefab;
        public GameObject staffFrost3Prefab;
        public GameObject staffLightning1Prefab;
        public GameObject staffLightning2Prefab;
        public GameObject staffLightning3Prefab;

        // Projectiles
        public GameObject projectileMushroomPrefab;

        // Books
        public GameObject simpleSpellbookPrefab;
        public GameObject advancedSpellbookPrefab;
        public GameObject masterSpellbookPrefab;

        // Swamp
        public GameObject helmetMageSwamp;
        public GameObject ArmorMageSwampChest;
        public GameObject ArmorMageSwampLegs;
        public StatusEffect SwampMageArmorSetSE;

        // Special
        public GameObject PlayerHead;
        public GameObject EvilSmoke;

        //// Use this class to add your own localization to the game
        //// https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        // TODO: Disable certain abilities in dungeons because of hitting through walls or being completely ineffective

        private void Awake()
        {
            Instance = this;
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
            
            // ZoneManager.OnVegetationRegistered += CheckLocations;
            // ItemManager.OnItemsRegistered += LogRecipes;
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
            earth0Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth0Enable.Value : false;
            earth0Config.CraftingStation = ConfigStaffs.staffEarth0CraftingStation.Value;
            earth0Config.MinStationLevel = ConfigStaffs.staffEarth0MinStationLevel.Value;
            RequirementConfig[] earth0Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth0Recipe.Value, ConfigStaffs.staffEarth0RecipeUpgrade.Value, ConfigStaffs.staffEarth0RecipeMultiplier.Value);

            if (earth0Requirements == null || earth0Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth0_DW");
            else
                earth0Config.Requirements = earth0Requirements;

            ConfigHelper.PatchStats(staffEarth0Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffEarth0Name.Value,
                description = ConfigStaffs.staffEarth0Description.Value,
                maxQuality = ConfigStaffs.staffEarth0MaxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth0MovementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth0BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth0DeflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth0AttackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth0DamageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth0DamageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth0UseEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffEarth0Prefab, true, earth0Config));

            // Earth1 staff
            ItemConfig earth1Config = new ItemConfig();
            earth1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth1Enable.Value : false;
            earth1Config.CraftingStation = ConfigStaffs.staffEarth1CraftingStation.Value;
            earth1Config.MinStationLevel = ConfigStaffs.staffEarth1MinStationLevel.Value;
            RequirementConfig[] earth1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth1Recipe.Value, ConfigStaffs.staffEarth1RecipeUpgrade.Value, ConfigStaffs.staffEarth1RecipeMultiplier.Value);

            if (earth1Requirements == null || earth1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth1_DW");
            else
                earth1Config.Requirements = earth1Requirements;

            ConfigHelper.PatchStats(staffEarth1Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffEarth1Name.Value,
                description = ConfigStaffs.staffEarth1Description.Value,
                maxQuality = ConfigStaffs.staffEarth1MaxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth1MovementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth1BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth1DeflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth1AttackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth1DamageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth1DamageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth1UseEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffEarth1Prefab, true, earth1Config));

            // Earth2 staff
            ItemConfig earth2Config = new ItemConfig();
            earth2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth2Enable.Value : false;
            earth2Config.CraftingStation = ConfigStaffs.staffEarth2CraftingStation.Value;
            earth2Config.MinStationLevel = ConfigStaffs.staffEarth2MinStationLevel.Value;
            RequirementConfig[] earth2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth2Recipe.Value, ConfigStaffs.staffEarth2RecipeUpgrade.Value, ConfigStaffs.staffEarth2RecipeMultiplier.Value);

            if (earth2Requirements == null || earth2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth2_DW");
            else
                earth2Config.Requirements = earth2Requirements;

            ConfigHelper.PatchStats(staffEarth2Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffEarth2Name.Value,
                description = ConfigStaffs.staffEarth2Description.Value,
                maxQuality = ConfigStaffs.staffEarth2MaxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth2MovementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth2BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth2DeflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth2AttackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth2DamageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth2DamageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth2UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffEarth2UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffEarth2Prefab, true, earth2Config));

            // Earth3 staff
            ItemConfig earth3Config = new ItemConfig();
            earth3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffEarth3Enable.Value : false;
            earth3Config.CraftingStation = ConfigStaffs.staffEarth3CraftingStation.Value;
            earth3Config.MinStationLevel = ConfigStaffs.staffEarth3MinStationLevel.Value;
            RequirementConfig[] earth3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffEarth3Recipe.Value, ConfigStaffs.staffEarth3RecipeUpgrade.Value, ConfigStaffs.staffEarth3RecipeMultiplier.Value);

            if (earth3Requirements == null || earth3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffEarth3_DW");
            else
                earth3Config.Requirements = earth3Requirements;

            ConfigHelper.PatchStats(staffEarth3Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffEarth3Name.Value,
                description = ConfigStaffs.staffEarth3Description.Value,
                maxQuality = ConfigStaffs.staffEarth3MaxQuality.Value,
                movementModifier = ConfigStaffs.staffEarth3MovementSpeed.Value,
                blockPower = ConfigStaffs.staffEarth3BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffEarth3DeflectionForce.Value,
                attackForce = ConfigStaffs.staffEarth3AttackForce.Value,
                damageBlunt = ConfigStaffs.staffEarth3DamageBlunt.Value,
                damageSpirit = ConfigStaffs.staffEarth3DamageSpirit.Value,
                attackEitr = ConfigStaffs.staffEarth3UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffEarth3UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffEarth3Prefab, true, earth3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
        }

        private void AddFireStaffs()
        {
            // Fire1 staff
            ItemConfig fire1Config = new ItemConfig();
            fire1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire1Enable.Value : false;
            fire1Config.CraftingStation = ConfigStaffs.staffFire1CraftingStation.Value;
            fire1Config.MinStationLevel = ConfigStaffs.staffFire1MinStationLevel.Value;
            RequirementConfig[] fire1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire1Recipe.Value, ConfigStaffs.staffFire1RecipeUpgrade.Value, ConfigStaffs.staffFire1RecipeMultiplier.Value);

            if (fire1Requirements == null || fire1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire1_DW");
            else
                fire1Config.Requirements = fire1Requirements;

            ConfigHelper.PatchStats(staffFire1Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFire1Name.Value,
                description = ConfigStaffs.staffFire1Description.Value,
                maxQuality = ConfigStaffs.staffFire1MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFire1MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFire1BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire1DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFire1AttackForce.Value,
                damageBlunt = ConfigStaffs.staffFire1DamageBlunt.Value,
                damageFire = ConfigStaffs.staffFire1DamageFire.Value,
                attackEitr = ConfigStaffs.staffFire1UseEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFire1Prefab, true, fire1Config));

            // Fire2 staff
            ItemConfig fire2Config = new ItemConfig();
            fire2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire2Enable.Value : false;
            fire2Config.CraftingStation = ConfigStaffs.staffFire2CraftingStation.Value;
            fire2Config.MinStationLevel = ConfigStaffs.staffFire2MinStationLevel.Value;
            RequirementConfig[] fire2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire2Recipe.Value, ConfigStaffs.staffFire2RecipeUpgrade.Value, ConfigStaffs.staffFire2RecipeMultiplier.Value);

            if (fire2Requirements == null || fire2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire2_DW");
            else
                fire2Config.Requirements = fire2Requirements;

            ConfigHelper.PatchStats(staffFire2Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFire2Name.Value,
                description = ConfigStaffs.staffFire2Description.Value,
                maxQuality = ConfigStaffs.staffFire2MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFire2MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFire2BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire2DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFire2AttackForce.Value,
                damageBlunt = ConfigStaffs.staffFire2DamageBlunt.Value,
                damageFire = ConfigStaffs.staffFire2DamageFire.Value,
                attackEitr = ConfigStaffs.staffFire2UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFire2UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFire2Prefab, true, fire2Config));

            // Fire3 staff
            ItemConfig fire3Config = new ItemConfig();
            fire3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFire3Enable.Value : false;
            fire3Config.CraftingStation = ConfigStaffs.staffFire3CraftingStation.Value;
            fire3Config.MinStationLevel = ConfigStaffs.staffFire3MinStationLevel.Value;
            RequirementConfig[] fire3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFire3Recipe.Value, ConfigStaffs.staffFire3RecipeUpgrade.Value, ConfigStaffs.staffFire3RecipeMultiplier.Value);

            if (fire3Requirements == null || fire3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFire3_DW");
            else
                fire3Config.Requirements = fire3Requirements;

            ConfigHelper.PatchStats(staffFire3Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFire3Name.Value,
                description = ConfigStaffs.staffFire3Description.Value,
                maxQuality = ConfigStaffs.staffFire3MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFire3MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFire3BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFire3DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFire3AttackForce.Value,
                damageBlunt = ConfigStaffs.staffFire3DamageBlunt.Value,
                damageFire = ConfigStaffs.staffFire3DamageFire.Value,
                attackEitr = ConfigStaffs.staffFire3UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFire3UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFire3Prefab, true, fire3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFireStaffs;
        }

        private void AddFrostStaffs()
        {
            // Frost1 staff
            ItemConfig frost1Config = new ItemConfig();
            frost1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost1Enable.Value : false;
            frost1Config.CraftingStation = ConfigStaffs.staffFrost1CraftingStation.Value;
            frost1Config.MinStationLevel = ConfigStaffs.staffFrost1MinStationLevel.Value;
            RequirementConfig[] frost1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost1Recipe.Value, ConfigStaffs.staffFrost1RecipeUpgrade.Value, ConfigStaffs.staffFrost1RecipeMultiplier.Value);

            if (frost1Requirements == null || frost1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost1_DW");
            else
                frost1Config.Requirements = frost1Requirements;

            ConfigHelper.PatchStats(staffFrost1Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFrost1Name.Value,
                description = ConfigStaffs.staffFrost1Description.Value,
                maxQuality = ConfigStaffs.staffFrost1MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost1MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost1BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost1DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost1AttackForce.Value,
                damagePierce = ConfigStaffs.staffFrost1DamagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost1DamageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost1UseEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFrost1Prefab, true, frost1Config));

            // Frost2 staff
            ItemConfig frost2Config = new ItemConfig();
            frost2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost2Enable.Value : false;
            frost2Config.CraftingStation = ConfigStaffs.staffFrost2CraftingStation.Value;
            frost2Config.MinStationLevel = ConfigStaffs.staffFrost2MinStationLevel.Value;
            RequirementConfig[] frost2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost2Recipe.Value, ConfigStaffs.staffFrost2RecipeUpgrade.Value, ConfigStaffs.staffFrost2RecipeMultiplier.Value);

            if (frost2Requirements == null || frost2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost2_DW");
            else
                frost2Config.Requirements = frost2Requirements;

            ConfigHelper.PatchStats(staffFrost2Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFrost2Name.Value,
                description = ConfigStaffs.staffFrost2Description.Value,
                maxQuality = ConfigStaffs.staffFrost2MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost2MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost2BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost2DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost2AttackForce.Value,
                damagePierce = ConfigStaffs.staffFrost2DamagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost2DamageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost2UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFrost2UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFrost2Prefab, true, frost2Config));

            // Frost3 staff
            ItemConfig frost3Config = new ItemConfig();
            frost3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffFrost3Enable.Value : false;
            frost3Config.CraftingStation = ConfigStaffs.staffFrost3CraftingStation.Value;
            frost3Config.MinStationLevel = ConfigStaffs.staffFrost3MinStationLevel.Value;
            RequirementConfig[] frost3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffFrost3Recipe.Value, ConfigStaffs.staffFrost3RecipeUpgrade.Value, ConfigStaffs.staffFrost3RecipeMultiplier.Value);

            if (frost3Requirements == null || frost3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffFrost3_DW");
            else
                frost3Config.Requirements = frost3Requirements;

            ConfigHelper.PatchStats(staffFrost3Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffFrost3Name.Value,
                description = ConfigStaffs.staffFrost3Description.Value,
                maxQuality = ConfigStaffs.staffFrost3MaxQuality.Value,
                movementModifier = ConfigStaffs.staffFrost3MovementSpeed.Value,
                blockPower = ConfigStaffs.staffFrost3BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffFrost3DeflectionForce.Value,
                attackForce = ConfigStaffs.staffFrost3AttackForce.Value,
                damagePierce = ConfigStaffs.staffFrost3DamagePierce.Value,
                damageFrost = ConfigStaffs.staffFrost3DamageFrost.Value,
                attackEitr = ConfigStaffs.staffFrost3UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFrost3UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFrost3Prefab, true, frost3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFrostStaffs;
        }

        private void AddLightningStaffs()
        {
            // Lightning1 staff
            ItemConfig lightning1Config = new ItemConfig();
            lightning1Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning1Enable.Value : false;
            lightning1Config.CraftingStation = ConfigStaffs.staffLightning1CraftingStation.Value;
            lightning1Config.MinStationLevel = ConfigStaffs.staffLightning1MinStationLevel.Value;
            RequirementConfig[] lightning1Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning1Recipe.Value, ConfigStaffs.staffLightning1RecipeUpgrade.Value, ConfigStaffs.staffLightning1RecipeMultiplier.Value);

            if (lightning1Requirements == null || lightning1Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning1_DW");
            else
                lightning1Config.Requirements = lightning1Requirements;

            ConfigHelper.PatchStats(staffLightning1Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffLightning1Name.Value,
                description = ConfigStaffs.staffLightning1Description.Value,
                maxQuality = ConfigStaffs.staffLightning1MaxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning1MovementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning1BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning1DeflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning1AttackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning1DamagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning1DamagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning1DamageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning1UseEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffLightning1Prefab, true, lightning1Config));

            // Lightning2 staff
            ItemConfig lightning2Config = new ItemConfig();
            lightning2Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning2Enable.Value : false;
            lightning2Config.CraftingStation = ConfigStaffs.staffLightning2CraftingStation.Value;
            lightning2Config.MinStationLevel = ConfigStaffs.staffLightning2MinStationLevel.Value;
            RequirementConfig[] lightning2Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning2Recipe.Value, ConfigStaffs.staffLightning2RecipeUpgrade.Value, ConfigStaffs.staffLightning2RecipeMultiplier.Value);

            if (lightning2Requirements == null || lightning2Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning2_DW");
            else
                lightning2Config.Requirements = lightning2Requirements;

            ConfigHelper.PatchStats(staffLightning2Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffLightning2Name.Value,
                description = ConfigStaffs.staffLightning2Description.Value,
                maxQuality = ConfigStaffs.staffLightning2MaxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning2MovementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning2BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning2DeflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning2AttackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning2DamagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning2DamagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning2DamageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning2UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffLightning2UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffLightning2Prefab, true, lightning2Config));

            // Lightning3 staff
            ItemConfig lightning3Config = new ItemConfig();
            lightning3Config.Enabled = ConfigPlugin.configEnable.Value ? ConfigStaffs.staffLightning3Enable.Value : false;
            lightning3Config.CraftingStation = ConfigStaffs.staffLightning3CraftingStation.Value;
            lightning3Config.MinStationLevel = ConfigStaffs.staffLightning3MinStationLevel.Value;
            RequirementConfig[] lightning3Requirements = RecipeHelper.GetAsRequirementConfigArray(ConfigStaffs.staffLightning3Recipe.Value, ConfigStaffs.staffLightning3RecipeUpgrade.Value, ConfigStaffs.staffLightning3RecipeMultiplier.Value);

            if (lightning3Requirements == null || lightning3Requirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: StaffLightning3_DW");
            else
                lightning3Config.Requirements = lightning3Requirements;

            ConfigHelper.PatchStats(staffLightning3Prefab, new PatchStatsOptions()
            {
                name = ConfigStaffs.staffLightning3Name.Value,
                description = ConfigStaffs.staffLightning3Description.Value,
                maxQuality = ConfigStaffs.staffLightning3MaxQuality.Value,
                movementModifier = ConfigStaffs.staffLightning3MovementSpeed.Value,
                blockPower = ConfigStaffs.staffLightning3BlockArmor.Value,
                deflectionForce = ConfigStaffs.staffLightning3DeflectionForce.Value,
                attackForce = ConfigStaffs.staffLightning3AttackForce.Value,
                damagePickaxe = ConfigStaffs.staffLightning3DamagePickaxe.Value,
                damagePierce = ConfigStaffs.staffLightning3DamagePierce.Value,
                damageLightning = ConfigStaffs.staffLightning3DamageLightning.Value,
                attackEitr = ConfigStaffs.staffLightning3UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffLightning3UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffLightning3Prefab, true, lightning3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddLightningStaffs;
        }

        private void AddArmor()
        {
            ItemConfig shamanConfig = new ItemConfig();
            shamanConfig.Enabled = true;
            shamanConfig.Name = "Shaman Hood";
            shamanConfig.Description = "New armor!";
            shamanConfig.CraftingStation = "Workbench";
            shamanConfig.AddRequirement(new RequirementConfig("WolfHairBundle", 1, 1));
            shamanConfig.AddRequirement(new RequirementConfig("WolfPelt", 1, 1));
            shamanConfig.AddRequirement(new RequirementConfig("LeatherScraps", 1, 1));
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetShaman_DW"), true, shamanConfig));
            shamanConfig.Name = "Shaman Cape";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeShaman_DW"), true, shamanConfig));
            shamanConfig.Name = "Shaman Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorShamanChest_DW"), true, shamanConfig));
            shamanConfig.Name = "Shaman Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorShamanLegs_DW"), true, shamanConfig));
            shamanConfig.Name = "Charred Mask";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetCharredRoot_DW"), true, shamanConfig));
            shamanConfig.Name = "Charred Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorCharredRootChest_DW"), true, shamanConfig));
            shamanConfig.Name = "Charred Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorCharredRootLegs_DW"), true, shamanConfig));
            shamanConfig.Name = "Mountain Hood";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("HelmetMountain_DW"), true, shamanConfig));
            shamanConfig.Name = "Mountain Cape";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeMountain_DW"), true, shamanConfig));
            shamanConfig.Name = "Mountain Chest";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorMountainChest_DW"), true, shamanConfig));
            shamanConfig.Name = "Mountain Legs";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("ArmorMountainLegs_DW"), true, shamanConfig));
            shamanConfig.Name = "Mystic Lantern";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("Lantern_DW"), true, shamanConfig));

            shamanConfig.Name = "Swamp Hood";
            ItemManager.Instance.AddItem(new CustomItem(helmetMageSwamp, true, shamanConfig));
            shamanConfig.Name = "Swamp Cloak";
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CapeMageSwamp_DW"), true, shamanConfig));
            shamanConfig.Name = "Swamp Chest";
            ItemManager.Instance.AddItem(new CustomItem(ArmorMageSwampChest, true, shamanConfig));
            shamanConfig.Name = "Swamp Legs";
            ItemManager.Instance.AddItem(new CustomItem(ArmorMageSwampLegs, true, shamanConfig));

            //// Set evil smoke to player model
            GameObject player = PrefabManager.Instance.GetPrefab("Player");
            GameObject playerHead = player.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head").gameObject;
            GameObject playerChest = player.transform.Find("Visual/Armature/Hips/Spine/Spine1").gameObject;
            GameObject playerArmLeft = player.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHand_Attach").gameObject;
            GameObject playerArmRight = player.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/RightHand_Attach").gameObject;
            GameObject playerLegLeft = player.transform.Find("Visual/Armature/Hips/LeftUpLeg/LeftLeg").gameObject;
            GameObject playerLegRight = player.transform.Find("Visual/Armature/Hips/RightUpLeg/RightLeg").gameObject;

            GameObject eyeLeft = helmetMageSwamp.transform.Find("eye_left").gameObject;
            GameObject eyeRight = helmetMageSwamp.transform.Find("eye_right").gameObject;

            GameObject evilSmokeHead = helmetMageSwamp.transform.Find("evil_smoke").gameObject;
            GameObject evilSmokeHeadFace = helmetMageSwamp.transform.Find("evil_smoke_face").gameObject;
            GameObject evilSmokeChest = ArmorMageSwampChest.transform.Find("evil_smoke").gameObject;
            GameObject evilSmokeArmLeft = ArmorMageSwampChest.transform.Find("evil_smoke_left").gameObject;
            GameObject evilSmokeArmRight = ArmorMageSwampChest.transform.Find("evil_smoke_right").gameObject;
            GameObject evilSmokeLegsLeft = ArmorMageSwampLegs.transform.Find("evil_smoke_left").gameObject;
            GameObject evilSmokeLegsRight = ArmorMageSwampLegs.transform.Find("evil_smoke_right").gameObject;

            Quaternion eyeLeftRotation = new Quaternion(0, 0, 0, 0);
            eyeLeftRotation.eulerAngles = new Vector3(0f, 340.544f, 0f);
            eyeLeft.transform.parent = playerHead.transform;
            eyeLeft.transform.localPosition = new Vector3(-0.00087f, 0.00178f, -0.00035f);
            eyeLeft.transform.localRotation = eyeLeftRotation;
            eyeLeft.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeLeft.SetActive(false);

            Quaternion eyeRightRotation = new Quaternion(0, 0, 0, 0);
            eyeRightRotation.eulerAngles = new Vector3(0f, 27.096f, 0f);
            eyeRight.transform.parent = playerHead.transform;
            eyeRight.transform.localPosition = new Vector3(-0.00087f, 0.00178f, 0.00044f);
            eyeRight.transform.localRotation = eyeRightRotation;
            eyeRight.transform.localScale = new Vector3(0.0002f, 0.0001f, 0.0003f);
            eyeRight.SetActive(false);

            evilSmokeHead.transform.parent = playerHead.transform;
            evilSmokeHead.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeHead.SetActive(false);
            evilSmokeHead.FixReferences();

            evilSmokeHeadFace.transform.parent = playerHead.transform;
            evilSmokeHeadFace.transform.localPosition = new Vector3(-0.0015f, 0.001f, 0f);
            evilSmokeHeadFace.SetActive(false);
            evilSmokeHeadFace.FixReferences();

            evilSmokeChest.transform.parent = playerChest.transform;
            evilSmokeChest.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeChest.SetActive(false);
            evilSmokeChest.FixReferences();

            evilSmokeArmLeft.transform.parent = playerArmLeft.transform;
            evilSmokeArmLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeArmLeft.SetActive(false);
            evilSmokeArmLeft.FixReferences();

            evilSmokeArmRight.transform.parent = playerArmRight.transform;
            evilSmokeArmRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeArmRight.SetActive(false);
            evilSmokeArmRight.FixReferences();

            evilSmokeLegsLeft.transform.parent = playerLegLeft.transform;
            evilSmokeLegsLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeLegsLeft.SetActive(false);
            evilSmokeLegsLeft.FixReferences();

            evilSmokeLegsRight.transform.parent = playerLegRight.transform;
            evilSmokeLegsRight.transform.localPosition = new Vector3(0f, 0f, 0f);
            evilSmokeLegsRight.SetActive(false);
            evilSmokeLegsRight.FixReferences();

            // ArmorStand

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
            simpleConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.simpleSpellbookEnable.Value : false;
            simpleConfig.CraftingStation = ConfigSpellbooks.simpleSpellbookCraftingStation.Value;
            simpleConfig.MinStationLevel = ConfigSpellbooks.simpleSpellbookMinStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.simpleSpellbookRecipe.Value, null, null);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: SimpleSpellbook_DW");
            else
                simpleConfig.Requirements = simpleRequirements;

            ItemDrop simpleDrop = simpleSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr simpleStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            simpleStatusEffect.name = "SimpleEitrStatusEffect_DW";
            simpleStatusEffect.m_name = simpleDrop.m_itemData.m_shared.m_name;
            simpleStatusEffect.m_icon = simpleDrop.m_itemData.GetIcon();
            simpleStatusEffect.SetEitr(ConfigSpellbooks.simpleSpellbookEitr.Value);
            simpleDrop.m_itemData.m_shared.m_equipStatusEffect = simpleStatusEffect;

            ConfigHelper.PatchStats(simpleSpellbookPrefab, new PatchStatsOptions()
            {
                name = ConfigSpellbooks.simpleSpellbookName.Value,
                description = ConfigSpellbooks.simpleSpellbookDescription.Value,
                eitrRegen = ConfigSpellbooks.simpleSpellbookEitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(simpleSpellbookPrefab, true, simpleConfig));

            // Advanced Spellbook
            ItemConfig advancedConfig = new ItemConfig();
            advancedConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.advancedSpellbookEnable.Value : false;
            advancedConfig.CraftingStation = ConfigSpellbooks.advancedSpellbookCraftingStation.Value;
            advancedConfig.MinStationLevel = ConfigSpellbooks.advancedSpellbookMinStationLevel.Value;
            RequirementConfig[] advancedRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.advancedSpellbookRecipe.Value, null, null);

            if (advancedRequirements == null || advancedRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: AdvancedSpellbook_DW");
            else
                advancedConfig.Requirements = advancedRequirements;

            ItemDrop advancedDrop = advancedSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr advancedStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            advancedStatusEffect.name = "AdvancedEitrStatusEffect_DW";
            advancedStatusEffect.m_name = advancedDrop.m_itemData.m_shared.m_name;
            advancedStatusEffect.m_icon = advancedDrop.m_itemData.GetIcon();
            advancedStatusEffect.SetEitr(ConfigSpellbooks.advancedSpellbookEitr.Value);
            advancedDrop.m_itemData.m_shared.m_equipStatusEffect = advancedStatusEffect;

            ConfigHelper.PatchStats(advancedSpellbookPrefab, new PatchStatsOptions()
            {
                name = ConfigSpellbooks.advancedSpellbookName.Value,
                description = ConfigSpellbooks.advancedSpellbookDescription.Value,
                eitrRegen = ConfigSpellbooks.advancedSpellbookEitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(advancedSpellbookPrefab, true, advancedConfig));

            // Master Spellbook
            ItemConfig masterConfig = new ItemConfig();
            masterConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.masterSpellbookEnable.Value : false;
            masterConfig.CraftingStation = ConfigSpellbooks.masterSpellbookCraftingStation.Value;
            masterConfig.MinStationLevel = ConfigSpellbooks.masterSpellbookMinStationLevel.Value;
            RequirementConfig[] masterRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.masterSpellbookRecipe.Value, null, null);

            if (masterRequirements == null || masterRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: MasterSpellbook_DW");
            else
                masterConfig.Requirements = masterRequirements;

            ItemDrop masterDrop = masterSpellbookPrefab.GetComponent<ItemDrop>();
            StatusEffectWithEitr masterStatusEffect = ScriptableObject.CreateInstance<StatusEffectWithEitr>();
            masterStatusEffect.name = "MasterEitrStatusEffect_DW";
            masterStatusEffect.m_name = masterDrop.m_itemData.m_shared.m_name;
            masterStatusEffect.m_icon = masterDrop.m_itemData.GetIcon();
            masterStatusEffect.SetEitr(ConfigSpellbooks.masterSpellbookEitr.Value);
            masterDrop.m_itemData.m_shared.m_equipStatusEffect = masterStatusEffect;

            ConfigHelper.PatchStats(masterSpellbookPrefab, new PatchStatsOptions()
            {
                name = ConfigSpellbooks.masterSpellbookName.Value,
                description = ConfigSpellbooks.masterSpellbookDescription.Value,
                eitrRegen = ConfigSpellbooks.masterSpellbookEitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(masterSpellbookPrefab, true, masterConfig));
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
            crudeConfig.Name = ConfigMaterials.crudeEitrName.Value;
            crudeConfig.Description = ConfigMaterials.crudeEitrDescription.Value;
            crudeConfig.CraftingStation = ConfigMaterials.crudeEitrCraftingStation.Value;
            //crudeConfig.AddRequirement(new RequirementConfig("GreydwarfEye", 5));
            //crudeConfig.AddRequirement(new RequirementConfig("Resin", 5));

            RequirementConfig[] crudeRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigMaterials.crudeEitrRecipe.Value, null, null);

            if (crudeRequirements == null || crudeRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: CrudeEitr_DW");
            else
                crudeConfig.Requirements = crudeRequirements;

            ItemManager.Instance.AddItem(new CustomItem(crudeEitrPrefab, true, crudeConfig));

            // Fine Eitr
            ItemConfig fineConfig = new ItemConfig();
            fineConfig.Name = ConfigMaterials.fineEitrName.Value;
            fineConfig.Description = ConfigMaterials.fineEitrDescription.Value;
            fineConfig.CraftingStation = ConfigMaterials.fineEitrCraftingStation.Value;
            fineConfig.MinStationLevel = ConfigMaterials.fineEitrMinStationLevel.Value;
            //fineConfig.AddRequirement(new RequirementConfig("Crystal", 5));
            //fineConfig.AddRequirement(new RequirementConfig("Coal", 5));

            RequirementConfig[] fineRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigMaterials.fineEitrRecipe.Value, null, null);

            if (fineRequirements == null || fineRequirements.Length == 0)
                Jotunn.Logger.LogWarning("Could not resolve recipe for: FineEitr_DW");
            else
                fineConfig.Requirements = fineRequirements;

            
            ItemManager.Instance.AddItem(new CustomItem(fineEitrPrefab, true, fineConfig));
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
            pircedMushroomConfig.Description = "They have a nice crunch and teste oddly sweet.";
            pircedMushroomConfig.Weight = 0.1f;

            CookingConversionConfig cookedMushroomConfig = new CookingConversionConfig();
            cookedMushroomConfig.FromItem = "MagicalMushroom_DW";
            cookedMushroomConfig.ToItem = "CookedMagicalMushroom_DW";
            cookedMushroomConfig.Station = CookingStations.CookingStation;
            cookedMushroomConfig.CookTime = 20f;

            ItemManager.Instance.AddItem(new CustomItem(magicalMushroomPrefab, true, magicMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(magicalCookedMushroomPrefab, true, cookedMagicMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(gribSnowMushroomPrefab, true, gribsnowConfig));
            ItemManager.Instance.AddItem(new CustomItem(bogMushroomPrefab, true, bogMushroomConfig));
            ItemManager.Instance.AddItem(new CustomItem(pircedMushroomPrefab, true, pircedMushroomConfig));
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(cookedMushroomConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFood;
        }

        private void AddLocations()
        {
            // Magical Mushroom
            List<Heightmap.Biome> magicMushroomBiomeList = new List<Heightmap.Biome>();
            magicMushroomBiomeList.Add(Heightmap.Biome.Meadows);
            magicMushroomBiomeList.Add(Heightmap.Biome.BlackForest);

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
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(magicalMushroomPickablePrefab, true, magicMushroomVegetationConfig));

            // GribSnow
            List<Heightmap.Biome> gribSnowBiomeList = new List<Heightmap.Biome>();
            gribSnowBiomeList.Add(Heightmap.Biome.BlackForest);

            VegetationConfig gribSnowVegetationConfig = new VegetationConfig();
            gribSnowVegetationConfig.Biome = ZoneManager.AnyBiomeOf(gribSnowBiomeList.ToArray());
            gribSnowVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
            gribSnowVegetationConfig.BlockCheck = true;
            gribSnowVegetationConfig.GroupRadius = 5;
            gribSnowVegetationConfig.GroupSizeMin = 1;
            gribSnowVegetationConfig.GroupSizeMax = 3;
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
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(gribSnowMushroomPickablePrefab, true, gribSnowVegetationConfig));

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
            bogMushroomVegetationConfig.ScaleMin = 0.5f;
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
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(bogMushroomPickablePrefab, true, bogMushroomVegetationConfig));

            // pirced Mushroom
            List<Heightmap.Biome> pircedMushroomBiomeList = new List<Heightmap.Biome>();
            pircedMushroomBiomeList.Add(Heightmap.Biome.Mountain);

            VegetationConfig pircedMushroomVegetationConfig = new VegetationConfig();
            pircedMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(pircedMushroomBiomeList.ToArray());
            pircedMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
            pircedMushroomVegetationConfig.BlockCheck = true;
            pircedMushroomVegetationConfig.GroupRadius = 4;
            pircedMushroomVegetationConfig.GroupSizeMin = 2;
            pircedMushroomVegetationConfig.GroupSizeMax = 5;
            pircedMushroomVegetationConfig.ScaleMin = 0.5f;
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
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(pircedMushroomPickablePrefab, true, pircedMushroomVegetationConfig));
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
            cooldownEffect.m_ttl = ConfigStaffs.staffEarth3SecondaryCooldown.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(cooldownEffect, fixReference: false));

            StatusEffect exhaustAndFhoulMagicEffect = ScriptableObject.CreateInstance<StatusEffect>();
            exhaustAndFhoulMagicEffect.name = "ExhaustAndFoulMagicEffect_DW";
            exhaustAndFhoulMagicEffect.m_name = "Exhausted by foul magic";
            exhaustAndFhoulMagicEffect.m_icon = magicExtendedBundle.LoadAsset<Sprite>("staffEarth3Sprite");
            exhaustAndFhoulMagicEffect.m_startMessageType = MessageHud.MessageType.Center;
            exhaustAndFhoulMagicEffect.m_startMessage = "";
            exhaustAndFhoulMagicEffect.m_stopMessageType = MessageHud.MessageType.Center;
            exhaustAndFhoulMagicEffect.m_stopMessage = "";
            exhaustAndFhoulMagicEffect.m_tooltip = "You are exhausted by the use of foul magic, reducing your strength and magic effectiveness";
            exhaustAndFhoulMagicEffect.m_ttl = 300;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(exhaustAndFhoulMagicEffect, fixReference: false));

            SwampMageArmorSetSE.name = "SwampMageArmorSet_DW";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(SwampMageArmorSetSE, true));
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            magicExtendedBundle = AssetUtils.LoadAssetBundleFromResources("magicextended_dw");

            // Materials
            crudeEitrPrefab = magicExtendedBundle.LoadAsset<GameObject>("CrudeEitr_DW");
            fineEitrPrefab = magicExtendedBundle.LoadAsset<GameObject>("FineEitr_DW");

            // Food
            magicalMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("MagicalMushroom_DW");
            magicalCookedMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("CookedMagicalMushroom_DW");
            magicalMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_MagicalMushroom_DW");
            gribSnowMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("GribSnow_DW");
            gribSnowMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_GribSnow_DW");
            bogMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("BogMushroom_DW");
            bogMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_BogMushroom_DW");
            pircedMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("PircedMushroom_DW");
            pircedMushroomPickablePrefab = magicExtendedBundle.LoadAsset<GameObject>("Pickable_PircedMushroom_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicalMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(gribSnowMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(bogMushroomPickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(pircedMushroomPickablePrefab, true));

            // Earth assets
            staffEarth0Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth0_DW");
            staffEarth1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth1_DW");
            staffEarth2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth2_DW");
            staffEarth3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth3_DW");
            projectileMushroomPrefab = magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_mushroom_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(projectileMushroomPrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_spawn_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_big_stone_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_roots_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("TentaRoot_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_burst_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_DW"), true));

            // Fire assets
            staffFire1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire1_DW");
            staffFire2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire2_DW");
            staffFire3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFire3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_fire_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("cinder_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_fire_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_fire_nova_DW"), true));

            // Frost assets
            staffFrost1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost1_DW");
            staffFrost2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost2_DW");
            staffFrost3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffFrost3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_spawn_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_script_iceshards_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_nova_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_nova_AOE_DW"), true));

            // Lightning
            staffLightning1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning1_DW");
            staffLightning2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning2_DW");
            staffLightning3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_nova_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_DW"), true));

            // Books
            simpleSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("SimpleSpellbook_DW");
            advancedSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("AdvancedSpellbook_DW");
            masterSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("MasterSpellbook_DW");

            // Swamp
            helmetMageSwamp = magicExtendedBundle.LoadAsset<GameObject>("HelmetMageSwamp_DW");
            ArmorMageSwampChest = magicExtendedBundle.LoadAsset<GameObject>("ArmorMageSwampChest_DW");
            ArmorMageSwampLegs = magicExtendedBundle.LoadAsset<GameObject>("ArmorMageSwampLegs_DW");
            SwampMageArmorSetSE = magicExtendedBundle.LoadAsset<StatusEffect>("SetEffect_SwampMageArmor_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("SwampMageArmorSetSEEffect_DW"), true));
        }
    }
}

