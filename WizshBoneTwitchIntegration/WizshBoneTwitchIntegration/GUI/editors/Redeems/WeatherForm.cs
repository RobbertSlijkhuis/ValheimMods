using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Weather - all 10 fields are live, nothing to cut. Height+Radius, Duration+Force and Cycle+Time per weather paired. Items uses the SearchableChecklist widget sourced from RedeemPrefabCatalog.WeatherNames.</summary>
    internal class WeatherForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} is changing the skybox!";
        private const int DefaultDuration = 300;
        private const bool DefaultForce = false;
        private const bool DefaultCycle = false;
        private const float DefaultCycleInterval = 10f;
        private const bool DefaultFollowPlayer = false;
        private const bool DefaultFrostResist = false;
        private const float DefaultHeight = 100f;
        private const float DefaultRadius = 300f;

        internal static readonly string[] DefaultItems =
        {
            "Clear", "LightRain", "Rain", "Misty", "ThunderStorm", "nofogts", "SwampRain",
            "Mistlands_clear", "Mistlands_rain", "Mistlands_thunder", "Twilight_Clear", "Heath clear",
            "Eikthyr", "GDKing", "Bonemass", "GoblinKing", "Queen", "Fader", "Ghosts",
        };

        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private Toggle m_force;
        private Toggle m_cycle;
        private InputField m_cycleInterval;
        private Toggle m_followPlayer;
        private Toggle m_frostResist;
        private InputField m_height;
        private InputField m_radius;
        private SearchableChecklist m_items;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "WeatherScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_items = layout.ChecklistRow("Weathers", "Which weather types this redeem uses: one is picked at random, or all of them in order when cycling.",
                RedeemPrefabCatalog.WeatherNames, new List<string>(DefaultItems), v => m_working.weatherData.items = v, defaultValues: new List<string>(DefaultItems));
            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.weatherData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_duration = layout.IntRow("Duration", "How long the weather event lasts, in seconds (0 = indefinite).",
                    DefaultDuration, v => m_working.weatherData.duration = v, defaultValue: DefaultDuration),
                () => m_force = layout.ToggleRow("Force", "Whether to force this weather event to take over immediately.",
                    DefaultForce, v => m_working.weatherData.force = v, defaultValue: DefaultForce));
            layout.PairRow(
                () => m_height = layout.FloatRow("Height", "Vertical size of the weather zone, in meters.",
                    DefaultHeight, v => m_working.weatherData.height = v, defaultValue: DefaultHeight),
                () => m_radius = layout.FloatRow("Radius", "Horizontal size of the weather zone, in meters.",
                    DefaultRadius, v => m_working.weatherData.radius = v, defaultValue: DefaultRadius));
            layout.PairRow(
                () => m_cycle = layout.ToggleRow("Cycle weathers", "Instead of picking one random weather, step through the list in order and keep looping until the duration ends.",
                    DefaultCycle, v => m_working.weatherData.cycle = v, defaultValue: DefaultCycle),
                () => m_cycleInterval = layout.FloatRow("Time per weather", "How long each weather is shown while cycling, in seconds (minimum 0.1). Weathers blend over a few seconds, so very short values won't look distinct.",
                    DefaultCycleInterval, v => m_working.weatherData.cycleInterval = v, defaultValue: DefaultCycleInterval, min: 0.1f));
            layout.PairRow(
                () => m_followPlayer = layout.ToggleRow("Follow player", "The weather zone follows the redeemer instead of staying where it spawned, so the weather keeps applying wherever they go.",
                    DefaultFollowPlayer, v => m_working.weatherData.followPlayer = v, defaultValue: DefaultFollowPlayer),
                () => m_frostResist = layout.ToggleRow("Frost resistance", "Gives frost resistance to anyone standing in the zone, so cold weathers (snow, blizzards) can't freeze them. Wears off moments after they leave the zone or it ends.",
                    DefaultFrostResist, v => m_working.weatherData.frostResist = v, defaultValue: DefaultFrostResist));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            WeatherData data = working.weatherData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_force.isOn = data.force;
            m_cycle.isOn = data.cycle;
            m_cycleInterval.text = data.cycleInterval.ToString("G");
            m_followPlayer.isOn = data.followPlayer;
            m_frostResist.isOn = data.frostResist;
            m_height.text = data.height.ToString("G");
            m_radius.text = data.radius.ToString("G");

            List<string> savedItems = data.items ?? new List<string>();
            List<DropdownOption> options = RedeemPrefabCatalog.WeatherNames;
            foreach (string value in savedItems)
                options = RedeemPrefabCatalog.EnsureIncludesCurrentValue(options, value);
            m_items.SetOptions(options, savedItems);
        }

        public void ApplyDefaults(RedeemData working)
        {
            WeatherData data = working.weatherData;
            data.announceMessage = DefaultAnnounceMessage;
            data.duration = DefaultDuration;
            data.force = DefaultForce;
            data.cycle = DefaultCycle;
            data.cycleInterval = DefaultCycleInterval;
            data.followPlayer = DefaultFollowPlayer;
            data.frostResist = DefaultFrostResist;
            data.height = DefaultHeight;
            data.radius = DefaultRadius;
            data.items = new List<string>(DefaultItems);
        }
    }
}
