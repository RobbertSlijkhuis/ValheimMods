using ModularMagic_Core.Types;
using ModularMagic_LightningStaffs.Configs;
using System;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using Imbuement = ModularMagic_Core.Models.Imbuement;

namespace ModularMagic_LightningStaffs.Helpers
{
    /// <summary>
    /// The Attack Speed rune on the normal attack of a Lightning staff. The game has no attack speed stat, how often a staff can
    /// attack is the length of its attack animation (the projectile is fired by an event of that animation), so the rune speeds up
    /// the animation of the player while the attack runs. An attack that fires bursts (the cone) is timed by its burst interval instead.
    /// </summary>
    internal static class AttackSpeedHelper
    {
        // True while the animation speed of the local player was set here, so it is put back to normal when the attack is over
        private static bool applied;

        // Called when the local player has started an attack, the speed is set for the whole attack. Only the normal attack is sped up
        public static void OnAttackStarted(Humanoid humanoid, bool secondaryAttack)
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
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the attack speed: " + e);
            }
        }

        // Called every update of the attack of a humanoid, puts the speed back when the attack is over
        public static void OnAttackUpdated(Humanoid humanoid)
        {
            if (!applied || humanoid == null || humanoid != Player.m_localPlayer)
                return;

            // The attack object stays after it is done, it is only gone when the weapon was unequipped
            if (humanoid.m_currentAttack == null || humanoid.m_currentAttack.IsDone())
                Reset(humanoid);
        }

        private static void Reset(Humanoid humanoid)
        {
            if (!applied)
                return;

            applied = false;

            if (humanoid.m_zanim != null)
                humanoid.m_zanim.SetSpeed(1f);
        }

        // How much faster the animation of the normal attack of this staff is, 1 is the normal speed
        private static float GetFactor(ItemData weapon)
        {
            if (!LightningImbuementHelper.IsLightningStaff(weapon) || !CoreImbuementHelper.HasImbuements(weapon))
                return 1f;

            int levels = 0;

            foreach (Imbuement imbuement in CoreImbuementHelper.Read(weapon))
            {
                if (imbuement.type == ImbuementType.AttackSpeed)
                    levels += imbuement.level;
            }

            return 1f + PluginConfig.imbuementConfig.AttackSpeedAnimation.Value * levels;
        }
    }
}
