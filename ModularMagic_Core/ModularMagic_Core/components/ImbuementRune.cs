using ModularMagic_Core.Helpers;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// The definition of a rune, added to the prefab of every rune at startup. There is one prefab per rune, the
    /// level of a rune is the quality of the item (ItemData.m_quality), so it is not part of the definition.
    /// Which weapons support the rune is decided by the weapon (ImbuementSlots.m_allowedRunes).
    /// </summary>
    public class ImbuementRune : MonoBehaviour
    {
        // The English text that is shown when the weapon has no text for the rune
        public string m_defaultDescription;
        public string m_defaultName;
        public string m_id;
        public int m_maxLevel = 1;
        // The slot tier that is needed to slot the rune, per level (index is the level minus one)
        public int[] m_tiers = new int[] { 1 };
        public string m_type;
        public string m_value;

        public void Init(string id, string type, string value, int maxLevel, int[] tiers, string defaultName, string defaultDescription)
        {
            m_defaultDescription = defaultDescription;
            m_defaultName = defaultName;
            m_id = id;
            m_maxLevel = maxLevel;
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

        // The text of the weapon type (added by the staff mod), or the default text when the weapon has none
        public string GetName(string weaponType)
        {
            return Translate(ImbuementHelper.GetNameKey(m_id, weaponType), m_defaultName);
        }

        public string GetDescription(string weaponType)
        {
            return Translate(ImbuementHelper.GetDescriptionKey(m_id, weaponType), m_defaultDescription);
        }

        private static string Translate(string key, string fallback)
        {
            string text = Localization.instance.Localize($"${key}");

            // The game shows a missing key as [key]
            return text == $"[{key}]" ? fallback : text;
        }
    }
}
