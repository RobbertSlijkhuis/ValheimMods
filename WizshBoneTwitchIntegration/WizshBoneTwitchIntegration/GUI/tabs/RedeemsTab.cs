using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Commands;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Redeems tab content - list/toggle/test/copy/delete plus a profile quick-switcher (built
    /// directly against RedesignUI.dc.html's structure, mirroring <see cref="ProfilesTab"/>'s
    /// toolbar/column-header/row patterns), and the create/edit wizard
    /// (<see cref="RedeemWizard"/>) swapped in over the list the same way
    /// GUI_OLD/tabs/RedeemsTab.cs swaps its list/create views. Backend calls
    /// (<see cref="RedeemManager"/>/<see cref="ProfileManager"/>) are reused unchanged.
    /// </summary>
    internal class RedeemsTab : IShellTabView
    {
        private GameObject m_root;
        private GameObject m_listRoot;
        private GameObject m_redeemListContainer;
        private string m_searchText = "";

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();
        private readonly CopyRedeemDialog m_copyRedeemDialog = new CopyRedeemDialog();
        private readonly RedeemWizard m_redeemWizard = new RedeemWizard();

        private SearchableDropdown m_profileDropdown;
        private Text m_titleText;
        private Button m_newRedeemBtn;

        private readonly ColumnSortState m_sortState = new ColumnSortState("title");
        private readonly List<(string Text, string SortKey, Text Label)> m_sortableHeaders = new List<(string, string, Text)>();

        /// <summary>
        /// Wired up by <see cref="WizshBoneShellGUI"/> right after construction (same pattern as
        /// <see cref="HomeTab.OnCreateRedeemRequested"/>) so the Test action can close the whole shell
        /// panel before running the redeem, matching GUI_OLD's <c>OnTestRedeem</c>.
        /// </summary>
        public Action OnCloseRequested;

        /// <summary>
        /// Wired up by <see cref="WizshBoneShellGUI"/> right after construction (same pattern as
        /// <see cref="OnCloseRequested"/>) so the disable-redeem confirm dialog's "Open history"
        /// cancel option can open <see cref="RedeemHistorySection"/> without this class needing a
        /// reference to the shell itself.
        /// </summary>
        public Action OnOpenHistoryRequested;

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;   // 980
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;                   // -460
        private const float RightEdgeX = (ContentWidth / 2f) - ContentMargin - ScrollableList.ScrollbarWidth; // 444

        private const float TitleY   = -30f;
        private const float TitleWidth = 300f;
        private const float DescriptionY = -50f;
        private const float ToolbarY = -99f;
        private const float ColumnHeaderY = -136f;
        private const float ListTopInset = 155f;

        private const float SearchWidth = 300f;
        private const float ToolbarButtonSpacing = 10f;
        private const float NewRedeemBtnWidth = 150f;
        private const float HistoryBtnWidth = 160f;
        private const float ProfileDropdownWidth = 180f; // narrowed from 220 to make room for the 160-wide history button
        private const float ProfileLabelWidth = 60f;

        // Title was far wider than any real title needs (leaving a large dead gap before Type),
        // while the action-button cluster sat flush against Cost with no breathing room at all
        // (see ActionsClusterWidth/ActionsClusterLeft below) - narrowed Title and handed the
        // freed width to Type/Cost (now comfortably wide for their new left-aligned text, see
        // BuildColumnHeaders/BuildRow) and to the gap in front of Actions.
        private const float ColOnW      = 60f;
        private const float ColTitleW   = 280f;
        private const float ColTypeW    = 160f;
        private const float ColCostW    = 120f;
        private const float ColActionsW = 324f;

        // On is the only column flush with the list's true left edge - inset just its content,
        // not the column boundary itself, so ColTitleX/ColTypeX/ColCostX/ColActionsX (all derived
        // from ColOnW) don't shift.
        private const float ColOnTextW  = ColOnW - ListRow.LeftPadding;
        private const float ColOnX      = LeftEdgeX + ListRow.LeftPadding + ColOnTextW / 2f;

        // Title also gets a little breathing room from the On toggle just left of it, plus a
        // little more before Type starts on its right - same shrink-and-shift treatment as On
        // above (left edge stays at ColOnW + LeftPadding, only the width/center shift), so
        // ColTypeX/ColCostX/ColActionsX (derived from the full ColTitleW) don't move.
        private const float ColTitleRightGap = 10f;
        private const float ColTitleTextW = ColTitleW - ListRow.LeftPadding - ColTitleRightGap;
        private const float ColTitleX   = LeftEdgeX + ColOnW + ListRow.LeftPadding + ColTitleTextW / 2f;
        private const float ColTypeX    = LeftEdgeX + ColOnW + ColTitleW + ColTypeW / 2f;
        private const float ColCostX    = LeftEdgeX + ColOnW + ColTitleW + ColTypeW + ColCostW / 2f;
        private const float ColActionsX = RightEdgeX - ColActionsW / 2f;

        private const float ActionBtnWidth = 70f;
        private const float DeleteBtnWidth = 40f;

        // The button cluster is anchored to the list's true right edge (RightEdgeX), not to
        // ColActionsW's own left edge - so Delete's right edge always lands flush at RightEdgeX
        // (matching the right-aligned Actions header above it) regardless of how wide the mostly-
        // empty ColActionsW column itself is, and the cluster no longer eats into Cost's column
        // the way "RightEdgeX - ColActionsW" used to.
        private const float ActionsClusterWidth = ActionBtnWidth * 3f + DeleteBtnWidth + ListRow.ItemSpacing * 3f;
        private const float ActionsClusterLeft  = RightEdgeX - ActionsClusterWidth;

        private const float BtnTestX   = ActionsClusterLeft + ActionBtnWidth / 2f;
        private const float BtnEditX   = BtnTestX + ActionBtnWidth / 2f + ListRow.ItemSpacing + ActionBtnWidth / 2f;
        private const float BtnCopyX   = BtnEditX + ActionBtnWidth / 2f + ListRow.ItemSpacing + ActionBtnWidth / 2f;
        private const float BtnDeleteX = BtnCopyX + ActionBtnWidth / 2f + ListRow.ItemSpacing + DeleteBtnWidth / 2f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "RedeemsTab");

            m_confirmDialog.Init();
            m_copyRedeemDialog.Init();

            // UIContainer.Create defaults to inactive - fine for every other tab's own m_root,
            // since WizshBoneShellGUI.SelectTab explicitly activates whichever tab root is
            // selected. m_listRoot is a second, nested container inside that root (needed so this
            // tab can swap it out for the wizard) that nothing outside RedeemsTab manages, so it
            // needs its own explicit activation here - without it the tab opens with both
            // m_listRoot and the (still-closed) wizard root inactive, i.e. showing nothing.
            m_listRoot = UIContainer.Create(m_root, "RedeemListView", startActive: true);

            m_titleText = GuiHelper.CreateTitle("Redeems", m_listRoot, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "These are your channel point rewards. Twitch allows a maximum of 50 enabled at once - keep an eye on how many you have active.",
                m_listRoot, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            BuildToolbar();
            BuildColumnHeaders();

            m_redeemListContainer = ScrollableList.CreateStretched(
                m_listRoot, "RedeemList",
                offsetMin: new Vector2(ContentMargin, ContentMargin),
                offsetMax: new Vector2(-ContentMargin, -ListTopInset),
                autoHideScrollbar: true);

            m_redeemWizard.Create(m_root);
            m_redeemWizard.OnFinished = OnWizardFinished;

            RefreshList();

            return m_root;
        }

        /// <summary>
        /// Re-populates the row list and profile dropdown. Call whenever the shell re-shows this
        /// tab (redeems/profiles on disk may have changed via sync, the Profiles tab, or another
        /// session). Doesn't force the wizard closed if it's open - matches GUI_OLD's Refresh(),
        /// which only ever touched the list view regardless of which sub-view was showing.
        /// </summary>
        public void Refresh()
        {
            m_profileDropdown?.SetOptions(ProfileManager.GetProfiles(), ProfileManager.ActiveProfile);
            RefreshList();
        }

        /// <summary>
        /// Called when the active profile's redeems were replaced (sync, switch, import or reload).
        /// An open wizard was working on the old data - saving it would fail, or add the redeem to
        /// the wrong profile - and for a profile that just became synced would still allow saving
        /// over it, so it is closed without saving before the list refreshes.
        /// </summary>
        public void OnActiveProfileReplaced(ProfileManager.ActiveProfileChange reason)
        {
            bool wizardWasOpen = m_redeemWizard.IsOpen;
            m_redeemWizard.ForceClose();   // its OnFinished refreshes the list

            if (wizardWasOpen)
                ToastNotifications.Show($"{DescribeReplacement(reason)} - the editor was closed without saving.", ToastType.Warning);
            else
                RefreshList();
        }

        private static string DescribeReplacement(ProfileManager.ActiveProfileChange reason)
        {
            switch (reason)
            {
                case ProfileManager.ActiveProfileChange.Sync:   return "This profile was just updated by a sync";
                case ProfileManager.ActiveProfileChange.Switch: return "The active profile was switched";
                case ProfileManager.ActiveProfileChange.Import: return "This profile was just replaced by an import";
                default:                                        return "This profile was reloaded from disk";
            }
        }

        // ── toolbar / column headers ────────────────────────────────────────

        private void BuildToolbar()
        {
            float searchX = LeftEdgeX + SearchWidth / 2f;
            InputField searchField = GuiFieldBuilder.CreateInputField(m_listRoot, new Vector2(searchX, ToolbarY), SearchWidth, placeholderText: "Search redeems by title or type...");
            searchField.onValueChanged.AddListener(OnSearchChanged);

            float cursor = LeftEdgeX + SearchWidth;
            m_newRedeemBtn = CreateToolbarButton("+ New redeem", NewRedeemBtnWidth, cursor, OpenCreate);
            cursor += ToolbarButtonSpacing + NewRedeemBtnWidth;
            CreateToolbarButton("View history",HistoryBtnWidth, cursor, () => OnOpenHistoryRequested?.Invoke());

            float dropdownX = RightEdgeX - ProfileDropdownWidth / 2f;
            float labelX = dropdownX - ProfileDropdownWidth / 2f - 8f - ProfileLabelWidth / 2f;

            Text profileLabel = GUIManager.Instance.CreateText(
                text: "Profile:",
                parent: m_listRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(labelX, ToolbarY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: ProfileLabelWidth,
                height: 36f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            profileLabel.alignment = TextAnchor.MiddleRight;

            m_profileDropdown = new SearchableDropdown();
            m_profileDropdown.Build(m_listRoot, new Vector2(dropdownX, ToolbarY), ProfileDropdownWidth, GuiFieldBuilder.FieldHeight, ProfileManager.GetProfiles(), ProfileManager.ActiveProfile, showSearch: false);
            m_profileDropdown.OnValueChanged += OnProfileSelected;
        }

        private Button CreateToolbarButton(string text, float width, float cursorX, UnityEngine.Events.UnityAction onClick)
        {
            float centerX = cursorX + ToolbarButtonSpacing + width / 2f;

            GameObject btnObj = GuiHelper.CreateButton(
                text: text,
                parent: m_listRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(centerX, ToolbarY),
                width: width,
                height: 36f
            );
            btnObj.SetActive(true);
            Button button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            return button;
        }

        private void BuildColumnHeaders()
        {
            CreateSortableHeader("On", ColOnX, ColOnTextW, TextAnchor.MiddleLeft, "enabled");
            CreateSortableHeader("Title", ColTitleX, ColTitleTextW, TextAnchor.MiddleLeft, "title");
            CreateSortableHeader("Type", ColTypeX, ColTypeW, TextAnchor.MiddleLeft, "type");
            CreateSortableHeader("Cost", ColCostX, ColCostW, TextAnchor.MiddleLeft, "points");
            CreateSortableHeader("Actions", ColActionsX, ColActionsW, TextAnchor.MiddleRight, null);
        }

        private void CreateSortableHeader(string text, float x, float width, TextAnchor alignment, string sortKey)
        {
            Text header = GUIManager.Instance.CreateText(
                text: text,
                parent: m_listRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(x, ColumnHeaderY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            header.alignment = alignment;

            if (sortKey == null)
                return;

            Button headerBtn = header.gameObject.AddComponent<Button>();
            headerBtn.targetGraphic = header;
            headerBtn.transition = Selectable.Transition.None;
            headerBtn.onClick.AddListener(() => OnHeaderClicked(sortKey));

            m_sortableHeaders.Add((text, sortKey, header));
        }

        private void RefreshHeaderIndicators()
        {
            foreach (var (text, sortKey, label) in m_sortableHeaders)
                label.text = sortKey == m_sortState.Key ? $"{text} {(m_sortState.Ascending ? "▲" : "▼")}" : text;
        }

        private void OnHeaderClicked(string sortKey)
        {
            m_sortState.ToggleOrSet(sortKey);
            RefreshHeaderIndicators();
            RefreshList();
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            RefreshList();
        }

        private void OnProfileSelected(string selected)
        {
            if (selected == ProfileManager.ActiveProfile)
                return;

            // With redeems live this asks first (see LiveRedeemsPrompt). Whenever the switch
            // doesn't happen - cancelled, Open history, or a failed switch (defensive only, the
            // dropdown only ever lists ProfileManager.GetProfiles()) - the dropdown has already
            // displayed the picked name, so snap it back to the profile that is actually active.
            LiveRedeemsPrompt.Request(
                m_confirmDialog,
                title: "Switch Profile",
                actionDescription: "Switching profiles now can break them",
                perform: () => ProfileManager.SelectProfile(selected) ? null : $"Could not switch to '{selected}'.",
                onOpenHistory: () => OnOpenHistoryRequested?.Invoke(),
                onDone: RefreshList,
                onNotDone: () => m_profileDropdown.SetOptions(ProfileManager.GetProfiles(), ProfileManager.ActiveProfile),
                liveSuccessToast: $"Switched to '{selected}'. Redeems turned off.");
        }

        // ── row list ─────────────────────────────────────────────────────────

        private List<RedeemData> GetSortedRedeems()
        {
            var redeems = new List<RedeemData>(RedeemHelper.redeems);

            Comparison<RedeemData> comparison;
            switch (m_sortState.Key)
            {
                case "enabled":
                    comparison = (a, b) => a.enabled.CompareTo(b.enabled);
                    break;
                case "type":
                    comparison = (a, b) => string.Compare(RedeemEffectCatalog.LabelFor(a.type), RedeemEffectCatalog.LabelFor(b.type), StringComparison.OrdinalIgnoreCase);
                    break;
                case "points":
                    comparison = (a, b) => a.points.CompareTo(b.points);
                    break;
                default:
                    comparison = (a, b) => string.Compare(a.title, b.title, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            redeems.Sort(comparison);
            if (!m_sortState.Ascending)
                redeems.Reverse();

            return redeems;
        }

        private void RefreshList()
        {
            GuiHelper.ClearContainer(m_redeemListContainer);
            RefreshHeaderIndicators();

            // Total redeems in the active profile - deliberately not the search-filtered count.
            // CreateTitle upper-cases its text, so match that here.
            m_titleText.text = $"REDEEMS ({RedeemHelper.redeems.Count})";

            // A synced profile is read-only here: no new redeems, no toggling/deleting, and Edit
            // becomes a view-only "Show" (see BuildRow).
            bool isSynced = IsActiveProfileSynced();
            m_newRedeemBtn.interactable = !isSynced;

            float yOffset = -(ListRow.ListTopPadding + ListRow.ItemHeight / 2f);
            int rowIndex = 0;

            foreach (RedeemData redeem in GetSortedRedeems())
            {
                // Search matches the Twitch-facing (prefixed) title shown in the list, not the
                // raw stored RedeemData.title, so typing the prefix (or the plain name) both work.
                if (!string.IsNullOrEmpty(m_searchText)
                    && RedeemManager.GetFullTitle(redeem).IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) < 0
                    && redeem.type.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                BuildRow(redeem, yOffset, rowIndex, isSynced);
                yOffset -= ListRow.ItemHeight + ListRow.ItemSpacing;
                rowIndex++;
            }

            RectTransform contentRt = m_redeemListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ListRow.ItemHeight / 2f);
        }

        private static bool IsActiveProfileSynced()
        {
            return ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile);
        }

        /// <summary>
        /// Guards every action that would change a synced profile's redeems. The buttons are
        /// already disabled/hidden for synced profiles (see <see cref="BuildRow"/>); this is the
        /// backstop for entry points that aren't a button on this tab (Home's "Create a new redeem").
        /// </summary>
        private static bool BlockedBySync()
        {
            if (!IsActiveProfileSynced())
                return false;

            ToastNotifications.Show("This profile is synced and read-only - only the owner can change it.", ToastType.Warning);
            return true;
        }

        private void BuildRow(RedeemData redeem, float yOffset, int rowIndex, bool isSynced)
        {
            RedeemData captured = redeem;

            var (row, rowBackground) = ListRow.Create(m_redeemListContainer, "RedeemRow", yOffset);
            var revealOnHover = new List<GameObject>();

            Color toggleColor = captured.enabled ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
            GameObject toggleBtn = ListRow.CreateActionButton(row, captured.enabled ? "On" : "Off", ColOnX - 5f, ColOnTextW - 10f, ListRow.ItemHeight, toggleColor, () => OnToggleRedeem(captured));
            toggleBtn.SetActive(true);
            // Still shows On/Off on a synced profile, just can't be flipped.
            toggleBtn.GetComponent<Button>().interactable = !isSynced;

            // Shows the Twitch-facing (prefixed) title - RedeemData.title itself is stored
            // prefix-free, see RedeemManager.GetFullTitle.
            Text titleText = GUIManager.Instance.CreateText(
                text: RedeemManager.GetFullTitle(captured),
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColTitleX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColTitleTextW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            titleText.alignment = TextAnchor.MiddleLeft;

            Text typeText = GUIManager.Instance.CreateText(
                text: RedeemEffectCatalog.LabelFor(captured.type),
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColTypeX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: new Color(0.56f, 0.7f, 0.79f, 1f),
                outline: true,
                outlineColor: Color.black,
                width: ColTypeW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            typeText.alignment = TextAnchor.MiddleLeft;

            string typeDescription = RedeemEffectCatalog.DescriptionFor(captured.type);
            if (!string.IsNullOrEmpty(typeDescription))
                typeText.gameObject.AddComponent<TooltipTrigger>().Init("Effect type", typeDescription);

            Text costText = GUIManager.Instance.CreateText(
                text: $"{captured.points} pts",
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColCostX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColCostW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            costText.alignment = TextAnchor.MiddleLeft;

            GameObject testBtn = ListRow.CreateActionButton(row, "Test", BtnTestX, ActionBtnWidth, ListRow.ItemHeight, Color.yellow, () => OnTestRedeem(captured));
            testBtn.AddComponent<TooltipTrigger>().Init("Test redeem", "Executes the redeem so you can see how it works in-game.");
            revealOnHover.Add(testBtn);

            // A synced profile can be looked at but not changed: "Show" opens the wizard view-only.
            GameObject editBtn = isSynced
                ? ListRow.CreateActionButton(row, "Show", BtnEditX, ActionBtnWidth, ListRow.ItemHeight, GUIManager.Instance.ValheimBeige, () => OpenView(captured))
                : ListRow.CreateActionButton(row, "Edit", BtnEditX, ActionBtnWidth, ListRow.ItemHeight, Color.cyan, () => OpenEdit(captured));
            revealOnHover.Add(editBtn);

            GameObject copyBtn = ListRow.CreateActionButton(row, "Copy", BtnCopyX, ActionBtnWidth, ListRow.ItemHeight, GUIManager.Instance.ValheimBeige, () => OnCopyRedeem(captured));
            revealOnHover.Add(copyBtn);

            // No Delete on a synced profile (hidden rather than disabled, like the old GUI).
            if (!isSynced)
            {
                GameObject deleteBtn = ListRow.CreateActionButton(row, "X", BtnDeleteX, DeleteBtnWidth, ListRow.ItemHeight, Color.red, () => OnDeleteRedeem(captured));
                deleteBtn.SetActive(true);
            }

            ListRow.AttachHoverReveal(row, rowBackground, revealOnHover, ListRow.ZebraColor(rowIndex));
        }

        // ── wizard hookup ────────────────────────────────────────────────────

        /// <summary>
        /// Called by <see cref="WizshBoneShellGUI"/> when Home's "Create a new redeem" button
        /// navigates here (same pattern as <see cref="OnCloseRequested"/>), in addition to this
        /// tab's own toolbar "+ New" button.
        /// </summary>
        public void OpenCreate()
        {
            if (BlockedBySync())
                return;

            m_listRoot.SetActive(false);
            m_redeemWizard.OpenCreate();
        }

        private void OpenEdit(RedeemData redeem)
        {
            if (BlockedBySync())
                return;

            m_listRoot.SetActive(false);
            m_redeemWizard.OpenEdit(redeem);
        }

        private void OpenView(RedeemData redeem)
        {
            m_listRoot.SetActive(false);
            m_redeemWizard.OpenView(redeem);
        }

        private void OnWizardFinished(string toastMessage)
        {
            m_listRoot.SetActive(true);
            RefreshList();

            if (toastMessage != null)
                ToastNotifications.Show(toastMessage, ToastType.Success);
        }

        // ── row action handlers ─────────────────────────────────────────────

        private void OnToggleRedeem(RedeemData redeem)
        {
            if (BlockedBySync())
                return;

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            string fullTitle = RedeemManager.GetFullTitle(redeem);

            // HasUnresolvedRedeemsFor/history entries key off the Twitch-facing (prefixed) title,
            // not RedeemData.title - see RedeemManager.GetFullTitle.
            bool willDisable = redeem.enabled;
            if (willDisable && customRewards.HasUnresolvedRedeemsFor(fullTitle))
            {
                m_confirmDialog.Show(
                    title:       "Disable Redeem",
                    description: $"Auto-resolve is off. Pending '{fullTitle}' redeems won't be refunded automatically. Are you sure?",
                    onConfirm:   () =>
                    {
                        RedeemManager.SetEnabled(redeem, false, out string disableError);
                        ToastNotifications.Show(disableError ?? $"'{fullTitle}' disabled.", disableError != null ? ToastType.Error : ToastType.Success);
                        RefreshList();
                    },
                    confirmText: "Disable",
                    cancelText:  "Open history",
                    onCancel:    () => OnOpenHistoryRequested?.Invoke());
                return;
            }

            RedeemManager.SetEnabled(redeem, !redeem.enabled, out string error);
            ToastNotifications.Show(error ?? $"'{fullTitle}' {(redeem.enabled ? "enabled" : "disabled")}.", error != null ? ToastType.Error : ToastType.Success);
            RefreshList();
        }

        private void OnTestRedeem(RedeemData redeem)
        {
            // A background row's Test button is still clickable while these dialogs are open (no
            // full-screen blocker) - close everything first, matching GUI_OLD's OnTestRedeem, so
            // nothing is left orphaned on screen holding an outstanding InputBlockGate.Push().
            m_confirmDialog.Hide();
            m_copyRedeemDialog.Hide();

            OnCloseRequested?.Invoke();

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            CustomRewardEvent rewardEvent = new CustomRewardEvent
            {
                RedeemerName      = "WizshBone",
                RedeemedAt        = DateTime.Now.ToShortDateString(),
                // Must match what TwitchCustomRewards.HandleRedeem looks the redeem up by - the
                // Twitch-facing (prefixed) title, not the raw RedeemData.title.
                CustomRewardTitle = RedeemManager.GetFullTitle(redeem),
                CustomRewardCost  = redeem.points,
                Status            = CustomRewardRedemptionState.Unfulfilled
            };

            FakeRedeemerCommand.Apply(rewardEvent); // debug: only does anything after WBTIFakeRedeemer

            WizshBoneTwitchIntegration.useRedeemCommand = true;
            customRewards.HandleRedeem(rewardEvent);
        }

        private void OnCopyRedeem(RedeemData redeem)
        {
            m_copyRedeemDialog.Show(
                redeemTitle:    RedeemManager.GetFullTitle(redeem),
                profiles:       ProfileManager.GetProfiles(),
                defaultProfile: ProfileManager.ActiveProfile,
                // Seeds the new redeem's raw (prefix-free) title, so keep this based on the raw
                // RedeemData.title rather than the displayed full title.
                suggestedName:  $"{redeem.title} - copy",
                onConfirm:      (targetProfile, newTitle) => HandleCopyRedeemConfirm(redeem, targetProfile, newTitle));
        }

        private string HandleCopyRedeemConfirm(RedeemData redeem, string targetProfile, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return "Please enter a name.";

            if (ProfileManager.IsSyncedProfile(targetProfile))
                return $"Cannot copy into '{targetProfile}' - it is a synced (read-only) profile.";

            string sourceFullTitle = RedeemManager.GetFullTitle(redeem);

            if (targetProfile == ProfileManager.ActiveProfile)
            {
                bool copiedWithin = RedeemManager.CopyRedeemWithinProfile(redeem, newTitle, out string withinError);
                if (!copiedWithin)
                    return withinError;

                ToastNotifications.Show($"Copied '{sourceFullTitle}' to '{newTitle}'.", ToastType.Success);
                RefreshList();
                return null;
            }

            bool copied = ProfileManager.CopyRedeemToOtherProfile(redeem, targetProfile, newTitle, out string error);
            if (copied)
                ToastNotifications.Show($"Copied '{sourceFullTitle}' to '{targetProfile}' as '{newTitle}'.", ToastType.Success);

            return copied ? null : error;
        }

        private void OnDeleteRedeem(RedeemData redeem)
        {
            if (BlockedBySync())
                return;

            string fullTitle = RedeemManager.GetFullTitle(redeem);

            m_confirmDialog.Show(
                title:       "Delete Redeem",
                description: $"Are you sure you want to delete '{fullTitle}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    bool deleted = RedeemManager.DeleteRedeem(redeem, out string error);
                    ToastNotifications.Show(deleted ? $"'{fullTitle}' removed." : error, deleted ? ToastType.Success : ToastType.Error);
                    RefreshList();
                },
                confirmText: "Delete");
        }
    }
}
