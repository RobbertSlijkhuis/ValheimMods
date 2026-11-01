using ModularMagic_FireStaffs.Configs;
using ModularMagic_FireStaffs.Models;
using UnityEngine;

namespace ModularMagic_FireStaffs.Helpers
{
    internal class AttackHelper
    {
        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
                damageFire = config.damageFire.Value,
                attackForce = config.attackForce.Value,
            });
        }

        public static void UpdateRain(GameObject attackPrefab, GameObject meteorPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateItemData(attackPrefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                launchAngle = config.launchAngle.Value,
                projectileVelocity = config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy.Value,
            });

            UpdateHelper.UpdateProjectile(meteorPrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                damageFire = config.damageFire.Value,
                damageBlunt = config.damageBlunt.Value,
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
