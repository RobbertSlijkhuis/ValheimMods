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
    /// <para>
    /// The whole scaffold lives inside ONE outer scroll view (like the other step-2 forms) - the
    /// list and card are plain panels sized to their content, not scroll views of their own, so a
    /// tall entry form scrolls the page instead of being squeezed into whatever height is left
    /// below the toggle and buttons. The caller's form must report its content height through
    /// <see cref="SetCardContentHeight"/> after every (re)build. The one exception is a list that
    /// overflows its panel: once pinned, the list scrolls its own rows with the mouse wheel
    /// (<see cref="ScrollForwardPanel"/>) instead of moving the page.
    /// </para>
    /// </summary>
    internal class EntryListEditor<TEntry> where TEntry : CloneableData, new()
    {
        // Same width as step 1's effect-type list, so the two lists line up across wizard steps.
        private const float ListWidth = RedeemWizard.TypeListWidth;
        private const float ColumnGap = 20f;
        private const float ListItemHeight = 32f;
        private const float ListItemGap = 4f;
        private const float ActionButtonHeight = 36f;
        private const float EmptyTextInset = 14f;

        private readonly SelectorList m_selectorList = new SelectorList();

        private GameObject m_scrollContent;
        private GameObject m_listContent;
        private GameObject m_listItems;
        private GameObject m_cardRoot;
        private Toggle m_randomToggle;

        // Distance from the outer scroll content's top to the list/card panels' top, and the
        // shortest the panels may be (so a short form still fills the visible viewport).
        private float m_panelsTopOffset;
        private float m_minPanelHeight;
        private float m_listHeight;
        private float m_formHeight;

        // A header (caller rows above the toggle) puts the list far down the page, so switching
        // entries must keep the current scroll position instead of jumping back to the very top.
        private bool m_hasHeader;
        private float m_totalContentHeight;

        // Once the page scrolls past the Add/Delete row, that row pins to the top of the viewport
        // and the list panel pins directly beneath it while the (taller) form scrolls past. All
        // three shift by the same overshoot; m_listTravel caps the list's share so it can't slide
        // below the card's bottom edge.
        private RectTransform m_addButton;
        private RectTransform m_deleteButton;
        private float m_buttonRowTopOffset;
        private float m_buttonRestY;
        private float m_listTravel;

        // The list panel clips its buttons (m_listItems, a child of m_listContent) and scrolls them
        // itself once it is pinned (or can't pin because it already spans the whole card) - so the
        // mouse wheel over a pinned list moves the list, not the page/form. Before it pins, the
        // wheel scrolls the page as usual until the list reaches the top and sticks.
        private float m_listPanelHeight;
        private float m_listScroll;
        private float m_pinnedAmount;

        private float ListScrollRange => Mathf.Max(0f, m_listHeight - m_listPanelHeight);

        // Space the pinned button row occupies above the pinned list panel.
        private static float PinnedRowHeight => ActionButtonHeight + RedeemWizard.RowGap;

        private List<TEntry> m_entries = new List<TEntry>();
        private Func<bool> m_getRandom;
        private Action<bool> m_setRandom;
        private Func<TEntry, string> m_itemLabel;
        private Action<GameObject, TEntry> m_buildEntryForm;
        private int m_selectedIndex;

        /// <summary>
        /// Width for the caller's entry-form Step2RowLayout - the full card-column width, no inset:
        /// the column itself has no background (each field row is its own dark card), so the
        /// field cards sit flush with the Delete button above them.
        /// </summary>
        public float CardContentWidth => CardWidth;

        /// <summary>Top Y (relative to the card column's own top edge) the caller's entry-form Step2RowLayout should start at.</summary>
        public float CardContentTopY => 0f;

        private float CardWidth { get; set; }

        /// <summary>
        /// Builds the list+card scaffold once as children of <paramref name="parent"/>, starting
        /// at <paramref name="topY"/>. <paramref name="itemLabel"/> renders one entry as the left
        /// list's row text; <paramref name="buildEntryForm"/> fills the right card for whichever
        /// entry is currently selected (called on every selection change, add, or delete).
        /// </summary>
        /// <param name="buildHeader">
        /// Optional. Lays out extra rows in the same outer scroll view, ABOVE the random toggle
        /// (e.g. a form's own chest-level settings). Receives the scroll content and the row width,
        /// and returns the absolute Y the rows ended at (its <c>Step2RowLayout.CurrentY</c>). The
        /// scaffold then starts below that, so the page scrolls as one - the pinned button row /
        /// list math already works off wherever the toggle row lands.
        /// </param>
        /// <param name="randomLabel">Optional override for the random toggle's title.</param>
        /// <param name="randomDescription">Optional override for the random toggle's description.</param>
        public void Build(GameObject parent, float topY, Func<TEntry, string> itemLabel, Action<GameObject, TEntry> buildEntryForm,
            Func<GameObject, float, float> buildHeader = null, string randomLabel = null, string randomDescription = null)
        {
            m_itemLabel = itemLabel;
            m_buildEntryForm = buildEntryForm;
            m_hasHeader = buildHeader != null;

            // One outer scroll view holds everything (toggle, buttons, list, card) - everything
            // below is laid out inside it at natural height, so the Step2RowLayout defaults (width
            // minus the scrollbar) apply to the whole scaffold.
            m_scrollContent = ScrollableList.CreateFixed(parent, "EntryListEditor",
                new Vector2(0f, topY), RedeemWizard.Step2FieldWidth, RedeemWizard.Step2ContentHeight,
                backgroundColor: Color.clear, autoHideScrollbar: true);
            float contentWidth = RedeemWizard.Step2FieldWidth - ScrollableList.ScrollbarWidth;

            float toggleTopY = m_hasHeader ? buildHeader(m_scrollContent, contentWidth) : 0f;
            var toggleLayout = new Step2RowLayout(m_scrollContent, toggleTopY, contentWidth);
            m_randomToggle = toggleLayout.ToggleRow(
                randomLabel ?? "Pick one at random",
                randomDescription ?? "If enabled, one entry from the list below is chosen at random each time this redeem fires. Otherwise every entry is used.",
                false, v => m_setRandom?.Invoke(v));

            float listX = -contentWidth / 2f + ListWidth / 2f;
            CardWidth = contentWidth - ListWidth - ColumnGap;
            float cardX = listX + ListWidth / 2f + ColumnGap + CardWidth / 2f;

            // Add/Delete buttons sit directly below the toggle row, above the list/card.
            float buttonRowTopY = toggleLayout.CurrentY;
            float buttonCenterY = buttonRowTopY - ActionButtonHeight / 2f;

            GameObject addBtnObj = GuiHelper.CreateButton(
                text: "+ Add",
                parent: m_scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(listX, buttonCenterY),
                width: ListWidth,
                height: ActionButtonHeight
            );
            addBtnObj.SetActive(true);
            addBtnObj.GetComponent<Button>().onClick.AddListener(OnAddClicked);
            m_addButton = (RectTransform)addBtnObj.transform;

            GameObject deleteBtnObj = GuiHelper.CreateButton(
                text: "Delete this entry",
                parent: m_scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(cardX, buttonCenterY),
                width: CardWidth,
                height: ActionButtonHeight
            );
            deleteBtnObj.SetActive(true);
            deleteBtnObj.GetComponent<Button>().onClick.AddListener(OnDeleteClicked);
            m_deleteButton = (RectTransform)deleteBtnObj.transform;
            m_buttonRowTopOffset = Mathf.Abs(buttonRowTopY);
            m_buttonRestY = m_addButton.anchoredPosition.y;

            // List/card start a RowGap below the button row. They are plain panels (not scroll
            // views) sized by LayoutPanels() to whichever of the list/form is taller, but never
            // shorter than what's left of the visible viewport, so a short form still fills it.
            float listTopY = buttonRowTopY - ActionButtonHeight - RedeemWizard.RowGap;
            m_panelsTopOffset = Mathf.Abs(listTopY);
            m_minPanelHeight = Mathf.Max(0f, RedeemWizard.Step2ContentHeight - m_panelsTopOffset);

            m_listContent = GuiHelper.CreateCard(m_scrollContent, new Vector2(listX, listTopY), ListWidth, m_minPanelHeight);
            m_listContent.AddComponent<RectMask2D>();

            m_listItems = new GameObject("ListItems");
            m_listItems.transform.SetParent(m_listContent.transform, false);
            var itemsRt = m_listItems.AddComponent<RectTransform>();
            itemsRt.anchorMin = new Vector2(0.5f, 1f);
            itemsRt.anchorMax = new Vector2(0.5f, 1f);
            itemsRt.pivot = new Vector2(0.5f, 1f);
            itemsRt.sizeDelta = new Vector2(ListWidth, 0f);
            itemsRt.anchoredPosition = Vector2.zero;

            m_cardRoot = GuiHelper.CreateCard(m_scrollContent, new Vector2(cardX, listTopY), CardWidth, m_minPanelHeight);

            // No backdrop on the form column: every field row is already its own dark card, so a
            // dark panel behind them would double the transparency. The Image stays (just clear)
            // so the empty gaps between rows still take mouse-wheel input.
            m_cardRoot.GetComponent<Image>().color = Color.clear;

            // The buttons were created first, so the card would draw over them once the form
            // scrolls up underneath the pinned Delete button - move them back on top.
            m_addButton.SetAsLastSibling();
            m_deleteButton.SetAsLastSibling();

            // ScrollRect raises this from its own LateUpdate whenever the content moved, i.e. in
            // the same frame as the scroll - so the pinned elements never lag a frame behind.
            // includeInactive: the wizard builds every step's forms while step 2 is still hidden,
            // and GetComponentInParent skips inactive objects by default (returning null).
            ScrollRect outerScroll = m_scrollContent.GetComponentInParent<ScrollRect>(true);
            outerScroll.onValueChanged.AddListener(_ => UpdatePins());

            // Wheel events over the list (buttons included - they have no scroll handler of their
            // own, so the event bubbles up to this panel) go to TryScrollList first.
            m_listContent.AddComponent<ScrollForwardPanel>().Init(outerScroll, TryScrollList);
        }

        /// <summary>
        /// The caller's entry form reports the content height it just laid out (its
        /// <c>Step2RowLayout.CurrentY</c>, absolute) so the card, list and outer scroll range can
        /// grow to fit it. Call at the end of every (re)build of the entry form - including
        /// rebuilds triggered by a field that gates other fields.
        /// </summary>
        public void SetCardContentHeight(float contentHeight)
        {
            m_formHeight = contentHeight;
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            // Only the form drives the page height - a list taller than its panel scrolls inside
            // the panel instead of stretching the page.
            float panelHeight = Mathf.Max(m_minPanelHeight, m_formHeight);

            // The list panel is at most one viewport (minus the pinned button row above it) tall so
            // it still fits on screen while pinned.
            float pinnedListHeight = RedeemWizard.Step2ContentHeight - PinnedRowHeight;
            m_listPanelHeight = Mathf.Min(panelHeight, pinnedListHeight);
            m_listTravel = panelHeight - m_listPanelHeight;

            ScrollableList.SetContentHeight(m_listContent, m_listPanelHeight);
            ScrollableList.SetContentHeight(m_listItems, m_listHeight);
            ScrollableList.SetContentHeight(m_cardRoot, panelHeight);
            m_totalContentHeight = m_panelsTopOffset + panelHeight;
            ScrollableList.SetContentHeight(m_scrollContent, m_totalContentHeight);

            m_listScroll = Mathf.Clamp(m_listScroll, 0f, ListScrollRange);
            ApplyListScroll();
            UpdatePins();
        }

        private void ApplyListScroll()
        {
            var rt = (RectTransform)m_listItems.transform;
            rt.anchoredPosition = new Vector2(0f, Mathf.Round(m_listScroll));
        }

        // Wheel handler for the list panel. Consumes the scroll (returns true) only while the list
        // is sticky - pinned under the button row, or spanning the full card so pinning can't
        // happen - and actually overflows; otherwise the wheel falls through to the page.
        private bool TryScrollList(float delta)
        {
            float range = ListScrollRange;
            bool sticky = m_pinnedAmount > 0f || m_listTravel <= 0f;
            if (!sticky || range <= 0f)
                return false;

            m_listScroll = Mathf.Clamp(m_listScroll + delta, 0f, range);
            ApplyListScroll();
            return true;
        }

        // Brings the selected row into the list panel's visible range (used after adding an entry,
        // which lands at the top of a possibly-scrolled list).
        private void ScrollListToSelected()
        {
            if (m_entries.Count == 0)
                return;

            // The list renders newest-first, so the row's position is counted from the end.
            int displayIndex = m_entries.Count - 1 - m_selectedIndex;
            float itemTop = displayIndex * (ListItemHeight + ListItemGap);
            float itemBottom = itemTop + ListItemHeight;
            if (itemTop < m_listScroll)
                m_listScroll = itemTop;
            else if (itemBottom > m_listScroll + m_listPanelHeight)
                m_listScroll = itemBottom - m_listPanelHeight;

            m_listScroll = Mathf.Clamp(m_listScroll, 0f, ListScrollRange);
            ApplyListScroll();
        }

        // Once the page has scrolled the Add/Delete row's resting top edge past the viewport top,
        // push the row back down by that overshoot so it stays at the top. The list panel rests
        // PinnedRowHeight below the row, so the same overshoot keeps it pinned right beneath it
        // (capped at m_listTravel so it stops at the card's bottom edge).
        // Rounded to whole pixels - legacy uGUI Text blurs on fractional positions.
        private void UpdatePins()
        {
            float scrolled = ((RectTransform)m_scrollContent.transform).anchoredPosition.y;
            float pinned = Mathf.Round(Mathf.Max(0f, scrolled - m_buttonRowTopOffset));
            m_pinnedAmount = pinned;

            m_addButton.anchoredPosition = new Vector2(m_addButton.anchoredPosition.x, m_buttonRestY - pinned);
            m_deleteButton.anchoredPosition = new Vector2(m_deleteButton.anchoredPosition.x, m_buttonRestY - pinned);

            var list = (RectTransform)m_listContent.transform;
            list.anchoredPosition = new Vector2(list.anchoredPosition.x, Mathf.Round(-m_panelsTopOffset) - Mathf.Min(pinned, m_listTravel));
        }

        // Jump back to the top when the shown entry changes - otherwise a shorter form could
        // leave the page scrolled past its own content.
        private void ResetScroll(bool forceTop = false)
        {
            var rt = (RectTransform)m_scrollContent.transform;

            // With a header, only pull the scroll back inside the (possibly shorter) new content
            // range - the user is working on the list far below the top, so don't yank them up.
            // (A fresh Populate still starts from the top.)
            float y = 0f;
            if (m_hasHeader && !forceTop)
            {
                float maxScroll = Mathf.Max(0f, m_totalContentHeight - RedeemWizard.Step2ContentHeight);
                y = Mathf.Clamp(rt.anchoredPosition.y, 0f, maxScroll);
            }

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            UpdatePins();
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
            ResetScroll(forceTop: true);
        }

        private void OnAddClicked()
        {
            m_entries.Add(new TEntry());
            m_selectedIndex = m_entries.Count - 1;
            RefreshList();
            RefreshCard();
            ScrollListToSelected();
            ResetScroll();
        }

        private void OnDeleteClicked()
        {
            if (m_entries.Count == 0)
                return;

            m_entries.RemoveAt(m_selectedIndex);
            m_selectedIndex = m_entries.Count > 0 ? Mathf.Clamp(m_selectedIndex, 0, m_entries.Count - 1) : 0;
            RefreshList();
            RefreshCard();
            ResetScroll();
        }

        private void OnEntryClicked(string key)
        {
            m_selectedIndex = int.Parse(key);
            m_selectorList.Select(key);
            RefreshCard();
            ResetScroll();
        }

        /// <summary>
        /// Re-renders just the left list's row labels (via the caller's <c>itemLabel</c>) without
        /// touching the form card - for when an edit in the form changes what the row should say
        /// (e.g. picking a prefab renames "New creature").
        /// </summary>
        public void RefreshListLabels() => RefreshList();

        private void RefreshList()
        {
            var items = new List<(string Key, string Label)>();
            // Entries are stored in add order but rendered newest-first; keys stay the data index.
            for (int i = m_entries.Count - 1; i >= 0; i--)
                items.Add((i.ToString(), m_itemLabel(m_entries[i]) ?? $"Entry {i + 1}"));

            // Old buttons are cleared (Destroy is end-of-frame, same as the card) so a rebuild
            // doesn't stack a second set on the panel.
            GuiHelper.ClearContainer(m_listItems);
            m_listHeight = m_selectorList.Build(m_listItems.transform, items, ListWidth - 8f, ListItemHeight, ListItemGap, OnEntryClicked);
            m_selectorList.Select(m_entries.Count > 0 ? m_selectedIndex.ToString() : null);
            LayoutPanels();
        }

        private void RefreshCard()
        {
            GuiHelper.ClearContainer(m_cardRoot);
            m_formHeight = 0f;

            if (m_entries.Count == 0)
            {
                GuiHelper.CreateCardText(m_cardRoot, "No entries yet - click + Add to create one.",
                    -EmptyTextInset, 14, GUIManager.Instance.ValheimBeige, CardContentWidth - 2f * EmptyTextInset);
                LayoutPanels();
                return;
            }

            m_buildEntryForm(m_cardRoot, m_entries[m_selectedIndex]);
        }
    }
}
