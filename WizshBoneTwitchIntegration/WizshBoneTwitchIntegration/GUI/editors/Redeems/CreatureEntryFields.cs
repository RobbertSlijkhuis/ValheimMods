using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The per-creature field rows and list-row label shared by every step-2 form that edits a
    /// <see cref="CreatureData"/> in an <see cref="EntryListEditor{TEntry}"/> card
    /// (<see cref="SpawnCreatureForm"/>, <see cref="SurpriseChestForm"/>), so both stay identical.
    /// </summary>
    internal static class CreatureEntryFields
    {
        // SpawnPositionType.Undefined is a "not set" marker, not a real choice, so it isn't listed.
        private static readonly List<DropdownOption> PositionOptions = new List<DropdownOption>
        {
            new DropdownOption(SpawnPositionType.InFrontOfPlayer, "In front of player"),
            new DropdownOption(SpawnPositionType.OnPlayer, "On player"),
            new DropdownOption(SpawnPositionType.Random, "Random"),
            new DropdownOption(SpawnPositionType.RandomBehind, "Random behind"),
            new DropdownOption(SpawnPositionType.RandomFlying, "Random flying"),
            new DropdownOption(SpawnPositionType.WorldPosition, "World position"),
        };

        /// <summary>Left-list row text for a creature entry: "Neck x3", or "New creature" until a prefab is picked.</summary>
        public static string Label(CreatureData creature)
        {
            if (string.IsNullOrEmpty(creature.prefabName))
                return "New creature";

            string name = RedeemPrefabCatalog.GetCreatureDisplayName(creature.prefabName);
            return $"{name} x{creature.amount}";
        }

        /// <summary>
        /// Adds the creature's field rows to <paramref name="layout"/>. <paramref name="onLabelChanged"/>
        /// runs when Prefab or Amount changes, since both appear in <see cref="Label"/>.
        /// <paramref name="allowDropsDefault"/> is only what the Allow drops reset button restores;
        /// the toggle's value itself comes from the entry.
        /// </summary>
        public static void Build(Step2RowLayout layout, CreatureData creature, Action onLabelChanged, bool allowDropsDefault = false)
        {
            List<DropdownOption> prefabOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.CreaturePrefabs, creature.prefabName);
            layout.DropdownRow("Prefab name", "Which creature prefab gets spawned.",
                prefabOptions, creature.prefabName, v =>
                {
                    creature.prefabName = v;
                    onLabelChanged?.Invoke();
                },
                defaultValue: null);

            layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                creature.announceMessage ?? "", "Optional announcement", v => creature.announceMessage = v, defaultValue: "");

            layout.PairRow(
                () => layout.IntRow("Amount", "How many of this creature to spawn.",
                    creature.amount, v =>
                    {
                        creature.amount = v;
                        onLabelChanged?.Invoke();
                    },
                    defaultValue: 1),
                () => layout.ToggleRow("Allow drops", "Whether the creature drops its loot when killed. Off by default to prevent loot farming.",
                    creature.allowDrops, v => creature.allowDrops = v, defaultValue: allowDropsDefault));

            layout.PairRow(
                () => layout.ToggleRow("Friendly", "Whether the creature is friendly toward the player.",
                    creature.friendly, v => creature.friendly = v, defaultValue: false),
                () => layout.ToggleRow("Commandable", "Requires Friendly to be enabled. Whether the player can command this creature.",
                    creature.commandable, v => creature.commandable = v, defaultValue: false));

            layout.PairRow(
                () => layout.IntRow("Level", "Star level of the creature (1 = none, up to 10 = max).",
                    creature.level, v => creature.level = v, defaultValue: 1, min: 1, max: 10),
                () => layout.FloatRow("Size", "Overall size multiplier of the creature.",
                    creature.size, v => creature.size = v, defaultValue: 1f));

            layout.PairRow(
                () => layout.ToggleRow("Is boss", "Treats the creature as a boss (boss health bar and music).",
                    creature.isBoss, v => creature.isBoss = v, defaultValue: false),
                () => layout.TextRow("Name", "The name shown for the creature. Leave empty to use the redeemer's name.",
                    creature.name ?? "", "Redeemer's name", v => creature.name = string.IsNullOrEmpty(v) ? null : v, defaultValue: ""));

            List<DropdownOption> positionOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(PositionOptions, creature.position);
            layout.PairRow(
                () => layout.DropdownRow("Position", "Where the creature spawns, relative to the player (or to the chest, for a chest's loot). World position treats the offset below as absolute world coordinates.",
                    positionOptions, creature.position, v => creature.position = v,
                    defaultValue: SpawnPositionType.Random, showSearch: false),
                () => layout.FloatRow("Position radius", "How far from the spawn point a creature can appear. Only used by the Random and Random flying positions.",
                    creature.positionRadius, v => creature.positionRadius = v, defaultValue: 10f, min: 0f));

            PositionOffsetData offset = creature.positionOffset ?? (creature.positionOffset = new PositionOffsetData());
            layout.Vector3Row("Position offset (X / Y / Z)", "Shifts the spawn point by this many meters, left to right: X (sideways), Y (up) and Z (forward). Ignored by the Random positions, except Random behind.",
                new Vector3(offset.x, offset.y, offset.z), v =>
                {
                    offset.x = v.x;
                    offset.y = v.y;
                    offset.z = v.z;
                });

            BuildColorRow(layout, creature);
        }

        // Color + Emission color on one row. The emission is "linked" to the main color while
        // CreatureData.emissionColor is empty: its swatch then follows the main swatch, and picking
        // (or resetting to) the main color stores null so a later main-color change keeps the glow in
        // step. Same behavior as ViewerEditDialog.
        private static void BuildColorRow(Step2RowLayout layout, CreatureData creature)
        {
            const string defaultColor = "#ffffff";
            GameObject emissionSwatch = null;

            layout.PairRow(
                () => layout.ColorRow("Color", "Overrides the creature's color. Leave default for no override.",
                    creature.color ?? defaultColor, v =>
                    {
                        creature.color = v;

                        if (string.IsNullOrEmpty(creature.emissionColor))
                            SetSwatchColor(emissionSwatch, v);
                    },
                    defaultValue: defaultColor),
                () => emissionSwatch = layout.ColorRow("Emission color", "Overrides the color of the creature's glow. Follows Color until a different color is picked here.",
                    string.IsNullOrEmpty(creature.emissionColor) ? creature.color ?? defaultColor : creature.emissionColor, v =>
                    {
                        string mainColor = creature.color ?? defaultColor;
                        bool follows = string.IsNullOrEmpty(v) || string.Equals(v, mainColor, StringComparison.OrdinalIgnoreCase);

                        creature.emissionColor = follows ? null : v;

                        // A reset hands back "", which the swatch itself can't paint.
                        if (follows)
                            SetSwatchColor(emissionSwatch, mainColor);
                    },
                    defaultValue: ""));
        }

        // CreateColorField's swatch has no "set color" API - paint the overlay Image the picker paints.
        private static void SetSwatchColor(GameObject swatch, string hex)
        {
            if (swatch != null && ColorUtility.TryParseHtmlString(hex, out Color color))
                swatch.transform.Find("ColorOverlay").GetComponent<Image>().color = color;
        }
    }
}
