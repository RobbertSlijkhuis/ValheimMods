using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Flashbang - all 6 fields are live, nothing to cut. Flat single column, no section headers.</summary>
    internal class FlashbangForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private InputField m_delay;
        private GameObject m_flashColorSwatch;
        private InputField m_flashStart;
        private InputField m_flashDuration;
        private InputField m_flashEnd;
        private InputField m_soundVolume;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "FlashbangScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_delay = layout.FloatRow("Delay before flash", "Seconds before the flash happens.",
                0.5f, v => m_working.flashbangData.delay = v, defaultValue: 0.5f);
            m_flashColorSwatch = layout.ColorRow("Flash color", "The color of the flash.",
                "#ffffff", v => m_working.flashbangData.flashColor = v, defaultValue: "#ffffff");
            m_flashStart = layout.FloatRow("Fade in duration", "How long the flash takes to fade in, in seconds.",
                0f, v => m_working.flashbangData.flashStartDuration = v, defaultValue: 0f);
            m_flashDuration = layout.FloatRow("Flash hold duration", "How long the full flash effect lasts, in seconds.",
                2f, v => m_working.flashbangData.flashDuration = v, defaultValue: 2f);
            m_flashEnd = layout.FloatRow("Fade out duration", "How long the flash takes to fade out, in seconds.",
                5.8f, v => m_working.flashbangData.flashEndDuration = v, defaultValue: 5.8f);
            m_soundVolume = layout.FloatRow("Sound volume", "Volume of the flash sound effect, from 0.0 (silent) to 1.0 (full volume).",
                0.7f, v => m_working.flashbangData.soundVolume = v, defaultValue: 0.7f);

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            FlashBangData data = working.flashbangData;

            m_delay.text = data.delay.ToString("G");
            if (ColorUtility.TryParseHtmlString(data.flashColor, out Color color))
                m_flashColorSwatch.transform.Find("ColorOverlay").GetComponent<Image>().color = color;
            m_flashStart.text = data.flashStartDuration.ToString("G");
            m_flashDuration.text = data.flashDuration.ToString("G");
            m_flashEnd.text = data.flashEndDuration.ToString("G");
            m_soundVolume.text = data.soundVolume.ToString("G");
        }
    }
}
