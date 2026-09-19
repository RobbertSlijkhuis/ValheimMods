using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// Added by a staff mod to the prefab of an imbuable weapon. The weapon is imbuable if its
    /// drop prefab has this component, and the imbuements themselves are stored in the item's custom data.
    /// </summary>
    public class ImbuementSlots : MonoBehaviour
    {
        public int m_slots = 1;
        public int m_tier = 1;
        public string m_weaponType;
    }
}
