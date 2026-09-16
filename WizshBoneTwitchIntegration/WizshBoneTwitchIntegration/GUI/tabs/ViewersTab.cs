using System;
using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Viewers tab content - list/search/sort/create/edit/delete, built directly against
    /// RedesignUI.dc.html's structure (search+New toolbar, sortable Name/Color/Effects columns,
    /// hover-revealed Edit + always-visible Delete) the same way <see cref="ProfilesTab"/> was.
    /// Row shell/hover/action-button plumbing is shared via <see cref="ListRow"/> rather than
    /// duplicated, since it's identical to what ProfilesTab already needed.
    ///
    /// No cached "working list" field (unlike GUI_OLD/tabs/ViewersTab.cs's m_workingViewers) -
    /// every mutation reads the current viewers fresh via <see cref="LoadViewers"/>, edits, and
    /// writes straight back through <see cref="Save"/>, matching how <see cref="ProfilesTab"/>
    /// has no cached list either and just re-reads <c>ProfileManager.GetProfiles()</c> each time.
    /// Viewer identity is the (unique, case-insensitive) name, same as GUI_OLD.
    /// </summary>
    internal class ViewersTab : IShellTabView
    {
        private GameObject m_root;
        private GameObject m_viewerListContainer;
        private string m_searchText = "";

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();
        private readonly ViewerEditDialog m_viewerEditDialog = new ViewerEditDialog();

        private readonly ColumnSortState m_sortState = new ColumnSortState("name");
        private readonly List<(string Text, string SortKey, Text Label)> m_sortableHeaders = new List<(string, string, Text)>();

        // Required entry that can't be deleted from the UI - EnsureRequiredViewers would just
        // re-add it on the next read/save anyway, so the Delete button is hidden for it rather
        // than let a delete silently no-op (mirrors GUI_OLD/tabs/ViewersTab.cs).
        private const string RequiredViewerName = "deathwizsh";

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;   // 980
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;                          // -460
        private const float RightEdgeX = (ContentWidth / 2f) - ContentMargin - ScrollableList.ScrollbarWidth; // 444

        private const float TitleY   = -30f;
        private const float TitleWidth = 300f;
        private const float ToolbarY = -75f;
        private const float ColumnHeaderY = -112f;
        private const float ListTopInset = 131f;

        private const float SearchWidth = 300f;
        private const float ToolbarButtonSpacing = 10f;
        private const float NewViewerBtnWidth = 150f;

        private const float ColNameW    = 340f;
        private const float ColColorW   = 220f;
        private const float ColEffectsW = 190f;
        private const float ColActionsW = 150f;

        // Name is the only column flush with the list's true left edge - inset just its text,
        // not the column boundary itself, so ColColorX/ColEffectsX/ColActionsX (all derived from
        // ColNameW) don't shift.
        private const float ColNameTextW = ColNameW - ListRow.LeftPadding;
        private const float ColNameX    = LeftEdgeX + ListRow.LeftPadding + ColNameTextW / 2f;
        private const float ColColorX   = LeftEdgeX + ColNameW + ColColorW / 2f;
        private const float ColEffectsX = LeftEdgeX + ColNameW + ColColorW + ColEffectsW / 2f;
        private const float ColActionsX = RightEdgeX - ColActionsW / 2f;

        // Color cell = a small swatch square + the hex value, left-aligned within the Color column.
        private const float SwatchSize  = 16f;
        private const float SwatchInset = 10f;
        private const float SwatchGap   = 8f;
        private const float HexTextWidth = ColColorW - SwatchSize - SwatchInset - SwatchGap - 10f;

        // The "Color" header text otherwise starts flush at the column's left edge, while
        // BuildColorCell insets the actual swatch by SwatchInset - shift/shrink the header to
        // start at the same X the swatch does, so the label lines up with what's beneath it.
        private const float ColColorHeaderW = ColColorW - SwatchInset;
        private const float ColColorHeaderX = ColColorX - ColColorW / 2f + SwatchInset + ColColorHeaderW / 2f;

        private const float EditBtnWidth = 70f;
        private const float BtnDeleteX = RightEdgeX - ListRow.ItemHeight / 2f;
        private const float BtnEditX   = BtnDeleteX - ListRow.ItemHeight / 2f - ListRow.ItemSpacing - EditBtnWidth / 2f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "ViewersTab");

            m_confirmDialog.Init();
            m_viewerEditDialog.Init();

            GuiHelper.CreateTitle("Viewers", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);

            BuildToolbar();
            BuildColumnHeaders();

            m_viewerListContainer = ScrollableList.CreateStretched(
                m_root, "ViewerList",
                offsetMin: new Vector2(ContentMargin, ContentMargin),
                offsetMax: new Vector2(-ContentMargin, -ListTopInset),
                autoHideScrollbar: true);

            RefreshList();

            return m_root;
        }

        /// <summary>
        /// Re-populates the row list. Call whenever the shell re-shows this tab (viewers.yaml may
        /// have changed via another session/hand-edit while this tab wasn't visible).
        /// </summary>
        public void Refresh()
        {
            RefreshList();
        }

        // ── toolbar / column headers ────────────────────────────────────────

        private void BuildToolbar()
        {
            float searchX = LeftEdgeX + SearchWidth / 2f;
            InputField searchField = GuiFieldBuilder.CreateInputField(m_root, new Vector2(searchX, ToolbarY), SearchWidth, placeholderText: "Search viewers...");
            searchField.onValueChanged.AddListener(OnSearchChanged);

            float newBtnX = LeftEdgeX + SearchWidth + ToolbarButtonSpacing + NewViewerBtnWidth / 2f;
            GameObject newBtnObj = GUIManager.Instance.CreateButton(
                text: "+ New viewer",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(newBtnX, ToolbarY),
                width: NewViewerBtnWidth,
                height: 36f
            );
            newBtnObj.SetActive(true);
            newBtnObj.GetComponent<Button>().onClick.AddListener(OnCreateViewer);
        }

        private void BuildColumnHeaders()
        {
            CreateSortableHeader("Name", ColNameX, ColNameTextW, TextAnchor.MiddleLeft, "name");
            CreateSortableHeader("Color", ColColorHeaderX, ColColorHeaderW, TextAnchor.MiddleLeft, "color1");
            CreateSortableHeader("Effects", ColEffectsX, ColEffectsW, TextAnchor.MiddleLeft, "effects");
            CreateSortableHeader("Actions", ColActionsX, ColActionsW, TextAnchor.MiddleRight, null);
        }

        private void CreateSortableHeader(string text, float x, float width, TextAnchor alignment, string sortKey)
        {
            Text header = GUIManager.Instance.CreateText(
                text: text,
                parent: m_root.transform,
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

        // ── row list ─────────────────────────────────────────────────────────

        private static List<ViewerEntry> LoadViewers() => ExtraConfigHelper.ReadViewersConfig() ?? new List<ViewerEntry>();

        private void SortViewers(List<ViewerEntry> viewers)
        {
            Comparison<ViewerEntry> comparison;
            switch (m_sortState.Key)
            {
                case "color1":
                    comparison = (a, b) => CompareByHue(a.color1, b.color1);
                    break;
                case "effects":
                    comparison = (a, b) => (a.effects?.Count ?? 0).CompareTo(b.effects?.Count ?? 0);
                    break;
                default:
                    comparison = (a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            viewers.Sort(comparison);
            if (!m_sortState.Ascending)
                viewers.Reverse();
        }

        /// <summary>
        /// Sorts colors by hue (then saturation, then brightness as tiebreaks) instead of the raw
        /// hex text - a plain string compare (even case-insensitive) groups colors by which digit/
        /// letter their hex happens to start with, not by how they actually look, so e.g. "#DB4A3F"
        /// and "#FF0000" (both red) can land far apart while unrelated colors that merely share a
        /// leading character end up next to each other. Hue-sorting makes visually similar colors
        /// (reds with reds, blues with blues, etc.) sort next to each other instead.
        /// </summary>
        private static int CompareByHue(string hexA, string hexB)
        {
            Color.RGBToHSV(ParseColor(hexA), out float hueA, out float satA, out float valA);
            Color.RGBToHSV(ParseColor(hexB), out float hueB, out float satB, out float valB);

            int hueCompare = hueA.CompareTo(hueB);
            if (hueCompare != 0)
                return hueCompare;

            int satCompare = satA.CompareTo(satB);
            return satCompare != 0 ? satCompare : valA.CompareTo(valB);
        }

        private static Color ParseColor(string hex)
        {
            Color color = Color.white;
            if (!string.IsNullOrEmpty(hex))
                ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }

        private void RefreshList()
        {
            GuiHelper.ClearContainer(m_viewerListContainer);
            RefreshHeaderIndicators();

            List<ViewerEntry> viewers = LoadViewers();
            SortViewers(viewers);

            float yOffset = -(ListRow.ListTopPadding + ListRow.ItemHeight / 2f);
            int rowIndex = 0;

            foreach (ViewerEntry viewer in viewers)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && viewer.name.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                BuildRow(viewer, yOffset, rowIndex);
                yOffset -= ListRow.ItemHeight + ListRow.ItemSpacing;
                rowIndex++;
            }

            RectTransform contentRt = m_viewerListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ListRow.ItemHeight / 2f);
        }

        private void BuildRow(ViewerEntry viewer, float yOffset, int rowIndex)
        {
            ViewerEntry captured = viewer;
            bool isRequired = string.Equals(viewer.name, RequiredViewerName, StringComparison.OrdinalIgnoreCase);

            var (row, rowBackground) = ListRow.Create(m_viewerListContainer, "ViewerRow", yOffset);
            var revealOnHover = new List<GameObject>();

            Text nameText = GUIManager.Instance.CreateText(
                text: viewer.name,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColNameX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColNameTextW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            nameText.alignment = TextAnchor.MiddleLeft;

            BuildColorCell(row, viewer.color1);

            string effectsLabel = viewer.effects != null && viewer.effects.Count > 0
                ? string.Join(", ", viewer.effects)
                : "-";

            Text effectsText = GUIManager.Instance.CreateText(
                text: effectsLabel,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColEffectsX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColEffectsW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            effectsText.alignment = TextAnchor.MiddleLeft;

            // Edit is hover-only (matches the mockup's pencil icon appearing only on row hover);
            // Delete stays permanently visible except for the required entry.
            GameObject editBtn = ListRow.CreateActionButton(row, "Edit", BtnEditX, EditBtnWidth, ListRow.ItemHeight, Color.cyan, () => ShowEditDialog(captured));
            revealOnHover.Add(editBtn);

            if (!isRequired)
            {
                GameObject deleteBtn = ListRow.CreateActionButton(row, "X", BtnDeleteX, ListRow.ItemHeight, ListRow.ItemHeight, Color.red, () => OnDeleteViewer(captured));
                deleteBtn.SetActive(true);
            }

            ListRow.AttachHoverReveal(row, rowBackground, revealOnHover, ListRow.ZebraColor(rowIndex));
        }

        /// <summary>
        /// A non-interactive color square (the new element this tab introduces to a list row) +
        /// its hex text, left-aligned within the Color column - matches the mockup's swatch+hex
        /// pair. Deliberately not built via <see cref="GuiFieldBuilder.CreateColorField"/> - that
        /// factory creates a *clickable* button that opens the color picker (used by
        /// <see cref="ViewerEditDialog"/>'s edit form); this is just read-only row content.
        /// </summary>
        private static void BuildColorCell(GameObject row, string colorHex)
        {
            Color swatchColor = ParseColor(colorHex);

            float colorLeftEdge = ColColorX - ColColorW / 2f;
            float swatchX = colorLeftEdge + SwatchInset + SwatchSize / 2f;
            float hexTextX = swatchX + SwatchSize / 2f + SwatchGap + HexTextWidth / 2f;

            GameObject swatch = new GameObject("ColorSwatch");
            swatch.transform.SetParent(row.transform, false);

            RectTransform swatchRt = swatch.AddComponent<RectTransform>();
            swatchRt.anchorMin = new Vector2(0.5f, 0.5f);
            swatchRt.anchorMax = new Vector2(0.5f, 0.5f);
            swatchRt.pivot = new Vector2(0.5f, 0.5f);
            swatchRt.sizeDelta = new Vector2(SwatchSize, SwatchSize);
            swatchRt.anchoredPosition = new Vector2(swatchX, 0f);

            swatch.AddComponent<Image>().color = swatchColor;
            GuiHelper.AddBorder(swatch, Color.black);

            Text hexText = GUIManager.Instance.CreateText(
                text: string.IsNullOrEmpty(colorHex) ? "-" : colorHex,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(hexTextX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: HexTextWidth,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            hexText.alignment = TextAnchor.MiddleLeft;
        }

        // ── create / edit / delete ───────────────────────────────────────────

        private void OnCreateViewer()
        {
            m_viewerEditDialog.Show(
                title: "New Viewer",
                description: "Enter a name and color for the new viewer.",
                suggestedName: "",
                suggestedColor: "#ffffff",
                onConfirm: HandleCreateConfirm,
                confirmText: "+ Add");
        }

        private string HandleCreateConfirm(string name, string color)
        {
            if (string.IsNullOrEmpty(name))
                return "Name is required.";

            List<ViewerEntry> viewers = LoadViewers();
            if (viewers.Exists(v => string.Equals(v.name, name, StringComparison.OrdinalIgnoreCase)))
                return $"A viewer named '{name}' already exists.";

            viewers.Add(new ViewerEntry { name = name, color1 = color });
            Save(viewers, $"'{name}' added.");
            return null;
        }

        private void ShowEditDialog(ViewerEntry viewer)
        {
            string originalName = viewer.name;

            m_viewerEditDialog.Show(
                title: "Edit Viewer",
                description: "Update this viewer's name and color.",
                suggestedName: viewer.name,
                suggestedColor: string.IsNullOrEmpty(viewer.color1) ? "#ffffff" : viewer.color1,
                onConfirm: (name, color) => HandleEditConfirm(originalName, name, color),
                confirmText: "Save");
        }

        private string HandleEditConfirm(string originalName, string newName, string color)
        {
            if (string.IsNullOrEmpty(newName))
                return "Name is required.";

            List<ViewerEntry> viewers = LoadViewers();
            ViewerEntry entry = viewers.Find(v => string.Equals(v.name, originalName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
                return "Viewer no longer exists.";

            bool nameTaken = !string.Equals(originalName, newName, StringComparison.OrdinalIgnoreCase)
                && viewers.Exists(v => string.Equals(v.name, newName, StringComparison.OrdinalIgnoreCase));
            if (nameTaken)
                return $"A viewer named '{newName}' already exists.";

            entry.name = newName;
            entry.color1 = color;
            Save(viewers, $"'{newName}' updated.");
            return null;
        }

        private void OnDeleteViewer(ViewerEntry viewer)
        {
            string name = viewer.name;

            m_confirmDialog.Show(
                title:       "Delete Viewer",
                description: $"Are you sure you want to delete '{name}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    List<ViewerEntry> viewers = LoadViewers();
                    viewers.RemoveAll(v => string.Equals(v.name, name, StringComparison.OrdinalIgnoreCase));
                    Save(viewers, $"'{name}' removed.");
                },
                confirmText: "Delete");
        }

        /// <summary>
        /// Writes <paramref name="viewers"/> to disk via <see cref="ExtraConfigHelper.WriteViewersConfig"/>,
        /// backing up the previous file first and restoring it if the write throws - same
        /// backup/restore shape as GUI_OLD/tabs/ViewersTab.cs's Save(), just delegating the actual
        /// serialize step to the shared helper instead of duplicating it.
        /// </summary>
        private void Save(List<ViewerEntry> viewers, string successMessage)
        {
            string path = WizshBoneTwitchIntegration.viewersPath;
            string backupPath = path + ".bak";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                ExtraConfigHelper.WriteViewersConfig(viewers);

                RecolorHelper.ReloadViewersConfig();
                ToastNotifications.Show(successMessage);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to save viewers, restoring backup: {ex}");

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    RecolorHelper.ReloadViewersConfig();
                }

                ToastNotifications.Show("Save failed! Restored previous viewers file.");
            }

            RefreshList();
        }
    }
}
