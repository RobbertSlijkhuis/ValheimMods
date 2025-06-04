using BepInEx.Configuration;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Models;

namespace ModularMagic_IceStaffs.configs
{
    internal class StaffConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<string> recipeUpgrade;
        public ConfigEntry<int> recipeMultiplier;
        public ConfigEntry<int> maxQuality;
        public ConfigEntry<float> movementSpeed;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<int> blockArmor;
        public ConfigEntry<int> deflectionForce;
        public ConfigEntry<int> attackForce;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<int> useEitrSecondary;
        public ConfigEntry<float> secondaryCooldown;

        public void GenerateConfig(StaffConfigOptions options)
        {
            ConfigFile Config = ModularMagic_IceStaffs.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Enable", null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = enable.Value,
                });
            };

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = craftingStation.Value,
                });
            };

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), options.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = minStationLevel.Value,
                });
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            recipeUpgrade = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade costs"), options.recipeUpgrade,
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipeUpgrade.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            recipeMultiplier = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade multiplier"), options.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipeMultiplier.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            maxQuality = Config.Bind(new ConfigDefinition(options.sectionName, "Max quality"), options.maxQuality,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            maxQuality.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxQuality = maxQuality.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            movementSpeed.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    movementSpeed = movementSpeed.Value,
                });
            };

            damageFrost = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage frost"), (float)options.damageFrost,
                new ConfigDescription("Frost damage on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            damageFrost.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    damageFrost = damageFrost.Value,
                });
            };

            damagePierce = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage pierce"), (float)options.damagePierce,
                new ConfigDescription("Pierce damage on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            damagePierce.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    damagePierce = damagePierce.Value,
                });
            };

            blockArmor = Config.Bind(new ConfigDefinition(options.sectionName, "Block armor"), (int)options.blockArmor,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            blockArmor.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    blockPower = blockArmor.Value,
                });
            };

            deflectionForce = Config.Bind(new ConfigDefinition(options.sectionName, "Block force"), (int)options.deflectionForce,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            deflectionForce.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    deflectionForce = deflectionForce.Value,
                });
            };

            attackForce = Config.Bind(new ConfigDefinition(options.sectionName, "Knockback"), (int)options.attackForce,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            attackForce.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    attackForce = attackForce.Value,
                });
            };

            useEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Attack eitr cost"), (int)options.useEitr,
                new ConfigDescription("Normal attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            useEitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    attackEitr = useEitr.Value,
                });
            };

            if (options.useEitrSecondary > -1)
            {
                useEitrSecondary = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary ability eitr cost"), (int)options.useEitrSecondary,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                useEitrSecondary.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        secondaryAttackEitr = useEitrSecondary.Value,
                    });
                };
            }

            if (options.secondaryCooldown > -1 && options.cooldownStatusEffectName != "")
            {
                secondaryCooldown = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary cooldown"), (float)options.secondaryCooldown,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                secondaryCooldown.SettingChanged += (obj, attr) =>
                {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(options.cooldownStatusEffectName.GetStableHashCode());

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = secondaryCooldown.Value;
                    }
                };
            }
        }
    }
}
