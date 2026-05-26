using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class SpawnWeatherEditor : BaseRedeemEditor
    {
        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_radius;
        private InputField m_height;
        private InputField m_force;
        private InputField m_attach;

        public override void BuildUI(GameObject parent)
        {
            float y = 0f;

            m_announceMessage = AddRow(parent, "Announce message",    ref y);
            m_duration        = AddRow(parent, "Duration (s)",        ref y);
            m_radius          = AddRow(parent, "Radius",              ref y);
            m_height          = AddRow(parent, "Height",              ref y);
            m_force           = AddRow(parent, "Force (ignore env)",  ref y);
            m_attach          = AddRow(parent, "Attach to player",    ref y);

            AddNote(parent, "Items list: configure in YAML", ref y);

            Reset();
        }

        public override void ApplyTo(RedeemData target)
        {
            target.weatherData.announceMessage = m_announceMessage.text.Trim();
            target.weatherData.duration        = GetIntOr(m_duration,   60);
            target.weatherData.radius          = GetFloatOr(m_radius,   60f);
            target.weatherData.height          = GetFloatOr(m_height,   200f);
            target.weatherData.force           = GetBool(m_force);
            target.weatherData.attach          = GetBool(m_attach);
        }

        public override void Reset()
        {
            m_announceMessage.text = "";
            m_duration.text        = "60";
            m_radius.text          = "60";
            m_height.text          = "200";
            m_force.text           = "false";
            m_attach.text          = "false";
        }
    }
}