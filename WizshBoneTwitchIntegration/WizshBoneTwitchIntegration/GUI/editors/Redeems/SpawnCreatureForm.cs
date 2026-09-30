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
    /// (its tooltip notes it only has an effect when Friendly is on). Allow drops + Is boss are
    /// paired on a row below (<see cref="CreatureData.allowDrops"/>/<see cref="CreatureData.isBoss"/>,
    /// both default off; already applied at spawn by TwitchCreaturePersistentData). A fresh list starts with one
    /// default entry, and a list row's label follows the entry's prefab/amount as they're edited.
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
            var layout = new Step2RowLayout(cardRoot, m_list.CardContentTopY, m_list.CardContentWidth);

            // Prefab and Amount both appear in the left list's row label (see CreatureEntryFields.Label),
            // so the row is re-rendered whenever either changes.
            CreatureEntryFields.Build(layout, creature, m_list.RefreshListLabels);

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
