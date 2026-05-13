using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    internal class SoulReaper : MonoBehaviour
    {
        public float m_charge = 0f;
        public float m_maxCharge = 1f;

        // Minimum seconds between reaching full charge and being allowed to discharge
        public float m_dischargeDelay = 0.5f;

        private float _chargeFullTime = -1f;

        private void Awake() { }

        public void Charge(float amount)
        {
            bool wasCharged = m_charge >= m_maxCharge;

            m_charge += amount;

            if (m_charge > m_maxCharge)
                m_charge = m_maxCharge;

            // Record the moment the weapon first reaches full charge
            if (!wasCharged && m_charge >= m_maxCharge)
                _chargeFullTime = Time.time;
        }

        /// <summary>Returns true if enough time has passed since the charge became full.</summary>
        public bool CanDischarge()
        {
            return m_charge >= m_maxCharge
                && _chargeFullTime >= 0f
                && Time.time - _chargeFullTime >= m_dischargeDelay;
        }

        public void Discharge(float amount)
        {
            m_charge -= amount;

            if (m_charge < 0f)
                m_charge = 0f;
        }

        public void ResetCharge()
        {
            m_charge = 0f;
            _chargeFullTime = -1f;
        }
    }
}
