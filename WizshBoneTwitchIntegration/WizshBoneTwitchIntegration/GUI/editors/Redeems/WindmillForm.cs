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
    /// clone, never user-editable. Damage tab hidden for now (no tabs at all).
    /// </summary>
    internal class WindmillForm : IRedeemStep2Form, IForcesValuesOnSave
    {
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
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            layout.PairRow(
                () => m_duration = layout.IntRow("Duration", "How long the effect lasts, in seconds (0 = indefinite).",
                    0, v => m_working.spawnAbilityData.duration = v, defaultValue: 0),
                () => m_rotationSpeed = layout.FloatRow("Windmill rotation speed", "Rotation speed applied to spawned windmills (0 = disabled, clamped 0-10 at runtime).",
                    0f, v => m_working.spawnAbilityData.windmillRotationSpeed = v, defaultValue: 0f));
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of windmills spawned.",
                    1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on windmills spawned (exclusive - the actual count is randomized below this).",
                    3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds before each windmill is spawned.",
                    0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the windmills can spawn within, in meters.",
                    5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f));

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_duration.text = data.duration.ToString();
            m_minToSpawn.text = (data.minToSpawn ?? 1).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? 3).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? 0f).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? 5f).ToString("G");
            m_rotationSpeed.text = data.windmillRotationSpeed.ToString("G");
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
