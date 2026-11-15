using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using UnityEngine;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The Blood Tap rune: the secondary attack of the scythe pays health (and eitr) for a full charge. The attack pays per burst
    /// (m_perBurstResourceUsage), so the game checks the health when the burst fires: without enough health it stops the attack without
    /// paying, and it never takes the last health (Attack.FireProjectileBurst). The burst spawns the trigger that gives the charge
    /// (BloodTapTrigger), so the charge is only given when the cost was paid.
    /// </summary>
    internal static class BloodTapHelper
    {
        /// <summary>
        /// Called by Core before Blood Tap starts (SecondaryAttackHelper.Register). A full scythe can not take more charge, so the attack is
        /// blocked and costs nothing. Otherwise the health cost is set on this attack: a part of the maximum health. The game's own
        /// percentage (m_attackHealthPercentage) is a part of the current health, which is always there, so it could never block the attack.
        /// </summary>
        public static bool PrepareStart(Humanoid character, Attack attack)
        {
            Player? player = character as Player;

            if (player == null || player != Player.m_localPlayer)
                return true;

            SoulReaper? reaper = SoulReaperHelper.GetDrawnReaper(out _);

            if (reaper == null)
                return true;

            if (reaper.m_charge >= reaper.m_maxCharge)
            {
                player.Message(MessageHud.MessageType.Center, "The scythe is already charged");
                return false;
            }

            // The attack is the copy for this one attack (Humanoid.StartAttack), so the cost does not stay on the scythe
            attack.m_attackHealth = player.GetMaxHealth() * PluginConfig.bloodTapHealthCost.Value;
            attack.m_attackHealthPercentage = 0f;

            return true;
        }

        /// <summary>
        /// Called on the client of the caster when Blood Tap fired (BloodTapTrigger), after the game took its cost: fills the drawn scythe.
        /// </summary>
        public static void Tap(Character owner)
        {
            if (owner == null || owner != Player.m_localPlayer)
                return;

            SoulReaper? reaper = SoulReaperHelper.GetDrawnReaper(out _);

            if (reaper == null)
                return;

            SoulReaperHelper.FillCharge(reaper, owner.GetCenterPoint(), owner.transform.forward);
        }
    }
}
