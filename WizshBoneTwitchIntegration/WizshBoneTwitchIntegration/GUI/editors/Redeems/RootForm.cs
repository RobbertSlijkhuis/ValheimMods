using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Root ("Roots on the line") - field set ported from
    /// <c>models/views/RootView.cs</c>'s curation. Implements <see cref="IForcesValuesOnSave"/>
    /// since the prefab is always "TentaRoot" (matches <c>RootView</c>'s own constructor-forced
    /// value - previously neither forced it, so an unset <c>spawns</c> silently fell back to
    /// <c>SpawnAbilityHelper</c>'s fish-rain default instead of spawning roots). Only Announcement +
    /// Min/Max + Spawn delay/radius are shown. Batch size/Duration/Ground offset/Snap to terrain
    /// and the Damage tab (no tabs at all) are hidden and only ever set by
    /// <see cref="ApplyDefaults"/> - none of the class-level defaults match what roots need.
    /// </summary>
    internal class RootForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "Major inconveniences deployed by {{user}}!";
        private const int DefaultBatchSize = 4;
        private const int DefaultDuration = 60;
        private const float DefaultGroundOffset = 0f;
        private const int DefaultMinToSpawn = 150;
        private const int DefaultMaxToSpawn = 150;
        private const float DefaultSpawnDelay = 3f;
        private const float DefaultSpawnRadius = 4f;

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
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "RootContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of roots spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Max allowed to spawn (count is randomized if Min is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each batch of roots spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the roots can spawn within, in meters.",
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
            data.batchSize = DefaultBatchSize;
            data.duration = DefaultDuration;
            data.groundOffset = DefaultGroundOffset;
            data.snapToterrain = true;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { blunt = 100f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.3f };
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // No other vanilla prefab fits "roots on the line" - matches RootView's own constructor-forced value.
            working.spawnAbilityData.spawns = new List<string> { "TentaRoot" };
        }
    }
}
