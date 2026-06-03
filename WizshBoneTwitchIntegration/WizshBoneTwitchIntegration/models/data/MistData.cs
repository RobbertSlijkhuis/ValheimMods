using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class MistData : CloneableData
    {
        [EditorLabel("Announce message")]
        [EditorTooltip("A custom message to announce that a mist is about to occur. {{user}} will be replaced with the player's name.")]
        public string announceMessage;

        [EditorLabel("Duration")]
        [EditorTooltip("The duration of the mist in seconds.")]
        public int duration = 60;

        [EditorLabel("Height")]
        [EditorTooltip("The height of the mist in meters.")]
        public float height = 15f;

        [EditorLabel("Radius")]
        [EditorTooltip("The radius of the mist in meters.")]
        public float radius = 60f;

        public MistData() { }
    }
}
