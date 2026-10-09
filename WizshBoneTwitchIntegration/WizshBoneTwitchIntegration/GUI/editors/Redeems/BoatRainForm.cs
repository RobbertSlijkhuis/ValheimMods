using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for BoatRain ("BOATPOCALYPSE") - a Rain variant that always rains boats, the
    /// same relationship LogRain has to logs, so its defaults can be tuned separately from Rain's
    /// fish. Only Announcement + Allow drops/Duration + Min/Max + Spawn delay/radius are shown.
    /// The boat prefab (<c>spawns</c>), Accuracy, Ground offset and the Damage tab are hidden and
    /// only ever set by <see cref="ApplyDefaults"/> - the class-level defaults don't match what a
    /// boat rain needs (no drops, 50 m spawn height, blunt damage scaled to the target).
    /// </summary>
    internal class BoatRainForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        private const string DefaultBoatPrefab = "Karve";
        private const string DefaultAnnounceMessage = "Current weather forecast: 100% chance of BOATPOCALYPSE (sponsored by {{user}})";
        private const float DefaultAccuracy = 1f;
        private const bool DefaultAllowDrops = false;
        private const bool DefaultBreakOnDestroy = true;
        private const int DefaultDuration = 60;
        private const float DefaultGroundOffset = 50f;
        private const int DefaultMinToSpawn = 50;
        private const int DefaultMaxToSpawn = 50;
        // Matches spawn_fish_WBTI's own m_spawnDelay, which is what an unset value falls back to
        // (BoatRain shares the fish-rain shower prefab, same as LogRain).
        private const float DefaultSpawnDelay = 0.25f;
        private const float DefaultSpawnRadius = 15f;

        private RedeemData m_working;

        private Toggle m_allowDrops;
        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "BoatRainScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_allowDrops = layout.ToggleRow("Allow drops", "Whether the spawned boats can drop items when destroyed.",
                    DefaultAllowDrops, v => m_working.spawnAbilityData.allowDrops = v, defaultValue: DefaultAllowDrops),
                () => m_duration = layout.IntRow("Duration", "How long the spawned boats last before they clean themselves up, in seconds (0 = indefinite).",
                    DefaultDuration, v => m_working.spawnAbilityData.duration = v, defaultValue: DefaultDuration));
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of boats spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", $"Max allowed to spawn (count is randomized if {Emphasis.Of("Min")} is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each boat spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target boats can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_allowDrops.isOn = data.allowDrops;
            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_minToSpawn.text = (data.minToSpawn ?? DefaultMinToSpawn).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? DefaultMaxToSpawn).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? DefaultSpawnDelay).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? DefaultSpawnRadius).ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            SpawnAbilityData data = working.spawnAbilityData;
            data.spawns = new List<string> { DefaultBoatPrefab };
            data.announceMessage = DefaultAnnounceMessage;
            data.accuracy = DefaultAccuracy;
            data.allowDrops = DefaultAllowDrops;
            data.breakOnDestroy = DefaultBreakOnDestroy;
            data.duration = DefaultDuration;
            data.groundOffset = DefaultGroundOffset;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { blunt = 50f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.4f };
        }
    }
}
