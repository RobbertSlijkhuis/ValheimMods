using UnityEngine;

namespace ModularMagic_Utilities.StatusEffects
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

        public void SetEitr(float value)
        {
            this.m_eitr = value;
            UpdateTooltip();
        }

        public void SetElementalMagic(float value)
        {
            this.m_elementalMagic = value;
            UpdateTooltip();
        }

        public void SetBloodMagic(float value)
        {
            this.m_bloodMagic = value;
            UpdateTooltip();
        }

        public void SetAll(float eitr, float elementalMagic, float bloodMagic)
        {
            this.m_eitr = eitr;
            this.m_elementalMagic = elementalMagic;
            this.m_bloodMagic = bloodMagic;
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            string tooltip = "";



            if (this.m_elementalMagic > 0)
                tooltip += $"ElementalMagic: <color=orange>+{this.m_elementalMagic}</color>" + (this.m_bloodMagic > 0 || this.m_eitr > 0 ? "\n" : "");

            if (this.m_bloodMagic > 0)
                tooltip += $"BloodMagic: <color=orange>+{this.m_bloodMagic}</color>" + (this.m_eitr > 0 ? "\n" : "");

            if (this.m_eitr > 0)
                tooltip += $"Eitr: <color=orange>+{this.m_eitr}</color>";

            this.m_tooltip = tooltip;
        }
    }
}