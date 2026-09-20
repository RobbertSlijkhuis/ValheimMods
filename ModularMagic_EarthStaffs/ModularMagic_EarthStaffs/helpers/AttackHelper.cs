using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class AttackHelper
    {
        // The secondary attacks of the staffs are marked with this value (m_drawStaminaDrain in Unity)
        public const int SecondaryAttackMarker = 8901;

        public static bool IsSecondaryAttack(Attack attack)
        {
            return attack != null && attack.m_drawStaminaDrain == SecondaryAttackMarker;
        }

        /// <summary>
        /// The game takes the eitr cost from the main attack of the weapon, also when the secondary attack is used.
        /// The secondary attacks of the staffs have their own cost (with the same skill discount the game applies).
        /// </summary>
        public static float GetSecondaryAttackEitr(Attack attack, Character character, ItemData weapon)
        {
            if (attack.m_attackEitr <= 0f)
                return 0f;

            float skillFactor = character.GetSkillFactor(weapon.m_shared.m_skillType);
            return attack.m_attackEitr - attack.m_attackEitr * 0.33f * skillFactor;
        }

        public static void UpdateCone(GameObject aoePrefab, SecondaryAttackConfig config)
        {
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

