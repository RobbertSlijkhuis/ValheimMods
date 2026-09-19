using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Redeem history section - an inline swap-in view within <see cref="Tabs.HomeTab"/>, built
    /// the same way <see cref="Tabs.RedeemsTab"/> swaps its list view out for
    /// <see cref="RedeemWizard"/>: no floating card or dimmed scrim of its own (unlike this
    /// class's predecessor, <c>RedeemHistoryPanel</c>, or GUI_OLD/WizshBoneRedeemHistoryGUI.cs's
    /// separate wood panel) - just built directly into whatever <see cref="GameObject"/>
    /// <see cref="Create"/> is given.
    ///
    /// <see cref="Show"/> activates this section's own root and resets to page 1, mirroring
    /// <see cref="RedeemWizard.OpenCreate"/>/<see cref="RedeemWizard.OpenEdit"/>. Backing out fires
    /// <see cref="OnBack"/> instead of just hiding itself, letting the owning tab decide what to
    /// show in its place (<see cref="Tabs.HomeTab"/>'s own overview grid) - the exact same shape as
    /// <see cref="RedeemWizard.OnFinished"/>.
    ///
    /// Data/pagination/bulk-resolve logic is ported near-verbatim from
    /// GUI_OLD/WizshBoneRedeemHistoryGUI.cs (same <see cref="TwitchCustomRewards.m_redeemHistory"/>/
    /// <see cref="BulkRedeemResolveHelper"/> backend, same 30-per-page/newest-first pagination) -
    /// only the visual layout differs, to the mockup's 4-column (Viewer/Redeem/Cost/Actions) grid
    /// built with <see cref="ScrollableList"/>/<see cref="ListRow"/> instead of GUI_OLD's single
    /// combined-text row.
    ///
    /// Looks up <see cref="TwitchCustomRewards"/> fresh via <c>Game.instance.gameObject</c> on
    /// every refresh rather than caching it, matching every other new-UI tab's "own its
    /// dependencies" convention.
    /// </summary>
    internal class RedeemHistorySection
    {
        private const int PageSize = 30;

        private GameObject m_root;
        private GameObject m_listContainer;
        private InputField m_searchField;
        private Text m_pendingCountText;
        private Text m_pageLabel;
        private Button m_prevButton;
        private Button m_nextButton;
        private Button m_completeAllButton;
        private Button m_refundAllButton;

        private string m_searchText = "";
        private int m_currentPage = 0;

        /// <summary>
        /// Fired when the user clicks "Back" - the owning tab is responsible for showing whatever
        /// replaces this section (see <see cref="Tabs.HomeTab.ShowHistory"/>'s counterpart).
        /// </summary>
        public Action OnBack;

        private static readonly Color CompletedColor = new Color(0.95f, 0.65f, 0.2f, 1f);
        private static readonly Color RefundedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        private static readonly Color CompleteBtnColor = new Color(0.95f, 0.65f, 0.2f, 1f);
        private static readonly Color RefundBtnColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        // ── layout constants (same derived-from-shell-size pattern as RedeemsTab.cs) ───────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;   // 980
        private const float ContentHeight = WizshBoneShellGUI.PanelHeight - ShellTopBar.Height;  // 660
        private const float ContentMargin = 30f;

        // Same section padding as the other tabs: ContentMargin from the section root on every
        // side, TitleY at -30. The rightmost edge reserves the scrollbar's width (see ProfilesTab).
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;
        private const float RightEdgeX =  (ContentWidth / 2f) - ContentMargin - ScrollableList.ScrollbarWidth;

        private const float ActionBtnHeight = 40f;
        private const float TitleY          = -30f;
        private const float TitleWidth      = 300f;
        private const float PendingCountWidth = 200f;
        private const float PendingCountGap   = 14f;   // between the search field's right edge and the pending text
        private const float SearchWidth     = 300f;   // same as the other tabs' list search fields
        // Same vertical rhythm as ProfilesTab/RedeemsTab/ViewersTab: under-title description,
        // then the search row, column headers, and the list.
        private const float DescriptionY    = -50f;
        private const float SearchY         = -99f;
        private const float ColumnHeaderY   = -136f;
        private const float ListOffsetTop   = 155f;
        private const float ActionRowY      = ContentMargin + ActionBtnHeight / 2f;   // measured from the root's bottom edge
        private const float PaginationY     = ActionRowY + 50f;  // measured from the root's bottom edge
        private const float ListOffsetBottom = PaginationY + 30f;

        private const float AvailableWidth = ContentWidth - 2f * ContentMargin - ScrollableList.ScrollbarWidth;

        // The columns (headers + row content) sit ListRow.LeftPadding in from the list's left and
        // right edges, so row text/status/buttons aren't flush against the zebra stripe's ends.
        // The title, search field and pending count above keep the true LeftEdgeX/RightEdgeX.
        private const float ColumnsLeftX  = LeftEdgeX + ListRow.LeftPadding;
        private const float ColumnsRightX = RightEdgeX - ListRow.LeftPadding;
        private const float ColumnsWidth  = AvailableWidth - 2f * ListRow.LeftPadding;

        private const float ColViewerW  = ColumnsWidth * 1.4f / 6.8f;
        private const float ColRedeemW  = ColumnsWidth * 2.6f / 6.8f;
        private const float ColCostW    = ColumnsWidth * 0.8f / 6.8f;
        private const float ColActionsW = ColumnsWidth * 2.0f / 6.8f;

        private const float ColViewerX  = ColumnsLeftX + ColViewerW / 2f;
        private const float ColRedeemX  = ColumnsLeftX + ColViewerW + ColRedeemW / 2f;
        private const float ColCostX    = ColumnsLeftX + ColViewerW + ColRedeemW + ColCostW / 2f;
        private const float ColActionsX = ColumnsRightX - ColActionsW / 2f;

        private const float ActionBtnWidth = 90f;
        private const float ActionBtnGap   = 10f;
        private const float BtnRefundX   = ColumnsRightX - ActionBtnWidth / 2f;
        private const float BtnCompleteX = BtnRefundX - ActionBtnWidth - ActionBtnGap;

        /// <summary>
        /// Builds this section directly into <paramref name="parent"/> (the owning tab's own root)
        /// - no card/scrim wrapper, same as <see cref="RedeemWizard.Create"/>. Starts inactive.
        /// </summary>
        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "RedeemHistorySection");

            GuiHelper.CreateTitle("Redeem History", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "Redeems triggered this session. Complete a redeem to mark it fulfilled on Twitch, or refund it to give the viewer their points back.",
                m_root, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            m_pendingCountText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(LeftEdgeX + SearchWidth + PendingCountGap + PendingCountWidth / 2f, SearchY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: PendingCountWidth,
                height: 24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_pendingCountText.alignment = TextAnchor.MiddleLeft;

            m_searchField = GuiFieldBuilder.CreateInputField(m_root, new Vector2(LeftEdgeX + SearchWidth / 2f, SearchY), SearchWidth, placeholderText: "Search by viewer or redeem...");
            m_searchField.onValueChanged.AddListener(OnSearchChanged);

            BuildColumnHeaders();

            m_listContainer = ScrollableList.CreateStretched(
                m_root, "History",
                offsetMin: new Vector2(ContentMargin, ListOffsetBottom),
                offsetMax: new Vector2(-ContentMargin, -ListOffsetTop),
                autoHideScrollbar: true);

            BuildPaginationRow();
            BuildActionRow();

            m_root.SetActive(false);
            return m_root;
        }

        public bool IsVisible => m_root != null && m_root.activeSelf;

        /// <summary>Shows this section, resetting to page 1 and refreshing its contents.</summary>
        public void Show()
        {
            if (m_root == null)
                return;

            m_currentPage = 0;
            m_root.SetActive(true);
            RefreshPage();
        }

        private void GoBack()
        {
            m_root.SetActive(false);
            OnBack?.Invoke();
        }

        /// <summary>
        /// Hides this section without firing <see cref="OnBack"/> - used when the owning tab needs
        /// to force back to its overview outside the normal Back-button flow (see
        /// <see cref="Tabs.HomeTab.Refresh"/>).
        /// </summary>
        public void Hide()
        {
            if (m_root != null)
                m_root.SetActive(false);
        }

        // ── column headers / rows ────────────────────────────────────────────

        private void BuildColumnHeaders()
        {
            CreateHeader("Viewer", ColViewerX, ColViewerW, TextAnchor.MiddleLeft);
            CreateHeader("Redeem", ColRedeemX, ColRedeemW, TextAnchor.MiddleLeft);
            CreateHeader("Cost", ColCostX, ColCostW, TextAnchor.MiddleLeft);
            CreateHeader("Actions", ColActionsX, ColActionsW, TextAnchor.MiddleRight);
        }

        // Legacy uGUI Text doesn't pixel-snap, and the column centers/widths are fractions of
        // AvailableWidth - so a text box's edges can land on half pixels and blur every glyph (same
        // issue as GuiHelper.CreateCard). The position is rounded to a whole pixel at the call
        // site; rounding the width to an even number keeps center +/- width/2 on whole pixels too.
        private static float EvenWidth(float width) => Mathf.Round(width / 2f) * 2f;

        private void CreateHeader(string text, float x, float width, TextAnchor alignment)
        {
            Text header = GUIManager.Instance.CreateText(
                text: text,
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(Mathf.Round(x), Mathf.Round(ColumnHeaderY)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: EvenWidth(width),
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            header.alignment = alignment;
        }

        private void RefreshPage()
        {
            GuiHelper.ClearContainer(m_listContainer);

            TwitchCustomRewards customRewards = GetCustomRewards();
            List<CustomRewardEvent> filtered = GetFilteredHistory(customRewards);

            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)filtered.Count / PageSize));
            m_currentPage = Mathf.Clamp(m_currentPage, 0, totalPages - 1);

            bool bulkRunning = customRewards != null && customRewards.m_bulkResolveHelper.IsRunning;

            if (m_pageLabel != null)
                m_pageLabel.text = bulkRunning ? BuildBulkProgressText(customRewards) : $"Page {m_currentPage + 1} / {totalPages}";

            if (m_prevButton != null)
                m_prevButton.interactable = !bulkRunning && m_currentPage > 0;

            if (m_nextButton != null)
                m_nextButton.interactable = !bulkRunning && m_currentPage < totalPages - 1;

            if (m_completeAllButton != null)
                m_completeAllButton.interactable = !bulkRunning;

            if (m_refundAllButton != null)
                m_refundAllButton.interactable = !bulkRunning;

            if (m_pendingCountText != null)
            {
                int pending = CountUnresolved(filtered);
                m_pendingCountText.text = $"{pending} pending";
                m_pendingCountText.color = GUIManager.Instance.ValheimOrange;
                m_pendingCountText.gameObject.SetActive(pending > 0);
            }

            if (filtered.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    text: "No redeems yet this session!",
                    parent: m_listContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -(ListRow.ListTopPadding + 10f)),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 15,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: AvailableWidth,
                    height: 30f,
                    addContentSizeFitter: false
                );
                return;
            }

            float yOffset = -(ListRow.ListTopPadding + ListRow.ItemHeight / 2f);
            int rowIndex = 0;

            foreach (CustomRewardEvent entry in GetVisiblePageHistory(filtered, m_currentPage))
            {
                BuildRow(entry, yOffset, rowIndex);
                yOffset -= ListRow.ItemHeight + ListRow.ItemSpacing;
                rowIndex++;
            }

            RectTransform contentRt = m_listContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ListRow.ItemHeight / 2f);
        }

        private void BuildRow(CustomRewardEvent entry, float yOffset, int rowIndex)
        {
            bool isTestRedeem = entry.RedemptionId == Guid.Empty.ToString();
            bool isResolved = entry.Status == CustomRewardRedemptionState.Fulfilled
                || entry.Status == CustomRewardRedemptionState.Canceled;

            Color textColor = entry.Status == CustomRewardRedemptionState.Fulfilled ? CompletedColor
                : entry.Status == CustomRewardRedemptionState.Canceled ? RefundedColor
                : GUIManager.Instance.ValheimBeige;

            var (row, background) = ListRow.Create(m_listContainer, "HistoryRow", yOffset);
            background.color = ListRow.ZebraColor(rowIndex);

            CreateRowText(row, entry.RedeemerName, ColViewerX, ColViewerW, textColor, bold: true);
            CreateRowText(row, entry.CustomRewardTitle, ColRedeemX, ColRedeemW, textColor, bold: false);
            CreateRowText(row, $"{entry.CustomRewardCost} pts", ColCostX, ColCostW, textColor, bold: false);

            // Checked before isTestRedeem: a resolved test redeem (e.g. RedeemsTab's "Test" button,
            // or any redeem auto-resolved while ProfileSettingsHelper.Current.autoResolveRedeems is
            // on - see TwitchCustomRewards.HandleRedeem, which flips Status regardless of test-ness)
            // should still show "Completed"/"Refunded" - only the buttons below are test-redeem-only
            // skipped, since there's no real Twitch API call to make for a fake redemption.
            if (isResolved)
            {
                string statusLabel = entry.Status == CustomRewardRedemptionState.Fulfilled ? "Completed" : "Refunded";
                CreateRowText(row, statusLabel, ColActionsX, ColActionsW, textColor, bold: true, alignment: TextAnchor.MiddleRight);
                return;
            }

            if (isTestRedeem)
                return;

            CustomRewardEvent captured = entry;
            ListRow.CreateActionButton(row, "Complete", BtnCompleteX, ActionBtnWidth, ListRow.ItemHeight, CompleteBtnColor, () => OnComplete(captured)).SetActive(true);
            ListRow.CreateActionButton(row, "Refund", BtnRefundX, ActionBtnWidth, ListRow.ItemHeight, RefundBtnColor, () => OnRefund(captured)).SetActive(true);
        }

        private static void CreateRowText(GameObject row, string text, float x, float width, Color color, bool bold, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(Mathf.Round(x), 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: EvenWidth(width),
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = alignment;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        }

        // ── pagination / bulk resolve row ────────────────────────────────────

        private void BuildPaginationRow()
        {
            GameObject prevBtnObj = GuiHelper.CreateButton(
                text: "<",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-100f, PaginationY),
                width: 50f,
                height: 32f
            );
            prevBtnObj.SetActive(true);
            m_prevButton = prevBtnObj.GetComponent<Button>();
            m_prevButton.onClick.AddListener(PrevPage);

            m_pageLabel = GUIManager.Instance.CreateText(
                text: "Page 1 / 1",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, PaginationY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 180f,
                height: 32f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_pageLabel.alignment = TextAnchor.MiddleCenter;

            GameObject nextBtnObj = GuiHelper.CreateButton(
                text: ">",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(100f, PaginationY),
                width: 50f,
                height: 32f
            );
            nextBtnObj.SetActive(true);
            m_nextButton = nextBtnObj.GetComponent<Button>();
            m_nextButton.onClick.AddListener(NextPage);
        }

        private void BuildActionRow()
        {
            const float btnWidth = 220f;
            const float gap = 14f;

            GameObject completeAllObj = GuiHelper.CreateButton(
                text: "Complete All",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-(btnWidth + gap), ActionRowY),
                width: btnWidth,
                height: ActionBtnHeight
            );
            completeAllObj.SetActive(true);
            m_completeAllButton = completeAllObj.GetComponent<Button>();
            m_completeAllButton.onClick.AddListener(() => StartBulkResolve(CustomRewardRedemptionState.Fulfilled));

            GameObject refundAllObj = GuiHelper.CreateButton(
                text: "Refund All",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, ActionRowY),
                width: btnWidth,
                height: ActionBtnHeight
            );
            refundAllObj.SetActive(true);
            m_refundAllButton = refundAllObj.GetComponent<Button>();
            m_refundAllButton.onClick.AddListener(() => StartBulkResolve(CustomRewardRedemptionState.Canceled));

            GameObject backObj = GuiHelper.CreateButton(
                text: "Back",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(btnWidth + gap, ActionRowY),
                width: btnWidth,
                height: ActionBtnHeight
            );
            backObj.SetActive(true);
            backObj.GetComponent<Button>().onClick.AddListener(GoBack);
        }

        private void StartBulkResolve(CustomRewardRedemptionState targetState)
        {
            TwitchCustomRewards customRewards = GetCustomRewards();
            if (customRewards == null)
                return;

            List<CustomRewardEvent> eligible = GetFilteredHistory(customRewards).FindAll(BulkRedeemResolveHelper.IsEligible);

            customRewards.m_bulkResolveHelper.EnqueueAll(
                host: customRewards,
                entries: eligible,
                targetState: targetState,
                onProgress: OnBulkResolveProgress,
                onFinished: OnBulkResolveFinished);

            // Immediately reflects the disabled buttons / initial progress text, even before the
            // first WaitForSecondsRealtime tick fires - see BulkRedeemResolveHelper.
            RefreshPage();
        }

        private void OnBulkResolveProgress()
        {
            // Cheap label-only update, matching GUI_OLD/WizshBoneRedeemHistoryGUI.cs - do NOT call
            // the full RefreshPage() here, it tears down and rebuilds every row several times a
            // second for no reason. Per-row state catches up once, in OnBulkResolveFinished.
            if (!IsVisible || m_pageLabel == null)
                return;

            m_pageLabel.text = BuildBulkProgressText(GetCustomRewards());
        }

        private void OnBulkResolveFinished()
        {
            if (!IsVisible)
                return;

            RefreshPage();
        }

        private static string BuildBulkProgressText(TwitchCustomRewards customRewards)
        {
            if (customRewards == null)
                return "";

            BulkRedeemResolveHelper resolver = customRewards.m_bulkResolveHelper;
            string verb = resolver.TargetState == CustomRewardRedemptionState.Fulfilled ? "Completing" : "Refunding";
            return $"{verb} {resolver.Completed}/{resolver.Total}...";
        }

        // ── row action handlers ──────────────────────────────────────────────

        private void OnComplete(CustomRewardEvent entry)
        {
            GameTask task = Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Fulfilled);
            task.GetAwaiter().OnCompleted(() => OnResolveCompleted(entry, CustomRewardRedemptionState.Fulfilled));
        }

        private void OnRefund(CustomRewardEvent entry)
        {
            GameTask task = Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Canceled);
            task.GetAwaiter().OnCompleted(() => OnResolveCompleted(entry, CustomRewardRedemptionState.Canceled));
        }

        private void OnResolveCompleted(CustomRewardEvent entry, CustomRewardRedemptionState state)
        {
            entry.Status = state;

            if (IsVisible)
                RefreshPage();
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            m_currentPage = 0;
            RefreshPage();
        }

        private void PrevPage()
        {
            m_currentPage--;
            RefreshPage();
        }

        private void NextPage()
        {
            m_currentPage++;
            RefreshPage();
        }

        // ── data ──────────────────────────────────────────────────────────────

        private static TwitchCustomRewards GetCustomRewards() =>
            Game.instance != null ? Game.instance.gameObject.GetComponent<TwitchCustomRewards>() : null;

        private List<CustomRewardEvent> GetFilteredHistory(TwitchCustomRewards customRewards)
        {
            if (customRewards == null)
                return new List<CustomRewardEvent>();

            if (string.IsNullOrEmpty(m_searchText))
                return new List<CustomRewardEvent>(customRewards.m_redeemHistory);

            return customRewards.m_redeemHistory.FindAll(entry =>
                entry.RedeemerName.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.CustomRewardTitle.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// The subset of <paramref name="filtered"/> actually rendered on <paramref name="page"/>,
        /// newest-first - mirrors GUI_OLD/WizshBoneRedeemHistoryGUI.cs's GetVisiblePageHistory.
        /// "Complete All"/"Refund All" operate on the full filtered list instead (see
        /// <see cref="StartBulkResolve"/>), not just this page's slice.
        /// </summary>
        private static List<CustomRewardEvent> GetVisiblePageHistory(List<CustomRewardEvent> filtered, int page)
        {
            List<CustomRewardEvent> visible = new List<CustomRewardEvent>();

            int startIndex = filtered.Count - 1 - page * PageSize;
            for (int i = startIndex; i >= 0 && i > startIndex - PageSize; i--)
                visible.Add(filtered[i]);

            return visible;
        }

        private static int CountUnresolved(List<CustomRewardEvent> entries) =>
            entries.FindAll(e => e.Status == CustomRewardRedemptionState.Unfulfilled).Count;
    }
}
