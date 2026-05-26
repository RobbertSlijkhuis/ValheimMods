using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class DetonateEditor : BaseRedeemEditor
    {
        private InputField m_announceMessage;
        private InputField m_radius;
        private InputField m_onlySpawned;

        public override void BuildUI(GameObject parent)
        {
            float y = 0f;

            m_announceMessage = AddRow(parent, "Announce message",         ref y);
            m_radius          = AddRow(parent, "Radius",                   ref y);
            m_onlySpawned     = AddRow(parent, "Only spawned (true/false)", ref y);

            AddNote(parent, "Values list: configure in YAML", ref y);

            Reset();
        }

        public override void ApplyTo(RedeemData target)
        {
            target.detonateData.announceMessage = m_announceMessage.text.Trim();
            target.detonateData.radius          = GetFloatOr(m_radius,  20f);
            target.detonateData.onlySpawned     = GetBool(m_onlySpawned);
        }

        public override void Reset()
        {
            m_announceMessage.text = "";
            m_radius.text          = "20";
            m_onlySpawned.text     = "false";
        }
    }
}