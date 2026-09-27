using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Meteor (covers both "fader" and "Yagluth" meteor flavors) - field set ported from <c>models/views/MeteorView.cs</c>'s curation. No forced/locked values.</summary>
    internal class MeteorForm : IRedeemStep2Form
    {
        private RedeemData m_working;
        private readonly GeneralDamageTabSwitcher m_tabs = new GeneralDamageTabSwitcher();
        private readonly DamageTabForm m_damageTab = new DamageTabForm();

        private InputField m_accuracy;
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

            m_accuracy = layout.FloatRow("Accuracy", "Accuracy of the spawned prefabs.",
                1f, v => m_working.spawnAbilityData.accuracy = v, defaultValue: 1f);
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            m_duration = layout.IntRow("Duration", "How long the effect lasts, in seconds (0 = indefinite).",
                0, v => m_working.spawnAbilityData.duration = v, defaultValue: 0);
            m_groundOffset = layout.FloatRow("Ground offset", "Vertical offset from the ground at which the prefabs spawn.",
                0f, v => m_working.spawnAbilityData.groundOffset = v, defaultValue: 0f);
            m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of meteors spawned.",
                1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1);
            m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on meteors spawned (exclusive - the actual count is randomized below this).",
                3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3);
            m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds between each meteor falling.",
                0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f);
            m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target meteors can spawn within, in meters.",
                5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f);

            m_tabs.SetGeneralContentHeight(layout.CurrentY);
            m_damageTab.Build(m_tabs, new DamageData());
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_accuracy.text = (data.accuracy ?? 1f).ToString("G");
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
