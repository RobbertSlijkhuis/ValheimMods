using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over <see cref="SpawnAbilityData"/> for the "Log Rain" redeem
    /// variant - exposes only the common spawn-ability fields plus a curated Logs picker,
    /// while every edit writes straight through to the real <see cref="SpawnAbilityData"/>
    /// instance.
    /// </summary>
    internal sealed class LogRainView
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

        [EditorLabel("Logs")]
        [EditorTooltip("Log prefabs this redeem rains. Pick 'Biome specific' to replace this list with the 8 default per-biome logs and switch to biome-based selection.")]
        [LogPrefabNameDropdown]
        [OnValueChanged(nameof(OnBiomeSpecificSelected))]
        public List<string> spawns;

        public DamageData damage;

        private readonly SpawnAbilityData m_real;

        private static readonly string[] DefaultBiomeLogs =
        {
            "beech_log_half", "PineTree_log_half", "SwampTree1_log", "FirTree_log_half",
            "Birch_log_half", "yggashoot_log_half", "AshlandsTreeLogHalf2", "Oak_log_half"
        };

        public LogRainView(SpawnAbilityData real)
        {
            m_real = real;

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

            spawns = real.spawns ?? (real.spawns = new List<string>());

            damage = real.damage ?? (real.damage = new DamageData());
        }

        public void OnBiomeSpecificSelected()
        {
            m_real.isBiomeList = true;
            m_real.spawns.Clear();
            m_real.spawns.AddRange(DefaultBiomeLogs);
        }
    }
}
