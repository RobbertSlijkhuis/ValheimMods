using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Modal, read-only leaderboard of the active profile's viewer stats (see
    /// <see cref="LeaderboardHelper"/>): a search box + world dropdown, sortable column headers, a
    /// scrolling list and a scope-wide totals footer pinned below it. Built like
    /// <see cref="NewsDialog"/> (full-screen dim blocker, wood panel, <see cref="InputBlockGate"/>)
    /// and opened from a sidebar action button.
    ///
    /// Placement (#) is computed over the whole selected scope before search filtering, from the
    /// active numeric column (highest = #1; a text column falls back to points), so reversing the
    /// sort or searching never renumbers anyone. Equal values share a number (1, 2, 2, 4).
    /// </summary>
    internal class LeaderboardsDialog
    {
        private GameObject m_root;
        private GameObject m_panel;
        private Text m_titleText;
        private InputField m_searchField;
        private readonly SearchableDropdown m_scopeDropdown = new SearchableDropdown();
        private GameObject m_listContent;
        private Text m_footerText;
        private Text m_footerNoteText;
        private bool m_blockingInput;

        private readonly ColumnSortState m_sort = new ColumnSortState(SortPoints) { Ascending = false };
        private readonly List<(string Text, string SortKey, Text Label)> m_sortableHeaders = new List<(string, string, Text)>();

        private List<LeaderboardWorld> m_worlds = new List<LeaderboardWorld>();
        private List<LeaderboardRow> m_rows = new List<LeaderboardRow>();
        private string m_scope = AllWorldsValue;
        private string m_searchText = "";
        private long m_totalPoints;
        private long m_totalRedeems;

        private const string SortPoints = "points";
        private const string SortRedeems = "redeems";
        private const string SortName = "name";
        private const string SortFavourite = "favourite";
        private const string AllWorldsValue = "all";

        // ── layout ───────────────────────────────────────────────────────────
        private const float PanelWidth = 960f;
        private const float PanelHeight = 640f;
        private const float Margin = 30f;

        private const float LeftEdgeX = -(PanelWidth / 2f) + Margin;                      // -450
        private const float RightEdgeX = (PanelWidth / 2f) - Margin;                      // 450
        private const float ListRightX = RightEdgeX - ScrollableList.ScrollbarWidth;       // 434 (the list's usable right edge)
        private const float ListWidth = PanelWidth - 2f * Margin;

        private const float TitleY = -35f;
        private const float ToolbarY = -85f;
        private const float HeaderY = -128f;
        private const float ListTopY = -145f;
        private const float ListHeight = 355f;
        private const float FooterY = -523f;
        private const float BtnY = 40f;
        private const float BtnWidth = 160f;
        private const float BtnHeight = 50f;

        private const float SearchWidth = 380f;
        private const float DropdownWidth = 280f;

        private const float ColRankW = 70f;
        private const float ColNameW = 230f;
        private const float ColPointsW = 200f;
        private const float ColCountW = 110f;
        private const float ColFavW = ListRightX - (LeftEdgeX + ColRankW + ColNameW + ColPointsW + ColCountW);

        // The first column is the only one flush with the list's left edge - inset just its text.
        private const float ColRankTextW = ColRankW - ListRow.LeftPadding;
        private const float ColRankX = LeftEdgeX + ListRow.LeftPadding + ColRankTextW / 2f;
        private const float ColNameX = LeftEdgeX + ColRankW + ColNameW / 2f;
        private const float ColPointsX = LeftEdgeX + ColRankW + ColNameW + ColPointsW / 2f;
        private const float ColCountX = LeftEdgeX + ColRankW + ColNameW + ColPointsW + ColCountW / 2f;
        private const float ColFavX = LeftEdgeX + ColRankW + ColNameW + ColPointsW + ColCountW + ColFavW / 2f;

        // Small gap so a long left-aligned value doesn't touch the next column's text.
        private const float CellGap = 10f;

        private static readonly Color GoldColor = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SilverColor = new Color(0.78f, 0.78f, 0.82f, 1f);
        private static readonly Color BronzeColor = new Color(0.80f, 0.50f, 0.20f, 1f);

        public bool IsVisible => m_root != null && m_root.activeSelf;

        /// <summary>
        /// Creates the dialog once and hides it. Must be called after
        /// <see cref="GUIManager.OnCustomGUIAvailable"/>.
        /// </summary>
        public void Init()
        {
            if (m_root != null)
                return;

            m_root = GuiHelper.CreateRegion(
                GUIManager.CustomGUIFront, "LeaderboardsDialog",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                offsetMin: Vector2.zero, offsetMax: Vector2.zero);
            GuiHelper.AddBackground(m_root, new Color(0f, 0f, 0f, 0.55f)); // raycast target: blocks clicks underneath

            m_panel = GUIManager.Instance.CreateWoodpanel(
                parent:    m_root.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position:  new Vector2(0f, 0f),
                width:     PanelWidth,
                height:    PanelHeight,
                draggable: false
            );

            m_titleText = CreateLabel(m_panel, "", new Vector2(0f, TitleY), PanelWidth - 80f, 30f, 20,
                GUIManager.Instance.ValheimOrange, TextAnchor.MiddleCenter, anchorTop: true);

            BuildToolbar();
            BuildHeaders();

            m_listContent = ScrollableList.CreateFixed(
                m_panel, "Leaderboard", new Vector2(0f, ListTopY), ListWidth, ListHeight, autoHideScrollbar: true);

            m_footerText = CreateLabel(m_panel, "", new Vector2(LeftEdgeX + 300f, FooterY), 600f, 24f, 14,
                GUIManager.Instance.ValheimBeige, TextAnchor.MiddleLeft, anchorTop: true);
            m_footerNoteText = CreateLabel(m_panel, "", new Vector2(RightEdgeX - 150f, FooterY), 300f, 24f, 14,
                GUIManager.Instance.ValheimOrange, TextAnchor.MiddleRight, anchorTop: true);

            GameObject closeBtn = GuiHelper.CreateButton(
                text:      "Close",
                parent:    m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position:  new Vector2(0f, BtnY),
                width:     BtnWidth,
                height:    BtnHeight
            );
            closeBtn.SetActive(true);
            closeBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimOrange;
            closeBtn.GetComponent<Button>().onClick.AddListener(Hide);

            m_root.SetActive(false);
        }

        /// <summary>
        /// Shows the active profile's leaderboard, defaulting to the current world. Only usable
        /// inside a world (the stats are per world).
        /// </summary>
        public void Show()
        {
            if (m_root == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] LeaderboardsDialog.Show called before Init().");
                return;
            }

            if (!LeaderboardHelper.TryGetCurrentWorld(out long currentUid, out _))
            {
                ToastNotifications.Show("Leaderboards are only available inside a world.", ToastType.Warning);
                return;
            }

            GuiHelper.SetTruncatedText(m_titleText, $"Leaderboards - {ProfileManager.ActiveProfile}", PanelWidth - 80f);

            m_searchText = "";
            m_searchField.text = "";

            m_worlds = LeaderboardHelper.LoadWorlds();
            m_scope = currentUid.ToString(CultureInfo.InvariantCulture);
            m_scopeDropdown.SetOptions(BuildScopeOptions(), m_scope);

            RebuildRows();

            m_root.transform.SetAsLastSibling();
            m_root.SetActive(true);

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }
        }

        private void Hide()
        {
            if (m_root != null)
                m_root.SetActive(false);

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }
        }

        // ── toolbar / headers ────────────────────────────────────────────────

        private void BuildToolbar()
        {
            m_searchField = GuiFieldBuilder.CreateInputField(
                m_panel, new Vector2(LeftEdgeX + SearchWidth / 2f, ToolbarY), SearchWidth,
                placeholderText: "Search viewers or redeems...");
            m_searchField.onValueChanged.AddListener(OnSearchChanged);

            m_scopeDropdown.Build(
                m_panel, new Vector2(RightEdgeX - DropdownWidth / 2f, ToolbarY), DropdownWidth, GuiFieldBuilder.FieldHeight,
                new List<DropdownOption> { new DropdownOption(AllWorldsValue, "All worlds") }, AllWorldsValue, showSearch: false);
            m_scopeDropdown.OnValueChanged += OnScopeChanged;
        }

        private void BuildHeaders()
        {
            m_sortableHeaders.Clear();

            // "#" is the placement, derived from the sort - not a sort column itself.
            CreateHeader("#", ColRankX, ColRankTextW, null);
            CreateHeader("Viewer", ColNameX, ColNameW - CellGap, SortName);
            CreateHeader("Points spent", ColPointsX, ColPointsW - CellGap, SortPoints);
            CreateHeader("Redeems", ColCountX, ColCountW - CellGap, SortRedeems);
            CreateHeader("Favourite redeem", ColFavX, ColFavW - CellGap, SortFavourite);
        }

        private void CreateHeader(string text, float x, float width, string sortKey)
        {
            Text header = CreateLabel(m_panel, text, new Vector2(x, HeaderY), width, 20f,
                GuiFieldBuilder.FieldFontSize, GUIManager.Instance.ValheimOrange, TextAnchor.MiddleLeft, anchorTop: true);

            if (sortKey == null)
                return;

            // Same look, just clickable - an invisible Button on the existing Text GameObject.
            Button headerBtn = header.gameObject.AddComponent<Button>();
            headerBtn.targetGraphic = header;
            headerBtn.transition = Selectable.Transition.None;
            headerBtn.onClick.AddListener(() => OnHeaderClicked(sortKey));

            m_sortableHeaders.Add((text, sortKey, header));
        }

        private void RefreshHeaderIndicators()
        {
            foreach (var (text, sortKey, label) in m_sortableHeaders)
                label.text = sortKey == m_sort.Key ? $"{text} {(m_sort.Ascending ? "▲" : "▼")}" : text;
        }

        private void OnHeaderClicked(string sortKey)
        {
            bool wasActive = m_sort.Key == sortKey;
            m_sort.ToggleOrSet(sortKey);

            // A leaderboard reads best biggest-first: the first click on a number column goes
            // descending (text columns keep A-Z first).
            if (!wasActive && IsNumeric(sortKey))
                m_sort.Ascending = false;

            RefreshList();
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value ?? "";
            RefreshList();
        }

        private void OnScopeChanged(string value)
        {
            m_scope = value;
            RebuildRows();
        }

        // ── data ─────────────────────────────────────────────────────────────

        private List<DropdownOption> BuildScopeOptions()
        {
            List<DropdownOption> options = new List<DropdownOption> { new DropdownOption(AllWorldsValue, "All worlds") };

            // Current world first, then the rest alphabetically.
            foreach (LeaderboardWorld world in m_worlds.OrderByDescending(w => w.isCurrent).ThenBy(w => w.name, StringComparer.OrdinalIgnoreCase))
            {
                bool duplicateName = m_worlds.Count(w => string.Equals(w.name, world.name, StringComparison.OrdinalIgnoreCase)) > 1;
                string label = world.name;

                if (duplicateName)
                    label += $" ({world.uid})";

                if (world.isCurrent)
                    label += " (current)";

                options.Add(new DropdownOption(world.uid.ToString(CultureInfo.InvariantCulture), label));
            }

            return options;
        }

        private void RebuildRows()
        {
            IEnumerable<LeaderboardWorld> selected = m_scope == AllWorldsValue
                ? m_worlds
                : m_worlds.Where(w => w.uid.ToString(CultureInfo.InvariantCulture) == m_scope);

            m_rows = LeaderboardHelper.BuildRows(selected);
            m_totalPoints = m_rows.Sum(r => r.points);
            m_totalRedeems = m_rows.Sum(r => (long)r.redeemCount);

            RefreshList();
        }

        // ── sorting / ranking ────────────────────────────────────────────────

        private static bool IsNumeric(string sortKey) => sortKey == SortPoints || sortKey == SortRedeems;

        /// <summary>
        /// Placement over the whole scope: by the active numeric column, highest first (points when
        /// a text column is active). Equal values share a number (1, 2, 2, 4); within a tie the
        /// order is the file order (OrderByDescending is stable).
        /// </summary>
        private Dictionary<string, int> ComputeRanks()
        {
            Func<LeaderboardRow, long> value;
            if (m_sort.Key == SortRedeems)
                value = r => r.redeemCount;
            else
                value = r => r.points;

            List<LeaderboardRow> byRank = m_rows.OrderByDescending(value).ToList();
            Dictionary<string, int> ranks = new Dictionary<string, int>(byRank.Count);

            int rank = 0;
            long previous = 0;

            for (int i = 0; i < byRank.Count; i++)
            {
                long current = value(byRank[i]);
                if (i == 0 || current != previous)
                    rank = i + 1;

                ranks[byRank[i].id] = rank;
                previous = current;
            }

            return ranks;
        }

        private List<LeaderboardRow> SortRows()
        {
            bool asc = m_sort.Ascending;
            IEnumerable<LeaderboardRow> ordered;

            switch (m_sort.Key)
            {
                case SortRedeems:
                    ordered = asc ? m_rows.OrderBy(r => r.redeemCount) : m_rows.OrderByDescending(r => r.redeemCount);
                    break;
                case SortName:
                    ordered = asc ? m_rows.OrderBy(r => r.name, StringComparer.OrdinalIgnoreCase) : m_rows.OrderByDescending(r => r.name, StringComparer.OrdinalIgnoreCase);
                    break;
                case SortFavourite:
                    ordered = asc ? m_rows.OrderBy(r => r.favouriteTitle ?? "", StringComparer.OrdinalIgnoreCase) : m_rows.OrderByDescending(r => r.favouriteTitle ?? "", StringComparer.OrdinalIgnoreCase);
                    break;
                default:
                    ordered = asc ? m_rows.OrderBy(r => r.points) : m_rows.OrderByDescending(r => r.points);
                    break;
            }

            return ordered.ToList();
        }

        private bool MatchesSearch(LeaderboardRow row)
        {
            if (string.IsNullOrEmpty(m_searchText))
                return true;

            return Contains(row.name, m_searchText)
                || Contains(row.favouriteTitle, m_searchText)
                || row.previousNames.Any(n => Contains(n, m_searchText));
        }

        private static bool Contains(string text, string search)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ── list ─────────────────────────────────────────────────────────────

        private void RefreshList()
        {
            GuiHelper.ClearContainer(m_listContent);
            RefreshHeaderIndicators();

            Dictionary<string, int> ranks = ComputeRanks();
            List<LeaderboardRow> visible = SortRows().Where(MatchesSearch).ToList();

            float yOffset = -(ListRow.ListTopPadding + ListRow.ItemHeight / 2f);

            for (int i = 0; i < visible.Count; i++)
            {
                BuildRow(visible[i], ranks[visible[i].id], yOffset, i);
                yOffset -= ListRow.ItemHeight + ListRow.ItemSpacing;
            }

            if (m_rows.Count == 0)
                BuildEmptyState("No redeems have been recorded yet.");
            else if (visible.Count == 0)
                BuildEmptyState("No viewers match your search.");

            RectTransform contentRt = m_listContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ListRow.ItemHeight / 2f);

            RefreshFooter(visible.Count);
        }

        private void RefreshFooter(int shownCount)
        {
            m_footerText.text = $"{m_rows.Count:N0} viewers   |   {m_totalPoints:N0} points spent   |   {m_totalRedeems:N0} redeems";
            m_footerNoteText.text = string.IsNullOrEmpty(m_searchText) ? "" : $"Showing {shownCount:N0} of {m_rows.Count:N0}";
        }

        private void BuildEmptyState(string message)
        {
            CreateLabel(m_listContent, message, new Vector2(0f, -(ListRow.ListTopPadding + 30f)), ListWidth - 2f * Margin, 30f, 16,
                GUIManager.Instance.ValheimBeige, TextAnchor.MiddleCenter, anchorTop: true);
        }

        private void BuildRow(LeaderboardRow data, int rank, float yOffset, int rowIndex)
        {
            var (row, rowBackground) = ListRow.Create(m_listContent, "LeaderboardRow", yOffset);
            rowBackground.color = ListRow.ZebraColor(rowIndex);

            Color textColor = GUIManager.Instance.ValheimBeige;
            Color rankColor = rank == 1 ? GoldColor : rank == 2 ? SilverColor : rank == 3 ? BronzeColor : textColor;

            CreateCell(row, rank.ToString(CultureInfo.InvariantCulture), ColRankX, ColRankTextW, rankColor, TextAnchor.MiddleLeft);

            Text nameText = CreateCell(row, data.name, ColNameX, ColNameW - CellGap, textColor, TextAnchor.MiddleLeft);
            GuiHelper.SetTruncatedText(nameText, data.name, ColNameW - CellGap);
            if (data.previousNames.Count > 0)
                nameText.gameObject.AddComponent<TooltipTrigger>().Init("Previously known as: " + string.Join(", ", data.previousNames));

            double share = m_totalPoints > 0 ? data.points * 100.0 / m_totalPoints : 0.0;
            string pointsLabel = $"{data.points:N0} ({share.ToString("0.#", CultureInfo.InvariantCulture)}%)";
            CreateCell(row, pointsLabel, ColPointsX, ColPointsW - CellGap, textColor, TextAnchor.MiddleLeft);

            CreateCell(row, data.redeemCount.ToString("N0"), ColCountX, ColCountW - CellGap, textColor, TextAnchor.MiddleLeft);

            string favouriteLabel = string.IsNullOrEmpty(data.favouriteTitle) ? "-" : $"{data.favouriteTitle} x{data.favouriteCount}";
            Text favouriteText = CreateCell(row, favouriteLabel, ColFavX, ColFavW - CellGap, textColor, TextAnchor.MiddleLeft);
            GuiHelper.SetTruncatedText(favouriteText, favouriteLabel, ColFavW - CellGap);
        }

        private static Text CreateCell(GameObject row, string text, float x, float width, Color color, TextAnchor alignment)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(x, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = alignment;
            return label;
        }

        private static Text CreateLabel(GameObject parent, string text, Vector2 position, float width, float height, int fontSize, Color color, TextAnchor alignment, bool anchorTop)
        {
            float anchorY = anchorTop ? 1f : 0.5f;

            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, anchorY),
                anchorMax: new Vector2(0.5f, anchorY),
                position: position,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: height,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = alignment;
            return label;
        }
    }
}
