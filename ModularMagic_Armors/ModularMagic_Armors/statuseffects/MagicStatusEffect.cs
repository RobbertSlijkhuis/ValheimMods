namespace ModularMagic_Armors.StatusEffects
{
    internal class MagicStatusEffect : StatusEffect
    {
        public float m_eitr;
        public float m_elementalMagic;
        public float m_bloodMagic;

        public void Awake()
        {
            UpdateTooltip();
        }

        // Vanilla Skills.GetSkillLevel asks every active status effect, so each worn piece adds its own bonus
        public override void ModifySkillLevel(Skills.SkillType skill, ref float level)
        {
            if (skill == Skills.SkillType.ElementalMagic)
                level += m_elementalMagic;
            else if (skill == Skills.SkillType.BloodMagic)
                level += m_bloodMagic;
        }

        public void SetEitr(float value)
        {
            m_eitr = value;
            UpdateTooltip();
        }

        public void SetElementalMagic(float value)
        {
            m_elementalMagic = value;
            UpdateTooltip();
        }

        public void SetBloodMagic(float value)
        {
            m_bloodMagic = value;
            UpdateTooltip();
        }

        public void SetAll(float eitr, float elementalMagic, float bloodMagic)
        {
            m_eitr = eitr;
            m_elementalMagic = elementalMagic;
            m_bloodMagic = bloodMagic;
            UpdateTooltip();
        }

        public void UpdateTooltip()
        {
            string tooltip = "";

            if (m_elementalMagic > 0)
                tooltip += $"ElementalMagic: <color=orange>+{m_elementalMagic}</color>" + (m_bloodMagic > 0 || m_eitr > 0 ? "\n" : "");

            if (m_bloodMagic > 0)
                tooltip += $"BloodMagic: <color=orange>+{m_bloodMagic}</color>" + (m_eitr > 0 ? "\n" : "");

            if (m_eitr > 0)
                tooltip += $"Eitr: <color=orange>+{m_eitr}</color>";

            m_tooltip = tooltip;
        }
    }
}