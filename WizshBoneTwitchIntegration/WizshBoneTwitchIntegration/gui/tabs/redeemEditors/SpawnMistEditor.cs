using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class SpawnMistEditor : BaseRedeemEditor
    {
        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_radius;
        private InputField m_height;

        public override void BuildUI(GameObject parent)
        {
            float y = 0f;

            m_announceMessage = AddRow(parent, "Announce message", ref y);
            m_duration        = AddRow(parent, "Duration (s)",     ref y);
            m_radius          = AddRow(parent, "Radius",           ref y);
            m_height          = AddRow(parent, "Height",           ref y);

            Reset();
        }

        public override void ApplyTo(RedeemData target)
        {
            target.mistData.announceMessage = m_announceMessage.text.Trim();
            target.mistData.duration        = GetIntOr(m_duration,  60);
            target.mistData.radius          = GetFloatOr(m_radius,  60f);
            target.mistData.height          = GetFloatOr(m_height,  15f);
        }

        public override void Reset()
        {
            m_announceMessage.text = "";
            m_duration.text        = "60";
            m_radius.text          = "60";
            m_height.text          = "15";
        }
    }
}