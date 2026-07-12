using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Thou shall be
    /// smited" redeem variant - exposes only the common spawn-ability fields, while every
    /// edit writes straight through to the real <see cref="SpawnAbilityData"/> instance.
    /// </summary>
    internal sealed class SmiteView
    {
        [EditorLabel("Min to spawn")]
        public BoundField<int?> minToSpawn;

        [EditorLabel("Max to spawn")]
        public BoundField<int?> maxToSpawn;

        [EditorLabel("Spawn radius")]
        public BoundField<float?> spawnRadius;

        [EditorLabel("Spawn delay")]
        [EditorTooltip("Delay in seconds before the smite strikes.")]
        public BoundField<float?> spawnDelay;

        // Hidden: lightningAOE needs to strike the ground to look/behave right and always hit
        // its target, and already carries its own strike VFX - defaulted below rather than
        // left as user-facing controls.
        [EditorHidden]
        public BoundField<float?> accuracy;

        [EditorHidden]
        public BoundField<float?> groundOffset;

        [EditorHidden]
        public BoundField<bool> snapToterrain;

        [EditorHidden]
        public BoundField<bool> noSpawnEffect;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public DamageData damage;

        public SmiteView(SpawnAbilityData real)
        {
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

            snapToterrain = new BoundField<bool>(
                () => real.snapToterrain,
                v => real.snapToterrain = v);

            noSpawnEffect = new BoundField<bool>(
                () => real.noSpawnEffect,
                v => real.noSpawnEffect = v);

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);

            damage = real.damage ?? (real.damage = new DamageData());

            if (real.accuracy == null) real.accuracy = 1f;
            if (real.groundOffset == null) real.groundOffset = 0f;
            real.snapToterrain = true;
            real.noSpawnEffect = true;

            // No other vanilla prefab fits "smite" - hardcode lightningAOE rather than
            // trusting the generic spawns list, so editing the YAML by hand can't point this
            // redeem at an arbitrary/broken prefab.
            real.spawns = new List<string> { "lightningAOE" };
        }
    }
}
