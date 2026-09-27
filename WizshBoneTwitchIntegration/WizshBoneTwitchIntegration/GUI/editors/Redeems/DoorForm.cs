using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Door ("Doors of Doom") - one of the 9 SpawnAbilityFamily sub-types, all
    /// sharing <see cref="SpawnAbilityData"/>. Field set/order ported from
    /// <c>models/views/DoorView.cs</c>'s curation. No Damage tab (doors don't damage).
    /// </summary>
    internal class DoorForm : IRedeemStep2Form
    {
        private RedeemData m_working;

        private SearchableDropdown m_doorPrefab;
        private Toggle m_allowDrops;
        private InputField m_announceMessage;
        private InputField m_doorInterval;
        private InputField m_duration;
        private InputField m_groundOffset;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "DoorScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_doorPrefab = layout.DropdownRow("Door prefab", "Which door prefab gets spawned.",
                RedeemPrefabCatalog.DoorPrefabs, null, SetDoorPrefab, defaultValue: null);
            m_allowDrops = layout.ToggleRow("Allow drops", "Whether the spawned doors can drop items when destroyed.",
                true, v => m_working.spawnAbilityData.allowDrops = v, defaultValue: true);
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: "");
            m_doorInterval = layout.FloatRow("Door interval", "Interval in seconds at which spawned doors toggle open/closed (0 = disabled).",
                0f, v => m_working.spawnAbilityData.doorInterval = v, defaultValue: 0f);
            m_duration = layout.IntRow("Duration", "How long the effect lasts, in seconds (0 = indefinite).",
                0, v => m_working.spawnAbilityData.duration = v, defaultValue: 0);
            m_groundOffset = layout.FloatRow("Ground offset", "Vertical offset from the ground at which the doors spawn.",
                0f, v => m_working.spawnAbilityData.groundOffset = v, defaultValue: 0f);
            m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of doors spawned.",
                1, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: 1);
            m_maxToSpawn = layout.IntRow("Max to spawn", "Upper bound on doors spawned (exclusive - the actual count is randomized below this).",
                3, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: 3);
            m_spawnDelay = layout.FloatRow("Spawn delay", "Delay in seconds before each door is spawned.",
                0f, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: 0f);
            m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the doors can spawn within, in meters.",
                5f, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: 5f);

            ScrollableList.SetContentHeight(content, Mathf.Abs(layout.CurrentY));
        }

        private void SetDoorPrefab(string prefabName)
        {
            m_working.spawnAbilityData.spawns = string.IsNullOrEmpty(prefabName)
                ? new List<string>()
                : new List<string> { prefabName };
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            SpawnAbilityData data = working.spawnAbilityData;

            string currentDoor = data.spawns != null && data.spawns.Count > 0 ? data.spawns[0] : null;
            m_doorPrefab.SetOptions(RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.DoorPrefabs, currentDoor), currentDoor);
            m_allowDrops.isOn = data.allowDrops;
            m_announceMessage.text = data.announceMessage ?? "";
            m_doorInterval.text = data.doorInterval.ToString("G");
            m_duration.text = data.duration.ToString();
            m_groundOffset.text = (data.groundOffset ?? 0f).ToString("G");
            m_minToSpawn.text = (data.minToSpawn ?? 1).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? 3).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? 0f).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? 5f).ToString("G");
        }
    }
}
