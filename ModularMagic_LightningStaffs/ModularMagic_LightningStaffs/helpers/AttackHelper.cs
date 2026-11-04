using ModularMagic_LightningStaffs.Configs;
using ModularMagic_LightningStaffs.Models;
using UnityEngine;

namespace ModularMagic_LightningStaffs.Helpers
{
    internal class AttackHelper
    {
        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
                damageLightning = config.damageLightning.Value,
                attackForce = config.attackForce.Value,
            });
        }

        // The shape of the cone comes from the collider of the prefab, so only the damage and the force are applied
        public static void UpdateCone(GameObject aoePrefab, MainAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                damageLightning = config.damageLightning.Value,
                attackForce = config.attackForce.Value,
            });
        }

        public static void UpdateRain(GameObject attackPrefab, GameObject boltPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateItemData(attackPrefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                launchAngle = config.launchAngle.Value,
                projectileVelocity = config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy.Value,
            });

            UpdateHelper.UpdateProjectile(boltPrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                damageLightning = config.damageLightning.Value,
                damageChop = config.damageChop.Value,
                damagePickaxe = config.damagePickaxe.Value,
                attackForce = config.attackForce.Value,
            });

            UpdateHelper.UpdateSpawnAbility(spawnAbilityPrefab, new UpdateSpawnAbilityOptions()
            {
                minToSpawn = config.minToSpawn.Value,
                maxToSpawn = config.maxToSpawn.Value,
                spawnRadius = config.spawnRadius.Value,
            });
        }
    }
}
