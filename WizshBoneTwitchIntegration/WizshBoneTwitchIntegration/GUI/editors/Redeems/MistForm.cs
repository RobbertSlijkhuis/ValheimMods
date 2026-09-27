using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Mist - all 4 fields are live, nothing to cut. No natural pairs, plain alphabetical order.</summary>
    internal class MistForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_height;
        private InputField m_radius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "MistScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.mistData.announceMessage = v, defaultValue: "");
            m_duration = layout.IntRow("Duration", "How long the mist lasts, in seconds (0 = indefinite).",
                60, v => m_working.mistData.duration = v, defaultValue: 60);
            m_height = layout.FloatRow("Height", "Vertical size of the mist cloud, in meters.",
                15f, v => m_working.mistData.height = v, defaultValue: 15f);
            m_radius = layout.FloatRow("Radius", "Horizontal size of the mist cloud, in meters.",
                60f, v => m_working.mistData.radius = v, defaultValue: 60f);

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
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
    }
}
