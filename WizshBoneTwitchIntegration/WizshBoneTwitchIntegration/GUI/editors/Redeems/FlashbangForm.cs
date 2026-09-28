using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Flashbang - only Flash color/Flash hold duration/Sound volume are shown.
    /// Delay/fade-in duration/fade-out duration are hidden, transparent since each cut field's
    /// class-level default in <see cref="FlashBangData"/> already matches what this form used to
    /// default them to.
    /// </summary>
    internal class FlashbangForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private GameObject m_flashColorSwatch;
        private InputField m_flashDuration;
        private InputField m_soundVolume;

        public void Build(GameObject parent)
        {
            // Only 3 card rows - never needs scrolling, so skip ScrollableList entirely rather
            // than reserving scrollbar width for a bar that would never appear.
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "FlashbangContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_flashColorSwatch = layout.ColorRow("Flash color", "The color of the flash.",
                "#ffffff", v => m_working.flashbangData.flashColor = v, defaultValue: "#ffffff");
            m_flashDuration = layout.FloatRow("Flash hold duration", "How long the full flash effect lasts, in seconds.",
                2f, v => m_working.flashbangData.flashDuration = v, defaultValue: 2f);
            m_soundVolume = layout.FloatRow("Sound volume", "Volume of the flash sound effect, from 0.0 (silent) to 1.0 (full volume).",
                0.7f, v => m_working.flashbangData.soundVolume = v, defaultValue: 0.7f);
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            FlashBangData data = working.flashbangData;

            if (ColorUtility.TryParseHtmlString(data.flashColor, out Color color))
                m_flashColorSwatch.transform.Find("ColorOverlay").GetComponent<Image>().color = color;
            m_flashDuration.text = data.flashDuration.ToString("G");
            m_soundVolume.text = data.soundVolume.ToString("G");
        }
    }
}
