using BepInEx.Configuration;
using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
using ModularMagic_BloodMagic.Types;

namespace ModularMagic_BloodMagic.Configs
{
    internal class WeaponConfig
    {
        public static string[] craftingStationOptions = new string[] {
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter,
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

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
        public ConfigEntry<float> damageFire;
        public ConfigEntry<float> damageFirePerLevel;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> damageFrostPerLevel;
        public ConfigEntry<float> damageGeneral;
        public ConfigEntry<float> damageGeneralPerLevel;
        public ConfigEntry<float> damageLightning;
        public ConfigEntry<float> damageLightningPerLevel;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePickaxePerLevel;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<float> damagePiercePerLevel;
        public ConfigEntry<float> damagePoison;
        public ConfigEntry<float> damagePoisonPerLevel;
        public ConfigEntry<float> damageSlash;
        public ConfigEntry<float> damageSlashPerLevel;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<float> damageSpiritPerLevel;

        public ConfigEntry<int> attackEitr;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;
        public ConfigEntry<float> projectileBurst;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> maxDurability;
        public ConfigEntry<float> movementSpeed;
        public ConfigEntry<int> blockArmor;
        public ConfigEntry<int> deflectionForce;
        public ConfigEntry<int> attackForce;

        private int entryCount = 100;

        public void GenerateConfig(WeaponConfigOptions options)
        {
            ConfigFile Config = ModularMagic_BloodMagic.Instance.Config;

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
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            description.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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

            if (options.damageBlunt != null)
            {
                damageBlunt = Config.Bind(new ConfigDefinition(options.sectionName, "Blunt damage"), (float)options.damageBlunt,
                    new ConfigDescription("Blunt damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageBlunt.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageBlunt = damageBlunt.Value,
                    });
                };

                damageBluntPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Blunt damage per level"), (float)options.damageBluntPerLevel,
                    new ConfigDescription("Blunt damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageBluntPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageBluntPerLevel = damageBluntPerLevel.Value,
                    });
                };
            }

            if (options.damageChop != null)
            {
                damageChop = Config.Bind(new ConfigDefinition(options.sectionName, "Chop damage"), (float)options.damageChop,
                    new ConfigDescription("Chop damage on the item (not visible ingame, effects wood cutting damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageChop.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageChop = damageChop.Value,
                    });
                };

                damageChopPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Chop damage per level"), (float)options.damageChopPerLevel,
                    new ConfigDescription("Chop damage per upgrade level on the item (not visible ingame, effects wood cutting damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageChopPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePickaxe = damagePickaxe.Value,
                    });
                };

                damagePickaxePerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Pickaxe damage per level"), (float)options.damagePickaxePerLevel,
                    new ConfigDescription("Pickaxe damage per upgrade level on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePickaxePerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePoison = damagePoison.Value,
                    });
                };

                damagePoisonPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Poison damage per level"), (float)options.damagePoisonPerLevel,
                    new ConfigDescription("Poison damage per upgrade level on the item (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePoisonPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePoisonPerLevel = damagePoisonPerLevel.Value,
                    });
                };
            }

            if (options.damageSpirit != null)
            {
                damageSpirit = Config.Bind(new ConfigDefinition(options.sectionName, "Spirit damage"), (float)options.damageSpirit,
                    new ConfigDescription("Spirit damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageSpirit.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageSpirit = damageSpirit.Value,
                    });
                };

                damageSpiritPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Spirit damage per level"), (float)options.damageSpiritPerLevel,
                    new ConfigDescription("Spirit damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageSpiritPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageSpiritPerLevel = damageSpiritPerLevel.Value,
                    });
                };
            }

            if (options.damageSlash != null)
            {
                damageSlash = Config.Bind(new ConfigDefinition(options.sectionName, "Slash damage"), (float)options.damageSlash,
                    new ConfigDescription("Slash damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageSlash.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageSlash = damageSlash.Value,
                    });
                };

                damageSlashPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Slash damage per level"), (float)options.damageSlashPerLevel,
                    new ConfigDescription("Slash damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageSlashPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageSlashPerLevel = damageSlashPerLevel.Value,
                    });
                };
            }

            if (options.damagePierce != null)
            {
                damagePierce = Config.Bind(new ConfigDefinition(options.sectionName, "Pierce damage"), (float)options.damagePierce,
                    new ConfigDescription("Pierce damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePierce.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePierce = damagePierce.Value,
                    });
                };

                damagePiercePerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Pierce damage per level"), (float)options.damagePiercePerLevel,
                    new ConfigDescription("Pierce damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePiercePerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damagePiercePerLevel = damagePiercePerLevel.Value,
                    });
                };
            }

            if (options.damageFire != null)
            {
                damageFire = Config.Bind(new ConfigDefinition(options.sectionName, "Fire damage"), (float)options.damageFire,
                    new ConfigDescription("Fire damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageFire.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageFire = damageFire.Value,
                    });
                };

                damageFirePerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Fire damage per level"), (float)options.damageFirePerLevel,
                    new ConfigDescription("Fire damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageFirePerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageFirePerLevel = damageFirePerLevel.Value,
                    });
                };
            }

            if (options.damageFrost != null)
            {
                damageFrost = Config.Bind(new ConfigDefinition(options.sectionName, "Frost damage"), (float)options.damageFrost,
                    new ConfigDescription("Frost damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageFrost.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageFrost = damageFrost.Value,
                    });
                };

                damageFrostPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Frost damage per level"), (float)options.damageFrostPerLevel,
                    new ConfigDescription("Frost damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageFrostPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageFrostPerLevel = damageFrostPerLevel.Value,
                    });
                };
            }

            if (options.damageLightning != null)
            {
                damageLightning = Config.Bind(new ConfigDefinition(options.sectionName, "Lightning damage"), (float)options.damageLightning,
                    new ConfigDescription("Lightning damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageLightning.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageLightning = damageLightning.Value,
                    });
                };

                damageLightningPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Lightning damage per level"), (float)options.damageLightningPerLevel,
                    new ConfigDescription("Lightning damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageLightningPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageLightningPerLevel = damageLightningPerLevel.Value,
                    });
                };
            }

            if (options.damageGeneral != null)
            {
                damageGeneral = Config.Bind(new ConfigDefinition(options.sectionName, "General damage"), (float)options.damageGeneral,
                    new ConfigDescription("General (typeless) damage on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageGeneral.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageGeneral = damageGeneral.Value,
                    });
                };

                damageGeneralPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "General damage per level"), (float)options.damageGeneralPerLevel,
                    new ConfigDescription("General (typeless) damage per upgrade level on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageGeneralPerLevel.SettingChanged += (obj, attr) =>
                {
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        damageGeneralPerLevel = damageGeneralPerLevel.Value,
                    });
                };
            }

            attackEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Attack eitr cost"), options.attackEitr,
                new ConfigDescription("Normal attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            attackEitr.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    attackEitr = attackEitr.Value,
                });
            };

            projectileVelocity = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile velocity"), options.projectileVelocity,
                new ConfigDescription("The velocity of the projectiles emitted from this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            projectileVelocity.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    projectileVelocity = projectileVelocity.Value,
                });
            };

            projectileAccuracy = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile accuracy"), options.projectileAccuracy,
                new ConfigDescription("The accuracy (spread) of the projectiles emitted from this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            projectileAccuracy.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
                    ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    weight = weight.Value,
                });
            };

            maxDurability = Config.Bind(new ConfigDefinition(options.sectionName, "Max durability"), options.maxDurability,
                new ConfigDescription("The maximum durability of the item", null,
                 new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            maxDurability.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    maxDurability = maxDurability.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), (float)options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            movementSpeed.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    movementModifier = movementSpeed.Value,
                });
            };

            blockArmor = Config.Bind(new ConfigDefinition(options.sectionName, "Block armor"), (int)options.blockArmor,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            blockArmor.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    blockPower = blockArmor.Value,
                });
            };

            deflectionForce = Config.Bind(new ConfigDefinition(options.sectionName, "Block force"), (int)options.deflectionForce,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            deflectionForce.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    deflectionForce = deflectionForce.Value,
                });
            };

            attackForce = Config.Bind(new ConfigDefinition(options.sectionName, "Knockback"), (int)options.attackForce,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            attackForce.SettingChanged += (obj, attr) =>
            {
                ItemHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
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
