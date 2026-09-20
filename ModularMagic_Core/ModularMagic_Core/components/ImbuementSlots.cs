using ModularMagic_Core.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// Added by a staff mod to the prefab of an imbuable weapon. The weapon is imbuable if its
    /// drop prefab has this component, and the imbuements themselves are stored in the item's custom data.
    /// The weapon decides how many slots it has, the tier of its slots and which runes it supports.
    /// </summary>
    public class ImbuementSlots : MonoBehaviour
    {
        public int m_slots = 1;
        public int m_tier = 1;
        // The suffix of the rune texts of this weapon (see ImbuementHelper.GetNameKey). When a staff mod forgets it the runes show their default text
        public string m_weaponType = WeaponType.None;
        // The ids of the runes this weapon supports (see RuneId). Empty by default, so a forgotten list shows up as runes that can not be slotted
        public List<string> m_allowedRunes = new List<string>();

        public bool IsRuneAllowed(string runeId)
        {
            return m_allowedRunes != null && m_allowedRunes.Contains(runeId);
        }
    }
}
