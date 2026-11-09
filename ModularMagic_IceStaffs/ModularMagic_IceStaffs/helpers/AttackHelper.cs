using ModularMagic_IceStaffs.configs;
using ModularMagic_IceStaffs.Models;
using UnityEngine;

namespace ModularMagic_IceStaffs.Helpers
{
    internal class AttackHelper
    {
        // The damage and the attack force of the nova come from the staff (see the multipliers of the config), so only the radius is applied
        public static void UpdateNova(GameObject aoePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateAoe(aoePrefab, new UpdateAoeOptions()
            {
                aoe = config.aoe.Value,
            });
        }

        // The frost and pierce damage and the attack force of the shards come from the staff, see IceImbuementHelper
        public static void UpdateRain(GameObject shardPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateProjectile(shardPrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
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
