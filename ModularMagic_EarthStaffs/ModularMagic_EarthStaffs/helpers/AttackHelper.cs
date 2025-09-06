using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class AttackHelper
    {
        public static void UpdateBoulder(GameObject attackPrefab, GameObject projectilePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateItemData(attackPrefab, new UpdateItemDataOptions()
            {
                secondaryLaunchAngle = config.launchAngle.Value,
                projectileVelocity = config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy.Value,
            });

            UpdateHelper.UpdateProjectile(projectilePrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                damageBlunt = config.damageBlunt.Value,
                damageChop = config.damageChop.Value,
                damagePickaxe = config.damagePickaxe.Value,
                damagePoison = config.damagePoison.Value,
                damageSpirit = config.damageSpirit.Value,
                attackForce = config.attackForce.Value,
            });
        }

        public static void UpdateRoots(GameObject attackPrefab, GameObject rootPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateHumanoid(rootPrefab, new UpdateHumanoidOptions()
            {
                health = config.health.Value,
            });

            UpdateHelper.UpdateHumanoidAttackItemData(rootPrefab, new UpdateItemDataOptions()
            {
                damageBlunt = config.damageBlunt.Value,
                damageChop = config.damageChop.Value,
                damagePickaxe = config.damagePickaxe.Value,
                damagePoison = config.damagePoison.Value,
                damageSpirit = config.damageSpirit.Value,
                attackForce = config.attackForce.Value,
            });

            UpdateHelper.UpdateSpawnAbility(attackPrefab, new UpdateSpawnAbilityOptions()
            {
                minToSpawn = config.minToSpawn.Value,
                maxToSpawn = config.maxToSpawn.Value,
                maxSpawns = config.maxSpawns.Value,
                spawnRadius = config.spawnRadius.Value,
            });
        }
    }
}

