using BepInEx.Configuration;
using MagicExtended.Configs;
using MagicExtended.Helpers;
using UnityEngine;

namespace MagicExtended.Models
{
    internal class StaffConfig
    {
        // Required variables
        public string sectionName;
        public string recipeName;
        public string cooldownStatusEffectName = "";
        public GameObject prefab;

        // Default values
        public bool enable;
        public string name;
        public string description;
        public string craftingStation;
        public int minStationLevel;
        public string recipe;
        public string recipeUpgrade;
        public int recipeMultiplier;
        public int maxQuality;
        public float movementSpeed;
        public float damageBlunt = -1;
        public float damageChop = -1;
        public float damageFire = -1;
        public float damageFrost = -1;
        public float damageLightning = -1;
        public float damagePickaxe = -1;
        public float damagePierce = -1;
        public float damageSlash = -1;
        public float damageSpirit = -1;
        public int blockArmor;
        public int deflectionForce;
        public int attackForce;
        public int useEitr;
        public int useEitrSecondary = -1;
        public float secondaryCooldown = -1;

        // The config fields to generate
        public ConfigEntry<bool> configEnable;
        public ConfigEntry<string> configName;
        public ConfigEntry<string> configDescription;
        public ConfigEntry<string> configCraftingStation;
        public ConfigEntry<int> configMinStationLevel;
        public ConfigEntry<string> configRecipe;
        public ConfigEntry<string> configRecipeUpgrade;
        public ConfigEntry<int> configRecipeMultiplier;
        public ConfigEntry<int> configMaxQuality;
        public ConfigEntry<float> configMovementSpeed;
        public ConfigEntry<float> configDamageBlunt;
        public ConfigEntry<float> configDamageChop;
        public ConfigEntry<float> configDamageFire;
        public ConfigEntry<float> configDamageFrost;
        public ConfigEntry<float> configDamageLightning;
        public ConfigEntry<float> configDamagePickaxe;
        public ConfigEntry<float> configDamagePierce;
        public ConfigEntry<float> configDamageSlash;
        public ConfigEntry<float> configDamageSpirit;
        public ConfigEntry<int> configBlockArmor;
        public ConfigEntry<int> configDeflectionForce;
        public ConfigEntry<int> configAttackForce;
        public ConfigEntry<int> configUseEitr;
        public ConfigEntry<int> configUseEitrSecondary;
        public ConfigEntry<float> configSecondaryCooldown;

        public void GenerateConfig()
        {
            ConfigFile Config = MagicExtended.Instance.Config;

            this.configEnable = Config.Bind(new ConfigDefinition(this.sectionName, "Enable"), this.enable,
               new ConfigDescription("Enable " + this.name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configEnable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = this.configEnable.Value,
                });
            };

