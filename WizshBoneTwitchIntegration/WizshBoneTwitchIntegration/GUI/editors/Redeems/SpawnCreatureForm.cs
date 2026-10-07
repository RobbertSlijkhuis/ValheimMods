using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for SpawnCreature - full multi-entry list support via
    /// <see cref="EntryListEditor{TEntry}"/>. <see cref="CreatureData.group"/> (a reusable named
    /// CreatureGroup reference) is deliberately not exposed - CreatureGroups is still a stub in
    /// the new UI and gets its own future round. The per-entry global keys, boss event, hallucination
    /// flag and talk-interact flag are not exposed either.
    /// The rows themselves live in <see cref="CreatureEntryFields"/>, split over the General,
    /// Appearance, Behavior and Spawn tabs of the list editor's pinned tab strip. Commandable and
    /// Always follow owner are always shown (their tooltips note they only have an effect when
    /// Friendly is on). A fresh list starts with one default entry, and a list row's label follows
    /// the entry's prefab/amount as they're edited.
    /// </summary>
    internal class SpawnCreatureForm : IRedeemStep2Form
    {
        private readonly EntryListEditor<CreatureData> m_list = new EntryListEditor<CreatureData>();

        public void Build(GameObject parent)
        {
            m_list.Build(parent, RedeemWizard.BodyTopY, CreatureEntryFields.Label, BuildEntryFields);
        }

        private void BuildEntryFields(GameObject cardRoot, CreatureData creature)
        {
            GuiHelper.ClearContainer(cardRoot);

            // Prefab and Amount both appear in the left list's row label (see CreatureEntryFields.Label),
            // so the row is re-rendered whenever either changes. BuildTabbed also reports every tab's
            // content height, so the card and page scroll range always match the rendered rows.
            CreatureEntryFields.BuildTabbed(m_list, cardRoot, creature, m_list.RefreshListLabels);
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
