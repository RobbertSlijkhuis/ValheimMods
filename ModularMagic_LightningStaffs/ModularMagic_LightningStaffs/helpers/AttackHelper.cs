using ModularMagic_LightningStaffs.Configs;
using ModularMagic_LightningStaffs.Models;
using UnityEngine;

namespace ModularMagic_LightningStaffs.Helpers
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

        // The lightning damage and the attack force of the bolts come from the staff, see LightningImbuementHelper
        public static void UpdateRain(GameObject boltPrefab, GameObject spawnAbilityPrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateProjectile(boltPrefab, new UpdateProjectileOptions()
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
