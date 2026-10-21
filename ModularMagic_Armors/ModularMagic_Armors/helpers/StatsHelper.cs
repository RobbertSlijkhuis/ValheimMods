using ModularMagic_Armors.StatusEffects;
using System.Collections.Generic;

namespace ModularMagic_Armors.Helpers
{
    internal class StatsHelper
    {
        /**
         * Total eitr the worn armor adds to the max eitr of the player.
         * Every worn piece with a MagicStatusEffect as equip effect is counted, so new armors need no extra code here.
         */
        public static float GetEquippedEitr(Player player)
        {
            if (player == null)
                return 0f;

            float eitr = 0f;
            List<StatusEffect> effects = player.GetSEMan().GetStatusEffects();

            foreach (StatusEffect effect in effects)
            {
                if (effect is MagicStatusEffect magicEffect)
                    eitr += magicEffect.m_eitr;
            }

            return eitr;
        }
    }
}
