namespace ModularMagic_Utilities.StatusEffects
{
    internal class StatusEffectWithEitr : StatusEffect
    {
        public float m_eitr;
        public void Awake() => this.m_tooltip = this.m_eitr <= 0 ? "" : "Eitr: <color=orange>+" + this.m_eitr + "</color>";
        public void SetEitr(float eitr)
        {
            this.m_eitr = eitr;
            this.m_tooltip = eitr <= 0 ? "" : "Eitr: <color=orange>+" + this.m_eitr + "</color>";
        }
    }
}