using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class TerrainEditEditor : BaseRedeemEditor
    {
        private UnityEngine.UI.InputField m_announceMessage;

        public override void BuildUI(GameObject parent)
        {
            float y = 0f;

            m_announceMessage = AddRow(parent, "Announce message", ref y);

            Reset();
        }

        public override void ApplyTo(RedeemData target)
        {
            target.terrainEditData.announceMessage = m_announceMessage.text.Trim();
        }

        public override void Reset()
        {
            m_announceMessage.text = "";
        }
    }
}