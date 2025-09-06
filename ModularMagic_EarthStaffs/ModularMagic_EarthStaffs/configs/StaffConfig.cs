using BepInEx.Configuration;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;

namespace ModularMagic_EarthStaffs.Configs
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
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageBluntPerLevel;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damageChopPerLevel;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePickaxePerLevel;
        public ConfigEntry<float> damagePoison;
        public ConfigEntry<float> damagePoisonPerLevel;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<float> damageSpiritPerLevel;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;
        public ConfigEntry<float> projectileBurst;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> maxDurability;
        public ConfigEntry<int> maxQuality;
        public ConfigEntry<float> movementSpeed;
        public ConfigEntry<int> blockArmor;
        public ConfigEntry<int> deflectionForce;
        public ConfigEntry<int> attackForce;

        // Other
        private int entryCount = 100;

        public void GenerateConfig(StaffConfigOptions options)
        {
            ConfigFile Config = ModularMagic_EarthStaffs.Instance.Config;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Wether the recipe for this item is enabled", null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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
              new ConfigDescription("The name of the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be crafted in",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = craftingStation.Value,
                });
            };

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level"), options.minStationLevel,
                new ConfigDescription("The required station level to craft this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = minStationLevel.Value,
                });
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to craft this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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

            recipeUpgrade = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade recipe"), options.recipeUpgrade,
                new ConfigDescription("The items required to upgrade this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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

            recipeMultiplier = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade recipe multiplier"), options.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
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

            damageBlunt = Config.Bind(new ConfigDefinition(options.sectionName, "Blunt damage"), options.damageBlunt,
                new ConfigDescription("Blunt damage on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageBlunt.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    damageBlunt = damageBlunt.Value,
                });
            };

            damageBluntPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Blunt damage per level"), options.damageBluntPerLevel,
                new ConfigDescription("Blunt damage per upgrade level on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageBluntPerLevel.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    damageBluntPerLevel = damageBluntPerLevel.Value,
                });
            };

            if (options.damageChop != null)
            {
                damageChop = Config.Bind(new ConfigDefinition(options.sectionName, "Chop damage"), (float)options.damageChop,
                    new ConfigDescription("Chop damage on the item (not visible ingame, effects wood cutting damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageChop.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageChop = damageChop.Value,
                    });
                };

                damageChopPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Chop damage per level"), (float)options.damageChopPerLevel,
                    new ConfigDescription("Chop damage per upgrade level on the item (not visible ingame, effects wood cutting damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageChopPerLevel.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageChopPerLevel = damageChopPerLevel.Value,
                    });
                };
            }

            if (options.damagePickaxe != null)
            {
                damagePickaxe = Config.Bind(new ConfigDefinition(options.sectionName, "Pickaxe damage"), (float)options.damagePickaxe,
                    new ConfigDescription("Pickaxe damage on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePickaxe.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePickaxe = damagePickaxe.Value,
                    });
                };

                damagePickaxePerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Pickaxe damage per level"), (float)options.damagePickaxePerLevel,
                    new ConfigDescription("Pickaxe damage per upgrade level on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePickaxePerLevel.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePickaxePerLevel = damagePickaxePerLevel.Value,
                    });
                };
            }

            if (options.damagePoison != null)
            {
                damagePoison = Config.Bind(new ConfigDefinition(options.sectionName, "Poison damage"), (float)options.damagePoison,
                    new ConfigDescription("Poison damage on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePoison.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePoison = damagePoison.Value,
                    });
                };

                damagePoisonPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Poison damage per level"), (float)options.damagePoisonPerLevel,
                    new ConfigDescription("Poison damage per upgrade level on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePoisonPerLevel.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePoisonPerLevel = damagePoisonPerLevel.Value,
                    });
                };
            }

            damageSpirit = Config.Bind(new ConfigDefinition(options.sectionName, "Spirit damage"), options.damageSpirit,
                new ConfigDescription("Spirit damage on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageSpirit.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    damageSpirit = damageSpirit.Value,
                });
            };

            damageSpiritPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Spirit damage per level"), options.damageSpiritPerLevel,
                new ConfigDescription("Spirit damage per upgrade level on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageSpiritPerLevel.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    damageSpiritPerLevel = damageSpiritPerLevel.Value,
                });
            };

            useEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Attack eitr cost"), options.useEitr,
                new ConfigDescription("Normal attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            useEitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    attackEitr = useEitr.Value,
                });
            };

            projectileVelocity = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile velocity"), options.projectileVelocity,
                new ConfigDescription("The velocity of the projectiles emitted from this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            projectileVelocity.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    projectileVelocity = projectileVelocity.Value,
                });
            };

            projectileAccuracy = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile accuracy"), options.projectileAccuracy,
                new ConfigDescription("The accuracy (spread) of the projectiles emitted from this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            projectileAccuracy.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    projectileAccuracy = projectileAccuracy.Value,
                });
            };

            if (options.projectileBurst > -1)
            {
                projectileBurst = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile burst"), (float)options.projectileBurst,
                    new ConfigDescription("The burst (attack speed) of the projectiles emitted from this item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                projectileBurst.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        projectileBurst = projectileBurst.Value,
                    });
                };
            }

            weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), options.weight,
               new ConfigDescription("The weight of the item", null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    weight = weight.Value,
                });
            };

            maxDurability = Config.Bind(new ConfigDefinition(options.sectionName, "Max durability"), options.maxDurability,
                new ConfigDescription("The maximum durability of the item", null,
                 new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            maxDurability.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    maxDurability = maxDurability.Value,
                });
            };

            maxQuality = Config.Bind(new ConfigDefinition(options.sectionName, "Max quality"), (int)options.maxQuality,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            maxQuality.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    maxQuality = maxQuality.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), (float)options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            movementSpeed.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    movementModifier = movementSpeed.Value,
                });
            };

            blockArmor = Config.Bind(new ConfigDefinition(options.sectionName, "Block armor"), (int)options.blockArmor,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            blockArmor.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    blockPower = blockArmor.Value,
                });
            };

            deflectionForce = Config.Bind(new ConfigDefinition(options.sectionName, "Block force"), (int)options.deflectionForce,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            deflectionForce.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    deflectionForce = deflectionForce.Value,
                });
            };

            attackForce = Config.Bind(new ConfigDefinition(options.sectionName, "Knockback"), (int)options.attackForce,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            attackForce.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    attackForce = attackForce.Value,
                });
            };
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
