using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Meteors" redeem
    /// variant (covers both the "fader" and "Yagluth" meteor flavors) - exposes only the
    /// common spawn-ability fields, while every edit writes straight through to the real
    /// <see cref="SpawnAbilityData"/> instance.
    /// </summary>
    internal sealed class MeteorView
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
        [EditorTooltip("Delay in seconds between each meteor falling.")]
        public BoundField<float?> spawnDelay;

        [EditorLabel("Accuracy")]
        [EditorTooltip("Accuracy of the spawned prefabs.")]
        public BoundField<float?> accuracy;

        [EditorLabel("Ground offset")]
        [EditorTooltip("Vertical offset from the ground at which the prefab spawn.")]
        public BoundField<float?> groundOffset;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DamageData damage;

        public MeteorView(SpawnAbilityData real)
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

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);

            damage = real.damage ?? (real.damage = new DamageData());
        }
    }
}
