using BepInEx.Configuration;
using MagicExtended.Models;
using System;

namespace MagicExtended.Configs
{
    internal static class ConfigStaffs
    {
        // Eart0 staff
        public static string sectionStaffEarth0 = "02. Staff of Mushrooms";
        public static string staffEarth0RecipeName = "Recipe_StaffEarth0_DW";
        public static string staffEarth0DefaultRecipe = "RoundLog:20, Stone:20, Resin:20, CrudeEitr_DW:16";
        public static string staffEarth0DefaultUpgradeRecipe = "RoundLog:10, Stone:10, CrudeEitr_DW:8";

        public static ConfigEntry<bool> staffEarth0Enable;
        public static ConfigEntry<string> staffEarth0Name;
        public static ConfigEntry<string> staffEarth0Description;
        public static ConfigEntry<string> staffEarth0CraftingStation;
        public static ConfigEntry<int> staffEarth0MinStationLevel;
        public static ConfigEntry<string> staffEarth0Recipe;
        public static ConfigEntry<string> staffEarth0RecipeUpgrade;
        public static ConfigEntry<int> staffEarth0RecipeMultiplier;
        public static ConfigEntry<int> staffEarth0MaxQuality;
        public static ConfigEntry<float> staffEarth0MovementSpeed;
        public static ConfigEntry<float> staffEarth0DamageBlunt;
        public static ConfigEntry<float> staffEarth0DamageSpirit;
        public static ConfigEntry<int> staffEarth0BlockArmor;
        public static ConfigEntry<int> staffEarth0DeflectionForce;
        public static ConfigEntry<int> staffEarth0AttackForce;
        public static ConfigEntry<int> staffEarth0UseEitr;

        // Eart1 staff
        public static string sectionStaffEarth1 = "03. Staff of Stone";
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
        public static string sectionStaffEarth2 = "04. Staff of Boulders";
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
        public static string sectionStaffEarth3 = "05. Staff of Earth";
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
        public static string sectionStaffFire1 = "06. Staff of Fireball";
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
        public static string sectionStaffFire2 = "07. Staff of Fire";
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
        public static ConfigEntry<float> staffFire2DamageFire;
        public static ConfigEntry<int> staffFire2BlockArmor;
        public static ConfigEntry<int> staffFire2DeflectionForce;
        public static ConfigEntry<int> staffFire2AttackForce;
        public static ConfigEntry<int> staffFire2UseEitr;
        public static ConfigEntry<int> staffFire2UseEitrSecondary;
        public static ConfigEntry<float> staffFire2SecondaryCooldown;

        // Fire3 Staff
        public static string sectionStaffFire3 = "08. Staff of Engulfing Flames";
        public static string staffFire3RecipeName = "Recipe_StaffFire3_DW";
        public static string staffFire3DefaultRecipe = "StaffFire2_DW:1, BlackMarble:20, BlackCore:10, Eitr:16";
        public static string staffFire3DefaultUpgradeRecipe = "BlackMarble:10, BlackCore:4, Eitr:8";
        public static string staffFire3CooldownStatusEffectName = "StaffFire3Cooldown_DW";

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
        public static ConfigEntry<float> staffFire3DamageFire;
        public static ConfigEntry<int> staffFire3BlockArmor;
        public static ConfigEntry<int> staffFire3DeflectionForce;
        public static ConfigEntry<int> staffFire3AttackForce;
        public static ConfigEntry<int> staffFire3UseEitr;
        public static ConfigEntry<int> staffFire3UseEitrSecondary;
        public static ConfigEntry<float> staffFire3SecondaryCooldown;

