using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class AttackHelper
    {
        public static void UpdateCone(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            Jotunn.Logger.LogWarning("prefab: " + aoePrefab?.name);
            Jotunn.Logger.LogWarning("cooldown: " + config?.cooldown?.Value);
            Jotunn.Logger.LogWarning("damageBlunt: " + config?.damageBlunt?.Value);
            Jotunn.Logger.LogWarning("damageChop: " + config?.damageChop?.Value);
            Jotunn.Logger.LogWarning("damageGeneral: " + config?.damageGeneral?.Value);
            Jotunn.Logger.LogWarning("damagePickaxe: " + config?.damagePickaxe?.Value);
            Jotunn.Logger.LogWarning("damagePierce: " + config?.damagePierce?.Value);
            Jotunn.Logger.LogWarning("damagePoison: " + config?.damagePoison?.Value);
            Jotunn.Logger.LogWarning("damageSpirit: " + config?.damageSpirit?.Value);


            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                damageBlunt = config.damageBlunt.Value,
                damageChop = config.damageChop.Value,
                damageGeneral = config.damageGeneral.Value,
                damagePickaxe = config.damagePickaxe.Value,
                damagePierce = config.damagePierce.Value,
                damagePoison = config.damagePoison.Value,
                damageSpirit = config.damageSpirit.Value,
            });
        }

        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            Jotunn.Logger.LogWarning("prefab: " + aoePrefab?.name);
            Jotunn.Logger.LogWarning("cooldown: " + config?.cooldown?.Value);
            Jotunn.Logger.LogWarning("damageBlunt: " + config?.damageBlunt?.Value);
            Jotunn.Logger.LogWarning("damageChop: " + config?.damageChop?.Value);
            Jotunn.Logger.LogWarning("damageGeneral: " + config?.damageGeneral?.Value);
            Jotunn.Logger.LogWarning("damagePickaxe: " + config?.damagePickaxe?.Value);
            Jotunn.Logger.LogWarning("damagePierce: " + config?.damagePierce?.Value);
            Jotunn.Logger.LogWarning("damagePoison: " + config?.damagePoison?.Value);
            Jotunn.Logger.LogWarning("damageSpirit: " + config?.damageSpirit?.Value);

            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
                damageBlunt = config.damageBlunt.Value,
                damageChop = config.damageChop.Value,
                damageGeneral = config.damageGeneral.Value,
                damagePickaxe = config.damagePickaxe.Value,
                damagePierce = config.damagePierce.Value,
                damagePoison = config.damagePoison.Value,
                damageSpirit = config.damageSpirit.Value,
                attackForce = config.attackForce.Value,
            });
        }

        public static void UpdateRain(GameObject attackPrefab, GameObject projectilePrefab, SecondaryAttackConfig config)
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

        public static void UpdateSummon(GameObject attackPrefab, GameObject rootPrefab, SecondaryAttackConfig config)
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

