using ModularMagic_Utilities.Types;
using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class WeatherZoneConfigOptions
    {
        public string sectionName;

        public bool enable = true;
        public string weatherName;
        public string prefabName;
        public string lightColorPreset = LightColorPresetType.White;
        public int order;

        public WeatherZoneConfigOptions(string sectionName)
        {
            this.sectionName = $"{sectionName.Replace("'", "")}";
        }
    }
}
