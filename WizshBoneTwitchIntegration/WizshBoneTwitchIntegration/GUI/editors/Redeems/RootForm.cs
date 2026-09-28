using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Root ("Roots on the line") - field set ported from
    /// <c>models/views/RootView.cs</c>'s curation. Implements <see cref="IForcesValuesOnSave"/>
    /// since the prefab is always "TentaRoot" (matches <c>RootView</c>'s own constructor-forced
    /// value - previously neither forced it, so an unset <c>spawns</c> silently fell back to
    /// <c>SpawnAbilityHelper</c>'s fish-rain default instead of spawning roots). Only Announcement +
    /// Min/Max + Spawn delay/radius are shown - Duration/Batch size are hidden, transparent since
    /// their class-level defaults already match. Damage tab hidden for now (no tabs at all).
    /// </summary>
    internal class RootForm : IRedeemStep2Form, IForcesValuesOnSave
    {
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
            GameObject content = GuiHelper.CreateFixedWidthContainer(parent, "RootContent",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth);
            var layout = new Step2RowLayout(content, 0f, RedeemWizard.Step2FieldWidth);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of roots spawned.",
                    1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on roots spawned (exclusive - the actual count is randomized below this).",
                    3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds before each root is spawned.",
                    0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the roots can spawn within, in meters.",
                    5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_minToSpawn.text = (data.minToSpawn ?? 1).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? 3).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? 0f).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? 5f).ToString("G");
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // No other vanilla prefab fits "roots on the line" - matches RootView's own constructor-forced value.
            working.spawnAbilityData.spawns = new List<string> { "TentaRoot" };
        }
    }
}
