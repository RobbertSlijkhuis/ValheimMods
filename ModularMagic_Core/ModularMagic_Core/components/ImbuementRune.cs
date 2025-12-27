using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    internal class ImbuementRune : MonoBehaviour
    {
        public List<string> m_allowedWeapons = new List<string>();
        public string m_descriptionKey;
        public int m_level;
        public string m_nameKey;
        public int m_tier;
        public string m_type;
        public string m_value;

        public void Init(string type, string value, int tier, int level, List<string> allowedWeapons, string nameKey, string descriptionKey)
        {
            m_allowedWeapons = allowedWeapons;
            m_descriptionKey = descriptionKey;
            m_level = level;
            m_nameKey = nameKey;
            m_tier = tier;
            m_type = type;
            m_value = value;
        }
    }
}
