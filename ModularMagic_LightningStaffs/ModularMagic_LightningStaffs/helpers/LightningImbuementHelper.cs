using ModularMagic_Core.Types;
using ModularMagic_LightningStaffs.Configs;
using System;
using static ItemDrop;

namespace ModularMagic_LightningStaffs.Helpers
{
    /// <summary>
    /// What is specific to the Lightning staffs about their imbuements. Core applies the runes (see StaffImbuementHelper), this mod tells it
    /// which runes the staffs support and what the bolts of the Rain rune do.
    /// </summary>
    internal class LightningImbuementHelper
    {
        // The runes the Lightning staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.AttackSpeed, RuneId.ProjectileSpeed,
            RuneId.Nova, RuneId.Rain, RuneId.Cone,
        };

        // The script that spawns the bolts (SpawnAbility) does not pass the damage of the staff on, so every bolt it spawns gets the lightning damage of
        // the staff that cast the rain (quality included) times the damage multiplier, and the attack force from the attack force of the staff times
        // the force multiplier. It is set on the spawned bolt, not on the prefab, so it never holds the damage of another staff.
        // Registered with Core's SpawnedProjectileHelper for the bolt prefab. The bolts are hit by the client of the shooter, and the chop and
        // pickaxe damage stay config values
        public static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null || staff == null)
                    return;

                HitData.DamageTypes damage = staff.GetDamage();
                SecondaryAttackConfig rain = PluginConfig.secondaryAttackRain;

                projectile.m_damage.m_lightning = damage.m_lightning * rain.damageMultiplier.Value;
                projectile.m_attackForce = staff.m_shared.m_attackForce * rain.forceMultiplier.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the damage of the bolt: " + e);
            }
        }
    }
}
