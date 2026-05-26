using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class FlashbangEditor : BaseRedeemEditor
    {
        private InputField m_announceMessage;
        private InputField m_flashColor;
        private InputField m_flashDuration;
        private InputField m_flashStartDuration;
        private InputField m_flashEndDuration;
        private InputField m_delay;
        private InputField m_soundVolume;

        public override void BuildUI(GameObject parent)
        {
            float y = 0f;

            m_announceMessage    = AddRow(parent, "Announce message",         ref y);
            m_flashColor         = AddRow(parent, "Flash color (#hex)",       ref y);
            m_flashDuration      = AddRow(parent, "Flash duration (s)",       ref y);
            m_flashStartDuration = AddRow(parent, "Flash start duration (s)", ref y);
            m_flashEndDuration   = AddRow(parent, "Flash end duration (s)",   ref y);
            m_delay              = AddRow(parent, "Delay (s)",                ref y);
            m_soundVolume        = AddRow(parent, "Sound volume (0-1)",       ref y);

            Reset();
        }

        public override void ApplyTo(RedeemData target)
        {
            target.flashbangData.announceMessage    = m_announceMessage.text.Trim();
            target.flashbangData.flashColor         = GetStringOr(m_flashColor,         "#fff");
            target.flashbangData.flashDuration      = GetFloatOr(m_flashDuration,       2f);
            target.flashbangData.flashStartDuration = GetFloatOr(m_flashStartDuration,  0f);
            target.flashbangData.flashEndDuration   = GetFloatOr(m_flashEndDuration,    5.8f);
            target.flashbangData.delay              = GetFloatOr(m_delay,               0.5f);
            target.flashbangData.soundVolume        = GetFloatOr(m_soundVolume,         0.7f);
        }

        public override void Reset()
        {
            m_announceMessage.text    = "";
            m_flashColor.text         = "#fff";
            m_flashDuration.text      = "2";
            m_flashStartDuration.text = "0";
            m_flashEndDuration.text   = "5.8";
            m_delay.text              = "0.5";
            m_soundVolume.text        = "0.7";
        }
    }
}