using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>Step-2 form for Root ("Roots on the line") - field set ported from <c>models/views/RootView.cs</c>'s curation. No forced/locked values.</summary>
    internal class RootForm : IRedeemStep2Form
    {
        private RedeemData m_working;
        private readonly GeneralDamageTabSwitcher m_tabs = new GeneralDamageTabSwitcher();
        private readonly DamageTabForm m_damageTab = new DamageTabForm();

        private InputField m_announceMessage;
        private InputField m_batchSize;
        private InputField m_duration;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            m_tabs.Build(parent);
            var layout = new Step2RowLayout(m_tabs.GeneralRoot, m_tabs.ContentTopY);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            m_batchSize = layout.IntRow("Batch size", "Number of roots spawned at once.",
                1, v => m_working.spawnAbilityData.batchSize = v, defaultValue: 1);
            m_duration = layout.IntRow("Duration", "How long the effect lasts, in seconds (0 = indefinite).",
                0, v => m_working.spawnAbilityData.duration = v, defaultValue: 0);
            m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of roots spawned.",
                1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1);
            m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on roots spawned (exclusive - the actual count is randomized below this).",
                3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3);
            m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds before each root is spawned.",
                0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f);
            m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the roots can spawn within, in meters.",
                5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f);

            m_tabs.SetGeneralContentHeight(layout.CurrentY);
            m_damageTab.Build(m_tabs, new DamageData());
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_batchSize.text = data.batchSize.ToString();
            m_duration.text = data.duration.ToString();
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
