using ModularMagic_IceStaffs.configs;
using ModularMagic_IceStaffs.Models;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_IceStaffs.Helpers
{
    internal class AttackHelper
    {
        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
                damageFrost = config.damageFrost.Value,
                damagePierce = config.damagePierce.Value,
                attackForce = config.attackForce.Value,
            });
        }

        public static void UpdateRain(GameObject attackPrefab, GameObject shardPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateItemData(attackPrefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                launchAngle = config.launchAngle.Value,
                projectileVelocity = config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy.Value,
            });

            UpdateHelper.UpdateProjectile(shardPrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                damageFrost = config.damageFrost.Value,
                damagePierce = config.damagePierce.Value,
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
