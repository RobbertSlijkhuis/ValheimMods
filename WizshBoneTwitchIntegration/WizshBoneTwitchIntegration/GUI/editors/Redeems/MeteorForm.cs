using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Meteor (covers both "fader" and "Yagluth" meteor flavors). Includes the
    /// Meteor type dropdown bound to <c>prefabName</c> (<see cref="MeteorView"/> never exposed it,
    /// so it was always null - which <see cref="Helpers.SpawnAbilityHelper"/> silently falls back to
    /// a fish-rain shower prefab for, same class of bug <see cref="RootForm"/> had for
    /// <c>spawns</c>). Only Meteor type/Accuracy/Announcement + Min/Max + Spawn radius are shown.
    /// Spawn delay is hidden (<see cref="ShowSpawnDelay"/>) and never written by
    /// <see cref="ApplyDefaults"/>, so a fresh redeem keeps the meteor prefab's own delay.
    /// Ground offset/Is owner/Target type and the Damage tab are hidden too - their values only
    /// ever come from <see cref="ApplyDefaults"/>, since neither the class-level defaults
    /// (<c>isOwner = true</c>, <c>targetType = Caster</c>) nor the prefab match what meteors need.
    /// </summary>
    internal class MeteorForm : IRedeemStep2Form, IAppliesDefaultsOnSelect
    {
        // Flip to true to show Spawn delay (on its own row below Accuracy/Spawn radius) again.
        private const bool ShowSpawnDelay = false;

        private const string DefaultPrefabName = "spawn_meteors";
        private const string DefaultAnnounceMessage = "{{user}} wished on a falling star, extinction event initiated!";
        private const float DefaultAccuracy = 20f;
        private const float DefaultGroundOffset = 50f;
        private const int DefaultMinToSpawn = 15;
        private const int DefaultMaxToSpawn = 15;
        private const float DefaultSpawnDelay = 0f;
        private const float DefaultSpawnRadius = 15f;

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
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of meteors spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Max allowed to spawn (count is randomized if Min is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));

            layout.PairRow(
                () => m_accuracy = layout.FloatRow("Accuracy", "Accuracy of the spawned prefabs.",
                    DefaultAccuracy, v => m_working.spawnAbilityData.accuracy = v, defaultValue: DefaultAccuracy),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target meteors can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));

            if (ShowSpawnDelay)
            {
                m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each meteor spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay);
            }

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

            m_accuracy.text = (data.accuracy ?? DefaultAccuracy).ToString("G");
            m_announceMessage.text = data.announceMessage ?? "";
            m_minToSpawn.text = (data.minToSpawn ?? DefaultMinToSpawn).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? DefaultMaxToSpawn).ToString();
            if (m_spawnDelay != null)
                m_spawnDelay.text = (data.spawnDelay ?? DefaultSpawnDelay).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? DefaultSpawnRadius).ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            SpawnAbilityData data = working.spawnAbilityData;
            data.prefabName = DefaultPrefabName;
            data.announceMessage = DefaultAnnounceMessage;
            data.accuracy = DefaultAccuracy;
            data.groundOffset = DefaultGroundOffset;
            data.isOwner = false;
            data.targetType = SpawnAbilityTargetType.Position;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnRadius = DefaultSpawnRadius;
            data.damage = new DamageData
            {
                blunt = 40f,
                chop = 0f,
                fire = 120f,
                pickaxe = 0f,
                basedOnMaxHealthAndArmor = true,
                maxHealthPercentage = 0.3f,
            };
        }
    }
}
