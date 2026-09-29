using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Door ("Doors of Doom") - one of the 9 SpawnAbilityFamily sub-types, all
    /// sharing <see cref="SpawnAbilityData"/>. Field set/order ported from
    /// <c>models/views/DoorView.cs</c>'s curation. No Damage tab (doors don't damage). Ground offset
    /// and Snap to terrain are hidden and forced on save (snap = true, offset = per-prefab, default 0), as is Allow
    /// drops: <see cref="SpawnAbilityData.allowDrops"/> defaults to <c>true</c> at the class level
    /// while the intended behavior here is always <c>false</c> - these need an explicit
    /// <see cref="IForcesValuesOnSave"/> force rather than relying on the class default.
    /// </summary>
    internal class DoorForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultDoorPrefab = "piece_hexagonal_door";
        private const string DefaultAnnounceMessage = "{{user}} proudly presents, the Doors of DOOM!";
        private const bool DefaultBreakOnDestroy = true;
        private const int DefaultDuration = 60;
        private const float DefaultDoorInterval = 1f;
        private const int DefaultMinToSpawn = 300;
        private const int DefaultMaxToSpawn = 300;
        private const float DefaultSpawnDelay = 0.0001f;
        private const float DefaultSpawnRadius = 30f;
        private const float DefaultGroundOffset = 0f;

        // Doors whose prefab origin isn't at their base and so spawn partly buried at offset 0.
        // Add an entry here when another door turns out to sink into the ground.
        private static readonly Dictionary<string, float> GroundOffsetByPrefab = new Dictionary<string, float>
        {
            { "piece_hexagonal_door", 1.5f },
            { "iron_grate", 1.5f },       // Iron Gate
            { "wood_door", 1.5f },        // Wood Door
            { "wood_gate", 1.5f },        // Wood Gate
            { "wood_window", 1.5f },      // Wood Shutter
            { "wood_fence_gate", 1.5f },  // Roundpole Gate
        };

        private RedeemData m_working;

        private SearchableDropdown m_doorPrefab;
        private InputField m_announceMessage;
        private InputField m_doorInterval;
        private InputField m_duration;
        private InputField m_minToSpawn;
        private InputField m_maxToSpawn;
        private InputField m_spawnDelay;
        private InputField m_spawnRadius;

        public void Build(GameObject parent)
        {
            GameObject content = ScrollableList.CreateFixed(parent, "DoorScroll",
                new Vector2(0f, RedeemWizard.BodyTopY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            var layout = new Step2RowLayout(content, 0f);

            m_doorPrefab = layout.DropdownRow("Door prefab", "Which door prefab gets spawned.",
                RedeemPrefabCatalog.DoorPrefabs, DefaultDoorPrefab, SetDoorPrefab, defaultValue: DefaultDoorPrefab);
            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.spawnAbilityData.announceMessage = v, defaultValue: DefaultAnnounceMessage);
            layout.PairRow(
                () => m_duration = layout.IntRow("Duration", "How long the spawned doors last before they clean themselves up, in seconds (0 = indefinite).",
                    DefaultDuration, v => m_working.spawnAbilityData.duration = v, defaultValue: DefaultDuration),
                () => m_doorInterval = layout.FloatRow("Door interval", "Interval in seconds at which spawned doors toggle open/closed (0 = disabled).",
                    DefaultDoorInterval, v => m_working.spawnAbilityData.doorInterval = v, defaultValue: DefaultDoorInterval));
            layout.PairRow(
                () => m_minToSpawn = layout.IntRow("Min to spawn", "Minimum number of doors spawned.",
                    DefaultMinToSpawn, v => m_working.spawnAbilityData.minToSpawn = v, defaultValue: DefaultMinToSpawn),
                () => m_maxToSpawn = layout.IntRow("Max to spawn", "Max allowed to spawn (count is randomized if Min is less).",
                    DefaultMaxToSpawn, v => m_working.spawnAbilityData.maxToSpawn = v, defaultValue: DefaultMaxToSpawn));
            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each door spawning.",
                    DefaultSpawnDelay, v => m_working.spawnAbilityData.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_spawnRadius = layout.FloatRow("Spawn radius", "Radius around the target the doors can spawn within, in meters.",
                    DefaultSpawnRadius, v => m_working.spawnAbilityData.spawnRadius = v, defaultValue: DefaultSpawnRadius));

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
            m_announceMessage.text = data.announceMessage ?? "";
            m_doorInterval.text = data.doorInterval.ToString("G");
            m_duration.text = data.duration.ToString();
            m_minToSpawn.text = (data.minToSpawn ?? DefaultMinToSpawn).ToString();
            m_maxToSpawn.text = (data.maxToSpawn ?? DefaultMaxToSpawn).ToString();
            m_spawnDelay.text = (data.spawnDelay ?? DefaultSpawnDelay).ToString("G");
            m_spawnRadius.text = (data.spawnRadius ?? DefaultSpawnRadius).ToString("G");
        }

        public void ApplyDefaults(RedeemData working)
        {
            SpawnAbilityData data = working.spawnAbilityData;
            data.spawns = new List<string> { DefaultDoorPrefab };
            data.announceMessage = DefaultAnnounceMessage;
            data.breakOnDestroy = DefaultBreakOnDestroy;
            data.duration = DefaultDuration;
            data.doorInterval = DefaultDoorInterval;
            data.minToSpawn = DefaultMinToSpawn;
            data.maxToSpawn = DefaultMaxToSpawn;
            data.spawnDelay = DefaultSpawnDelay;
            data.spawnRadius = DefaultSpawnRadius;
        }

        public void ApplyForcedValues(RedeemData working)
        {
            // Hidden field - SpawnAbilityData.allowDrops defaults to true at the class level, but
            // Door wants it false, so (unlike every other cut field this round) it needs an
            // explicit force rather than relying on the class default.
            working.spawnAbilityData.allowDrops = false;

            // Also hidden, and neither matches its class-level default (snapToterrain = false,
            // groundOffset = null). Snap puts each door at the terrain's solid height, and vanilla
            // SpawnAbility then adds groundOffset on top of that (it is not skipped when snapping),
            // so an offset of 0 places a base-pivot door exactly on the ground. The offset depends
            // on the chosen door prefab (see GroundOffsetByPrefab), so it's derived here from the
            // final selection rather than at dropdown-change time.
            List<string> spawns = working.spawnAbilityData.spawns;
            string prefab = spawns != null && spawns.Count > 0 ? spawns[0] : null;
            working.spawnAbilityData.snapToterrain = true;
            working.spawnAbilityData.groundOffset = prefab != null && GroundOffsetByPrefab.TryGetValue(prefab, out float offset)
                ? offset
                : DefaultGroundOffset;
        }
    }
}
