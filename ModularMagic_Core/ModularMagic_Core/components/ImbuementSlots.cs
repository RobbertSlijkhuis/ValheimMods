using ModularMagic_Core.Types;
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
        // Decides which runes are allowed. When a staff mod forgets to set it no rune can be slotted
        public string m_weaponType = WeaponType.None;
    }
}
