using ModularMagic_Core.Types;
using ModularMagic_FireStaffs.Configs;
using System;
using static ItemDrop;

namespace ModularMagic_FireStaffs.Helpers
{
    /// <summary>
    /// What is specific to the Fire staffs about their imbuements. Core applies the runes (see StaffImbuementHelper), this mod tells it
    /// which runes the staffs support and what the meteors of the Rain rune do.
    /// </summary>
    internal class FireImbuementHelper
    {
        // The runes the Fire staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.AttackSpeed, RuneId.ProjectileSpeed,
            RuneId.Nova, RuneId.Rain, RuneId.Cone,
        };

        // The script that spawns the meteors (SpawnAbility) does not pass the damage of the staff on, so every meteor it spawns gets the fire and blunt
        // damage of the staff that cast the rain (quality included) times the damage multiplier, and the attack force from the attack force of the
        // staff times the force multiplier. It is set on the spawned meteor, not on the prefab, so it never holds the damage of another staff.
        // Registered with Core's SpawnedProjectileHelper for the meteor prefab. The meteors are hit by the client of the shooter, and the chop and
        // pickaxe damage stay config values
        public static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null || staff == null)
                    return;

                HitData.DamageTypes damage = staff.GetDamage();
                float multiplier = PluginConfig.secondaryAttackRain.damageMultiplier.Value;

                projectile.m_damage.m_fire = damage.m_fire * multiplier;
                projectile.m_damage.m_blunt = damage.m_blunt * multiplier;
                projectile.m_attackForce = staff.m_shared.m_attackForce * PluginConfig.secondaryAttackRain.forceMultiplier.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the damage of the meteor: " + e);
            }
        }
    }
}