        // Frost1 Staff
        public static string sectionStaffFrost1 = "09. Staff of Ice";
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
        public static string sectionStaffFrost2 = "10. Staff of Iceshards";
        public static string staffFrost2RecipeName = "Recipe_StaffFrost2_DW";
        public static string staffFrost2DefaultRecipe = "FineWood:20, FreezeGland:10, FineEitr_DW:16";
        public static string staffFrost2DefaultUpgradeRecipe = "FineWood:10, FreezeGland:4, FineEitr_DW:8";
        public static string staffFrost2CooldownStatusEffectName = "StaffFrost2Cooldown_DW";

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
        public static string sectionStaffFrost3 = "11. Staff of Frost";
        public static string staffFrost3RecipeName = "Recipe_StaffFrost3_DW";
        public static string staffFrost3DefaultRecipe = "YggdrasilWood:20, FreezeGland:10, Eitr:16";
        public static string staffFrost3DefaultUpgradeRecipe = "YggdrasilWood:10, FreezeGland:4, Eitr:8";
        public static string staffFrost3CooldownStatusEffectName = "StaffFrost3Cooldown_DW";

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

        // Lightning1 Staff
        public static string sectionStaffLightning1 = "12. Staff of Eikthyr";
        public static string staffLightning1RecipeName = "Recipe_StaffLightning1_DW";
        public static string staffLightning1DefaultRecipe = "YggdrasilWood:20, Thunderstone:10, Eitr:16";
        public static string staffLightning1DefaultUpgradeRecipe = "YggdrasilWood:10, Thunderstone:4, Eitr:8";

        public static ConfigEntry<bool> staffLightning1Enable;
        public static ConfigEntry<string> staffLightning1Name;
        public static ConfigEntry<string> staffLightning1Description;
        public static ConfigEntry<string> staffLightning1CraftingStation;
        public static ConfigEntry<int> staffLightning1MinStationLevel;
        public static ConfigEntry<string> staffLightning1Recipe;
        public static ConfigEntry<string> staffLightning1RecipeUpgrade;
        public static ConfigEntry<int> staffLightning1RecipeMultiplier;
        public static ConfigEntry<int> staffLightning1MaxQuality;
        public static ConfigEntry<float> staffLightning1MovementSpeed;
        public static ConfigEntry<float> staffLightning1DamagePickaxe;
        public static ConfigEntry<float> staffLightning1DamagePierce;
        public static ConfigEntry<float> staffLightning1DamageLightning;
        public static ConfigEntry<int> staffLightning1BlockArmor;
        public static ConfigEntry<int> staffLightning1DeflectionForce;
        public static ConfigEntry<int> staffLightning1AttackForce;
        public static ConfigEntry<int> staffLightning1UseEitr;

        // Lightning2 Staff
        public static string sectionStaffLightning2 = "13. Staff of Sparcs";
        public static string staffLightning2RecipeName = "Recipe_StaffLightning2_DW";
        public static string staffLightning2DefaultRecipe = "YggdrasilWood:20, Thunderstone:10, Eitr:16";
        public static string staffLightning2DefaultUpgradeRecipe = "YggdrasilWood:10, Thunderstone:4, Eitr:8";
        public static string staffLightning2CooldownStatusEffectName = "StaffLightning2Cooldown_DW";

        public static ConfigEntry<bool> staffLightning2Enable;
        public static ConfigEntry<string> staffLightning2Name;
        public static ConfigEntry<string> staffLightning2Description;
        public static ConfigEntry<string> staffLightning2CraftingStation;
        public static ConfigEntry<int> staffLightning2MinStationLevel;
        public static ConfigEntry<string> staffLightning2Recipe;
        public static ConfigEntry<string> staffLightning2RecipeUpgrade;
        public static ConfigEntry<int> staffLightning2RecipeMultiplier;
        public static ConfigEntry<int> staffLightning2MaxQuality;
        public static ConfigEntry<float> staffLightning2MovementSpeed;
        public static ConfigEntry<float> staffLightning2DamagePickaxe;
        public static ConfigEntry<float> staffLightning2DamagePierce;
        public static ConfigEntry<float> staffLightning2DamageLightning;
        public static ConfigEntry<int> staffLightning2BlockArmor;
        public static ConfigEntry<int> staffLightning2DeflectionForce;
        public static ConfigEntry<int> staffLightning2AttackForce;
        public static ConfigEntry<int> staffLightning2UseEitr;
        public static ConfigEntry<int> staffLightning2UseEitrSecondary;
        public static ConfigEntry<float> staffLightning2SecondaryCooldown;

