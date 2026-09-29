using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Rain ("Fish Rain" - <c>prefabName</c> is always left null, which
    /// <see cref="Helpers.SpawnAbilityHelper"/> falls back to a fish-rain shower prefab for, so this
    /// type always rains fish). Only Announcement + Min/Max + Spawn delay/radius are shown.
    /// Duration/Ground offset/Snap to terrain are hidden and only ever set by
    /// <see cref="ApplyDefaults"/> - the class-level defaults (duration 0, snap off) and the
    /// prefab's own ground offset don't match what a fish rain needs. Accuracy/Allow drops are
    /// hidden too, left at their defaults. Damage tab hidden for now (no tabs at all).
    /// </summary>
    internal class RainForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} believes something is smelling.... fizshy!";
        private const int DefaultDuration = 1800;
        private const float DefaultGroundOffset = 0f;
        private const int DefaultMinToSpawn = 50;
        private const int DefaultMaxToSpawn = 50;
        private const float DefaultSpawnDelay = 0f;
        private const float DefaultSpawnRadius = 40f;

        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            // Only 3 card rows - never needs scrolling, so skip ScrollableList entirely rather
            // than reserving scrollbar width for a bar that would never appear.
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "RainContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Max allowed to spawn (count is randomized if Min is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each spawn.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target prefabs can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_minToSpawn.text = (data.minToSpawn ?? DefaultMinToSpawn).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? DefaultMaxToSpawn).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? DefaultSpawnDelay).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? DefaultSpawnRadius).ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            SpawnAbilityData data = working.spawnAbilityData;
            data.announceMessage = DefaultAnnounceMessage;
            data.duration = DefaultDuration;
            data.groundOffset = DefaultGroundOffset;
            data.snapToterrain = true;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
        }
    }
}
