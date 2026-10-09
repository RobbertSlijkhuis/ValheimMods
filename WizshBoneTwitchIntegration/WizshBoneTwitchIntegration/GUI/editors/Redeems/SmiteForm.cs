using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Smite - field set ported from <c>models/views/SmiteView.cs</c>'s curation,
    /// all 5 fields kept. Everything else is locked to make the lightningAOE prefab behave
    /// correctly - confirmed via the effect-helper survey that these are genuinely the only fields
    /// with real effect on this type. Implements <see cref="IForcesValuesOnSave"/> for the locked
    /// prefab/physics flags. Damage tab hidden for now (no tabs at all), so the damage block only
    /// ever comes from <see cref="ApplyDefaults"/> - without it a fresh Smite would strike for nothing.
    /// </summary>
    internal class SmiteForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} has summoned the wrath of Thor!";
        private const int DefaultMinToSpawn = 50;
        private const int DefaultMaxToSpawn = 50;
        // Matches spawn_fish_WBTI's own m_spawnDelay, which is what an unset value falls back to
        // (Smite shares the fish-rain shower prefab, same as LogRain).
        private const float DefaultSpawnDelay = 0.25f;
        private const float DefaultSpawnRadius = 20f;

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
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "SmiteContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of lightning strikes.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", $"Max allowed to spawn (count is randomized if {Emphasis.Of("Min")} is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each strike.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the strikes can land within, in meters.",
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
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { lightning = 50f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.3f };
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // Matches SmiteView's own constructor-forced values verbatim: lightningAOE needs to
            // strike the ground to look/behave right and always hit its target.
            SpawnAbilityData data = working.spawnAbilityData;
            if (data.accuracy == null) data.accuracy = 1f;
            if (data.groundOffset == null) data.groundOffset = 0f;
            data.snapToterrain = true;
            data.noSpawnEffect = true;
            data.spawns = new List<string> { "lightningAOE" };
        }
    }
}
