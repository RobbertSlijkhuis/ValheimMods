using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for LogRain - field set ported from <c>models/views/LogRainView.cs</c>'s
    /// curation, including its "Biome specific" sentinel behavior (adapted from a single-select
    /// dropdown to the SearchableChecklist multi-select widget: checking the sentinel replaces the
    /// whole selection with the 8 default per-biome logs and sets <c>isBiomeList</c>, matching
    /// <c>LogRainView.OnBiomeSpecificSelected</c>'s exact effect). Log prefab(s) is hidden until
    /// it's finished (<see cref="ShowLogPicker"/>) - a fresh selection is forced onto the 8 default
    /// per-biome logs via <see cref="ApplyDefaults"/> meanwhile. Accuracy/Ground offset/Drop
    /// velocity and the Damage tab are hidden too - their values only ever come from
    /// <see cref="ApplyDefaults"/>, which is why every one of them is set there rather than left to
    /// the class-level defaults.
    /// </summary>
    internal class LogRainForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        // Flip to true once the log picker is finished.
        private const bool ShowLogPicker = false;

        private const string DefaultAnnounceMessage = "{{user}} is sending you some building materials: HARD WOOD!";
        private const float DefaultAccuracy = 1f;
        private const float DefaultDropVelocity = -25f;
        private const int DefaultDuration = 300;
        private const float DefaultGroundOffset = 50f;
        private const int DefaultMinToSpawn = 75;
        private const int DefaultMaxToSpawn = 75;
        // Matches spawn_fish_WBTI's own m_spawnDelay, which is what an unset value falls back to.
        private const float DefaultSpawnDelay = 0.25f;
        private const float DefaultSpawnRadius = 15f;

        private RedeemData m_working;

        private SearchableChecklist m_logs;
        private Toggle m_allowDrops;
        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "LogRainScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            if (ShowLogPicker)
                m_logs = layout.ChecklistRow("Log prefab(s)", "Which log prefabs this redeem rains. Checking 'Biome specific' replaces the selection with the 8 default per-biome logs.",
                    BuildLogOptions(), new List<string>(), OnLogsChanged, defaultValues: new List<string>());
            m_announceMessage = layout.TextRow("Announcement message", $"Shown on screen when triggered. {Emphasis.Of("{{user}}")} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_allowDrops = layout.ToggleRow("Allow drops", "Whether the spawned prefabs can drop items when destroyed.",
                    true, v => m_working.spawnAbilityData.allowDrops = v, defaultValue: true),
                () => m_duration = layout.IntRow("Duration", "How long the spawned logs last before they clean themselves up, in seconds (0 = indefinite).",
                    DefaultDuration, v => m_working.spawnAbilityData.duration = v, defaultValue: DefaultDuration));
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of logs spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", $"Max allowed to spawn (count is randomized if {Emphasis.Of("Min")} is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each log spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target logs can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        private static List<DropdownOption> BuildLogOptions()
        {
            var options = RedeemPrefabCatalog.LogPrefabs.Select(name => new DropdownOption(name, name)).ToList();
            options.Add(new DropdownOption(RedeemPrefabCatalog.BiomeSpecificSentinel, RedeemPrefabCatalog.BiomeSpecificSentinel));
            return options;
        }

        private void OnLogsChanged(List<string> selected)
        {
            if (selected.Contains(RedeemPrefabCatalog.BiomeSpecificSentinel))
            {
                m_working.spawnAbilityData.isBiomeList = true;
                m_working.spawnAbilityData.spawns = new List<string>(RedeemPrefabCatalog.DefaultBiomeLogs);
                // Re-sync the widget's own checkboxes: 8 biome logs checked, sentinel unchecked.
                m_logs.SetOptions(BuildLogOptions(), m_working.spawnAbilityData.spawns);
                return;
            }

            m_working.spawnAbilityData.isBiomeList = false;
            m_working.spawnAbilityData.spawns = selected;
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            if (m_logs != null)
            {
                List<string> savedSpawns = data.spawns ?? new List<string>();
                List<DropdownOption> options = BuildLogOptions();
                foreach (string value in savedSpawns)
                    options = RedeemPrefabCatalog.EnsureIncludesCurrentValue(options, value);
                m_logs.SetOptions(options, savedSpawns);
            }

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
            data.isBiomeList = true;
            data.spawns = new List<string>(RedeemPrefabCatalog.DefaultBiomeLogs);
            data.announceMessage = DefaultAnnounceMessage;
            data.accuracy = DefaultAccuracy;
            data.dropVelocity = DefaultDropVelocity;
            data.duration = DefaultDuration;
            data.groundOffset = DefaultGroundOffset;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData { blunt = 50f, basedOnMaxHealthAndArmor = true, maxHealthPercentage = 0.3f };
        }
    }
}
