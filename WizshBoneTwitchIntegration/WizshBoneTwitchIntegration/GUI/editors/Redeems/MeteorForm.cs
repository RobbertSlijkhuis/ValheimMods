using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Meteor (covers both "fader" and "Yagluth" meteor flavors). Includes the
    /// Meteor type dropdown bound to <c>prefabName</c> (<see cref="MeteorView"/> never exposed it,
    /// so it was always null - which <see cref="Helpers.SpawnAbilityHelper"/> silently falls back to
    /// a fish-rain shower prefab for, same class of bug <see cref="RootForm"/> had for
    /// <c>spawns</c>). Only Meteor type/Accuracy/Announcement + Min/Max + Spawn delay/radius are
    /// shown - Duration/Ground offset are hidden, transparent since their class-level defaults
    /// already match. Damage tab hidden for now (no tabs at all). No further forced/locked values.
    /// </summary>
    internal class MeteorForm : IRedeemStep2Form
    {
        private const string DefaultPrefabName = "spawn_meteors";

        private RedeemData m_working;

        private SearchableDropdown m_prefabName;
        private InputField m_accuracy;
        private InputField m_announceMessage;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        private static List<DropdownOption> GetMeteorTypeOptions()
        {
            return new List<DropdownOption>
            {
                new DropdownOption("spawn_meteors", "Yagluth meteors"),
                new DropdownOption("spawn_fader_meteors", "Fader meteors"),
            };
        }

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "MeteorScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            // Leads the form - "which kind of meteor" is the obvious first decision, same
            // functional-primacy exception SpawnCreatureForm's prefabName field uses.
            m_prefabName = layout.DropdownRow("Meteor type", "Which meteor effect falls.",
                GetMeteorTypeOptions(), DefaultPrefabName, v => m_working.spawnAbilityData.prefabName = v,
                defaultValue: DefaultPrefabName, showSearch: false);
            m_accuracy = layout.FloatRow("Accuracy", "Accuracy of the spawned prefabs.",
                1f, v => m_working.spawnAbilityData.accuracy = v, defaultValue: 1f);
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of meteors spawned.",
                    1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on meteors spawned (exclusive - the actual count is randomized below this).",
                    3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds between each meteor falling.",
                    0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target meteors can spawn within, in meters.",
                    5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            // Guarantees a real prefab is picked even if the user never opens the dropdown -
            // SpawnAbilityData is shared across every SpawnAbilityFamily type, so this can't be a
            // class-level default the way DetonateData.type's is; Meteor has to enforce its own.
            if (string.IsNullOrEmpty(data.prefabName))
                data.prefabName = DefaultPrefabName;
            m_prefabName.SetOptions(GetMeteorTypeOptions(), data.prefabName);

            m_accuracy.text = (data.accuracy ?? 1f).ToString("G");
            m_announceMessage.text = data.announceMessage ?? "";
            m_minToSpawn.text = (data.minToSpawn ?? 1).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? 3).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? 0f).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? 5f).ToString("G");
        }
    }
}
