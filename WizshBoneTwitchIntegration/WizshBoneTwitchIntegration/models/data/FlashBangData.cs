using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class FlashBangData : CloneableData
    {
        [EditorLabel("Delay")]
        [EditorTooltip("Delay before the flash happens in seconds.")]
        public float delay = 0.5f;

        [EditorLabel("Flash color")]
        [EditorTooltip("The color of the flash in hex format (e.g. #ffffff for white).")]
        [ColorPicker]
        public string flashColor = "#ffffff";

        [EditorLabel("Flash Duration")]
        [EditorTooltip("The duration of the full flash effect in seconds.")]
        public float flashDuration = 2f;

        [EditorLabel("Fade out duration")]
        [EditorTooltip("The duration of the flash fading out in seconds.")]
        public float flashEndDuration = 5.8f;
        [EditorLabel("Fade in duration")]
        [EditorTooltip("The duration of the flash fading in seconds.")]
        public float flashStartDuration = 0f;

        [EditorLabel("Sound volume")]
        [EditorTooltip("The volume of the flash sound effect, ranging from 0.0 (silent) to 1.0 (full volume).")]
        public float soundVolume = 0.7f;

        public FlashBangData() { }
    }
}
