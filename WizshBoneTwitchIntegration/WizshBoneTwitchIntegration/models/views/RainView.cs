using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Log rain" redeem
    /// variant - exposes only the common spawn-ability fields, while every edit writes
    /// straight through to the real <see cref="SpawnAbilityData"/> instance.
    /// </summary>
    internal sealed class RainView
    {
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
        [EditorTooltip("Delay in seconds between each log falling.")]
        public BoundField<float?> spawnDelay;

        [EditorLabel("Accuracy")]
        [EditorTooltip("Accuracy of the spawned prefabs.")]
        public BoundField<float?> accuracy;

        [EditorLabel("Ground offset")]
        [EditorTooltip("Vertical offset from the ground at which the prefab spawn.")]
        public BoundField<float?> groundOffset;

        [EditorLabel("Allow drops")]
        [EditorTooltip("Whether the spawned prefabs can drop items when destroyed.")]
        public BoundField<bool> allowDrops;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DamageData damage;

        public RainView(SpawnAbilityData real)
        {
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

            accuracy = new BoundField<float?>(
                () => real.accuracy,
                v => real.accuracy = v);

            groundOffset = new BoundField<float?>(
                () => real.groundOffset,
                v => real.groundOffset = v);

            allowDrops = new BoundField<bool>(
                () => real.allowDrops,
                v => real.allowDrops = v);

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);

            damage = real.damage ?? (real.damage = new DamageData());
        }
    }
}
