using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Doors of Doom"
    /// redeem variant - exposes only the fields relevant to toggling doors, while every
    /// edit writes straight through to the real <see cref="SpawnAbilityData"/> instance.
    /// </summary>
    internal sealed class DoorView
    {
        [EditorLabel("Door interval")]
        [EditorTooltip("Interval in seconds at which spawned doors toggle open/closed (0 = disabled).")]
        public BoundField<float> doorInterval;

        [EditorLabel("Duration")]
        [EditorTooltip("How long the effect lasts, in seconds (0 = indefinite).")]
        public BoundField<int> duration;

        [EditorLabel("Min to spawn")]
        public BoundField<int?> minToSpawn;

        [EditorLabel("Max to spawn")]
        public BoundField<int?> maxToSpawn;

        [EditorLabel("Spawn radius")]
        public BoundField<float?> spawnRadius;

        [EditorLabel("Spawn delay")]
        [EditorTooltip("Delay in seconds before each door is spawned.")]
        public BoundField<float?> spawnDelay;

        [EditorLabel("Ground offset")]
        [EditorTooltip("Vertical offset from the ground at which the prefab spawn.")]
        public BoundField<float?> groundOffset;

        [EditorLabel("Allow drops")]
        [EditorTooltip("Whether the spawned prefabs can drop items when destroyed.")]
        public BoundField<bool> allowDrops;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DoorView(SpawnAbilityData real)
        {
            doorInterval = new BoundField<float>(
                () => real.doorInterval,
                v => real.doorInterval = v);

            duration = new BoundField<int>(
                () => real.duration,
                v => real.duration = v);

            minToSpawn = new BoundField<int?>(
                () => real.minToSpawn,
                v => real.minToSpawn = v);

            maxToSpawn = new BoundField<int?>(
                () => real.maxToSpawn,
                v => real.maxToSpawn = v);

            spawnRadius = new BoundField<float?>(
                () => real.spawnRadius,
                v => real.spawnRadius = v);

            spawnDelay = new BoundField<float?>(
                () => real.spawnDelay,
                v => real.spawnDelay = v);

            groundOffset = new BoundField<float?>(
                () => real.groundOffset,
                v => real.groundOffset = v);

            allowDrops = new BoundField<bool>(
                () => real.allowDrops,
                v => real.allowDrops = v);

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);
        }
    }
}
