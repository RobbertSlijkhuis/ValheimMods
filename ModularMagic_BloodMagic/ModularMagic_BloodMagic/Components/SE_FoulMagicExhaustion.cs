using ModularMagic_BloodMagic.Configs;
using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    /// <summary>
    /// The price of the apparitions: every active apparition of the player lowers the damage of all attacks of the player, up to a
    /// maximum. ExhaustionHelper keeps the stacks equal to the number of active apparitions and removes the effect without any.
    /// </summary>
    internal class SE_FoulMagicExhaustion : StatusEffect
    {
        // The number of active apparitions of the player, set by ExhaustionHelper
        public int m_stacks;

        public override void Setup(Character character)
        {
            m_name    = "Foul Magic Exhaustion";
            m_tooltip = "Your apparitions feed on your strength.";
            m_ttl     = 0f;

            if (m_icon == null)
                m_icon = FetchIcon();

            base.Setup(character);
        }

        /// <summary>The part of the damage the player loses, 0.3 is 30%.</summary>
        public float GetDamageReduction()
        {
            float reduction = Mathf.Min(PluginConfig.exhaustionMax.Value, m_stacks * PluginConfig.exhaustionPerApparition.Value);

            return Mathf.Clamp01(reduction);
        }

        // The game calls this for every melee, projectile and area attack of the character that has the effect
        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            hitData.m_damage.Modify(1f - GetDamageReduction());
        }

        public override string GetIconText()
        {
            return m_stacks.ToString();
        }

        public override string GetTooltipString()
        {
            return $"{m_tooltip}\nActive apparitions: <color=orange>{m_stacks}</color>\nDamage: <color=orange>-{Mathf.RoundToInt(GetDamageReduction() * 100f)}%</color>";
        }

        private static Sprite? FetchIcon()
        {
            StatusEffect? se = ObjectDB.instance?.GetStatusEffect("Tared".GetStableHashCode());
            return se?.m_icon;
        }
    }
}