        // Lightning3 Staff
        public static string sectionStaffLightning3 = "14. Staff of Lightning";
        public static string staffLightning3RecipeName = "Recipe_StaffLightning3_DW";
        public static string staffLightning3DefaultRecipe = "YggdrasilWood:20, Thunderstone:10, Eitr:16";
        public static string staffLightning3DefaultUpgradeRecipe = "YggdrasilWood:10, Thunderstone:4, Eitr:8";
        public static string staffLightning3CooldownStatusEffectName = "StaffLightning3Cooldown_DW";

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
            InitStaffEarth0Config();
            InitStaffEarth1Config();
            InitStaffEarth2Config();
            InitStaffEarth3Config();
            InitStaffFire1Config();
            InitStaffFire2Config();
            InitStaffFire3Config();
            InitStaffFrost1Config();
            InitStaffFrost2Config();
            InitStaffFrost3Config();
            InitStaffLightning1Config();
            InitStaffLightning2Config();
            InitStaffLightning3Config();
        }

        private static void InitStaffEarth0Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffEarth0,
                    recipeName = staffEarth0RecipeName,
                    prefab = MagicExtended.Instance.staffEarth0Prefab,
                    enable = true,
                    name = "Staff of Mushrooms",
                    description = "You feel weird holding this staff... its as if everything seems different but you can put your finger on it. Finger! Haha!",
                    craftingStation = "Workbench",
                    minStationLevel = 1,
                    recipe = staffEarth0DefaultRecipe,
                    recipeUpgrade = staffEarth0DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 20f,
                    damageSpirit = 6f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 10,
                };
                staffConfig.GenerateConfig();

                staffEarth0Enable = staffConfig.configEnable;
                staffEarth0Name = staffConfig.configName;
                staffEarth0Description = staffConfig.configDescription;
                staffEarth0CraftingStation = staffConfig.configCraftingStation;
                staffEarth0MinStationLevel = staffConfig.configMinStationLevel;
                staffEarth0Recipe = staffConfig.configRecipe;
                staffEarth0RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffEarth0RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffEarth0MaxQuality = staffConfig.configMaxQuality;
                staffEarth0MovementSpeed = staffConfig.configMovementSpeed;
                staffEarth0DamageBlunt = staffConfig.configDamageBlunt;
                staffEarth0DamageSpirit = staffConfig.configDamageSpirit;
                staffEarth0BlockArmor = staffConfig.configBlockArmor;
                staffEarth0DeflectionForce = staffConfig.configDeflectionForce;
                staffEarth0AttackForce = staffConfig.configAttackForce;
                staffEarth0UseEitr = staffConfig.configUseEitr;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Stone config: " + error);
            }
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
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFire1,
                    recipeName = staffFire1RecipeName,
                    prefab = MagicExtended.Instance.staffFire1Prefab,
                    enable = true,
                    name = "Staff of Fireball",
                    description = "It is time for this world to BURN!",
                    craftingStation = "Forge",
                    minStationLevel = 1,
                    recipe = staffFire1DefaultRecipe,
                    recipeUpgrade = staffFire1DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 40f,
                    damageFire = 40f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffConfig.GenerateConfig();

                staffFire1Enable = staffConfig.configEnable;
                staffFire1Name = staffConfig.configName;
                staffFire1Description = staffConfig.configDescription;
                staffFire1CraftingStation = staffConfig.configCraftingStation;
                staffFire1MinStationLevel = staffConfig.configMinStationLevel;
                staffFire1Recipe = staffConfig.configRecipe;
                staffFire1RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFire1RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFire1MaxQuality = staffConfig.configMaxQuality;
                staffFire1MovementSpeed = staffConfig.configMovementSpeed;
                staffFire1DamageBlunt = staffConfig.configDamageBlunt;
                staffFire1DamageFire = staffConfig.configDamageFire;
                staffFire1BlockArmor = staffConfig.configBlockArmor;
                staffFire1DeflectionForce = staffConfig.configDeflectionForce;
                staffFire1AttackForce = staffConfig.configAttackForce;
                staffFire1UseEitr = staffConfig.configUseEitr;
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
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFire2,
                    recipeName = staffFire2RecipeName,
                    prefab = MagicExtended.Instance.staffFire2Prefab,
                    enable = true,
                    name = "Staff of Fire",
                    description = "Some other description",
                    craftingStation = "Forge",
                    minStationLevel = 4,
                    recipe = staffFire2DefaultRecipe,
                    recipeUpgrade = staffFire2DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 80f,
                    damageFire = 80f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                };
                staffConfig.GenerateConfig();

                staffFire2Enable = staffConfig.configEnable;
                staffFire2Name = staffConfig.configName;
                staffFire2Description = staffConfig.configDescription;
                staffFire2CraftingStation = staffConfig.configCraftingStation;
                staffFire2MinStationLevel = staffConfig.configMinStationLevel;
                staffFire2Recipe = staffConfig.configRecipe;
                staffFire2RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFire2RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFire2MaxQuality = staffConfig.configMaxQuality;
                staffFire2MovementSpeed = staffConfig.configMovementSpeed;
                staffFire2DamageBlunt = staffConfig.configDamageBlunt;
                staffFire2DamageFire = staffConfig.configDamageFire;
                staffFire2BlockArmor = staffConfig.configBlockArmor;
                staffFire2DeflectionForce = staffConfig.configDeflectionForce;
                staffFire2AttackForce = staffConfig.configAttackForce;
                staffFire2UseEitr = staffConfig.configUseEitr;
                staffFire2UseEitrSecondary = staffConfig.configUseEitrSecondary;
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
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFire3,
                    recipeName = staffFire3RecipeName,
                    cooldownStatusEffectName = staffFire3CooldownStatusEffectName,
                    prefab = MagicExtended.Instance.staffFire3Prefab,
                    enable = true,
                    name = "Staff of Engulfing Flames",
                    description = "It is time for this world to burn to ashes!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffFire3DefaultRecipe,
                    recipeUpgrade = staffFire3DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 120f,
                    damageFire = 120f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffFire3Enable = staffConfig.configEnable;
                staffFire3Name = staffConfig.configName;
                staffFire3Description = staffConfig.configDescription;
                staffFire3CraftingStation = staffConfig.configCraftingStation;
                staffFire3MinStationLevel = staffConfig.configMinStationLevel;
                staffFire3Recipe = staffConfig.configRecipe;
                staffFire3RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFire3RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFire3MaxQuality = staffConfig.configMaxQuality;
                staffFire3MovementSpeed = staffConfig.configMovementSpeed;
                staffFire3DamageBlunt = staffConfig.configDamageBlunt;
                staffFire3DamageFire = staffConfig.configDamageFire;
                staffFire3BlockArmor = staffConfig.configBlockArmor;
                staffFire3DeflectionForce = staffConfig.configDeflectionForce;
                staffFire3AttackForce = staffConfig.configAttackForce;
                staffFire3UseEitr = staffConfig.configUseEitr;
                staffFire3UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffFire3SecondaryCooldown = staffConfig.configSecondaryCooldown;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Engulfing Flames config: " + error);
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
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFrost2,
                    recipeName = staffFrost2RecipeName,
                    prefab = MagicExtended.Instance.staffFrost2Prefab,
                    enable = true,
                    name = "Staff of Iceshards",
                    description = "Frreeezzzze!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffFrost2DefaultRecipe,
                    recipeUpgrade = staffFrost2DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 17f,
                    damagePierce = 6f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffFrost2Enable = staffConfig.configEnable;
                staffFrost2Name = staffConfig.configName;
                staffFrost2Description = staffConfig.configDescription;
                staffFrost2CraftingStation = staffConfig.configCraftingStation;
                staffFrost2MinStationLevel = staffConfig.configMinStationLevel;
                staffFrost2Recipe = staffConfig.configRecipe;
                staffFrost2RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFrost2RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFrost2MaxQuality = staffConfig.configMaxQuality;
                staffFrost2MovementSpeed = staffConfig.configMovementSpeed;
                staffFrost2DamageFrost = staffConfig.configDamageFrost;
                staffFrost2DamagePierce = staffConfig.configDamagePierce;
                staffFrost2BlockArmor = staffConfig.configBlockArmor;
                staffFrost2DeflectionForce = staffConfig.configDeflectionForce;
                staffFrost2AttackForce = staffConfig.configAttackForce;
                staffFrost2UseEitr = staffConfig.configUseEitr;
                staffFrost2UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffFrost2SecondaryCooldown = staffConfig.configSecondaryCooldown;
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
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffFrost3,
                    recipeName = staffFrost3RecipeName,
                    cooldownStatusEffectName = staffFrost3CooldownStatusEffectName,
                    prefab = MagicExtended.Instance.staffFrost3Prefab,
                    enable = true,
                    name = "Staff of Frost",
                    description = "Frreeezzzze!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffFrost3DefaultRecipe,
                    recipeUpgrade = staffFrost3DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageFrost = 20f,
                    damagePierce = 10f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffFrost3Enable = staffConfig.configEnable;
                staffFrost3Name = staffConfig.configName;
                staffFrost3Description = staffConfig.configDescription;
                staffFrost3CraftingStation = staffConfig.configCraftingStation;
                staffFrost3MinStationLevel = staffConfig.configMinStationLevel;
                staffFrost3Recipe = staffConfig.configRecipe;
                staffFrost3RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffFrost3RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffFrost3MaxQuality = staffConfig.configMaxQuality;
                staffFrost3MovementSpeed = staffConfig.configMovementSpeed;
                staffFrost3DamageFrost = staffConfig.configDamageFrost;
                staffFrost3DamagePierce = staffConfig.configDamagePierce;
                staffFrost3BlockArmor = staffConfig.configBlockArmor;
                staffFrost3DeflectionForce = staffConfig.configDeflectionForce;
                staffFrost3AttackForce = staffConfig.configAttackForce;
                staffFrost3UseEitr = staffConfig.configUseEitr;
                staffFrost3UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffFrost3SecondaryCooldown = staffConfig.configSecondaryCooldown;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Frost config: " + error);
            }
        }

        private static void InitStaffLightning1Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffLightning1,
                    recipeName = staffLightning1RecipeName,
                    prefab = MagicExtended.Instance.staffLightning1Prefab,
                    enable = true,
                    name = "Staff of Eikthyr",
                    description = "STATIC!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffLightning1DefaultRecipe,
                    recipeUpgrade = staffLightning1DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 40f,
                    damagePickaxe = 30f,
                    damagePierce = 40f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffConfig.GenerateConfig();

                staffLightning1Enable = staffConfig.configEnable;
                staffLightning1Name = staffConfig.configName;
                staffLightning1Description = staffConfig.configDescription;
                staffLightning1CraftingStation = staffConfig.configCraftingStation;
                staffLightning1MinStationLevel = staffConfig.configMinStationLevel;
                staffLightning1Recipe = staffConfig.configRecipe;
                staffLightning1RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffLightning1RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffLightning1MaxQuality = staffConfig.configMaxQuality;
                staffLightning1MovementSpeed = staffConfig.configMovementSpeed;
                staffLightning1DamageLightning = staffConfig.configDamageLightning;
                staffLightning1DamagePickaxe = staffConfig.configDamagePickaxe;
                staffLightning1DamagePierce = staffConfig.configDamagePierce;
                staffLightning1BlockArmor = staffConfig.configBlockArmor;
                staffLightning1DeflectionForce = staffConfig.configDeflectionForce;
                staffLightning1AttackForce = staffConfig.configAttackForce;
                staffLightning1UseEitr = staffConfig.configUseEitr;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Sparcs config: " + error);
            }
        }

        private static void InitStaffLightning2Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffLightning2,
                    recipeName = staffLightning2RecipeName,
                    cooldownStatusEffectName = staffLightning2CooldownStatusEffectName,
                    prefab = MagicExtended.Instance.staffLightning2Prefab,
                    enable = true,
                    name = "Staff of Sparcs",
                    description = "STATIC!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffLightning2DefaultRecipe,
                    recipeUpgrade = staffLightning2DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 80f,
                    damagePickaxe = 30f,
                    damagePierce = 80f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffLightning2Enable = staffConfig.configEnable;
                staffLightning2Name = staffConfig.configName;
                staffLightning2Description = staffConfig.configDescription;
                staffLightning2CraftingStation = staffConfig.configCraftingStation;
                staffLightning2MinStationLevel = staffConfig.configMinStationLevel;
                staffLightning2Recipe = staffConfig.configRecipe;
                staffLightning2RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffLightning2RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffLightning2MaxQuality = staffConfig.configMaxQuality;
                staffLightning2MovementSpeed = staffConfig.configMovementSpeed;
                staffLightning2DamageLightning = staffConfig.configDamageLightning;
                staffLightning2DamagePickaxe = staffConfig.configDamagePickaxe;
                staffLightning2DamagePierce = staffConfig.configDamagePierce;
                staffLightning2BlockArmor = staffConfig.configBlockArmor;
                staffLightning2DeflectionForce = staffConfig.configDeflectionForce;
                staffLightning2AttackForce = staffConfig.configAttackForce;
                staffLightning2UseEitr = staffConfig.configUseEitr;
                staffLightning2UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffLightning2SecondaryCooldown = staffConfig.configSecondaryCooldown;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Sparcs config: " + error);
            }
        }

        private static void InitStaffLightning3Config()
        {
            try
            {
                StaffConfig staffConfig = new StaffConfig()
                {
                    sectionName = sectionStaffLightning3,
                    recipeName = staffLightning3RecipeName,
                    cooldownStatusEffectName = staffLightning3CooldownStatusEffectName,
                    prefab = MagicExtended.Instance.staffLightning3Prefab,
                    enable = true,
                    name = "Staff of Lightning",
                    description = "STATIC!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipe = staffLightning3DefaultRecipe,
                    recipeUpgrade = staffLightning3DefaultUpgradeRecipe,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageLightning = 120f,
                    damagePickaxe = 30f,
                    damagePierce = 120f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                    useEitrSecondary = 100,
                    secondaryCooldown = 20f,
                };
                staffConfig.GenerateConfig();

                staffLightning3Enable = staffConfig.configEnable;
                staffLightning3Name = staffConfig.configName;
                staffLightning3Description = staffConfig.configDescription;
                staffLightning3CraftingStation = staffConfig.configCraftingStation;
                staffLightning3MinStationLevel = staffConfig.configMinStationLevel;
                staffLightning3Recipe = staffConfig.configRecipe;
                staffLightning3RecipeUpgrade = staffConfig.configRecipeUpgrade;
                staffLightning3RecipeMultiplier = staffConfig.configRecipeMultiplier;
                staffLightning3MaxQuality = staffConfig.configMaxQuality;
                staffLightning3MovementSpeed = staffConfig.configMovementSpeed;
                staffLightning3DamageLightning = staffConfig.configDamageLightning;
                staffLightning3DamagePickaxe = staffConfig.configDamagePickaxe;
                staffLightning3DamagePierce = staffConfig.configDamagePierce;
                staffLightning3BlockArmor = staffConfig.configBlockArmor;
                staffLightning3DeflectionForce = staffConfig.configDeflectionForce;
                staffLightning3AttackForce = staffConfig.configAttackForce;
                staffLightning3UseEitr = staffConfig.configUseEitr;
                staffLightning3UseEitrSecondary = staffConfig.configUseEitrSecondary;
                staffLightning3SecondaryCooldown = staffConfig.configSecondaryCooldown;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise Staff of Lightning config: " + error);
            }
        }
    }
}
