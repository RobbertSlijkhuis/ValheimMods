using ModularMagic_Core.Types;
using ModularMagic_IceStaffs.Configs;
using System;
using static ItemDrop;

namespace ModularMagic_IceStaffs.Helpers
{
    /// <summary>
    /// What is specific to the Ice staffs about their imbuements. Core applies the runes (see StaffImbuementHelper), this mod tells it
    /// which runes the staffs support and what the shards of the Rain rune do.
    /// </summary>
    internal class IceImbuementHelper
    {
        // The runes the Ice staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.DamageBlunt, RuneId.DamageSlash, RuneId.EitrCost,
            RuneId.ProjectileAccuracy, RuneId.AttackSpeed, RuneId.ProjectileSpeed,
            RuneId.Nova, RuneId.Rain, RuneId.Cone, RuneId.Utility,
        };

        // The script that spawns the shards (SpawnAbility) does not pass the damage of the staff on, so every shard it spawns gets the frost and physical
        // damage of the staff that cast the rain (quality and damage type rune included) times the damage multiplier, and the attack force from the
        // attack force of the staff times the force multiplier. It is set on the spawned shard, not on the prefab, so it never holds the damage of
        // another staff. Registered with Core's SpawnedProjectileHelper for the shard prefab. The shards are hit by the client of the shooter, and
        // the chop and pickaxe damage stay config values
        public static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null || staff == null)
                    return;

                HitData.DamageTypes damage = staff.GetDamage();
                SecondaryAttackConfig rain = PluginConfig.secondaryAttackRain;
                float multiplier = rain.damageMultiplier.Value;

                projectile.m_damage.m_frost = damage.m_frost * multiplier;
                projectile.m_damage.m_pierce = damage.m_pierce * multiplier;
                projectile.m_damage.m_blunt = damage.m_blunt * multiplier;
                projectile.m_damage.m_slash = damage.m_slash * multiplier;
                projectile.m_attackForce = staff.m_shared.m_attackForce * rain.forceMultiplier.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the damage of the shard: " + e);
            }
        }
    }
}
