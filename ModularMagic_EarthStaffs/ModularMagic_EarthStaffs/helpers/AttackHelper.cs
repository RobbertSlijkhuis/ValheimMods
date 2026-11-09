using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
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

        // The damage (apart from chop and pickaxe) and the attack force of the boulders come from the staff, see EarthImbuementHelper
        public static void UpdateRain(GameObject projectilePrefab, SecondaryAttackConfig config)
        {
            UpdateHelper.UpdateProjectile(projectilePrefab, new UpdateProjectileOptions()
            {
                aoe = config.aoe.Value,
                damageChop = config.damageChop.Value,
                damagePickaxe = config.damagePickaxe.Value,
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
