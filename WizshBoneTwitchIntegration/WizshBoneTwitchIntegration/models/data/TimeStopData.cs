using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TimeStopData : CloneableData
    {
        [EditorLabel("Duration")]
        [EditorTooltip("How long the time stop zone stays active, in seconds.")]
        public float duration = 30f;

        [EditorLabel("Freeze enemies")]
        [EditorTooltip("Whether enemies that enter the zone are frozen.")]
        public bool freezeEnemies = true;

        [EditorLabel("Freeze player")]
        [EditorTooltip("Whether the player is also frozen while inside the zone.")]
        public bool freezePlayer = false;

        [EditorLabel("Freeze projectiles")]
        [EditorTooltip("Whether projectiles and other physics props (logs, debris, etc.) entering the zone are frozen.")]
        public bool freezeProjectiles = true;

        [EditorLabel("Radius")]
        [EditorTooltip("Radius of the time stop zone in meters.")]
        public float radius = 10f;

        [EditorLabel("Announce message")]
        [EditorTooltip("Message shown on screen when triggered. Use {{user}} for the redeemer's name.")]
        public string announceMessage = "";

        public TimeStopData() { }
    }
}
