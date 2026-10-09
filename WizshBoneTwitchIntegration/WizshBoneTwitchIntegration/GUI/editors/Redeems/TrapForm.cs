using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Trap - field set ported from <c>models/views/TrapView.cs</c>'s curation, all
    /// 4 fields kept. Implements <see cref="IForcesValuesOnSave"/> since the prefab is always
    /// "fuling_trap". Duration/Allow drops/Break on destroy/Ground offset/Snap to terrain and the
    /// Damage tab are hidden (no tabs at all) and only ever set by <see cref="ApplyDefaults"/> -
    /// none of the class-level defaults match what a trap field needs.
    /// </summary>
    internal class TrapForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "Minor inconveniences deployed by {{user}}!";
        private const bool DefaultAllowDrops = false;
        private const bool DefaultBreakOnDestroy = true;
        private const int DefaultDuration = 300;
        private const float DefaultGroundOffset = 0f;
        private const int DefaultMinToSpawn = 300;
        private const int DefaultMaxToSpawn = 300;
        private const float DefaultSpawnDelay = 0.0001f;
        private const float DefaultSpawnRadius = 25f;

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
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "TrapContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of traps spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", $"Max allowed to spawn (count is randomized if {Emphasis.Of("Min")} is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each trap being placed.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the traps can spawn within, in meters.",
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
            data.allowDrops = DefaultAllowDrops;
            data.breakOnDestroy = DefaultBreakOnDestroy;
            data.duration = DefaultDuration;
            data.groundOffset = DefaultGroundOffset;
            data.snapToterrain = true;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { blunt = 50f, pierce = 50f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.35f };
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // No other vanilla prefab fits "trap field" - matches TrapView's own constructor-forced value.
            working.spawnAbilityData.spawns = new List<string> { "fuling_trap" };
        }
    }
}
