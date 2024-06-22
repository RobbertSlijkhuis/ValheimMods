using BepInEx.Configuration;
using MagicExtended.Helpers;
using MagicExtended.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicExtended.Configs
{
    internal static class ConfigStaffs
    {
        // Eart1 staff
        public static string sectionStaffEarth1 = "02. Staff of Stone";
        public static string staffEarth1RecipeName = "Recipe_StaffEarth1_DW";
        public static string staffEarth1DefaultRecipe = "RoundLog:20, Stone:20, Resin:20, CrudeEitr_DW:16";
        public static string staffEarth1DefaultUpgradeRecipe = "RoundLog:10, Stone:10, CrudeEitr_DW:8";

        public static ConfigEntry<bool> staffEarth1Enable;
        public static ConfigEntry<string> staffEarth1Name;
        public static ConfigEntry<string> staffEarth1Description;
        public static ConfigEntry<string> staffEarth1CraftingStation;
        public static ConfigEntry<int> staffEarth1MinStationLevel;
        public static ConfigEntry<string> staffEarth1Recipe;
        public static ConfigEntry<string> staffEarth1RecipeUpgrade;
        public static ConfigEntry<int> staffEarth1RecipeMultiplier;
        public static ConfigEntry<int> staffEarth1MaxQuality;
        public static ConfigEntry<float> staffEarth1MovementSpeed;
        public static ConfigEntry<float> staffEarth1DamageBlunt;
        public static ConfigEntry<float> staffEarth1DamageSpirit;
        public static ConfigEntry<int> staffEarth1BlockArmor;
        public static ConfigEntry<int> staffEarth1DeflectionForce;
        public static ConfigEntry<int> staffEarth1AttackForce;
        public static ConfigEntry<int> staffEarth1UseEitr;

        // Earth2 staff
        public static string sectionStaffEarth2 = "03. Staff of Boulders";
        public static string staffEarth2RecipeName = "Recipe_StaffEarth2_DW";
        public static string staffEarth2DefaultRecipe = "StaffEarth1_DW:1, FineWood:20, Root:10, FineEitr_DW:16";
        public static string staffEarth2DefaultUpgradeRecipe = "FineWood:10, Root:4, FineEitr_DW:8";

        public static ConfigEntry<bool> staffEarth2Enable;
        public static ConfigEntry<string> staffEarth2Name;
        public static ConfigEntry<string> staffEarth2Description;
        public static ConfigEntry<string> staffEarth2CraftingStation;
        public static ConfigEntry<int> staffEarth2MinStationLevel;
        public static ConfigEntry<string> staffEarth2Recipe;
        public static ConfigEntry<string> staffEarth2RecipeUpgrade;
        public static ConfigEntry<int> staffEarth2RecipeMultiplier;
        public static ConfigEntry<int> staffEarth2MaxQuality;
        public static ConfigEntry<float> staffEarth2MovementSpeed;
        public static ConfigEntry<float> staffEarth2DamageBlunt;
        public static ConfigEntry<float> staffEarth2DamageSpirit;
        public static ConfigEntry<int> staffEarth2BlockArmor;
        public static ConfigEntry<int> staffEarth2DeflectionForce;
        public static ConfigEntry<int> staffEarth2AttackForce;
        public static ConfigEntry<int> staffEarth2UseEitr;
        public static ConfigEntry<int> staffEarth2UseEitrSecondary;
        public static ConfigEntry<float> staffEarth2SecondaryCooldown;

        // Earth3 Staff
        public static string sectionStaffEarth3 = "04. Staff of Earth";
        public static string staffEarth3RecipeName = "Recipe_StaffEarth3_DW";
        public static string staffEarth3DefaultRecipe = "StaffEarth2_DW:1, YggdrasilWood:20, Sap:10, Eitr:16";
        public static string staffEarth3DefaultUpgradeRecipe = "YggdrasilWood:10, Sap:4, Eitr:8";
        public static string staffEarth3CooldownStatusEffectName = "StaffEarth3Cooldown_DW";

        public static ConfigEntry<bool> staffEarth3Enable;
        public static ConfigEntry<string> staffEarth3Name;
        public static ConfigEntry<string> staffEarth3Description;
        public static ConfigEntry<string> staffEarth3CraftingStation;
        public static ConfigEntry<int> staffEarth3MinStationLevel;
        public static ConfigEntry<string> staffEarth3Recipe;
        public static ConfigEntry<string> staffEarth3RecipeUpgrade;
        public static ConfigEntry<int> staffEarth3RecipeMultiplier;
        public static ConfigEntry<int> staffEarth3MaxQuality;
        public static ConfigEntry<float> staffEarth3MovementSpeed;
        public static ConfigEntry<float> staffEarth3DamageBlunt;
        public static ConfigEntry<float> staffEarth3DamageSpirit;
        public static ConfigEntry<int> staffEarth3BlockArmor;
        public static ConfigEntry<int> staffEarth3DeflectionForce;
        public static ConfigEntry<int> staffEarth3AttackForce;
        public static ConfigEntry<int> staffEarth3UseEitr;
        public static ConfigEntry<int> staffEarth3UseEitrSecondary;
        public static ConfigEntry<float> staffEarth3SecondaryCooldown;

        // Fire1 Staff
        public static string sectionStaffFireBall = "05. Staff of Fireball";
        public static string staffFire1RecipeName = "Recipe_StaffFire1_DW";
        public static string staffFire1DefaultRecipe = "Bronze:10, SurtlingCore:10, Coal:20, CrudeEitr_DW:16";
        public static string staffFire1DefaultUpgradeRecipe = "Bronze:5, SurtlingCore:2, Coal:5, CrudeEitr_DW:8";

        public static ConfigEntry<bool> staffFire1Enable;
        public static ConfigEntry<string> staffFire1Name;
        public static ConfigEntry<string> staffFire1Description;
        public static ConfigEntry<string> staffFire1CraftingStation;
        public static ConfigEntry<int> staffFire1MinStationLevel;
        public static ConfigEntry<string> staffFire1Recipe;
        public static ConfigEntry<string> staffFire1RecipeUpgrade;
        public static ConfigEntry<int> staffFire1RecipeMultiplier;
        public static ConfigEntry<int> staffFire1MaxQuality;
        public static ConfigEntry<float> staffFire1MovementSpeed;
        public static ConfigEntry<float> staffFire1DamageBlunt;
        public static ConfigEntry<float> staffFire1DamageFire;
        public static ConfigEntry<int> staffFire1BlockArmor;
        public static ConfigEntry<int> staffFire1DeflectionForce;
        public static ConfigEntry<int> staffFire1AttackForce;
        public static ConfigEntry<int> staffFire1UseEitr;

        // Fire2 Staff
        public static string sectionStaffFire2 = "06. Staff of Fire";
        public static string staffFire2RecipeName = "Recipe_StaffFire2_DW";
        public static string staffFire2DefaultRecipe = "StaffFire1_DW:1, Iron:20, Obsidian:10, FineEitr_DW:16";
        public static string staffFire2DefaultUpgradeRecipe = "Iron:5, Obsidian:4, FineEitr_DW:8";

        public static ConfigEntry<bool> staffFire2Enable;
        public static ConfigEntry<string> staffFire2Name;
        public static ConfigEntry<string> staffFire2Description;
        public static ConfigEntry<string> staffFire2CraftingStation;
        public static ConfigEntry<int> staffFire2MinStationLevel;
        public static ConfigEntry<string> staffFire2Recipe;
        public static ConfigEntry<string> staffFire2RecipeUpgrade;
        public static ConfigEntry<int> staffFire2RecipeMultiplier;
        public static ConfigEntry<int> staffFire2MaxQuality;
        public static ConfigEntry<float> staffFire2MovementSpeed;
        public static ConfigEntry<float> staffFire2DamageBlunt;
        public static ConfigEntry<float> staffFire2DamageFire2;
        public static ConfigEntry<int> staffFire2BlockArmor;
        public static ConfigEntry<int> staffFire2DeflectionForce;
        public static ConfigEntry<int> staffFire2AttackForce;
        public static ConfigEntry<int> staffFire2UseEitr;
        public static ConfigEntry<int> staffFire2UseEitrSecondary;
        public static ConfigEntry<float> staffFire2SecondaryCooldown;

        // Fire3 Staff
        public static string sectionStaffFire3 = "07. Staff of Engulfing Flames";
        public static string staffFire3RecipeName = "Recipe_StaffFire3_DW";
        public static string staffFire3DefaultRecipe = "StaffFire2_DW:1, BlackMarble:20, BlackCore:10, Eitr:16";
        public static string staffFire3DefaultUpgradeRecipe = "BlackMarble:10, BlackCore:4, Eitr:8";

        public static ConfigEntry<bool> staffFire3Enable;
        public static ConfigEntry<string> staffFire3Name;
        public static ConfigEntry<string> staffFire3Description;
        public static ConfigEntry<string> staffFire3CraftingStation;
        public static ConfigEntry<int> staffFire3MinStationLevel;
        public static ConfigEntry<string> staffFire3Recipe;
        public static ConfigEntry<string> staffFire3RecipeUpgrade;
        public static ConfigEntry<int> staffFire3RecipeMultiplier;
        public static ConfigEntry<int> staffFire3MaxQuality;
        public static ConfigEntry<float> staffFire3MovementSpeed;
        public static ConfigEntry<float> staffFire3DamageBlunt;
        public static ConfigEntry<float> staffFire3DamageFire3;
        public static ConfigEntry<int> staffFire3BlockArmor;
        public static ConfigEntry<int> staffFire3DeflectionForce;
        public static ConfigEntry<int> staffFire3AttackForce;
        public static ConfigEntry<int> staffFire3UseEitr;
        public static ConfigEntry<int> staffFire3UseEitrSecondary;
        public static ConfigEntry<float> staffFire3SecondaryCooldown;

        // Frost1 Staff
        public static string sectionStaffFrost1 = "08. Staff of Ice";
        public static string staffFrost1RecipeName = "Recipe_StaffFrost1_DW";
        public static string staffFrost1DefaultRecipe = "FineWood:20, FreezeGland:10, FineEitr_DW:16";
        public static string staffFrost1DefaultUpgradeRecipe = "FineWood:10, FreezeGland:4, FineEitr_DW:8";

        public static ConfigEntry<bool> staffFrost1Enable;
        public static ConfigEntry<string> staffFrost1Name;
        public static ConfigEntry<string> staffFrost1Description;
        public static ConfigEntry<string> staffFrost1CraftingStation;
        public static ConfigEntry<int> staffFrost1MinStationLevel;
        public static ConfigEntry<string> staffFrost1Recipe;
        public static ConfigEntry<string> staffFrost1RecipeUpgrade;
        public static ConfigEntry<int> staffFrost1RecipeMultiplier;
        public static ConfigEntry<int> staffFrost1MaxQuality;
        public static ConfigEntry<float> staffFrost1MovementSpeed;
        public static ConfigEntry<float> staffFrost1DamageFrost;
        public static ConfigEntry<float> staffFrost1DamagePierce;
        public static ConfigEntry<int> staffFrost1BlockArmor;
        public static ConfigEntry<int> staffFrost1DeflectionForce;
        public static ConfigEntry<int> staffFrost1AttackForce;
        public static ConfigEntry<int> staffFrost1UseEitr;

        // Frost2 Staff
        public static string sectionStaffFrost2 = "09. Staff of Iceshards";
        public static string staffFrost2RecipeName = "Recipe_StaffFrost2_DW";
        public static string staffFrost2DefaultRecipe = "FineWood:20, FreezeGland:10, FineEitr_DW:16";
        public static string staffFrost2DefaultUpgradeRecipe = "FineWood:10, FreezeGland:4, FineEitr_DW:8";

        public static ConfigEntry<bool> staffFrost2Enable;
        public static ConfigEntry<string> staffFrost2Name;
        public static ConfigEntry<string> staffFrost2Description;
        public static ConfigEntry<string> staffFrost2CraftingStation;
        public static ConfigEntry<int> staffFrost2MinStationLevel;
        public static ConfigEntry<string> staffFrost2Recipe;
        public static ConfigEntry<string> staffFrost2RecipeUpgrade;
        public static ConfigEntry<int> staffFrost2RecipeMultiplier;
        public static ConfigEntry<int> staffFrost2MaxQuality;
        public static ConfigEntry<float> staffFrost2MovementSpeed;
        public static ConfigEntry<float> staffFrost2DamageFrost;
        public static ConfigEntry<float> staffFrost2DamagePierce;
        public static ConfigEntry<int> staffFrost2BlockArmor;
        public static ConfigEntry<int> staffFrost2DeflectionForce;
        public static ConfigEntry<int> staffFrost2AttackForce;
        public static ConfigEntry<int> staffFrost2UseEitr;
        public static ConfigEntry<int> staffFrost2UseEitrSecondary;
        public static ConfigEntry<float> staffFrost2SecondaryCooldown;

        // Frost3 Staff
        public static string sectionStaffFrost3 = "10. Staff of Frost";
        public static string staffFrost3RecipeName = "Recipe_StaffFrost3_DW";
        public static string staffFrost3DefaultRecipe = "YggdrasilWood:20, FreezeGland:10, Eitr:16";
        public static string staffFrost3DefaultUpgradeRecipe = "YggdrasilWood:10, FreezeGland:4, Eitr:8";

        public static ConfigEntry<bool> staffFrost3Enable;
        public static ConfigEntry<string> staffFrost3Name;
        public static ConfigEntry<string> staffFrost3Description;
        public static ConfigEntry<string> staffFrost3CraftingStation;
        public static ConfigEntry<int> staffFrost3MinStationLevel;
        public static ConfigEntry<string> staffFrost3Recipe;
        public static ConfigEntry<string> staffFrost3RecipeUpgrade;
        public static ConfigEntry<int> staffFrost3RecipeMultiplier;
        public static ConfigEntry<int> staffFrost3MaxQuality;
        public static ConfigEntry<float> staffFrost3MovementSpeed;
        public static ConfigEntry<float> staffFrost3DamageFrost;
        public static ConfigEntry<float> staffFrost3DamagePierce;
        public static ConfigEntry<int> staffFrost3BlockArmor;
        public static ConfigEntry<int> staffFrost3DeflectionForce;
        public static ConfigEntry<int> staffFrost3AttackForce;
        public static ConfigEntry<int> staffFrost3UseEitr;
        public static ConfigEntry<int> staffFrost3UseEitrSecondary;
        public static ConfigEntry<float> staffFrost3SecondaryCooldown;

        // Lightning3 Staff
        public static string sectionStaffLightning = "13. Staff of Lightning";
        public static string staffLightning3RecipeName = "Recipe_StaffLightning3_DW";
        public static string staffLightning3DefaultRecipe = "YggdrasilWood:20, Thunderstone:10, Eitr:16";
        public static string staffLightning3DefaultUpgradeRecipe = "YggdrasilWood:10, Thunderstone:4, Eitr:8";

        public static ConfigEntry<bool> staffLightning3Enable;
        public static ConfigEntry<string> staffLightning3Name;
        public static ConfigEntry<string> staffLightning3Description;
        public static ConfigEntry<string> staffLightning3CraftingStation;
        public static ConfigEntry<int> staffLightning3MinStationLevel;
        public static ConfigEntry<string> staffLightning3Recipe;
        public static ConfigEntry<string> staffLightning3RecipeUpgrade;
        public static ConfigEntry<int> staffLightning3RecipeMultiplier;
        public static ConfigEntry<int> staffLightning3MaxQuality;
        public static ConfigEntry<float> staffLightning3MovementSpeed;
        public static ConfigEntry<float> staffLightning3DamagePickaxe;
        public static ConfigEntry<float> staffLightning3DamagePierce;
        public static ConfigEntry<float> staffLightning3DamageLightning;
        public static ConfigEntry<int> staffLightning3BlockArmor;
        public static ConfigEntry<int> staffLightning3DeflectionForce;
        public static ConfigEntry<int> staffLightning3AttackForce;
        public static ConfigEntry<int> staffLightning3UseEitr;
        public static ConfigEntry<int> staffLightning3UseEitrSecondary;
        public static ConfigEntry<float> staffLightning3SecondaryCooldown;

        public static void Init()
        {
            InitStaffEarth1Config();
            InitStaffEarth2Config();
            InitStaffEarth3Config();
            InitStaffFire1Config();
            InitStaffFire2Config();
            InitStaffFire3Config();
            InitStaffFrost1Config();
            InitStaffFrost2Config();
            InitStaffFrost3Config();
            InitStaffLightning3Config();
        }

        private static void InitStaffEarth1Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffEarth1,
                    recipeName = staffEarth1RecipeName,
                    prefab = MagicExtended.Instance.staffEarth1Prefab,
                    enable = true,
                    name = "Staff of Stone",
                    description = "Insert cringy description about getting stoned or something",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipe = staffEarth1DefaultRecipe,
                    recipeUpgrade = staffEarth1DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 15f,
                    damageSpirit = 1f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffConfig.GenerateConfig();

                staffEarth1Enable = staffConfig.configEnable;
                staffEarth1Name = staffConfig.configName;
                staffEarth1Description = staffConfig.configDescription;
                staffEarth1CraftingStation = staffConfig.configCraftingStation;
                staffEarth1MinStationLevel = staffConfig.configMinStationLevel;
                staffEarth1Recipe = staffConfig.configRecipe;
                staffEarth1RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffEarth1RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffEarth1MaxQuality = staffConfig.configMaxQuality;
                staffEarth1MovementSpeed = staffConfig.configMovementSpeed;
                staffEarth1DamageBlunt = staffConfig.configDamageBlunt;
                staffEarth1DamageSpirit = staffConfig.configDamageSpirit;
                staffEarth1BlockArmor = staffConfig.configBlockArmor;
                staffEarth1DeflectionForce = staffConfig.configDeflectionForce;
                staffEarth1AttackForce = staffConfig.configAttackForce;
                staffEarth1UseEitr = staffConfig.configUseEitr;

                //ConfigFile Config = MagicExtended.Instance.Config;
                //GameObject prefab = MagicExtended.Instance.staffEarth1Prefab;

                //staffEarth1Enable = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Enable"), true,
                //   new ConfigDescription("Enable Staff of Stone", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1Enable.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.Enable,
                //        enable = staffEarth1Enable.Value,
                //    });
                //};

                //staffEarth1Name = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Name"), "Staff of Stone",
                //   new ConfigDescription("The name given to the item", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1Name.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        name = staffEarth1Name.Value,
                //    });
                //};

                //staffEarth1Description = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Description"), "The power of nature at your disposal!",
                //    new ConfigDescription("The description given to the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1Description.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        description = staffEarth1Description.Value,
                //    });
                //};

                //staffEarth1CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Crafting station"), "Workbench",
                //    new ConfigDescription("The crafting station the item can be created in",
                //    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1CraftingStation.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.CraftingStation,
                //        craftingStation = staffEarth1CraftingStation.Value,
                //    });
                //};

                //staffEarth1MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Required station level"), 3,
                //    new ConfigDescription("The required station level to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1MinStationLevel.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.MinRequiredStationLevel,
                //        requiredStationLevel = staffEarth1MinStationLevel.Value,
                //    });
                //};

                //staffEarth1Recipe = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Crafting costs"), staffEarth1DefaultRecipe,
                //    new ConfigDescription("The items required to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1Recipe.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth1Recipe.Value,
                //        upgradeRequirements = staffEarth1RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth1RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth1RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Upgrade costs"), staffEarth1DefaultUpgradeRecipe,
                //    new ConfigDescription("The costs to upgrade the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1RecipeUpgrade.SettingChanged += (obj, attr) =>
                //{
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth1Recipe.Value,
                //        upgradeRequirements = staffEarth1RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth1RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth1RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Upgrade multiplier"), 1,
                //    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1RecipeMultiplier.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth1RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth1Recipe.Value,
                //        upgradeRequirements = staffEarth1RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth1RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth1MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Max quality"), 4,
                //    new ConfigDescription("The maximum quality the item can become", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1MaxQuality.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        maxQuality = staffEarth1MaxQuality.Value,
                //    });
                //};

                //staffEarth1MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Movement speed"), -0.05f,
                //    new ConfigDescription("The movement speed stat on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1MovementSpeed.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        movementModifier = staffEarth1MovementSpeed.Value,
                //    });
                //};

                //staffEarth1DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Attack damage blunt"), 15f,
                //    new ConfigDescription("Blunt damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1DamageBlunt.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageBlunt = staffEarth1DamageBlunt.Value,
                //    });
                //};

                //staffEarth1DamageSpirit = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Attack damage spirit"), 1f,
                //    new ConfigDescription("Spirit damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1DamageSpirit.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageSpirit = staffEarth1DamageSpirit.Value,
                //    });
                //};

                //staffEarth1BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Block armor"), 48,
                //    new ConfigDescription("The block armor on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1BlockArmor.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        blockPower = staffEarth1BlockArmor.Value,
                //    });
                //};

                //staffEarth1DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Block force"), 20,
                //    new ConfigDescription("The block force on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1DeflectionForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        deflectionForce = staffEarth1DeflectionForce.Value,
                //    });
                //};

                //staffEarth1AttackForce = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Knockback"), 35,
                //    new ConfigDescription("The knockback on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1AttackForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackForce = staffEarth1AttackForce.Value,
                //    });
                //};

                //staffEarth1UseEitr = Config.Bind(new ConfigDefinition(sectionStaffEarth1, "Attack eitr cost"), 5,
                //    new ConfigDescription("Normal attack eitr cost", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth1UseEitr.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackEitr = staffEarth1UseEitr.Value,
                //    });
                //};
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Stone config: " + error);
            }
        }

        private static void InitStaffEarth2Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffEarth2,
                    recipeName = staffEarth2RecipeName,
                    prefab = MagicExtended.Instance.staffEarth2Prefab,
                    enable = true,
                    name = "Staff of Boulders",
                    description = "Something with bing stones",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipe = staffEarth2DefaultRecipe,
                    recipeUpgrade = staffEarth2DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 21f,
                    damageSpirit = 2f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                };
                staffConfig.GenerateConfig();

                staffEarth2Enable = staffConfig.configEnable;
                staffEarth2Name = staffConfig.configName;
                staffEarth2Description = staffConfig.configDescription;
                staffEarth2CraftingStation = staffConfig.configCraftingStation;
                staffEarth2MinStationLevel = staffConfig.configMinStationLevel;
                staffEarth2Recipe = staffConfig.configRecipe;
                staffEarth2RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffEarth2RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffEarth2MaxQuality = staffConfig.configMaxQuality;
                staffEarth2MovementSpeed = staffConfig.configMovementSpeed;
                staffEarth2DamageBlunt = staffConfig.configDamageBlunt;
                staffEarth2DamageSpirit = staffConfig.configDamageSpirit;
                staffEarth2BlockArmor = staffConfig.configBlockArmor;
                staffEarth2DeflectionForce = staffConfig.configDeflectionForce;
                staffEarth2AttackForce = staffConfig.configAttackForce;
                staffEarth2UseEitr = staffConfig.configUseEitr;
                staffEarth2UseEitrSecondary = staffConfig.configUseEitrSecondary;

                //ConfigFile Config = MagicExtended.Instance.Config;
                //GameObject prefab = MagicExtended.Instance.staffEarth2Prefab;

                //staffEarth2Enable = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Enable"), true,
                //   new ConfigDescription("Enable Staff of Boulder", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2Enable.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.Enable,
                //        enable = staffEarth2Enable.Value,
                //    });
                //};

                //staffEarth2Name = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Name"), "Staff of Boulders",
                //   new ConfigDescription("The name given to the item", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2Name.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        name = staffEarth2Name.Value,
                //    });
                //};

                //staffEarth2Description = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Description"), "The power of nature at your disposal!",
                //    new ConfigDescription("The description given to the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2Description.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        description = staffEarth2Description.Value,
                //    });
                //};

                //staffEarth2CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Crafting station"), "Workbench",
                //    new ConfigDescription("The crafting station the item can be created in",
                //    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2CraftingStation.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.CraftingStation,
                //        craftingStation = staffEarth2CraftingStation.Value,
                //    });
                //};

                //staffEarth2MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Required station level"), 5,
                //    new ConfigDescription("The required station level to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2MinStationLevel.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.MinRequiredStationLevel,
                //        requiredStationLevel = staffEarth2MinStationLevel.Value,
                //    });
                //};

                //staffEarth2Recipe = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Crafting costs"), staffEarth2DefaultRecipe,
                //    new ConfigDescription("The items required to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2Recipe.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth2Recipe.Value,
                //        upgradeRequirements = staffEarth2RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth2RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth2RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Upgrade costs"), staffEarth2DefaultUpgradeRecipe,
                //    new ConfigDescription("The costs to upgrade the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2RecipeUpgrade.SettingChanged += (obj, attr) =>
                //{
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth2Recipe.Value,
                //        upgradeRequirements = staffEarth2RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth2RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth2RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Upgrade multiplier"), 1,
                //    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2RecipeMultiplier.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth2RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth2Recipe.Value,
                //        upgradeRequirements = staffEarth2RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth2RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth2MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Max quality"), 4,
                //    new ConfigDescription("The maximum quality the item can become", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2MaxQuality.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        maxQuality = staffEarth2MaxQuality.Value,
                //    });
                //};

                //staffEarth2MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Movement speed"), -0.05f,
                //    new ConfigDescription("The movement speed stat on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2MovementSpeed.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        movementModifier = staffEarth2MovementSpeed.Value,
                //    });
                //};

                //staffEarth2DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Attack damage blunt"), 21f,
                //    new ConfigDescription("Blunt damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2DamageBlunt.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageBlunt = staffEarth2DamageBlunt.Value,
                //    });
                //};

                //staffEarth2DamageSpirit = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Attack damage spirit"), 2f,
                //    new ConfigDescription("Spirit damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2DamageSpirit.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageSpirit = staffEarth2DamageSpirit.Value,
                //    });
                //};

                //staffEarth2BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Block armor"), 48,
                //    new ConfigDescription("The block armor on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2BlockArmor.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        blockPower = staffEarth2BlockArmor.Value,
                //    });
                //};

                //staffEarth2DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Block force"), 20,
                //    new ConfigDescription("The block force on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2DeflectionForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        deflectionForce = staffEarth2DeflectionForce.Value,
                //    });
                //};

                //staffEarth2AttackForce = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Knockback"), 35,
                //    new ConfigDescription("The knockback on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2AttackForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackForce = staffEarth2AttackForce.Value,
                //    });
                //};

                //staffEarth2UseEitr = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Attack eitr cost"), 5,
                //    new ConfigDescription("Normal attack eitr cost", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2UseEitr.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackEitr = staffEarth2UseEitr.Value,
                //    });
                //};

                //staffEarth2UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Secondary ability eitr cost"), 100,
                //    new ConfigDescription("The secondary attack eitr cost", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2UseEitrSecondary.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        secondaryAttackEitr = staffEarth2UseEitrSecondary.Value,
                //    });
                //};

                //staffEarth2SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffBoulder, "Secondary cooldown"), 20f,
                //    new ConfigDescription("Cooldown duration of the secondary ability", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth2SecondaryCooldown.SettingChanged += (obj, attr) => {
                //    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffEarth2Cooldown_DW"));

                //    if (statusEffect != null)
                //    {
                //        statusEffect.m_ttl = staffEarth2SecondaryCooldown.Value;
                //    }
                //};
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Boulder config: " + error);
            }
        }

        private static void InitStaffEarth3Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffEarth3,
                    recipeName = staffEarth3RecipeName,
                    cooldownStatusEffectName = staffEarth3CooldownStatusEffectName,
                    prefab = MagicExtended.Instance.staffEarth3Prefab,
                    enable = true,
                    name = "Staff of Earth",
                    description = "Your foes will bend to natures will... or they will get bend!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffEarth3DefaultRecipe,
                    recipeUpgrade = staffEarth3DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 27f,
                    damageSpirit = 3f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffEarth3Enable = staffConfig.configEnable;
                staffEarth3Name = staffConfig.configName;
                staffEarth3Description = staffConfig.configDescription;
                staffEarth3CraftingStation = staffConfig.configCraftingStation;
                staffEarth3MinStationLevel = staffConfig.configMinStationLevel;
                staffEarth3Recipe = staffConfig.configRecipe;
                staffEarth3RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffEarth3RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffEarth3MaxQuality = staffConfig.configMaxQuality;
                staffEarth3MovementSpeed = staffConfig.configMovementSpeed;
                staffEarth3DamageBlunt = staffConfig.configDamageBlunt;
                staffEarth3DamageSpirit = staffConfig.configDamageSpirit;
                staffEarth3BlockArmor = staffConfig.configBlockArmor;
                staffEarth3DeflectionForce = staffConfig.configDeflectionForce;
                staffEarth3AttackForce = staffConfig.configAttackForce;
                staffEarth3UseEitr = staffConfig.configUseEitr;
                staffEarth3UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffEarth3SecondaryCooldown = staffConfig.configSecondaryCooldown;

                //ConfigFile Config = MagicExtended.Instance.Config;
                //GameObject prefab = MagicExtended.Instance.staffEarth3Prefab;

                //staffEarth3Enable = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Enable"), true,
                //   new ConfigDescription("Enable Staff of Earth", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3Enable.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.Enable,
                //        enable = staffEarth3Enable.Value,
                //    });
                //};

                //staffEarth3Name = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Name"), "Staff of Earth",
                //   new ConfigDescription("The name given to the item", null,
                //   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3Name.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        name = staffEarth3Name.Value,
                //    });
                //};

                //staffEarth3Description = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Description"), "The power of nature at your disposal!",
                //    new ConfigDescription("The description given to the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3Description.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        description = staffEarth3Description.Value,
                //    });
                //};

                //staffEarth3CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Crafting station"), "GaldrTable",
                //    new ConfigDescription("The crafting station the item can be created in",
                //    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3CraftingStation.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.CraftingStation,
                //        craftingStation = staffEarth3CraftingStation.Value,
                //    });
                //};

                //staffEarth3MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Required station level"), 1,
                //    new ConfigDescription("The required station level to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3MinStationLevel.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.MinRequiredStationLevel,
                //        requiredStationLevel = staffEarth3MinStationLevel.Value,
                //    });
                //};

                //staffEarth3Recipe = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Crafting costs"), staffEarth3DefaultRecipe,
                //    new ConfigDescription("The items required to craft", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3Recipe.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth3Recipe.Value,
                //        upgradeRequirements = staffEarth3RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth3RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth3RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Upgrade costs"), staffEarth3DefaultUpgradeRecipe,
                //    new ConfigDescription("The costs to upgrade the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3RecipeUpgrade.SettingChanged += (obj, attr) =>
                //{
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth3Recipe.Value,
                //        upgradeRequirements = staffEarth3RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth3RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth3RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Upgrade multiplier"), 1,
                //    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3RecipeMultiplier.SettingChanged += (obj, attr) => {
                //    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                //    {
                //        name = staffEarth3RecipeName,
                //        updateType = RecipeUpdateType.Recipe,
                //        requirements = staffEarth3Recipe.Value,
                //        upgradeRequirements = staffEarth3RecipeUpgrade.Value,
                //        upgradeMultiplier = staffEarth3RecipeMultiplier.Value,
                //    });
                //};

                //staffEarth3MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Max quality"), 4,
                //    new ConfigDescription("The maximum quality the item can become", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3MaxQuality.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        maxQuality = staffEarth3MaxQuality.Value,
                //    });
                //};

                //staffEarth3MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Movement speed"), -0.05f,
                //    new ConfigDescription("The movement speed stat on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3MovementSpeed.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        movementModifier = staffEarth3MovementSpeed.Value,
                //    });
                //};

                //staffEarth3DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Attack damage blunt"), 27f,
                //    new ConfigDescription("Blunt damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3DamageBlunt.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageBlunt = staffEarth3DamageBlunt.Value,
                //    });
                //};

                //staffEarth3DamageSpirit = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Attack damage spirit"), 3f,
                //    new ConfigDescription("Spirit damage on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3DamageSpirit.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        damageSpirit = staffEarth3DamageSpirit.Value,
                //    });
                //};

                //staffEarth3BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Block armor"), 48,
                //    new ConfigDescription("The block armor on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3BlockArmor.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        blockPower = staffEarth3BlockArmor.Value,
                //    });
                //};

                //staffEarth3DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Block force"), 20,
                //    new ConfigDescription("The block force on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3DeflectionForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        deflectionForce = staffEarth3DeflectionForce.Value,
                //    });
                //};

                //staffEarth3AttackForce = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Knockback"), 35,
                //    new ConfigDescription("The knockback on the item", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3AttackForce.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackForce = staffEarth3AttackForce.Value,
                //    });
                //};

                //staffEarth3UseEitr = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Attack eitr cost"), 5,
                //    new ConfigDescription("Normal attack eitr cost", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3UseEitr.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        attackEitr = staffEarth3UseEitr.Value,
                //    });
                //};

                //staffEarth3UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Secondary ability eitr cost"), 100,
                //    new ConfigDescription("The secondary attack eitr cost", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3UseEitrSecondary.SettingChanged += (obj, attr) => {
                //    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                //    {
                //        secondaryAttackEitr = staffEarth3UseEitrSecondary.Value,
                //    });
                //};

                //staffEarth3SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffEarth3, "Secondary cooldown"), 20f,
                //    new ConfigDescription("Cooldown duration of the secondary ability", null,
                //    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                //staffEarth3SecondaryCooldown.SettingChanged += (obj, attr) => {
                //    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffEarth3Cooldown_DW"));

                //    if (statusEffect != null)
                //    {
                //        statusEffect.m_ttl = staffEarth3SecondaryCooldown.Value;
                //    }
                //};
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Earth config: " + error);
            }
        }

        private static void InitStaffFire1Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffFire1Prefab;

                staffFire1Enable = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Enable"), true,
                   new ConfigDescription("Enable Staff of FireBall", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1Enable.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffFire1Enable.Value,
                    });
                };

                staffFire1Name = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Name"), "Staff of Fireball",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1Name.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffFire1Name.Value,
                    });
                };

                staffFire1Description = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Description"), "It is time for this world to BURN!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1Description.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffFire1Description.Value,
                    });
                };

                staffFire1CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1CraftingStation.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffFire1CraftingStation.Value,
                    });
                };

                staffFire1MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1MinStationLevel.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffFire1MinStationLevel.Value,
                    });
                };

                staffFire1Recipe = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Crafting costs"), staffFire1DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1Recipe.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire1Recipe.Value,
                        upgradeRequirements = staffFire1RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire1RecipeMultiplier.Value,
                    });
                };

                staffFire1RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Upgrade costs"), staffFire1DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire1Recipe.Value,
                        upgradeRequirements = staffFire1RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire1RecipeMultiplier.Value,
                    });
                };

                staffFire1RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1RecipeMultiplier.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire1RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire1Recipe.Value,
                        upgradeRequirements = staffFire1RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire1RecipeMultiplier.Value,
                    });
                };

                staffFire1MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1MaxQuality.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffFire1MaxQuality.Value,
                    });
                };

                staffFire1MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1MovementSpeed.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffFire1MovementSpeed.Value,
                    });
                };

                staffFire1DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Attack damage blunt"), 40f,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1DamageBlunt.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageBlunt = staffFire1DamageBlunt.Value,
                    });
                };

                staffFire1DamageFire = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Attack damage fire"), 40f,
                    new ConfigDescription("FireBall damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1DamageFire.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFire = staffFire1DamageFire.Value,
                    });
                };

                staffFire1BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1BlockArmor.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffFire1BlockArmor.Value,
                    });
                };

                staffFire1DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1DeflectionForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffFire1DeflectionForce.Value,
                    });
                };

                staffFire1AttackForce = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Knockback"), 35,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1AttackForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffFire1AttackForce.Value,
                    });
                };

                staffFire1UseEitr = Config.Bind(new ConfigDefinition(sectionStaffFireBall, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire1UseEitr.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffFire1UseEitr.Value,
                    });
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of FireBall config: " + error);
            }
        }

        private static void InitStaffFire2Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffFire2Prefab;

                staffFire2Enable = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Enable"), true,
                   new ConfigDescription("Enable Staff of Fire2", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2Enable.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffFire2Enable.Value,
                    });
                };

                staffFire2Name = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Name"), "Staff of Fire",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2Name.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffFire2Name.Value,
                    });
                };

                staffFire2Description = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Description"), "It is time for this world to BURN!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2Description.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffFire2Description.Value,
                    });
                };

                staffFire2CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2CraftingStation.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffFire2CraftingStation.Value,
                    });
                };

                staffFire2MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Required station level"), 4,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2MinStationLevel.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffFire2MinStationLevel.Value,
                    });
                };

                staffFire2Recipe = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Crafting costs"), staffFire2DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2Recipe.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire2Recipe.Value,
                        upgradeRequirements = staffFire2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire2RecipeMultiplier.Value,
                    });
                };

                staffFire2RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Upgrade costs"), staffFire2DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire2Recipe.Value,
                        upgradeRequirements = staffFire2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire2RecipeMultiplier.Value,
                    });
                };

                staffFire2RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2RecipeMultiplier.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire2Recipe.Value,
                        upgradeRequirements = staffFire2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire2RecipeMultiplier.Value,
                    });
                };

                staffFire2MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2MaxQuality.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffFire2MaxQuality.Value,
                    });
                };

                staffFire2MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2MovementSpeed.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffFire2MovementSpeed.Value,
                    });
                };

                staffFire2DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Attack damage blunt"), 80f,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2DamageBlunt.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageBlunt = staffFire2DamageBlunt.Value,
                    });
                };

                staffFire2DamageFire2 = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Attack damage fire"), 80f,
                    new ConfigDescription("Fire2 damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2DamageFire2.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFire = staffFire2DamageFire2.Value,
                    });
                };

                staffFire2BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2BlockArmor.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffFire2BlockArmor.Value,
                    });
                };

                staffFire2DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2DeflectionForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffFire2DeflectionForce.Value,
                    });
                };

                staffFire2AttackForce = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Knockback"), 35,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2AttackForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffFire2AttackForce.Value,
                    });
                };

                staffFire2UseEitr = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2UseEitr.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffFire2UseEitr.Value,
                    });
                };

                staffFire2UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Secondary ability eitr cost"), 100,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2UseEitrSecondary.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = staffFire2UseEitrSecondary.Value,
                    });
                };

                staffFire2SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffFire2, "Secondary cooldown"), 20f,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire2SecondaryCooldown.SettingChanged += (obj, attr) => {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffFire2Cooldown_DW"));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = staffFire2SecondaryCooldown.Value;
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Fire2 config: " + error);
            }
        }

        private static void InitStaffFire3Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffFire3Prefab;

                staffFire3Enable = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Enable"), true,
                   new ConfigDescription("Enable Staff of Fire3", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3Enable.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffFire3Enable.Value,
                    });
                };

                staffFire3Name = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Name"), "Staff of Engulfing Flames",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3Name.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffFire3Name.Value,
                    });
                };

                staffFire3Description = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Description"), "It is time for this world to BURN!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3Description.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffFire3Description.Value,
                    });
                };

                staffFire3CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Crafting station"), "GaldrTable",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3CraftingStation.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffFire3CraftingStation.Value,
                    });
                };

                staffFire3MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3MinStationLevel.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffFire3MinStationLevel.Value,
                    });
                };

                staffFire3Recipe = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Crafting costs"), staffFire3DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3Recipe.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire3Recipe.Value,
                        upgradeRequirements = staffFire3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire3RecipeMultiplier.Value,
                    });
                };

                staffFire3RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Upgrade costs"), staffFire3DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire3Recipe.Value,
                        upgradeRequirements = staffFire3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire3RecipeMultiplier.Value,
                    });
                };

                staffFire3RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3RecipeMultiplier.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFire3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFire3Recipe.Value,
                        upgradeRequirements = staffFire3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFire3RecipeMultiplier.Value,
                    });
                };

                staffFire3MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3MaxQuality.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffFire3MaxQuality.Value,
                    });
                };

                staffFire3MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3MovementSpeed.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffFire3MovementSpeed.Value,
                    });
                };

                staffFire3DamageBlunt = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Attack damage blunt"), 120f,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3DamageBlunt.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageBlunt = staffFire3DamageBlunt.Value,
                    });
                };

                staffFire3DamageFire3 = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Attack damage fire"), 120f,
                    new ConfigDescription("Fire3 damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3DamageFire3.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFire = staffFire3DamageFire3.Value,
                    });
                };

                staffFire3BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3BlockArmor.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffFire3BlockArmor.Value,
                    });
                };

                staffFire3DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3DeflectionForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffFire3DeflectionForce.Value,
                    });
                };

                staffFire3AttackForce = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Knockback"), 35,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3AttackForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffFire3AttackForce.Value,
                    });
                };

                staffFire3UseEitr = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3UseEitr.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffFire3UseEitr.Value,
                    });
                };

                staffFire3UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Secondary ability eitr cost"), 100,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3UseEitrSecondary.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = staffFire3UseEitrSecondary.Value,
                    });
                };

                staffFire3SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffFire3, "Secondary cooldown"), 20f,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFire3SecondaryCooldown.SettingChanged += (obj, attr) => {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffFire3Cooldown_DW"));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = staffFire3SecondaryCooldown.Value;
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Engulfing Flames config: " + error);
            }
        }

        private static void InitStaffLightning3Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffLightning3Prefab;

                staffLightning3Enable = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Enable"), true,
                   new ConfigDescription("Enable Staff of Lightning", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3Enable.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffLightning3Enable.Value,
                    });
                };

                staffLightning3Name = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Name"), "Staff of Lightning",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3Name.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffLightning3Name.Value,
                    });
                };

                staffLightning3Description = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Description"), "STATIC!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3Description.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffLightning3Description.Value,
                    });
                };

                staffLightning3CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Crafting station"), "GaldrTable",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3CraftingStation.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffLightning3CraftingStation.Value,
                    });
                };

                staffLightning3MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3MinStationLevel.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffLightning3MinStationLevel.Value,
                    });
                };

                staffLightning3Recipe = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Crafting costs"), staffLightning3DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3Recipe.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffLightning3Recipe.Value,
                        upgradeRequirements = staffLightning3RecipeUpgrade.Value,
                        upgradeMultiplier = staffLightning3RecipeMultiplier.Value,
                    });
                };

                staffLightning3RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Upgrade costs"), staffLightning3DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffLightning3Recipe.Value,
                        upgradeRequirements = staffLightning3RecipeUpgrade.Value,
                        upgradeMultiplier = staffLightning3RecipeMultiplier.Value,
                    });
                };

                staffLightning3RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3RecipeMultiplier.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffLightning3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffLightning3Recipe.Value,
                        upgradeRequirements = staffLightning3RecipeUpgrade.Value,
                        upgradeMultiplier = staffLightning3RecipeMultiplier.Value,
                    });
                };

                staffLightning3MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3MaxQuality.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffLightning3MaxQuality.Value,
                    });
                };

                staffLightning3MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3MovementSpeed.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffLightning3MovementSpeed.Value,
                    });
                };

                staffLightning3DamagePickaxe = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Attack damage pickaxe"), 30f,
                    new ConfigDescription("Pickaxe damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3DamagePickaxe.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePickaxe = staffLightning3DamagePickaxe.Value,
                    });
                };
                
                staffLightning3DamagePierce = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Attack damage pierce"), 120f,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3DamagePierce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePierce = staffLightning3DamagePierce.Value,
                    });
                };

                staffLightning3DamageLightning = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Attack damage lightning"), 120f,
                    new ConfigDescription("Lightning damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3DamageLightning.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageLightning = staffLightning3DamageLightning.Value,
                    });
                };

                staffLightning3BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3BlockArmor.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffLightning3BlockArmor.Value,
                    });
                };

                staffLightning3DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3DeflectionForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffLightning3DeflectionForce.Value,
                    });
                };

                staffLightning3AttackForce = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Knockback"), 35,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3AttackForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffLightning3AttackForce.Value,
                    });
                };

                staffLightning3UseEitr = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3UseEitr.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffLightning3UseEitr.Value,
                    });
                };

                staffLightning3UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Secondary ability eitr cost"), 100,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3UseEitrSecondary.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = staffLightning3UseEitrSecondary.Value,
                    });
                };

                staffLightning3SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffLightning, "Secondary cooldown"), 20f,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffLightning3SecondaryCooldown.SettingChanged += (obj, attr) => {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffLightning3Cooldown_DW"));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = staffLightning3SecondaryCooldown.Value;
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Lightning config: " + error);
            }
        }

        private static void InitStaffFrost1Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFrost1,
                    recipeName = staffFrost1RecipeName,
                    prefab = MagicExtended.Instance.staffFrost1Prefab,
                    enable = true,
                    name = "Staff of Ice",
                    description = "Icy!",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipe = staffFrost1DefaultRecipe,
                    recipeUpgrade = staffFrost1DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 15f,
                    damagePierce = 1f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffConfig.GenerateConfig();

                staffFrost1Enable = staffConfig.configEnable;
                staffFrost1Name = staffConfig.configName;
                staffFrost1Description = staffConfig.configDescription;
                staffFrost1CraftingStation = staffConfig.configCraftingStation;
                staffFrost1MinStationLevel = staffConfig.configMinStationLevel;
                staffFrost1Recipe = staffConfig.configRecipe;
                staffFrost1RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFrost1RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFrost1MaxQuality = staffConfig.configMaxQuality;
                staffFrost1MovementSpeed = staffConfig.configMovementSpeed;
                staffFrost1DamageFrost = staffConfig.configDamageFrost;
                staffFrost1DamagePierce = staffConfig.configDamagePierce;
                staffFrost1BlockArmor = staffConfig.configBlockArmor;
                staffFrost1DeflectionForce = staffConfig.configDeflectionForce;
                staffFrost1AttackForce = staffConfig.configAttackForce;
                staffFrost1UseEitr = staffConfig.configUseEitr;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Stone config: " + error);
            }
        }

        private static void InitStaffFrost2Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffFrost2Prefab;

                staffFrost2Enable = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Enable"), true,
                   new ConfigDescription("Enable Staff of Iceshards", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2Enable.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffFrost2Enable.Value,
                    });
                };

                staffFrost2Name = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Name"), "Staff of Iceshards",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2Name.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffFrost2Name.Value,
                    });
                };

                staffFrost2Description = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Description"), "Frreeezzzze!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2Description.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffFrost2Description.Value,
                    });
                };

                staffFrost2CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Crafting station"), "GaldrTable",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2CraftingStation.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffFrost2CraftingStation.Value,
                    });
                };

                staffFrost2MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2MinStationLevel.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffFrost2MinStationLevel.Value,
                    });
                };

                staffFrost2Recipe = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Crafting costs"), staffFrost2DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2Recipe.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost2Recipe.Value,
                        upgradeRequirements = staffFrost2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost2RecipeMultiplier.Value,
                    });
                };

                staffFrost2RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Upgrade costs"), staffFrost2DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost2Recipe.Value,
                        upgradeRequirements = staffFrost2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost2RecipeMultiplier.Value,
                    });
                };

                staffFrost2RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2RecipeMultiplier.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost2RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost2Recipe.Value,
                        upgradeRequirements = staffFrost2RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost2RecipeMultiplier.Value,
                    });
                };

                staffFrost2MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2MaxQuality.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffFrost2MaxQuality.Value,
                    });
                };

                staffFrost2MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2MovementSpeed.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffFrost2MovementSpeed.Value,
                    });
                };

                staffFrost2DamagePierce = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Attack damage pierce"), 6f,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2DamagePierce.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePierce = staffFrost2DamagePierce.Value,
                    });
                };

                staffFrost2DamageFrost = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Attack damage frost"), 18f,
                    new ConfigDescription("Frost damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2DamageFrost.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFrost = staffFrost2DamageFrost.Value,
                    });
                };

                staffFrost2BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2BlockArmor.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffFrost2BlockArmor.Value,
                    });
                };

                staffFrost2DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2DeflectionForce.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffFrost2DeflectionForce.Value,
                    });
                };

                staffFrost2AttackForce = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Knockback"), 25,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2AttackForce.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffFrost2AttackForce.Value,
                    });
                };

                staffFrost2UseEitr = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2UseEitr.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffFrost2UseEitr.Value,
                    });
                };

                staffFrost2UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Secondary ability eitr cost"), 100,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2UseEitrSecondary.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = staffFrost2UseEitrSecondary.Value,
                    });
                };

                staffFrost2SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffFrost2, "Secondary cooldown"), 20f,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost2SecondaryCooldown.SettingChanged += (obj, attr) =>
                {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffFrost2Cooldown_DW"));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = staffFrost2SecondaryCooldown.Value;
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Iceshards config: " + error);
            }
        }

        private static void InitStaffFrost3Config()
        {
            try
            {
                ConfigFile Config = MagicExtended.Instance.Config;
                GameObject prefab = MagicExtended.Instance.staffFrost3Prefab;

                staffFrost3Enable = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Enable"), true,
                   new ConfigDescription("Enable Staff of Frost", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3Enable.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.Enable,
                        enable = staffFrost3Enable.Value,
                    });
                };

                staffFrost3Name = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Name"), "Staff of Frost",
                   new ConfigDescription("The name given to the item", null,
                   new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3Name.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        name = staffFrost3Name.Value,
                    });
                };

                staffFrost3Description = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Description"), "Frreeezzzze!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3Description.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        description = staffFrost3Description.Value,
                    });
                };

                staffFrost3CraftingStation = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Crafting station"), "GaldrTable",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3CraftingStation.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.CraftingStation,
                        craftingStation = staffFrost3CraftingStation.Value,
                    });
                };

                staffFrost3MinStationLevel = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3MinStationLevel.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.MinRequiredStationLevel,
                        requiredStationLevel = staffFrost3MinStationLevel.Value,
                    });
                };

                staffFrost3Recipe = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Crafting costs"), staffFrost3DefaultRecipe,
                    new ConfigDescription("The items required to craft", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3Recipe.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost3Recipe.Value,
                        upgradeRequirements = staffFrost3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost3RecipeMultiplier.Value,
                    });
                };

                staffFrost3RecipeUpgrade = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Upgrade costs"), staffFrost3DefaultUpgradeRecipe,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3RecipeUpgrade.SettingChanged += (obj, attr) =>
                {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost3Recipe.Value,
                        upgradeRequirements = staffFrost3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost3RecipeMultiplier.Value,
                    });
                };

                staffFrost3RecipeMultiplier = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3RecipeMultiplier.SettingChanged += (obj, attr) => {
                    RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                    {
                        name = staffFrost3RecipeName,
                        updateType = RecipeUpdateType.Recipe,
                        requirements = staffFrost3Recipe.Value,
                        upgradeRequirements = staffFrost3RecipeUpgrade.Value,
                        upgradeMultiplier = staffFrost3RecipeMultiplier.Value,
                    });
                };

                staffFrost3MaxQuality = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3MaxQuality.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        maxQuality = staffFrost3MaxQuality.Value,
                    });
                };

                staffFrost3MovementSpeed = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3MovementSpeed.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        movementModifier = staffFrost3MovementSpeed.Value,
                    });
                };

                staffFrost3DamagePierce = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Attack damage pierce"), 10f,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3DamagePierce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePierce = staffFrost3DamagePierce.Value,
                    });
                };

                staffFrost3DamageFrost = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Attack damage frost"), 27f,
                    new ConfigDescription("Frost damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3DamageFrost.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFrost = staffFrost3DamageFrost.Value,
                    });
                };

                staffFrost3BlockArmor = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3BlockArmor.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        blockPower = staffFrost3BlockArmor.Value,
                    });
                };

                staffFrost3DeflectionForce = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3DeflectionForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        deflectionForce = staffFrost3DeflectionForce.Value,
                    });
                };

                staffFrost3AttackForce = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Knockback"), 35,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3AttackForce.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackForce = staffFrost3AttackForce.Value,
                    });
                };

                staffFrost3UseEitr = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Attack eitr cost"), 5,
                    new ConfigDescription("Normal attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3UseEitr.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        attackEitr = staffFrost3UseEitr.Value,
                    });
                };

                staffFrost3UseEitrSecondary = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Secondary ability eitr cost"), 100,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3UseEitrSecondary.SettingChanged += (obj, attr) => {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = staffFrost3UseEitrSecondary.Value,
                    });
                };

                staffFrost3SecondaryCooldown = Config.Bind(new ConfigDefinition(sectionStaffFrost3, "Secondary cooldown"), 20f,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                staffFrost3SecondaryCooldown.SettingChanged += (obj, attr) => {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffFrost3Cooldown_DW"));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = staffFrost3SecondaryCooldown.Value;
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Frost config: " + error);
            }


        }
    }
}
