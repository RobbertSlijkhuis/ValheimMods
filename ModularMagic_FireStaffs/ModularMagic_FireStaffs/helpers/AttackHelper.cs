using ModularMagic_FireStaffs.Configs;
using ModularMagic_FireStaffs.Models;
using UnityEngine;

namespace ModularMagic_FireStaffs.Helpers
{
    internal class AttackHelper
    {
        // The damage and the attack force of the nova come from the staff (see damageMultiplier of the config), so only the radius is applied
        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
            });
        }

        public static void UpdateRain(GameObject attackPrefab, GameObject meteorPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            // The launch angle, velocity and accuracy of the projectile that is fired from the staff are those of the attack prefab
            UpdateHelper.UpdateProjectile(meteorPrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                // The fire and blunt damage of the meteors come from the staff, see FireImbuementHelper
                damageChop = config.damageChop.Value,
                damagePickaxe = config.damagePickaxe.Value,
            });

            UpdateHelper.UpdateSpawnAbility(spawnAbilityPrefab, new UpdateSpawnAbilityOptions()
            {
                minToSpawn = config.minToSpawn.Value,
                maxToSpawn = config.maxToSpawn.Value,
                spawnRadius = config.spawnRadius.Value,
                projectileAccuracy = config.spawnAccuracy.Value,
                spawnDelay = config.spawnDelay.Value,
            });
        }
    }
}
