using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for StatusEffect - full multi-entry list support via
    /// <see cref="EntryListEditor{TEntry}"/>. The 7 <c>playerScale</c> sub-fields (shown only for
    /// PlayerShrink/PlayerGrow in the old form) are now hidden always, regardless of Name - each
    /// one's class-level default in <c>SE_PlayerScaleData</c> already matches the old form's
    /// defaults, so this is transparent. Persists through death+Renew are paired. A fresh list
    /// starts with one default entry, and a list row's label follows the entry's name as it's edited.
    /// </summary>
    internal class StatusEffectForm : IRedeemStep2Form
    {
        private readonly EntryListEditor<StatusEffectEntry> m_list = new EntryListEditor<StatusEffectEntry>();

        public void Build(GameObject parent)
        {
            m_list.Build(parent, RedeemWizard.BodyTopY, ItemLabel, BuildEntryFields);
        }

        private static string ItemLabel(StatusEffectEntry entry)
        {
            return string.IsNullOrEmpty(entry.name) ? "New status effect" : RedeemPrefabCatalog.GetStatusEffectDisplayName(entry.name);
        }

        private static bool IsPlayerScaleEffect(string name)
        {
            return name == "PlayerShrink" || name == "PlayerGrow";
        }

        private void BuildEntryFields(GameObject cardRoot, StatusEffectEntry entry)
        {
            GuiHelper.ClearContainer(cardRoot);
            var layout = new Step2RowLayout(cardRoot, m_list.CardContentTopY, m_list.CardContentWidth);

            List<DropdownOption> nameOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.StatusEffects, entry.name);
            layout.DropdownRow("Name", "The status effect to apply.", nameOptions, entry.name, v =>
            {
                entry.name = v;
                entry.duration = RedeemPrefabCatalog.LookupStatusEffectTTL(v);
                m_list.RefreshListLabels();
                BuildEntryFields(cardRoot, entry);
            },
            defaultValue: null);

            bool isPlayerScale = IsPlayerScaleEffect(entry.name);
            layout.FloatRow("Duration",
                isPlayerScale
                    ? "How long the scale/speed change lasts, in seconds. Unlike other effects, both -1 and 0 fall back to a fixed 30 seconds."
                    : "How long the effect lasts, in seconds. Use -1 for the effect's own default duration and 0 for infinite.",
                entry.duration, v => entry.duration = v, defaultValue: -1f);
            layout.PairRow(
                () => layout.ToggleRow("Persists through death", "Whether this status effect survives the player dying.",
                    entry.persistsThroughDeath, v => entry.persistsThroughDeath = v, defaultValue: false),
                () => layout.ToggleRow("Renew", $"Whether re-applying this redeem while already active resets its {Emphasis.Of("Duration")}.",
                    entry.renew, v => entry.renew = v, defaultValue: false));

            // Runs on every (re)build of this card, including the Name-dropdown-triggered rebuild
            // above, so the card and page scroll range always match the currently rendered rows.
            m_list.SetCardContentHeight(Mathf.Abs(layout.CurrentY));
        }

        public void Populate(RedeemData working)
        {
            StatusEffectData data = working.statusEffectData;

            // A redeem with no status effects does nothing, so a fresh (or emptied) list starts
            // with one default entry - same as SpawnCreatureForm.
            if (data.list.Count == 0)
                data.list.Add(new StatusEffectEntry());

            m_list.Populate(data.list, () => data.random, v => data.random = v);
        }
    }
}
