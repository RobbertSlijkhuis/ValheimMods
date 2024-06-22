using BepInEx;
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
        public GameObject staffEarth1Prefab;
        public GameObject staffEarth2Prefab;
        public GameObject staffEarth3Prefab;
        public GameObject staffFire1Prefab;
        public GameObject staffFire2Prefab;
        public GameObject staffFire3Prefab;
        public GameObject staffFrost1Prefab;
        public GameObject staffFrost2Prefab;
        public GameObject staffFrost3Prefab;
        public GameObject staffLightning3Prefab;

        public GameObject simpleSpellbookPrefab;
        public GameObject advancedSpellbookPrefab;
        public GameObject masterSpellbookPrefab;

        public CustomStatusEffect staffEarthRootCooldownEffect;

        //// Use this class to add your own localization to the game
        //// https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            InitConfig();
            InitStatusEffects();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddMaterials;
            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddFireStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddFrostStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddLightningStaffs;
            PrefabManager.OnVanillaPrefabsAvailable += AddSpellbooks;
            PrefabManager.OnVanillaPrefabsAvailable += AddFenringArmor;

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

            // Stone staff
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
                damageFire = ConfigStaffs.staffFire2DamageFire2.Value,
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
                damageFire = ConfigStaffs.staffFire3DamageFire3.Value,
                attackEitr = ConfigStaffs.staffFire3UseEitr.Value,
                secondaryAttackEitr = ConfigStaffs.staffFire3UseEitrSecondary.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(staffFire3Prefab, true, fire3Config));
            PrefabManager.OnVanillaPrefabsAvailable -= AddFireStaffs;
        }

        private void AddLightningStaffs()
        {
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

        private void AddSpellbooks()
        {
            // Simple Spellbook
            ItemConfig simpleConfig = new ItemConfig();
            simpleConfig.Enabled = ConfigPlugin.configEnable.Value ? ConfigSpellbooks.simpleSpellbookEnable.Value : false;
            simpleConfig.CraftingStation = ConfigSpellbooks.simpleSpellbookCraftingStation.Value;
            simpleConfig.MinStationLevel = ConfigSpellbooks.simpleSpellbookMinStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.simpleSpellbookRecipe.Value, ConfigSpellbooks.simpleSpellbookRecipeUpgrade.Value, ConfigSpellbooks.simpleSpellbookRecipeMultiplier.Value);

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
            RequirementConfig[] advancedRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.advancedSpellbookRecipe.Value, ConfigSpellbooks.advancedSpellbookRecipeUpgrade.Value, ConfigSpellbooks.advancedSpellbookRecipeMultiplier.Value);

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
            RequirementConfig[] masterRequirements = RecipeHelper.GetAsRequirementConfigArray(ConfigSpellbooks.masterSpellbookRecipe.Value, ConfigSpellbooks.masterSpellbookRecipeUpgrade.Value, ConfigSpellbooks.masterSpellbookRecipeMultiplier.Value);

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
            GameObject eitr = PrefabManager.Instance.GetPrefab("Eitr");
            Transform sparcs = eitr.transform.Find("attach/sparcs_world");
            ParticleSystem particles = sparcs.GetComponent<ParticleSystem>();
            particles.maxParticles = 9;
            particles.startLifetime = 8;
            particles.playbackSpeed = 1.2f;
            particles.startColor = new Color(255, 0, 0);


            ItemConfig crudeConfig = new ItemConfig();
            crudeConfig.CraftingStation = "Cauldron";
            crudeConfig.AddRequirement(new RequirementConfig("GreydwarfEye", 5));
            crudeConfig.AddRequirement(new RequirementConfig("Resin", 5));

            ItemConfig fineConfig = new ItemConfig();
            fineConfig.CraftingStation = "Cauldron";
            fineConfig.MinStationLevel = 3;
            fineConfig.AddRequirement(new RequirementConfig("Crystal", 5));
            fineConfig.AddRequirement(new RequirementConfig("Coal", 5));

            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("CrudeEitr_DW"), true, crudeConfig));
            ItemManager.Instance.AddItem(new CustomItem(magicExtendedBundle.LoadAsset<GameObject>("FineEitr_DW"), true, fineConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMaterials;
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

                //FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, configFileName);
                //configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                //configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                //configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                //configWatcher.IncludeSubdirectories = true;
                //configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                //configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise config: " + error);
            }
        }

        ///**
        // * Event handler for when the config file changes
        // */
        //private void OnConfigFileChange(object sender, FileSystemEventArgs e)
        //{
        //    if (!File.Exists(configFileFullPath))
        //        return;

        //    try
        //    {
        //        Config.Reload();
        //    }
        //    catch (Exception error)
        //    {
        //        Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
        //    }
        //}

        private void InitStatusEffects()
        {
            StatusEffect cooldownEffect = ScriptableObject.CreateInstance<StatusEffect>();
            cooldownEffect.name = ConfigStaffs.staffEarth3CooldownStatusEffectName;
            cooldownEffect.m_name = "Summon roots cooldown";
            cooldownEffect.m_icon = magicExtendedBundle.LoadAsset<Sprite>("staffEarthSprite");
            cooldownEffect.m_startMessageType = MessageHud.MessageType.Center;
            cooldownEffect.m_startMessage = "";
            cooldownEffect.m_stopMessageType = MessageHud.MessageType.Center;
            cooldownEffect.m_stopMessage = "";
            cooldownEffect.m_tooltip = "Be patient!";
            cooldownEffect.m_ttl = ConfigStaffs.staffEarth3SecondaryCooldown.Value;
            staffEarthRootCooldownEffect = new CustomStatusEffect(cooldownEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(staffEarthRootCooldownEffect);
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            magicExtendedBundle = AssetUtils.LoadAssetBundleFromResources("magicextended_dw");          

            // Earth assets
            staffEarth1Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth1_DW");
            staffEarth2Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth2_DW");
            staffEarth3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffEarth3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_secondary_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_big_stone_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_script_roots_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("TentaRoot_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_burst_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_earth_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_earth_projectile_spawn_DW"), true));

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
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_script_iceshards_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_nova_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_frost_windup_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_projectile_spawn_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_frost_nova_AOE_DW"), true));

            // Lightning
            staffLightning3Prefab = magicExtendedBundle.LoadAsset<GameObject>("StaffLightning3_DW");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("staff_lightning_projectile_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_AOE_DW"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(magicExtendedBundle.LoadAsset<GameObject>("fx_staff_lightning_windup_DW"), true));

            // Books
            simpleSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("SimpleSpellbook_DW");
            advancedSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("AdvancedSpellbook_DW");
            masterSpellbookPrefab = magicExtendedBundle.LoadAsset<GameObject>("MasterSpellbook_DW");
        }
    }
}

