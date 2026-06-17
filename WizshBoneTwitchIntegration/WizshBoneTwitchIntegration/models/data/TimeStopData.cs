using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TimeStopData : CloneableData
    {
        [EditorLabel("Duration")]
        [EditorTooltip("How long the time stop lasts in seconds.")]
        public float duration = 10f;

        [EditorLabel("Freeze enemies")]
        [EditorTooltip("Whether nearby enemies are frozen.")]
        public bool freezeEnemies = true;

        [EditorLabel("Freeze player")]
        [EditorTooltip("Whether the player is also frozen during the time stop.")]
        public bool freezePlayer = false;

        [EditorLabel("Radius")]
        [EditorTooltip("Max distance from the player to freeze enemies in meters. 0 = unlimited.")]
        public float radius = 30f;

        [EditorLabel("Announce message")]
        [EditorTooltip("Message shown on screen when triggered. Use {{user}} for the redeemer's name.")]
        public string announceMessage = "";

        public TimeStopData() { }
    }
}