            this.configName = Config.Bind(new ConfigDefinition(this.sectionName, "Name"), this.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configName.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    name = this.configName.Value,
                });
            };

            this.configDescription = Config.Bind(new ConfigDefinition(this.sectionName, "Description"), this.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configDescription.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    description = this.configDescription.Value,
                });
            }; ;

            this.configCraftingStation = Config.Bind(new ConfigDefinition(this.sectionName, "Crafting station"), this.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(ConfigPlugin.craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configCraftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.CraftingStation,
                    craftingStation = this.configCraftingStation.Value,
                });
            };

            this.configMinStationLevel = Config.Bind(new ConfigDefinition(this.sectionName, "Required station level to craft"), this.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configMinStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.MinRequiredStationLevel,
                    requiredStationLevel = this.configMinStationLevel.Value,
                });
            };

            this.configRecipe = Config.Bind(new ConfigDefinition(this.sectionName, "Crafting costs"), this.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configRecipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Recipe,
                    requirements = this.configRecipe.Value,
                    upgradeRequirements = this.configRecipeUpgrade.Value,
                    upgradeMultiplier = this.configRecipeMultiplier.Value,
                });
            };

            this.configRecipeUpgrade = Config.Bind(new ConfigDefinition(this.sectionName, "Upgrade costs"), this.recipeUpgrade,
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configRecipeUpgrade.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Recipe,
                    requirements = this.configRecipe.Value,
                    upgradeRequirements = this.configRecipeUpgrade.Value,
                    upgradeMultiplier = this.configRecipeMultiplier.Value,
                });
            };

            this.configRecipeMultiplier = Config.Bind(new ConfigDefinition(this.sectionName, "Upgrade multiplier"), this.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.configRecipeMultiplier.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = this.recipeName,
                    updateType = RecipeUpdateType.Recipe,
                    requirements = this.configRecipe.Value,
                    upgradeRequirements = this.configRecipeUpgrade.Value,
                    upgradeMultiplier = this.configRecipeMultiplier.Value,
                });
            };

            configMaxQuality = Config.Bind(new ConfigDefinition(sectionName, "Max quality"), this.maxQuality,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configMaxQuality.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    maxQuality = configMaxQuality.Value,
                });
            };

            configMovementSpeed = Config.Bind(new ConfigDefinition(sectionName, "Movement speed"), this.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configMovementSpeed.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    movementModifier = configMovementSpeed.Value,
                });
            };

            if (this.damageBlunt > -1)
            {
                configDamageBlunt = Config.Bind(new ConfigDefinition(sectionName, "Attack damage blunt"), this.damageBlunt,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamageBlunt.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageBlunt = configDamageBlunt.Value,
                    });
                };
            }

            if (this.damageFire > -1)
            {
                configDamageFire = Config.Bind(new ConfigDefinition(sectionName, "Attack damage fire"), this.damageFire,
                    new ConfigDescription("Fire damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamageFire.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFire = configDamageFire.Value,
                    });
                };
            }


            if (this.damageFrost > -1) {
                configDamageFrost = Config.Bind(new ConfigDefinition(sectionName, "Attack damage frost"), this.damageFrost,
                    new ConfigDescription("Frost damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamageFrost.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageFrost = configDamageFrost.Value,
                    });
                };
            }

            if (this.damageLightning > -1) {
                configDamageLightning = Config.Bind(new ConfigDefinition(sectionName, "Attack damage lightning"), this.damageLightning,
                    new ConfigDescription("Lightning damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamageLightning.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageLightning = configDamageLightning.Value,
                    });
                };
            }

            if (this.damagePickaxe > -1) {
                configDamagePickaxe = Config.Bind(new ConfigDefinition(sectionName, "Attack damage pickaxe"), this.damagePickaxe,
                    new ConfigDescription("Pickaxe damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                        configDamagePickaxe.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePickaxe = configDamagePickaxe.Value,
                    });
                };
            }

            if (this.damagePierce > -1) {
                configDamagePierce = Config.Bind(new ConfigDefinition(sectionName, "Attack damage pierce"), this.damagePierce,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamagePierce.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damagePierce = configDamagePierce.Value,
                    });
                };
            }

            if (this.damageSlash > -1)
            {
                configDamageSlash = Config.Bind(new ConfigDefinition(sectionName, "Attack damage slash"), this.damageSlash,
                    new ConfigDescription("Slash damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                        configDamageSlash.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageSlash = configDamageSlash.Value,
                    });
                };
            }

            if (this.damageSpirit > -1)
            {
                configDamageSpirit = Config.Bind(new ConfigDefinition(sectionName, "Attack damage spirit"), this.damageSpirit,
                    new ConfigDescription("Spirit damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDamageSpirit.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        damageSpirit = configDamageSpirit.Value,
                    });
                };
            }

            configBlockArmor = Config.Bind(new ConfigDefinition(sectionName, "Block armor"), this.blockArmor,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configBlockArmor.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    blockPower = configBlockArmor.Value,
                });
            };

            configDeflectionForce = Config.Bind(new ConfigDefinition(sectionName, "Block force"), this.deflectionForce,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configDeflectionForce.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    deflectionForce = configDeflectionForce.Value,
                });
            };

            configAttackForce = Config.Bind(new ConfigDefinition(sectionName, "Knockback"), this.attackForce,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configAttackForce.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    attackForce = configAttackForce.Value,
                });
            };

            configUseEitr = Config.Bind(new ConfigDefinition(sectionName, "Attack eitr cost"), this.useEitr,
                new ConfigDescription("Normal attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configUseEitr.SettingChanged += (obj, attr) => {
                ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                {
                    attackEitr = configUseEitr.Value,
                });
            };

            if (this.useEitrSecondary > -1)
            {
                configUseEitrSecondary = Config.Bind(new ConfigDefinition(sectionName, "Secondary ability eitr cost"), this.useEitrSecondary,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configUseEitrSecondary.SettingChanged += (obj, attr) =>
                {
                    ConfigHelper.PatchStats(prefab, new PatchStatsOptions()
                    {
                        secondaryAttackEitr = configUseEitrSecondary.Value,
                    });
                };
            }

            if (this.secondaryCooldown > -1 && cooldownStatusEffectName != "")
            {
                configSecondaryCooldown = Config.Bind(new ConfigDefinition(sectionName, "Secondary cooldown"), this.secondaryCooldown,
                    new ConfigDescription("Cooldown duration of the secondary ability", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configSecondaryCooldown.SettingChanged += (obj, attr) =>
                {
                    StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode(cooldownStatusEffectName));

                    if (statusEffect != null)
                    {
                        statusEffect.m_ttl = configSecondaryCooldown.Value;
                    }
                };
            }
        }
    }
}
