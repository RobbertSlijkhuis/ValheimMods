using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The Attack Speed rune on the normal attack of a staff. The game has no attack speed stat, how often a staff can
    /// attack is the length of its attack animation (the projectile is fired by an event of that animation), so the rune speeds up
    /// the animation of the player while the attack runs. An attack that fires bursts (the cone) is timed by its burst interval instead.
    /// A staff mod registers a function that gives the speed factor of its staffs.
    /// </summary>
    public static class AttackSpeedHelper
    {
        private static readonly List<Func<ItemData, float>> factors = new List<Func<ItemData, float>>();

        // True while the animation speed of the local player was set here, so it is put back to normal when the attack is over
        private static bool applied;

        // The staff the speed was set for, and whether the attack animation was seen yet: the animation starts a moment after the attack
        private static ItemData appliedWeapon;
        private static bool animationSeen;
        private static float appliedTime;

        // How long the attack animation may take to start before the speed is put back
        private const float AnimationStartTimeout = 1f;

        /// <summary>
        /// Registers the staffs of a mod. Call it once at startup (in Awake).
        /// </summary>
        /// <param name="getFactor">How much faster the animation of the normal attack of this weapon is, 1 is the normal speed. Return 1 for a weapon of another mod.</param>
        public static void Register(Func<ItemData, float> getFactor)
        {
            if (getFactor == null)
            {
                Jotunn.Logger.LogWarning("[AttackSpeed] Cannot register without a factor function");
                return;
            }

            factors.Add(getFactor);
        }

        // Called when the local player has started an attack, the speed is set for the whole attack. Only the normal attack is sped up
        internal static void OnAttackStarted(Humanoid humanoid, bool secondaryAttack)
        {
            try
            {
                if (humanoid == null || humanoid != Player.m_localPlayer)
                    return;

                // Every attack starts at the normal speed
                Reset(humanoid);

                if (secondaryAttack)
                    return;

                Attack attack = humanoid.m_currentAttack;
                ItemData weapon = humanoid.GetCurrentWeapon();

                // An attack that fires bursts is timed by its burst interval, not by the animation
                if (attack == null || weapon == null || attack.m_projectileBursts > 1)
                    return;

                float factor = GetFactor(weapon);

                if (factor <= 1f || humanoid.m_zanim == null)
                    return;

                humanoid.m_zanim.SetSpeed(factor);
                applied = true;
                appliedWeapon = weapon;
                animationSeen = false;
                appliedTime = Time.time;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the attack speed: " + e);
            }
        }

        // Called every update of the attack of a humanoid, puts the speed back when the attack is over
        internal static void OnAttackUpdated(Humanoid humanoid)
        {
            if (!applied || humanoid == null || humanoid != Player.m_localPlayer)
                return;

            // The attack object stays after it is done, it is only gone when the weapon was unequipped. The attack object is not
            // updated for a weapon that is no longer held, so the held weapon is checked too
            if (humanoid.m_currentAttack == null || humanoid.m_currentAttack.IsDone() || humanoid.IsDead() || humanoid.GetCurrentWeapon() != appliedWeapon)
            {
                Reset(humanoid);
                return;
            }

            // The animator leaves the attack state when the attack animation is over (this is also how the game ends an attack),
            // so the speed is put back then even when the attack object did not notice it
            if (humanoid.InAttack())
                animationSeen = true;
            else if (animationSeen || Time.time - appliedTime > AnimationStartTimeout)
                Reset(humanoid);
        }

        private static void Reset(Humanoid humanoid)
        {
            if (!applied)
                return;

            applied = false;
            appliedWeapon = null;

            if (humanoid.m_zanim != null)
                humanoid.m_zanim.SetSpeed(1f);
        }

        // The first mod that knows the weapon decides, the staffs of the mods are separate weapons
        private static float GetFactor(ItemData weapon)
        {
            foreach (Func<ItemData, float> getFactor in factors)
            {
                float factor = getFactor(weapon);

                if (factor > 1f)
                    return factor;
            }

            return 1f;
        }
    }
}
