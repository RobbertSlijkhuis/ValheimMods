using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class ItemHelper
    {
        public static void CreateStaff(GameObject prefab, StaffConfig config)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, config.recipeUpgrade.Value, config.recipeMultiplier.Value);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = simpleRequirements;

            UpdateHelper.UpdateItemData(prefab, new UpdateItemDataOptions()
            {
                damageBlunt = config.damageBlunt == null ? 0f : config.damageBlunt.Value,
                damageChop = config.damageChop == null ? 0f : config.damageChop.Value,
                damagePickaxe = config.damagePickaxe == null ? 0f : config.damagePickaxe.Value,
                damagePoison = config.damagePoison == null ? 0f : config.damagePoison.Value,
                damageSpirit = config.damageSpirit == null ? 0f : config.damageSpirit.Value,
                damageBluntPerLevel = config.damageBluntPerLevel == null ? 0f : config.damageBluntPerLevel.Value,
                damageChopPerLevel = config.damageChopPerLevel == null ? 0f : config.damageChopPerLevel.Value,
                damagePickaxePerLevel = config.damagePickaxePerLevel == null ? 0f : config.damagePickaxePerLevel.Value,
                damageSpiritPerLevel = config.damageSpiritPerLevel == null ? 0f : config.damageSpiritPerLevel.Value,
                attackEitr = config.useEitr.Value,
                projectileVelocity = config.projectileVelocity == null ? 0f : config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy == null ? 0f : config.projectileAccuracy.Value,
                projectileBurst = config.projectileBurst == null ? 0f : config.projectileBurst == null ? 0f : config.projectileBurst.Value,
                weight = config.weight.Value,
                maxDurability = config.maxDurability.Value,
                maxQuality = config.maxQuality.Value,
                movementModifier = config.movementSpeed.Value,
                blockPower = config.blockArmor.Value,
                deflectionForce = config.deflectionForce.Value,
                attackForce = config.attackForce.Value,
            });

            if (config.secondaryAttackConfig != null)
            {
                UpdateHelper.UpdateItemData(prefab, new UpdateItemDataOptions()
                {
                    secondaryAttackEitr = config.secondaryAttackConfig.useEitr.Value,
                    secondaryLaunchAngle = config.secondaryAttackConfig.launchAngle == null ? 0f : config.secondaryAttackConfig.launchAngle.Value,
                    secondaryProjectileVelocity = config.secondaryAttackConfig.projectileVelocity == null ? 0f : config.secondaryAttackConfig.projectileVelocity.Value,
                    secondaryProjectileAccuracy = config.secondaryAttackConfig.projectileAccuracy == null ? 0f : config.secondaryAttackConfig.projectileAccuracy.Value,
                });

                if (config.secondaryAttackConfig.type == SecondaryAttackType.PROJECTILE)
                {
                    UpdateHelper.UpdateProjectile(config.secondaryAttackConfig.prefab, new UpdateProjectileOptions()
                    {
                        aoe = config.secondaryAttackConfig.aoe == null ? 0f : config.secondaryAttackConfig.aoe.Value,
                        damageBlunt = config.secondaryAttackConfig.damageBlunt == null ? 0f : config.secondaryAttackConfig.damageBlunt.Value,
                        damageChop = config.secondaryAttackConfig.damageChop == null ? 0f : config.secondaryAttackConfig.damageChop.Value,
                        damagePickaxe = config.secondaryAttackConfig.damagePickaxe == null ? 0f : config.secondaryAttackConfig.damagePickaxe.Value,
                        damagePoison = config.secondaryAttackConfig.damagePoison == null ? 0f : config.secondaryAttackConfig.damagePoison.Value,
                        damageSpirit = config.secondaryAttackConfig.damageSpirit == null ? 0f : config.secondaryAttackConfig.damageSpirit.Value,
                        attackForce = config.secondaryAttackConfig.attackForce == null ? 0f : config.secondaryAttackConfig.attackForce.Value,
                    });
                }
                else if (config.secondaryAttackConfig.type == SecondaryAttackType.HUMANOID)
                {
                    UpdateHelper.UpdateHumanoid(config.secondaryAttackConfig.prefab, new UpdateHumanoidOptions()
                    {
                        health = config.secondaryAttackConfig.health == null ? 100f : config.secondaryAttackConfig.health.Value,
                    });

                    UpdateHelper.UpdateHumanoidAttackItemData(config.secondaryAttackConfig.prefab, new UpdateItemDataOptions()
                    {
                        damageBlunt = config.secondaryAttackConfig.damageBlunt == null ? 0f : config.secondaryAttackConfig.damageBlunt.Value,
                        damageChop = config.secondaryAttackConfig.damageChop == null ? 0f : config.secondaryAttackConfig.damageChop.Value,
                        damagePickaxe = config.secondaryAttackConfig.damagePickaxe == null ? 0f : config.secondaryAttackConfig.damagePickaxe.Value,
                        damagePoison = config.secondaryAttackConfig.damagePoison == null ? 0f : config.secondaryAttackConfig.damagePoison.Value,
                        damageSpirit = config.secondaryAttackConfig.damageSpirit == null ? 0f : config.secondaryAttackConfig.damageSpirit.Value,
                        attackForce = config.secondaryAttackConfig.attackForce == null ? 0f : config.secondaryAttackConfig.attackForce.Value,
                    });

                    UpdateHelper.UpdateSpawnAbility(config.secondaryAttackConfig.staffPrefab, new UpdateSpawnAbilityOptions()
                    {
                        minToSpawn = config.secondaryAttackConfig.minToSpawn == null ? 0 : config.secondaryAttackConfig.minToSpawn.Value,
                        maxToSpawn = config.secondaryAttackConfig.maxToSpawn == null ? 0 : config.secondaryAttackConfig.maxToSpawn.Value,
                        maxSpawns = config.secondaryAttackConfig.maxSpawns == null ? 0 : config.secondaryAttackConfig.maxSpawns.Value,
                        spawnRadius = config.secondaryAttackConfig.spawnRadius == null ? 0f : config.secondaryAttackConfig.spawnRadius.Value,
                    });
                }
            }

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
