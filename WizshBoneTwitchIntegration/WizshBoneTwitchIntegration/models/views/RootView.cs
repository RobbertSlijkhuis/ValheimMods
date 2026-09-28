using System.Collections.Generic;
using WizshBoneTwitchIntegration.GuiOld;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Roots on the
    /// line" redeem variant - exposes only the common spawn-ability fields, while every
    /// edit writes straight through to the real <see cref="SpawnAbilityData"/> instance.
    /// </summary>
    internal sealed class RootView
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
        [EditorTooltip("Delay in seconds before each root is spawned.")]
        public BoundField<float?> spawnDelay;

        [EditorLabel("Batch size")]
        [EditorTooltip("Number of roots spawned at once.")]
        public BoundField<int> batchSize;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DamageData damage;

        public RootView(SpawnAbilityData real)
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

            batchSize = new BoundField<int>(
                () => real.batchSize,
                v => real.batchSize = v);

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);

            damage = real.damage ?? (real.damage = new DamageData());

            // No other vanilla prefab fits "roots on the line" - hardcode TentaRoot rather than
            // trusting the generic spawns list, so editing the YAML by hand (or leaving it unset,
            // which previously fell back to SpawnAbilityHelper's fish-rain default) can't point
            // this redeem at an arbitrary/broken prefab.
            real.spawns = new List<string> { "TentaRoot" };
        }
    }
}
