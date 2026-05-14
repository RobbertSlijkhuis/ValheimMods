using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    internal class SoulReaper : MonoBehaviour
    {
        // ZDO key used to sync charge across all clients
        public static readonly int ZDOKey = "SoulReaperCharge".GetStableHashCode();

        public float      m_maxCharge      = 1f;
        public float      m_dischargeDelay = 0.5f;
        public GameObject? m_apparitionPrefab = null;
        public string     m_apparitionName = "Apparition";

        private float _chargeFullTime = -1f;

        /// <summary>Current charge — read from the local player's ZDO.</summary>
        public float m_charge
        {
            get
            {
                if (Player.m_localPlayer == null) return 0f;
                return Player.m_localPlayer.m_nview?.GetZDO()?.GetFloat(ZDOKey, 0f) ?? 0f;
            }
            private set
            {
                if (Player.m_localPlayer == null) return;
                Player.m_localPlayer.m_nview?.GetZDO()?.Set(ZDOKey, value);
            }
        }

        public void Charge(float amount)
        {
            bool wasCharged = m_charge >= m_maxCharge;
            float newCharge = Mathf.Min(m_charge + amount, m_maxCharge);
            m_charge = newCharge;

            if (!wasCharged && newCharge >= m_maxCharge)
                _chargeFullTime = Time.time;

            UpdateChargeStatusEffect();
        }

        public bool CanDischarge()
        {
            return m_charge >= m_maxCharge
                && _chargeFullTime >= 0f
                && Time.time - _chargeFullTime >= m_dischargeDelay;
        }

        public void Discharge(float amount)
        {
            m_charge = Mathf.Max(m_charge - amount, 0f);
        }

        public void ResetCharge()
        {
            m_charge        = 0f;
            _chargeFullTime = -1f;
            RemoveChargeStatusEffect();
        }

        private void UpdateChargeStatusEffect()
        {
            if (Player.m_localPlayer == null)
                return;

            SEMan seman = Player.m_localPlayer.GetSEMan();

            SE_SoulReaperCharge? existing = seman.GetStatusEffect(typeof(SE_SoulReaperCharge).Name.GetStableHashCode()) as SE_SoulReaperCharge;
            if (existing != null)
            {
                existing.m_soulReaper = this;
                return;
            }

            SE_SoulReaperCharge effect = ScriptableObject.CreateInstance<SE_SoulReaperCharge>();
            effect.name         = typeof(SE_SoulReaperCharge).Name;
            effect.m_soulReaper = this;
            seman.AddStatusEffect(effect);
        }

        private void RemoveChargeStatusEffect()
        {
            if (Player.m_localPlayer == null)
                return;

            Player.m_localPlayer.GetSEMan()
                .RemoveStatusEffect(typeof(SE_SoulReaperCharge).Name.GetStableHashCode());
        }
    }
}
