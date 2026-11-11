using ModularMagic_Core.Helpers;
using ModularMagic_Core.Types;
using ModularMagic_EarthStaffs.Configs;
using System;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// What is specific to the Earth staffs about their imbuements. Core applies the runes (see StaffImbuementHelper), this mod tells it
    /// which runes the staffs support and what the boulders of the Rain rune do.
    /// </summary>
    internal class EarthImbuementHelper
    {
        // The runes the Earth staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.DamagePierce, RuneId.DamageSlash, RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.AttackSpeed,
            RuneId.ProjectileSpeed, RuneId.Cone, RuneId.Creatures, RuneId.Nova, RuneId.Rain,
        };

        // The script that spawns the boulders (SpawnAbility) does not pass the damage of the staff on, so every boulder it spawns gets the damage of the
        // staff that cast the rain (apart from chop and pickaxe) from the staff (quality and damage type rune included) times the damage multiplier,
        // and the attack force from the attack force of the staff times the force multiplier. It is set on the spawned boulder, not on the prefab,
        // so it never holds the damage of another staff. Registered with Core's SpawnedProjectileHelper for the boulder prefab.
        // The boulders are hit by the client of the shooter, and the chop and pickaxe damage stay config values
        public static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null || staff == null)
                    return;

                HitData.DamageTypes damage = staff.GetDamage();
                SecondaryAttackConfig rain = PluginConfig.secondaryAttackRain;
                float multiplier = rain.damageMultiplier.Value;

                // A boulder is blunt, whatever the damage type rune of the staff is
                projectile.m_damage.m_blunt = DamageHelper.GetMainPhysical(damage) * multiplier;
                projectile.m_damage.m_slash = 0f;
                projectile.m_damage.m_pierce = 0f;
                projectile.m_damage.m_spirit = damage.m_spirit * multiplier;
                projectile.m_damage.m_poison = damage.m_poison * multiplier;
                projectile.m_attackForce = staff.m_shared.m_attackForce * rain.forceMultiplier.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the damage of the boulder: " + e);
            }
        }
    }
}
