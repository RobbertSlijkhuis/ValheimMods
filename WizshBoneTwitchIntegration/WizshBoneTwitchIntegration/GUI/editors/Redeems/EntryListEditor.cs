using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Shared left-list/right-form-card scaffold for step-2 forms whose data is a
    /// <c>List&lt;TEntry&gt;</c> (SpawnCreature/StatusEffect) - reuses the same visual split as
    /// <see cref="SelectorList"/>/<see cref="SelectorPreviewPanel"/> (extracted from HelpTab +
    /// RedeemWizard step 1), but the right panel is an *editable* field card instead of read-only
    /// text. Left: entry list + "+ Add". Right: the selected entry's fields (built by the caller's
    /// <c>buildEntryForm</c> delegate) + "Delete this entry". A "random" toggle sits above both,
    /// always shown regardless of entry count - with 0-1 entries it has no observable effect, but
    /// hiding it bought nothing and complicated the layout.
    /// </summary>
    internal class EntryListEditor<TEntry> where TEntry : CloneableData, new()
    {
        private const float ListWidth = 160f;
        private const float ColumnGap = 20f;
        private const float ListItemHeight = 32f;
        private const float ListItemGap = 4f;
        private const float ActionButtonHeight = 36f;
        private const float CardPadding = 14f;

        private readonly SelectorList m_selectorList = new SelectorList();

        private GameObject m_listContent;
        private GameObject m_cardRoot;
        private Toggle m_randomToggle;

        private List<TEntry> m_entries = new List<TEntry>();
        private Func<bool> m_getRandom;
        private Action<bool> m_setRandom;
        private Func<TEntry, string> m_itemLabel;
        private Action<GameObject, TEntry> m_buildEntryForm;
        private int m_selectedIndex;

        /// <summary>Usable width for the caller's entry-form Step2RowLayout (the card's own scrollable-viewport width minus padding on both sides).</summary>
        public float CardContentWidth => CardWidth - ScrollableList.ScrollbarWidth - 2f * CardPadding;

        /// <summary>Top Y (relative to the card's own top edge) the caller's entry-form Step2RowLayout should start at.</summary>
        public float CardContentTopY => -CardPadding;

        private float CardWidth { get; set; }

        /// <summary>
        /// Builds the list+card scaffold once as children of <paramref name="parent"/>, starting
        /// at <paramref name="topY"/>. <paramref name="itemLabel"/> renders one entry as the left
        /// list's row text; <paramref name="buildEntryForm"/> fills the right card for whichever
        /// entry is currently selected (called on every selection change, add, or delete).
        /// </summary>
        public void Build(GameObject parent, float topY, Func<TEntry, string> itemLabel, Action<GameObject, TEntry> buildEntryForm)
        {
            m_itemLabel = itemLabel;
            m_buildEntryForm = buildEntryForm;

            // Full Step2FieldWidth, not Step2RowLayout's own scrollbar-inset default - that default
            // is for rows inside a ScrollableList's content area; this toggle sits directly on the
            // step-2 root, alongside the full-width list/card row below it.
            var toggleLayout = new Step2RowLayout(parent, topY, RedeemWizard.Step2FieldWidth);
            m_randomToggle = toggleLayout.ToggleRow(
                "Pick one at random",
                "If enabled, one entry from the list below is chosen at random each time this redeem fires. Otherwise every entry is used.",
                false, v => m_setRandom?.Invoke(v));

            float listX = -RedeemWizard.Step2FieldWidth / 2f + ListWidth / 2f;
            CardWidth = RedeemWizard.Step2FieldWidth - ListWidth - ColumnGap;
            float cardX = listX + ListWidth / 2f + ColumnGap + CardWidth / 2f;

            // Add/Delete buttons sit directly below the toggle row, above the list/card.
            float buttonRowTopY = toggleLayout.CurrentY;
            float buttonCenterY = buttonRowTopY - ActionButtonHeight / 2f;

            GameObject addBtnObj = GuiHelper.CreateButton(
                text: "+ Add",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(listX, buttonCenterY),
                width: ListWidth,
                height: ActionButtonHeight
            );
            addBtnObj.SetActive(true);
            addBtnObj.GetComponent<Button>().onClick.AddListener(OnAddClicked);

            GameObject deleteBtnObj = GuiHelper.CreateButton(
                text: "Delete this entry",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(cardX, buttonCenterY),
                width: CardWidth,
                height: ActionButtonHeight
            );
            deleteBtnObj.SetActive(true);
            deleteBtnObj.GetComponent<Button>().onClick.AddListener(OnDeleteClicked);

            // List/card starts a RowGap below the button row; height is whatever's left down to
            // Step2ContentHeight's floor (RowGap above the Back/Next row) - not a hardcoded
            // constant, so the whole toggle+buttons+list/card stack always fits.
            float listTopY = buttonRowTopY - ActionButtonHeight - RedeemWizard.RowGap;
            float bottomY = topY - RedeemWizard.Step2ContentHeight;
            float listCardHeight = listTopY - bottomY;

            m_listContent = ScrollableList.CreateFixed(parent, "EntryList", new Vector2(listX, listTopY), ListWidth, listCardHeight, backgroundColor: GuiHelper.CardBackgroundColor);

            // A scroll view rather than a plain GuiHelper.CreateCard panel - an entry's field count
            // (e.g. SpawnCreature's Friendly-gated Commandable row) can exceed listCardHeight, and
            // the card needs to scroll internally rather than spill out of the wizard body.
            m_cardRoot = ScrollableList.CreateFixed(parent, "EntryCard", new Vector2(cardX, listTopY), CardWidth, listCardHeight,
                backgroundColor: GuiHelper.CardBackgroundColor, autoHideScrollbar: true);
        }

        /// <summary>
        /// Re-syncs the whole editor from <paramref name="entries"/> - called whenever the owning
        /// type-form's own Populate() runs (type-switch re-entry, edit-mode entry, or a fresh
        /// create session). <paramref name="getRandom"/>/<paramref name="setRandom"/> read/write
        /// the redeem's random flag.
        /// </summary>
        public void Populate(List<TEntry> entries, Func<bool> getRandom, Action<bool> setRandom)
        {
            m_entries = entries ?? new List<TEntry>();
            m_getRandom = getRandom;
            m_setRandom = setRandom;
            m_selectedIndex = m_entries.Count > 0 ? Mathf.Clamp(m_selectedIndex, 0, m_entries.Count - 1) : 0;

            m_randomToggle.isOn = m_getRandom();

            RefreshList();
            RefreshCard();
        }

        private void OnAddClicked()
        {
            m_entries.Add(new TEntry());
            m_selectedIndex = m_entries.Count - 1;
            RefreshList();
            RefreshCard();
        }

        private void OnDeleteClicked()
        {
            if (m_entries.Count == 0)
                return;

            m_entries.RemoveAt(m_selectedIndex);
            m_selectedIndex = m_entries.Count > 0 ? Mathf.Clamp(m_selectedIndex, 0, m_entries.Count - 1) : 0;
            RefreshList();
            RefreshCard();
        }

        private void OnEntryClicked(string key)
        {
            m_selectedIndex = int.Parse(key);
            m_selectorList.Select(key);
            RefreshCard();
        }

        private void RefreshList()
        {
            var items = new List<(string Key, string Label)>();
            for (int i = 0; i < m_entries.Count; i++)
                items.Add((i.ToString(), m_itemLabel(m_entries[i]) ?? $"Entry {i + 1}"));

            float listHeight = m_selectorList.Build(m_listContent.transform, items, ListWidth - ScrollableList.ScrollbarWidth - 8f, ListItemHeight, ListItemGap, OnEntryClicked);
            ScrollableList.SetContentHeight(m_listContent, listHeight);
            m_selectorList.Select(m_entries.Count > 0 ? m_selectedIndex.ToString() : null);
        }

        private void RefreshCard()
        {
            GuiHelper.ClearContainer(m_cardRoot);

            if (m_entries.Count == 0)
            {
                GuiHelper.CreateCardText(m_cardRoot, "No entries yet - click + Add to create one.",
                    CardContentTopY, 14, GUIManager.Instance.ValheimBeige, CardContentWidth);
                return;
            }

            m_buildEntryForm(m_cardRoot, m_entries[m_selectedIndex]);
        }
    }
}
