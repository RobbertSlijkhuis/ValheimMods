using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for TimeStop - all 6 fields are live, nothing to cut. Duration+Radius paired; the 3 freeze toggles stay stacked as separate rows (no 2-up precedent for 3), per project convention.</summary>
    internal class TimeStopForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} has stopped time for you!";
        private const float DefaultDuration = 15f;
        private const float DefaultRadius = 15f;
        private const bool DefaultFreezeEnemies = true;
        private const bool DefaultFreezePlayer = true;
        private const bool DefaultFreezeProjectiles = true;

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
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.timeStopData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_duration = layout.FloatRow("Duration", "How long the time stop zone stays active, in seconds.",
                    DefaultDuration, v => m_working.timeStopData.duration = v, defaultValue: DefaultDuration),
                () => m_radius = layout.FloatRow("Radius", "Radius of the time stop zone, in meters.",
                    DefaultRadius, v => m_working.timeStopData.radius = v, defaultValue: DefaultRadius));
            m_freezeEnemies = layout.ToggleRow("Freeze enemies", "Whether enemies that enter the zone are frozen.",
                DefaultFreezeEnemies, v => m_working.timeStopData.freezeEnemies = v, defaultValue: DefaultFreezeEnemies);
            m_freezePlayer = layout.ToggleRow("Freeze player", "Whether the player is also frozen while inside the zone.",
                DefaultFreezePlayer, v => m_working.timeStopData.freezePlayer = v, defaultValue: DefaultFreezePlayer);
            m_freezeProjectiles = layout.ToggleRow("Freeze projectiles", "Whether projectiles and other physics props (logs, debris, etc.) entering the zone are frozen.",
                DefaultFreezeProjectiles, v => m_working.timeStopData.freezeProjectiles = v, defaultValue: DefaultFreezeProjectiles);

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

        public void ApplyDefaults(RedeemData working)
        {
            TimeStopData data = working.timeStopData;
            data.announceMessage = DefaultAnnounceMessage;
            data.duration = DefaultDuration;
            data.radius = DefaultRadius;
            data.freezeEnemies = DefaultFreezeEnemies;
            data.freezePlayer = DefaultFreezePlayer;
            data.freezeProjectiles = DefaultFreezeProjectiles;
        }
    }
}
