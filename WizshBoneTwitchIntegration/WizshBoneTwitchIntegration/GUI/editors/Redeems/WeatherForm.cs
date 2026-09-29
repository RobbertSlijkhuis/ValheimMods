using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Weather - all 6 fields are live, nothing to cut. Height+Radius and Duration+Force paired. Items uses the SearchableChecklist widget sourced from RedeemPrefabCatalog.WeatherNames.</summary>
    internal class WeatherForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} is changing the skybox!";
        private const int DefaultDuration = 300;
        private const bool DefaultForce = false;
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
        private InputField m_height;
        private InputField m_radius;
        private SearchableChecklist m_items;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "WeatherScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
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
            m_items = layout.ChecklistRow("Weathers", "Which weather types can be randomly picked when this redeem fires.",
                RedeemPrefabCatalog.WeatherNames, new List<string>(DefaultItems), v => m_working.weatherData.items = v, defaultValues: new List<string>(DefaultItems));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            WeatherData data = working.weatherData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_force.isOn = data.force;
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
            data.height = DefaultHeight;
            data.radius = DefaultRadius;
            data.items = new List<string>(DefaultItems);
        }
    }
}
