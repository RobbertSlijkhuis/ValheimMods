using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchAllowDamage : MonoBehaviour
    {
        public bool m_allowDamageShips;
        public bool m_allowDamageStructures;

        public void Init(bool allowDamageShips, bool allowDamageStructures)
        {
            m_allowDamageShips = allowDamageShips;
            m_allowDamageStructures = allowDamageStructures;
        }
    }
}
