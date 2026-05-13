using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    internal class SE_SoulReaperCharge : StatusEffect
    {
        public SoulReaper? m_soulReaper;

        public override void Setup(Character character)
        {
            m_name    = "Soul Reaper";
            m_tooltip = "The Soul Reaper scythe is charging...";
            m_ttl     = 0f;

            if (m_icon == null)
                m_icon = FetchIcon();

            base.Setup(character);
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);

            if (m_soulReaper == null)
                return;

            int percent = Mathf.RoundToInt((m_soulReaper.m_charge / m_soulReaper.m_maxCharge) * 100f);
            bool ready  = m_soulReaper.CanDischarge();

            m_name = ready
                ? "Soul Reaper — READY"
                : $"Soul Reaper — {percent}%";
        }

        public override bool IsDone()
        {
            if (m_soulReaper == null || m_soulReaper.m_charge <= 0f)
                return true;

            return base.IsDone();
        }

        public override string GetTooltipString()
        {
            if (m_soulReaper == null)
                return m_tooltip;

            bool ready = m_soulReaper.CanDischarge();

            return ready
                ? "<color=orange>READY</color>"
                : $"Charging: <color=orange>{Mathf.RoundToInt((m_soulReaper.m_charge / m_soulReaper.m_maxCharge) * 100f)}%</color>";
        }

        private static Sprite? FetchIcon()
        {
            StatusEffect? se = ObjectDB.instance?.GetStatusEffect("SlowFall".GetStableHashCode());
            return se?.m_icon;
        }
    }
}