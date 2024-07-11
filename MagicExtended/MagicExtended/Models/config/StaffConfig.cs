using BepInEx.Configuration;
using MagicExtended.Configs;
using MagicExtended.Helpers;

namespace MagicExtended.Models
{
    internal class StaffConfig
    {
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
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damageFire;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> damageLightning;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<float> damageSlash;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<int> blockArmor;
        public ConfigEntry<int> deflectionForce;
        public ConfigEntry<int> attackForce;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<int> useEitrSecondary;
        public ConfigEntry<float> secondaryCooldown;

        public void GenerateConfig(StaffConfigOptions options)
        {
            ConfigFile Config = MagicExtended.Instance.Config;

            this.enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), (bool)options.enable,
               new ConfigDescription("Enable " + this.name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = this.enable.Value,
                });
            };

            this.name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.name.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = this.name.Value,
                });
            };

            this.description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.description.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = this.description.Value,
                });
            }; ;

            this.craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = this.craftingStation.Value,
                });
            };

            this.minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), (int)options.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = this.minStationLevel.Value,
                });
            };

            this.recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = this.recipe.Value,
                    upgradeRequirements = this.recipeUpgrade.Value,
                    upgradeMultiplier = this.recipeMultiplier.Value,
                });
            };

            this.recipeUpgrade = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade costs"), options.recipeUpgrade,
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.recipeUpgrade.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = this.recipe.Value,
                    upgradeRequirements = this.recipeUpgrade.Value,
                    upgradeMultiplier = this.recipeMultiplier.Value,
                });
            };

            this.recipeMultiplier = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade multiplier"), (int)options.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.recipeMultiplier.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = this.recipe.Value,
                    upgradeRequirements = this.recipeUpgrade.Value,
                    upgradeMultiplier = this.recipeMultiplier.Value,
                });
            };

            maxQuality = Config.Bind(new ConfigDefinition(options.sectionName, "Max quality"), (int)options.maxQuality,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            maxQuality.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxQuality = maxQuality.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), (float)options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            movementSpeed.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    movementModifier = movementSpeed.Value,
                });
            };

            if (options.damageBlunt > -1)
            {
                damageBlunt = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage blunt"), (float)options.damageBlunt,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damageBlunt.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageBlunt = damageBlunt.Value,
                    });
                };
            }

            if (options.damageFire > -1)
            {
                damageFire = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage fire"), (float)options.damageFire,
                    new ConfigDescription("Fire damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damageFire.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageFire = damageFire.Value,
                    });
                };
            }


            if (options.damageFrost > -1) {
                damageFrost = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage frost"), (float)options.damageFrost,
                    new ConfigDescription("Frost damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damageFrost.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageFrost = damageFrost.Value,
                    });
                };
            }

            if (options.damageLightning > -1) {
                damageLightning = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage lightning"), (float)options.damageLightning,
                    new ConfigDescription("Lightning damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damageLightning.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageLightning = damageLightning.Value,
                    });
                };
            }

            if (options.damagePickaxe > -1) {
                damagePickaxe = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage pickaxe"), (float)options.damagePickaxe,
                    new ConfigDescription("Pickaxe damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                        damagePickaxe.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damagePickaxe = damagePickaxe.Value,
                    });
                };
            }

            if (options.damagePierce > -1) {
                damagePierce = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage pierce"), (float)options.damagePierce,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damagePierce.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damagePierce = damagePierce.Value,
                    });
                };
            }

            if (options.damageSlash > -1)
            {
                damageSlash = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage slash"), (float)options.damageSlash,
                    new ConfigDescription("Slash damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                        damageSlash.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageSlash = damageSlash.Value,
                    });
                };
            }

            if (options.damageSpirit > -1)
            {
                damageSpirit = Config.Bind(new ConfigDefinition(options.sectionName, "Attack damage spirit"), (float)options.damageSpirit,
                    new ConfigDescription("Spirit damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                damageSpirit.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                    {
                        damageSpirit = damageSpirit.Value,
                    });
                };
            }

            blockArmor = Config.Bind(new ConfigDefinition(options.sectionName, "Block armor"), (int)options.blockArmor,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            blockArmor.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    blockPower = blockArmor.Value,
                });
            };

            deflectionForce = Config.Bind(new ConfigDefinition(options.sectionName, "Block force"), (int)options.deflectionForce,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            deflectionForce.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    deflectionForce = deflectionForce.Value,
                });
            };

            attackForce = Config.Bind(new ConfigDefinition(options.sectionName, "Knockback"), (int)options.attackForce,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            attackForce.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    attackForce = attackForce.Value,
                });
            };

            useEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Attack eitr cost"), (int)options.useEitr,
                new ConfigDescription("Normal attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            useEitr.SettingChanged += (obj, attr) => {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
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
                    ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
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
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode(options.cooldownStatusEffectName));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = secondaryCooldown.Value;
                    }
                };
            }
        }
    }
}
