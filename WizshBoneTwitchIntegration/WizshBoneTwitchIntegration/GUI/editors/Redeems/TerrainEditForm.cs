using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for TerrainEdit - scope deliberately narrowed to the "raise" operation only for
    /// v1 (level/smooth/paint are excluded entirely and left at their data-class defaults; `shape`
    /// stays locked to "Circle", `positionOffset` stays zero/on-player). Implements
    /// <see cref="IForcesValuesOnSave"/> to force `raise = true` on save, since this form never
    /// shows a toggle for it - level/smooth/paint/shape/positionOffset are deliberately NOT
    /// force-reset here, so a redeem previously configured via GUI_OLD's fuller editor doesn't
    /// silently lose that configuration just from being opened in this narrower form.
    /// </summary>
    internal class TerrainEditForm : IRedeemStep2Form, IForcesValuesOnSave
    {
        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private Toggle m_noFallDamage;
        private InputField m_raiseDelta;
        private InputField m_raisePower;
        private InputField m_raiseRadius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "TerrainEditScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.terrainEditData.announceMessage = v, defaultValue: "");
            layout.PairRow(
                () => m_duration = layout.FloatRow("Duration", "How long before the terrain resets, in seconds (0 = permanent).",
                    0f, v => m_working.terrainEditData.duration = v, defaultValue: 0f),
                () => m_noFallDamage = layout.ToggleRow("No fall damage", "Whether to protect the player from fall damage during the terrain edit.",
                    true, v => m_working.terrainEditData.noFallDamage = v, defaultValue: true));
            layout.PairRow(
                () => m_raiseDelta = layout.FloatRow("Raise delta", "How much the terrain is raised (negative values dig down instead).",
                    -6f, v => m_working.terrainEditData.raiseDelta = v, defaultValue: -6f),
                () => m_raisePower = layout.FloatRow("Raise power", "The power of the terrain raise.",
                    0f, v => m_working.terrainEditData.raisePower = v, defaultValue: 0f));
            m_raiseRadius = layout.FloatRow("Raise radius", "The radius of the terrain raise, in meters.",
                8f, v => m_working.terrainEditData.raiseRadius = v, defaultValue: 8f);

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            TerrainEditData data = working.terrainEditData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString("G");
            m_noFallDamage.isOn = data.noFallDamage;
            m_raiseDelta.text = data.raiseDelta.ToString("G");
            m_raisePower.text = data.raisePower.ToString("G");
            m_raiseRadius.text = data.raiseRadius.ToString("G");
        }

        public void ApplyForcedValues(RedeemData working)
        {
            working.terrainEditData.raise = true;
        }
    }
}
