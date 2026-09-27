using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for TimeStop - all 6 fields are live, nothing to cut. The 3 freeze toggles are stacked as separate rows (not laid out inline), per project convention.</summary>
    internal class TimeStopForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private Toggle m_freezeEnemies;
        private Toggle m_freezePlayer;
        private Toggle m_freezeProjectiles;
        private InputField m_radius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "TimeStopScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.timeStopData.announceMessage = v, defaultValue: "");
            m_duration = layout.FloatRow("Duration", "How long the time stop zone stays active, in seconds.",
                30f, v => m_working.timeStopData.duration = v, defaultValue: 30f);
            m_freezeEnemies = layout.ToggleRow("Freeze enemies", "Whether enemies that enter the zone are frozen.",
                true, v => m_working.timeStopData.freezeEnemies = v, defaultValue: true);
            m_freezePlayer = layout.ToggleRow("Freeze player", "Whether the player is also frozen while inside the zone.",
                false, v => m_working.timeStopData.freezePlayer = v, defaultValue: false);
            m_freezeProjectiles = layout.ToggleRow("Freeze projectiles", "Whether projectiles and other physics props (logs, debris, etc.) entering the zone are frozen.",
                true, v => m_working.timeStopData.freezeProjectiles = v, defaultValue: true);
            m_radius = layout.FloatRow("Radius", "Radius of the time stop zone, in meters.",
                10f, v => m_working.timeStopData.radius = v, defaultValue: 10f);

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            TimeStopData data = working.timeStopData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString("G");
            m_freezeEnemies.isOn = data.freezeEnemies;
            m_freezePlayer.isOn = data.freezePlayer;
            m_freezeProjectiles.isOn = data.freezeProjectiles;
            m_radius.text = data.radius.ToString("G");
        }
    }
}
