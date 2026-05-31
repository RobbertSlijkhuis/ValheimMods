using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class WeatherData : CloneableData
    {
        [EditorLabel("Announce message")] public string announceMessage;
        [EditorLabel("Duration")] public int duration = 60;
        [EditorLabel("Force")] public bool force = false;
        [EditorLabel("Height")] public float height = 200f;
        [EditorLabel("Radius")] public float radius = 60f;
        [EditorLabel("Weathers")] public List<string> items = new List<string>();
        public WeatherData() { }
    }
}
