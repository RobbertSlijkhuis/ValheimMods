using System;
using System.Collections.Generic;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The per-creature field rows and list-row label shared by every step-2 form that edits a
    /// <see cref="CreatureData"/> in an <see cref="EntryListEditor{TEntry}"/> card
    /// (<see cref="SpawnCreatureForm"/>, <see cref="SurpriseChestForm"/>), so both stay identical.
    /// </summary>
    internal static class CreatureEntryFields
    {
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

            layout.PairRow(
                () => layout.IntRow("Amount", "How many of this creature to spawn.",
                    creature.amount, v =>
                    {
                        creature.amount = v;
                        onLabelChanged?.Invoke();
                    },
                    defaultValue: 1),
                () => layout.ColorRow("Color", "Overrides the creature's color. Leave default for no override.",
                    creature.color ?? "#ffffff", v => creature.color = v, defaultValue: "#ffffff"));

            layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                creature.announceMessage ?? "", "Optional announcement", v => creature.announceMessage = v, defaultValue: "");

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
                () => layout.ToggleRow("Allow drops", "Whether the creature drops its loot when killed. Off by default to prevent loot farming.",
                    creature.allowDrops, v => creature.allowDrops = v, defaultValue: allowDropsDefault),
                () => layout.ToggleRow("Is boss", "Treats the creature as a boss (boss health bar and music).",
                    creature.isBoss, v => creature.isBoss = v, defaultValue: false));
        }
    }
}
