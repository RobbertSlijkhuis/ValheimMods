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
    /// <c>LogRainView.OnBiomeSpecificSelected</c>'s exact effect).
    /// </summary>
    internal class LogRainForm : IRedeemStep2Form
    {
        private RedeemData m_working;
        private readonly GeneralDamageTabSwitcher m_tabs = new GeneralDamageTabSwitcher();
        private readonly DamageTabForm m_damageTab = new DamageTabForm();

        private SearchableChecklist m_logs;
        private InputField m_accuracy;
        private Toggle m_allowDrops;
        private InputField m_announceMessage;
        private InputField m_duration;
        private InputField m_groundOffset;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            m_tabs.Build(parent);
            var layout = new Step2RowLayout(m_tabs.GeneralRoot, m_tabs.ContentTopY);

            m_logs = layout.ChecklistRow("Log prefab(s)", "Which log prefabs this redeem rains. Checking 'Biome specific' replaces the selection with the 8 default per-biome logs.",
                BuildLogOptions(), new List<string>(), OnLogsChanged, defaultValues: new List<string>());
            m_accuracy = layout.FloatRow("Accuracy", "Accuracy of the spawned prefabs.",
                1f, v => m_working.spawnAbilityData.accuracy = v, defaultValue: 1f);
            m_allowDrops = layout.ToggleRow("Allow drops", "Whether the spawned prefabs can drop items when destroyed.",
                true, v => m_working.spawnAbilityData.allowDrops = v, defaultValue: true);
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            m_duration = layout.IntRow("Duration", "How long the effect lasts, in seconds (0 = indefinite).",
                0, v => m_working.spawnAbilityData.duration = v, defaultValue: 0);
            m_groundOffset = layout.FloatRow("Ground offset", "Vertical offset from the ground at which the prefabs spawn.",
                0f, v => m_working.spawnAbilityData.groundOffset = v, defaultValue: 0f);
            m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of logs spawned.",
                1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1);
            m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on logs spawned (exclusive - the actual count is randomized below this).",
                3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3);
            m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds between each log falling.",
                0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f);
            m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target logs can spawn within, in meters.",
                5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f);

            m_tabs.SetGeneralContentHeight(layout.CurrentY);
            m_damageTab.Build(m_tabs, new DamageData());
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

            List<string> savedSpawns = data.spawns ?? new List<string>();
            List<DropdownOption> options = BuildLogOptions();
            foreach (string value in savedSpawns)
                options = RedeemPrefabCatalog.EnsureIncludesCurrentValue(options, value);
            m_logs.SetOptions(options, savedSpawns);

            m_accuracy.text = (data.accuracy ?? 1f).ToString("G");
            m_allowDrops.isOn = data.allowDrops;
            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_groundOffset.text = (data.groundOffset ?? 0f).ToString("G");
            m_minToSpawn.text = (data.minToSpawn ?? 1).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? 3).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? 0f).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? 5f).ToString("G");

            if (data.damage == null)
                data.damage = new DamageData();
            m_damageTab.Populate(data.damage);
        }
    }
}
