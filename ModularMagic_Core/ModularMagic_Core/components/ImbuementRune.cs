using ModularMagic_Core.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// The definition of a rune, added to the prefab of every rune at startup. There is one prefab per rune, the
    /// level of a rune is the quality of the item (ItemData.m_quality), so it is not part of the definition.
    /// </summary>
    public class ImbuementRune : MonoBehaviour
    {
        public List<string> m_allowedWeapons = new List<string>();
        public string m_descriptionKey;
        public string m_id;
        public int m_maxLevel = 1;
        public string m_nameKey;
        // The slot tier that is needed to slot the rune, per level (index is the level minus one)
        public int[] m_tiers = new int[] { 1 };
        public string m_type;
        public string m_value;

        public void Init(string id, string type, string value, int maxLevel, int[] tiers, List<string> allowedWeapons, string nameKey, string descriptionKey)
        {
            m_allowedWeapons = allowedWeapons;
            m_descriptionKey = descriptionKey;
            m_id = id;
            m_maxLevel = maxLevel;
            m_nameKey = nameKey;
            m_tiers = tiers;
            m_type = type;
            m_value = value;
        }

        // The tier a slot needs to hold this rune at the given level
        public int GetRequiredTier(int level)
        {
            if (m_tiers == null || m_tiers.Length == 0)
                return 1;

            return m_tiers[Mathf.Clamp(level, 1, m_tiers.Length) - 1];
        }

        public bool IsValidLevel(int level)
        {
            return level >= 1 && level <= m_maxLevel;
        }

        public string GetName(string weaponType)
        {
            return Localization.instance.Localize($"${m_nameKey}_{(weaponType ?? WeaponType.None).ToLower()}");
        }

        public string GetDescription(string weaponType)
        {
            return Localization.instance.Localize($"${m_descriptionKey}_{(weaponType ?? WeaponType.None).ToLower()}");
        }
    }
}
