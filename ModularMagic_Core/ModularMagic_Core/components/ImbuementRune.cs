using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    public class ImbuementRune : MonoBehaviour
    {
        public List<string> m_allowedWeapons = new List<string>();
        public string m_descriptionKey;
        public string m_id;
        public int m_level;
        public string m_nameKey;
        public int m_tier;
        public string m_type;
        public string m_value;

        public void Init(string id, string type, string value, int tier, int level, List<string> allowedWeapons, string nameKey, string descriptionKey)
        {
            m_allowedWeapons = allowedWeapons;
            m_descriptionKey = descriptionKey;
            m_id = id;
            m_level = level;
            m_nameKey = nameKey;
            m_tier = tier;
            m_type = type;
            m_value = value;
        }

        public string GetName(string weaponType)
        {
            return Localization.instance.Localize($"${m_nameKey}_{weaponType.ToLower()}");
        }

        public string GetDescription(string weaponType)
        {
            return Localization.instance.Localize($"${m_descriptionKey}_{weaponType.ToLower()}");
        }
    }
}
