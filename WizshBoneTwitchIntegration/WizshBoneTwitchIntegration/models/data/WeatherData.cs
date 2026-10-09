using System.Collections.Generic;
using WizshBoneTwitchIntegration.GuiOld;

namespace WizshBoneTwitchIntegration.Models
{
    internal class WeatherData : CloneableData
    {
        [EditorLabel("Announce message")]
        [EditorTooltip("A custom message to announce that a weather event is about to occur. {{user}} will be replaced with the player's name.")]
        public string announceMessage;

        // Cycle mode: step through `items` in order instead of picking one at random.
        public bool cycle = false;
        public float cycleInterval = 10f;

        // The zone follows the redeemer instead of staying where it spawned.
        public bool followPlayer = false;

        [EditorLabel("Duration")]
        [EditorTooltip("The duration of the weather event in seconds (0 = indefinite).")]
        public int duration = 60;

        [EditorLabel("Force")] 
        [EditorTooltip("Whether to force the weather event to occur.")]
        public bool force = false;

        [EditorLabel("Height")] 
        [EditorTooltip("The height of the weather event in meters.")]
        public float height = 50;

        [EditorLabel("Radius")] 
        [EditorTooltip("The radius of the weather event in meters.")]
        public float radius = 200f;

        [EditorLabel("Weathers")]
        [EditorTooltip("A list of weather types that can occur during this event.")]
        public List<string> items = new List<string>();
        public WeatherData() { }
    }
}
