using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class WeatherData
    {
        public string announceMessage;
        public bool attach = false;
        public int duration = 60;
        public bool force = false;
        public float height = 200f;
        public float radius = 60f;
        public List<string> items = new List<string>();

        public WeatherData() { }
    }
}
