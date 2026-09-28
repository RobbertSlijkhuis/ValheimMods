using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Detonate. Only Announcement + Radius/Damage terrain (paired) are shown for
    /// now - Type and Values are hidden entirely, which is safe to do with zero forcing code since
    /// <see cref="DetonateData"/>'s own class defaults already are <c>type = Fish</c> and
    /// <c>values = ["Fish"]</c>; with no UI left that can change <c>type</c> away from Fish, those
    /// defaults hold forever. The Damage tab is hidden for now too (no tabs at all).
    /// </summary>
    internal class DetonateForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private InputField m_announceMessage;
        private Toggle m_damageTerrain;
        private InputField m_radius;

        public void Build(GameObject parent)
        {
            // Only 2 card rows - never needs scrolling, so skip ScrollableList entirely rather
            // than reserving scrollbar width for a bar that would never appear.
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "DetonateContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.detonateData.announceMessage = v, defaultValue: "");
            layout.PairRow(
                () => m_radius = layout.FloatRow("Radius", "How far around the target to search for objects to detonate, in meters.",
                    20f, v => m_working.detonateData.radius = v, defaultValue: 20f),
                () => m_damageTerrain = layout.ToggleRow("Damage terrain", "Whether the explosion is allowed to dig/scorch terrain. Disabling avoids the heightmap edit, which helps performance on large detonations.",
                    true, v => m_working.detonateData.damageTerrain = v, defaultValue: true));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            DetonateData data = working.detonateData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_radius.text = data.radius.ToString("G");
            m_damageTerrain.isOn = data.damageTerrain;
        }
    }
}
