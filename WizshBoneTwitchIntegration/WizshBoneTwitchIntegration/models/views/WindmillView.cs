using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Windmills of
    /// Death" redeem variant - exposes only the fields relevant to spinning windmills,
    /// while every edit writes straight through to the real <see cref="SpawnAbilityData"/>
    /// instance.
    /// </summary>
    internal sealed class WindmillView
    {
        [EditorLabel("Windmill rotation speed")]
        [EditorTooltip("Rotation speed applied to spawned windmills (0 = disabled).")]
        public BoundField<float> windmillRotationSpeed;

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
        [EditorTooltip("Delay in seconds before each windmill is spawned.")]
        public BoundField<float?> spawnDelay;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DamageData damage;

        public WindmillView(SpawnAbilityData real)
        {
            windmillRotationSpeed = new BoundField<float>(
                () => real.windmillRotationSpeed,
                v => real.windmillRotationSpeed = v);

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

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);

            damage = real.damage ?? (real.damage = new DamageData());

            // Windmill is its own dedicated redeem type, not a name configured through the
            // generic spawns list - always spawn the lean "Windmill_WBTI" clone (see
            // WizshBoneTwitchIntegration.SetupPieces).
            real.spawns = new List<string> { "Windmill_WBTI" };
        }
    }
}
