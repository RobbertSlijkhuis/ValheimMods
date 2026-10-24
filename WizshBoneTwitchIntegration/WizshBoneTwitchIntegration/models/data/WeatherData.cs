using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class WeatherData : CloneableData
    {
        public string announceMessage;

        // Cycle mode: step through `items` in order instead of picking one at random.
        public bool cycle = false;
        public float cycleInterval = 10f;

        // The zone follows the redeemer instead of staying where it spawned.
        public bool followPlayer = false;

        // Frost resistance for anyone standing in the zone, so cold weathers don't deal weather damage.
        public bool frostResist = false;

        public int duration = 60;

        public bool force = false;

        public float height = 50;

        public float radius = 200f;

        public List<string> items = new List<string>();
        public WeatherData() { }
    }
}
