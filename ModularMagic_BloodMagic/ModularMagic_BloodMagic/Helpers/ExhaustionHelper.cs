using ModularMagic_BloodMagic.Components;
using UnityEngine;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// Foul Magic Exhaustion: the local player deals less damage for every active apparition it has (a husk does not count). The stacks
    /// are not stored, they are the number of active apparitions that is counted every frame, so they can not get out of step with it.
    /// </summary>
    internal static class ExhaustionHelper
    {
        private const string EffectName = nameof(SE_FoulMagicExhaustion);

        private static readonly int EffectHash = EffectName.GetStableHashCode();

        // The effect the game copies when it is added to the player, made once
        private static SE_FoulMagicExhaustion? template;

        /// <summary>
        /// Called every frame for every player. Adds, updates or removes the effect of the local player.
        /// </summary>
        public static void Update(Player player)
        {
            if (player == null || player != Player.m_localPlayer)
                return;

            ApparitionHelper.Count(player.GetPlayerID(), out int active);

            SEMan seman = player.GetSEMan();
            SE_FoulMagicExhaustion? effect = seman.GetStatusEffect(EffectHash) as SE_FoulMagicExhaustion;

            if (active <= 0)
            {
                if (effect != null)
                    seman.RemoveStatusEffect(EffectHash);

                return;
            }

            if (effect == null)
            {
                // The game adds a copy of the template (StatusEffect.Clone). The copy is the same Unity object as the template, so the
                // template is kept and used again: destroying it would destroy the effect on the player too
                if (template == null)
                {
                    template = ScriptableObject.CreateInstance<SE_FoulMagicExhaustion>();
                    template.name = EffectName;
                }

                effect = seman.AddStatusEffect(template) as SE_FoulMagicExhaustion;
            }

            if (effect != null)
                effect.m_stacks = active;
        }
    }
}
