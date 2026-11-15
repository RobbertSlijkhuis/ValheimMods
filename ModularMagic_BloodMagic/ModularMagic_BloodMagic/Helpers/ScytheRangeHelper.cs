using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using ImbuementType = ModularMagic_Core.Types.ImbuementType;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The Range rune: the normal swing of a scythe reaches further while the scythe is full, and for a few seconds after its charge was used
    /// to summon an apparition (SoulReaper.IsChargedOrJustDischarged). The reach is only added while the game finds what the swing hits, see
    /// SoulReaperPatches. The local player swings its own scythe, so nothing is synced.
    /// </summary>
    internal static class ScytheRangeHelper
    {
        /// <summary>
        /// Adds the reach of the Range rune to the attack when it is the normal swing of the local player with a charged scythe. Returns the
        /// reach to put back afterwards, or null when nothing was changed.
        /// </summary>
        public static float? ExtendReach(Attack attack)
        {
            Player player = Player.m_localPlayer;

            if (player == null || attack == null || attack.m_character != player || player.m_currentAttack != attack || player.m_currentAttackIsSecondary)
                return null;

            if (PluginConfig.imbuementConfig.AttackRange == null)
                return null;

            SoulReaper? reaper = SoulReaperHelper.GetDrawnReaper(out _);
            ItemDrop.ItemData? scythe = player.GetCurrentWeapon();

            if (reaper == null || scythe == null)
                return null;

            int level = ImbuementHelper.GetLevel(scythe, ImbuementType.AttackRange);

            if (level <= 0 || !reaper.IsChargedOrJustDischarged(PluginConfig.rangeLinger.Value))
                return null;

            float range = attack.m_attackRange;
            attack.m_attackRange += level * PluginConfig.imbuementConfig.AttackRange.Value;

            return range;
        }
    }
}
