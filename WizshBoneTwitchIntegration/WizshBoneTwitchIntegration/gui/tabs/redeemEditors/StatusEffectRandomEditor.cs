using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class StatusEffectRandomEditor : BaseRedeemEditor
    {
        public override void BuildUI(GameObject parent)
        {
            float y = 0f;
            AddNote(parent, "No extra settings for this type.", ref y);
        }

        public override void ApplyTo(RedeemData target) { }

        public override void Reset() { }
    }
}