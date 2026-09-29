using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for SpawnCreature - full multi-entry list support via
    /// <see cref="EntryListEditor{TEntry}"/>. <see cref="CreatureData.group"/> (a reusable named
    /// CreatureGroup reference) is deliberately not exposed - CreatureGroups is still a stub in
    /// the new UI and gets its own future round. Speed multiplier and Rename are hidden (both match
    /// <see cref="CreatureData"/>'s own class defaults, so hiding them is transparent).
    /// Amount+Color, Friendly+Commandable, and Level+Size are paired; Commandable is always shown
    /// (its tooltip notes it only has an effect when Friendly is on). A fresh list starts with one
    /// default entry, and a list row's label follows the entry's prefab/amount as they're edited.
    /// </summary>
    internal class SpawnCreatureForm : IRedeemStep2Form
    {
        private readonly EntryListEditor<CreatureData> m_list = new EntryListEditor<CreatureData>();

        public void Build(GameObject parent)
        {
            m_list.Build(parent, RedeemWizard.BodyTopY, ItemLabel, BuildEntryFields);
        }

        private static string ItemLabel(CreatureData creature)
        {
            if (string.IsNullOrEmpty(creature.prefabName))
                return "New creature";

            string name = RedeemPrefabCatalog.GetCreatureDisplayName(creature.prefabName);
            return $"{name} x{creature.amount}";
        }

        private void BuildEntryFields(GameObject cardRoot, CreatureData creature)
        {
            GuiHelper.ClearContainer(cardRoot);
            var layout = new Step2RowLayout(cardRoot, m_list.CardContentTopY, m_list.CardContentWidth);

            // Prefab and Amount both appear in the left list's row label (see ItemLabel), so the
            // row is re-rendered whenever either changes.
            List<DropdownOption> prefabOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.CreaturePrefabs, creature.prefabName);
            layout.DropdownRow("Prefab name", "Which creature prefab gets spawned.",
                prefabOptions, creature.prefabName, v =>
                {
                    creature.prefabName = v;
                    m_list.RefreshListLabels();
                },
                defaultValue: null);

            layout.PairRow(
                () => layout.IntRow("Amount", "How many of this creature to spawn.",
                    creature.amount, v =>
                    {
                        creature.amount = v;
                        m_list.RefreshListLabels();
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
                    creature.level, v => creature.level = v, defaultValue: 1),
                () => layout.FloatRow("Size", "Overall size multiplier of the creature.",
                    creature.size, v => creature.size = v, defaultValue: 1f));

            // Runs on every (re)build of this card so the card and page scroll range always match
            // the currently rendered rows.
            m_list.SetCardContentHeight(Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            SpawnCreatureData data = working.creatureData;

            // A redeem with no creatures does nothing, so a fresh (or emptied) list starts with one
            // default entry rather than an empty "click + Add" state. Done here rather than as a
            // SpawnCreatureData field initializer, which YAML deserialization would append to.
            if (data.list.Count == 0)
                data.list.Add(new CreatureData());

            m_list.Populate(data.list, () => data.random, v => data.random = v);
        }
    }
}
