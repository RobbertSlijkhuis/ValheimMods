using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Windmill ("Windmills of Death") - field set ported from
    /// <c>models/views/WindmillView.cs</c>'s curation, all 7 fields kept. Implements
    /// <see cref="IForcesValuesOnSave"/> since the prefab is always the dedicated "Windmill_WBTI"
    /// clone, never user-editable. Allow drops/Break on destroy/Ground offset/Snap to terrain and
    /// the Damage tab are hidden (no tabs at all) and only ever set by <see cref="ApplyDefaults"/> -
    /// none of the class-level defaults match what windmills need.
    /// </summary>
    internal class WindmillForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} has unleashed the Windmills of DEATH!";
        private const bool DefaultAllowDrops = false;
        private const bool DefaultBreakOnDestroy = true;
        private const int DefaultDuration = 60;
        private const float DefaultGroundOffset = -3f;
        private const int DefaultMinToSpawn = 100;
        private const int DefaultMaxToSpawn = 100;
        private const float DefaultRotationSpeed = 10f;
        private const float DefaultSpawnDelay = 0.0001f;
        private const float DefaultSpawnRadius = 30f;

        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;
        private InputField m_rotationSpeed;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "WindmillScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_duration = layout.IntRow("Duration", "How long the spawned windmills last before they clean themselves up, in seconds (0 = indefinite).",
                    DefaultDuration, v => m_working.spawnAbilityData.duration = v, defaultValue: DefaultDuration),
                () => m_rotationSpeed = layout.FloatRow("Windmill rotation speed", "Rotation speed applied to spawned windmills (0 = disabled, 0-10).",
                    DefaultRotationSpeed, v => m_working.spawnAbilityData.windmillRotationSpeed = v, defaultValue: DefaultRotationSpeed, min: 0f, max: 10f));
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of windmills spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Max allowed to spawn (count is randomized if Min is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each windmill spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the windmills can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_minToSpawn.text = (data.minToSpawn ?? DefaultMinToSpawn).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? DefaultMaxToSpawn).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? DefaultSpawnDelay).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? DefaultSpawnRadius).ToString("G");
            m_rotationSpeed.text = data.windmillRotationSpeed.ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            SpawnAbilityData data = working.spawnAbilityData;
            data.announceMessage = DefaultAnnounceMessage;
            data.allowDrops = DefaultAllowDrops;
            data.breakOnDestroy = DefaultBreakOnDestroy;
            data.duration = DefaultDuration;
            data.windmillRotationSpeed = DefaultRotationSpeed;
            data.groundOffset = DefaultGroundOffset;
            data.snapToterrain = true;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { blunt = 100f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.15f };
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // Windmill is its own dedicated redeem type, not a name configured through the
            // generic spawns list - always spawn the lean "Windmill_WBTI" clone, matching
            // WindmillView's own constructor-forced value.
            working.spawnAbilityData.spawns = new List<string> { "Windmill_WBTI" };
        }
    }
}
