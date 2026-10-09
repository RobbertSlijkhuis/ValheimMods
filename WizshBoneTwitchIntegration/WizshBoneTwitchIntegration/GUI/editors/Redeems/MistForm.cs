using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Mist - all 4 fields are live, nothing to cut. Height+Radius paired.</summary>
    internal class MistForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} made things slightly harder to see!";
        private const int DefaultDuration = 90;
        private const float DefaultHeight = 15f;
        private const float DefaultRadius = 65f;

        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_height;
        private InputField m_radius;

        public void Build(GameObject parent)
        {
            // Only 3 card rows - never needs scrolling, so skip ScrollableList entirely rather
            // than reserving scrollbar width for a bar that would never appear.
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "MistContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.mistData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            m_duration = layout.IntRow("Duration", "How long the spawned mist lasts before it cleans itself up, in seconds (0 = indefinite).",
                DefaultDuration, v => m_working.mistData.duration = v, defaultValue: DefaultDuration);
            layout.PairRow(
                () => m_height = layout.FloatRow("Height", "Vertical size of the mist cloud, in meters.",
                    DefaultHeight, v => m_working.mistData.height = v, defaultValue: DefaultHeight),
                () => m_radius = layout.FloatRow("Radius", "Horizontal size of the mist cloud, in meters.",
                    DefaultRadius, v => m_working.mistData.radius = v, defaultValue: DefaultRadius));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            MistData data = working.mistData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_height.text = data.height.ToString("G");
            m_radius.text = data.radius.ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            MistData data = working.mistData;
            data.announceMessage = DefaultAnnounceMessage;
            data.duration = DefaultDuration;
            data.height = DefaultHeight;
            data.radius = DefaultRadius;
        }
    }
}
