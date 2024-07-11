namespace MagicExtended.StatusEffects
{
    internal class StatusEffectWithEitr : StatusEffect
    {
        public float m_eitr;
        public void Awake() => this.m_tooltip = "Eitr: <color=orange>+" + this.m_eitr + "</color>";
        public void SetEitr(float eitr)
        {
            this.m_eitr = eitr;
            this.m_tooltip = "Eitr: <color=orange>+" + this.m_eitr + "</color>";
        }
    }
}