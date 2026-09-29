using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Profiles tab content - list/switch/create/copy/rename/delete/export/import/sync, built
    /// directly against RedesignUI.dc.html's structure (search+New+Import toolbar, sortable
    /// Name/Redeems/Actions columns, hover-revealed row actions) rather than a port of
    /// GUI_OLD/tabs/ProfilesTab.cs's anchored-button layout - see the round-2 plan.
    /// Backend calls (ProfileManager/ProfileSyncHelper/ExtraConfigHelper) are reused unchanged.
    /// </summary>
    internal class ProfilesTab : IShellTabView
    {
        private GameObject m_root;
        private GameObject m_profileListContainer;
        private string m_searchText = "";

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();
        private readonly InputDialog m_inputDialog = new InputDialog();

        private readonly ColumnSortState m_sortState = new ColumnSortState("name");
        private readonly List<(string Text, string SortKey, Text Label)> m_sortableHeaders = new List<(string, string, Text)>();

        // ── layout constants ────────────────────────────────────────────────
        // Derived from the shell's actual content-region size rather than guessed, so this
        // stays correct if the shell panel/sidebar/topbar dimensions ever change.
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;   // 980
        private const float ContentHeight = WizshBoneShellGUI.PanelHeight - ShellTopBar.Height;  // 660
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;  // -460
        // ScrollableList always reserves its scrollbar's width in the viewport/content, even
        // while the bar itself is hidden (autoHideScrollbar only toggles the bar's own visibility,
        // never the reserved space - see ScrollbarAutoHide) - so the Actions column (rightmost)
        // must fit within that narrower width too, or its Reload/Delete buttons end up sitting
        // under the bar's track the moment it actually shows.
        private const float RightEdgeX = (ContentWidth / 2f) - ContentMargin - ScrollableList.ScrollbarWidth; // 444

        private const float TitleY   = -30f;
        private const float TitleWidth = 300f;
        private const float DescriptionY = -50f;
        private const float ToolbarY = -99f;
        private const float ColumnHeaderY = -136f;
        private const float ListTopInset = 155f; // distance from the root's top edge to the list

        private const float SearchWidth = 300f;
        private const float ToolbarButtonSpacing = 10f;
        private const float NewProfileBtnWidth = 130f;
        private const float ImportToolbarBtnWidth = 90f;

        private const float ColNameW    = 400f;
        private const float ColRedeemsW = 150f;
        private const float ColActionsW = 370f;

        // Name is the only column flush with the list's true left edge - inset just its text,
        // not the column boundary itself, so ColRedeemsX/ColActionsX (both derived from ColNameW)
        // don't shift.
        private const float ColNameTextW = ColNameW - ListRow.LeftPadding;
        private const float ColNameX    = LeftEdgeX + ListRow.LeftPadding + ColNameTextW / 2f; // -254
        private const float ColRedeemsX = LeftEdgeX + ColNameW + ColRedeemsW / 2f;           //  15
        private const float ColActionsX = RightEdgeX - ColActionsW / 2f;                     // 259

        // Row action buttons chain left-to-right within the Actions column: Copy, Export, Sync,
        // Rename, then Reload/Delete share the rightmost slot - same order/sizing as
        // GUI_OLD/tabs/ProfilesTab.cs's button cluster (minus its per-row Import, dropped per
        // user feedback - only the toolbar's Import remains), recalculated against this tab's
        // own Actions column bounds.
        private const float CopyBtnWidth   = 70f;
        private const float ExportBtnWidth = 80f;
        private const float SyncBtnWidth   = 80f;
        private const float RenameBtnWidth = 80f;
        private const float ReloadDeleteWidth = 40f;

        private const float ActionsClusterLeft = RightEdgeX - ColActionsW; // = ColActionsX - ColActionsW/2

        private const float BtnCopyX         = ActionsClusterLeft + CopyBtnWidth / 2f;
        private const float BtnExportX       = BtnCopyX + CopyBtnWidth / 2f + ListRow.ItemSpacing + ExportBtnWidth / 2f;
        private const float BtnSyncX         = BtnExportX + ExportBtnWidth / 2f + ListRow.ItemSpacing + SyncBtnWidth / 2f;
        private const float BtnRenameX       = BtnSyncX + SyncBtnWidth / 2f + ListRow.ItemSpacing + RenameBtnWidth / 2f;
        private const float BtnReloadDeleteX = BtnRenameX + RenameBtnWidth / 2f + ListRow.ItemSpacing + ReloadDeleteWidth / 2f;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFilter;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFile;
            public int nMaxFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFileTitle;
            public int nMaxFileTitle;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrInitialDir;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpTemplateName;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetSaveFileName(ref OpenFileName ofn);

        private const int OFN_FILEMUSTEXIST   = 0x00001000;
        private const int OFN_PATHMUSTEXIST   = 0x00000800;
        private const int OFN_NOCHANGEDIR     = 0x00000008;
        private const int OFN_OVERWRITEPROMPT = 0x00000002;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "ProfilesTab");

            m_confirmDialog.Init();
            m_inputDialog.Init();

            GuiHelper.CreateTitle("Profiles", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "Profiles hold their own redeems and settings - switch between profiles to change your whole setup at once.",
                m_root, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            BuildToolbar();
            BuildColumnHeaders();

            m_profileListContainer = ScrollableList.CreateStretched(
                m_root, "ProfileList",
                offsetMin: new Vector2(ContentMargin, ContentMargin),
                offsetMax: new Vector2(-ContentMargin, -ListTopInset),
                autoHideScrollbar: true);

            RefreshList();

            return m_root;
        }

        /// <summary>
        /// Re-populates the row list. Call whenever the shell re-shows this tab (profiles on
        /// disk may have changed via sync or another session).
        /// </summary>
        public void Refresh()
        {
            RefreshList();
        }

        // ── toolbar / column headers ────────────────────────────────────────

        private void BuildToolbar()
        {
            float searchX = LeftEdgeX + SearchWidth / 2f;
            InputField searchField = GuiFieldBuilder.CreateInputField(m_root, new Vector2(searchX, ToolbarY), SearchWidth, placeholderText: "Search profiles...");
            searchField.onValueChanged.AddListener(OnSearchChanged);

            float cursor = LeftEdgeX + SearchWidth;
            cursor = CreateToolbarButton("+ New profile", NewProfileBtnWidth, cursor, OnCreateProfile);
            CreateToolbarButton("Import", ImportToolbarBtnWidth, cursor, BeginImport);
        }

        private float CreateToolbarButton(string text, float width, float cursorX, UnityEngine.Events.UnityAction onClick)
        {
            float centerX = cursorX + ToolbarButtonSpacing + width / 2f;

            GameObject btnObj = GuiHelper.CreateButton(
                text: text,
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(centerX, ToolbarY),
                width: width,
                height: 36f
            );
            btnObj.SetActive(true);
            btnObj.GetComponent<Button>().onClick.AddListener(onClick);

            return centerX + width / 2f;
        }

        private void BuildColumnHeaders()
        {
            CreateSortableHeader("Name", ColNameX, ColNameTextW, TextAnchor.MiddleLeft, "name");
            CreateSortableHeader("Redeems", ColRedeemsX, ColRedeemsW, TextAnchor.MiddleLeft, "redeemCount");
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

        private void RefreshList()
        {
            GuiHelper.ClearContainer(m_profileListContainer);
            RefreshHeaderIndicators();

            // Redeem count isn't a persisted field - it's read from each profile's YAML - so it
            // has to be computed up front (rather than lazily inside the row loop) whenever
            // sorting by it.
            List<(string Name, int RedeemCount)> profiles = ProfileManager.GetProfiles()
                .Select(p => (Name: p, RedeemCount: ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(p))?.redeems?.Count ?? 0))
                .ToList();

            Comparison<(string Name, int RedeemCount)> comparison = m_sortState.Key == "redeemCount"
                ? (a, b) => a.RedeemCount.CompareTo(b.RedeemCount)
                : (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            profiles.Sort(comparison);
            if (!m_sortState.Ascending)
                profiles.Reverse();

            float yOffset = -(ListRow.ListTopPadding + ListRow.ItemHeight / 2f);
            int rowIndex = 0;

            foreach ((string profile, int redeemCount) in profiles)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && profile.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                BuildRow(profile, redeemCount, yOffset, rowIndex);
                yOffset -= ListRow.ItemHeight + ListRow.ItemSpacing;
                rowIndex++;
            }

            RectTransform contentRt = m_profileListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ListRow.ItemHeight / 2f);
        }

        private void BuildRow(string profile, int redeemCount, float yOffset, int rowIndex)
        {
            string profileName = profile;
            bool isActive = profile == ProfileManager.ActiveProfile;
            bool isSynced = ProfileManager.IsSyncedProfile(profile);

            var (row, rowBackground) = ListRow.Create(m_profileListContainer, "ProfileRow", yOffset);
            var revealOnHover = new List<GameObject>();

            Text nameText = GUIManager.Instance.CreateText(
                text: profile,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColNameX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: isActive ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColNameTextW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            nameText.alignment = TextAnchor.MiddleLeft;

            Text countText = GUIManager.Instance.CreateText(
                text: redeemCount.ToString(),
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(ColRedeemsX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ColRedeemsW,
                height: ListRow.ItemHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            countText.alignment = TextAnchor.MiddleLeft;

            // The row itself is clickable (selects this profile) - purely functional, no visual
            // transition of its own; RowHoverReveal (added below) owns the hover-darken look.
            Button rowSelectButton = row.AddComponent<Button>();
            rowSelectButton.targetGraphic = rowBackground;
            rowSelectButton.transition = Selectable.Transition.None;
            if (!isActive)
                rowSelectButton.onClick.AddListener(() => OnSelectProfile(profileName));

            GameObject copyBtn = ListRow.CreateActionButton(row, "Copy", BtnCopyX, CopyBtnWidth, ListRow.ItemHeight, Color.yellow, () => OnRowCopy(profileName));
            revealOnHover.Add(copyBtn);

            GameObject exportBtn = ListRow.CreateActionButton(row, "Export", BtnExportX, ExportBtnWidth, ListRow.ItemHeight, Color.yellow, () => OnRowExport(profileName));
            revealOnHover.Add(exportBtn);

            GameObject syncBtn = ListRow.CreateActionButton(row, "Sync", BtnSyncX, SyncBtnWidth, ListRow.ItemHeight, Color.yellow, () => OnRowSync(profileName));
            syncBtn.GetComponent<Button>().interactable = !isSynced;
            revealOnHover.Add(syncBtn);

            GameObject renameBtn = ListRow.CreateActionButton(row, "Rename", BtnRenameX, RenameBtnWidth, ListRow.ItemHeight, Color.cyan, () => OnRenameProfile(profileName));
            renameBtn.GetComponent<Button>().interactable = !isSynced;
            revealOnHover.Add(renameBtn);

            // The Reload/Delete slot stays permanently visible (not added to revealOnHover) so a
            // row isn't empty-looking until hovered - matching GUI_OLD.
            if (isActive)
            {
                GameObject reloadBtn = ListRow.CreateActionButton(row, "↺", BtnReloadDeleteX, ReloadDeleteWidth, ListRow.ItemHeight, GUIManager.Instance.ValheimOrange, () => OnReloadProfile(profileName));
                reloadBtn.SetActive(true);
            }
            else
            {
                GameObject deleteBtn = ListRow.CreateActionButton(row, "X", BtnReloadDeleteX, ReloadDeleteWidth, ListRow.ItemHeight, Color.red, () => OnDeleteProfile(profileName));
                deleteBtn.SetActive(true);
            }

            ListRow.AttachHoverReveal(row, rowBackground, revealOnHover, ListRow.ZebraColor(rowIndex));
        }

        // ── row/toolbar action handlers ─────────────────────────────────────

        private void EnsureActive(string name)
        {
            if (name == ProfileManager.ActiveProfile)
                return;

            ProfileManager.SelectProfile(name);
            RefreshList();
        }

        private void OnSelectProfile(string name)
        {
            GuiHelper.PlayClickSound();
            ProfileManager.SelectProfile(name);
            RefreshList();
        }

        private void OnRowCopy(string name)
        {
            EnsureActive(name);
            OnCopyProfile();
        }

        private void OnRowExport(string name)
        {
            EnsureActive(name);
            OnExportProfile();
        }

        private void OnRowSync(string name)
        {
            EnsureActive(name);
            OnSyncProfile();
        }

        private void OnCreateProfile()
        {
            m_inputDialog.Show(
                title: "New Profile",
                description: "Enter a name for the new profile.",
                suggestedValue: "",
                onConfirm: HandleCreateConfirm,
                confirmText: "Create",
                maxLength: ProfileManager.MaxProfileNameLength);
        }

        private string HandleCreateConfirm(string name)
        {
            bool created = ProfileManager.CreateProfile(name, out string error);
            if (!created)
                return error;

            ToastNotifications.Show($"Profile '{name}' created!", ToastType.Success);
            RefreshList();
            return null;
        }

        private void OnCopyProfile()
        {
            string suggestedName = ProfileManager.GetUniqueProfileName(ProfileManager.ActiveProfile);

            m_inputDialog.Show(
                title: "Copy Profile",
                description: "Enter a name for the new profile copy.",
                suggestedValue: suggestedName,
                onConfirm: HandleCopyConfirm,
                confirmText: "Copy",
                maxLength: ProfileManager.MaxProfileNameLength);
        }

        private string HandleCopyConfirm(string newName)
        {
            bool copied = ProfileManager.CopyProfileTo(newName, out string error);
            if (!copied)
                return error;

            ToastNotifications.Show($"Copied '{ProfileManager.ActiveProfile}' to '{newName}'.", ToastType.Success);
            RefreshList();
            return null;
        }

        /// <summary>
        /// Shared Import flow, used both by the toolbar's Import button (no specific row
        /// selected first) and each row's Import action (<see cref="OnRowImport"/> calls
        /// <see cref="EnsureActive"/> first) - the actual import target always comes from the
        /// dialog's name field either way, not from which row (if any) triggered this.
        /// </summary>
        private void BeginImport()
        {
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            OpenFileName ofn = new OpenFileName();
            ofn.lStructSize     = Marshal.SizeOf(ofn);
            ofn.lpstrFilter     = "YAML Files (*.yaml)\0*.yaml\0All Files (*.*)\0*.*\0";
            ofn.lpstrFile       = new string('\0', 260);
            ofn.nMaxFile        = 260;
            ofn.lpstrTitle      = "Select a profile.yaml to import";
            ofn.lpstrInitialDir = downloadsPath;
            ofn.Flags           = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;

            if (!GetOpenFileName(ref ofn))
                return;

            try
            {
                string selectedFile = ofn.lpstrFile.TrimEnd('\0');
                string suggestedName = ProfileManager.ResolveImportProfileName(selectedFile);

                m_inputDialog.Show(
                    title: "Import Profile",
                    description: "Leave the name unchanged to update the existing profile.\nChange it to import as a new profile.",
                    suggestedValue: suggestedName,
                    onConfirm: newName => HandleImportConfirm(selectedFile, newName),
                    confirmText: "Import",
                    maxLength: ProfileManager.MaxProfileNameLength);
            }
            catch (Exception e)
            {
                ToastNotifications.Show("Import failed: " + e.Message, ToastType.Error);
                Jotunn.Logger.LogWarning("[WBTI] Profile import failed: " + e);
            }
        }

        private string HandleImportConfirm(string selectedFile, string targetName)
        {
            try
            {
                bool imported = ProfileManager.ImportProfileTo(selectedFile, targetName, out bool profileCreated, out string error);
                if (!imported)
                    return error;

                ToastNotifications.Show(profileCreated
                    ? $"Created and imported profile '{targetName}'."
                    : $"Updated profile '{targetName}' with imported file.", ToastType.Success);
                RefreshList();
                return null;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning("[WBTI] Profile import failed: " + e);
                return "Import failed: " + e.Message;
            }
        }

        private void OnExportProfile()
        {
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            string defaultFileName = $"{ProfileManager.ActiveProfile}.yaml";
            string fileBuffer      = defaultFileName + new string('\0', 260 - defaultFileName.Length);

            OpenFileName ofn = new OpenFileName();
            ofn.lStructSize     = Marshal.SizeOf(ofn);
            ofn.lpstrFilter     = "YAML Files (*.yaml)\0*.yaml\0All Files (*.*)\0*.*\0";
            ofn.lpstrFile       = fileBuffer;
            ofn.nMaxFile        = 260;
            ofn.lpstrDefExt     = "yaml";
            ofn.lpstrTitle      = "Export profile as";
            ofn.lpstrInitialDir = downloadsPath;
            ofn.Flags           = OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR | OFN_OVERWRITEPROMPT;

            if (!GetSaveFileName(ref ofn))
                return;

            try
            {
                string destPath = ofn.lpstrFile.TrimEnd('\0');
                bool exported = ProfileManager.ExportProfile(destPath);
                ToastNotifications.Show(exported
                    ? $"Exported '{ProfileManager.ActiveProfile}' to {Path.GetFileName(destPath)}."
                    : "Export failed: profile file not found.", exported ? ToastType.Success : ToastType.Error);
            }
            catch (Exception e)
            {
                ToastNotifications.Show("Export failed: " + e.Message, ToastType.Error);
                Jotunn.Logger.LogWarning("[WBTI] Profile export failed: " + e);
            }
        }

        private void OnSyncProfile()
        {
            if (ZNet.instance == null || ZNet.instance.GetPeers().Count == 0)
            {
                ToastNotifications.Show("Sync failed: no players connected.", ToastType.Error);
                return;
            }

            if (ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile))
            {
                ToastNotifications.Show("Only the original owner can sync this profile.", ToastType.Warning);
                return;
            }

            ProfileSyncHelper.SendActiveProfileToAll();
            ToastNotifications.Show($"Profile '{ProfileManager.ActiveProfile}' synced to all online players.", ToastType.Success);
        }

        private void OnReloadProfile(string name)
        {
            TwitchCustomRewards customRewards = Game.instance?.gameObject?.GetComponent<TwitchCustomRewards>();

            bool reloaded = customRewards != null
                ? customRewards.ReloadRewards()
                : RedeemHelper.Reload();

            reloaded &= ProfileSettingsHelper.Reload();

            ToastNotifications.Show(reloaded
                ? $"Profile '{name}' reloaded."
                : $"Failed to reload profile '{name}'.", reloaded ? ToastType.Success : ToastType.Error);
        }

        private void OnRenameProfile(string name)
        {
            m_inputDialog.Show(
                title: "Rename Profile",
                description: "Enter a new name for this profile.",
                suggestedValue: name,
                onConfirm: newName => HandleRenameConfirm(name, newName),
                confirmText: "Rename",
                maxLength: ProfileManager.MaxProfileNameLength);
        }

        private string HandleRenameConfirm(string oldName, string newName)
        {
            if (newName == oldName)
                return "Name unchanged.";

            try
            {
                bool renamed = ProfileManager.RenameProfile(oldName, newName, out string error);
                if (!renamed)
                    return error;

                ToastNotifications.Show($"Renamed '{oldName}' to '{newName}'.", ToastType.Success);
                RefreshList();
                return null;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning("[WBTI] Profile rename failed: " + e);
                return "Rename failed: " + e.Message;
            }
        }

        private void OnDeleteProfile(string name)
        {
            m_confirmDialog.Show(
                title:       "Delete Profile",
                description: $"Are you sure you want to delete '{name}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    bool deleted = ProfileManager.DeleteProfile(name);
                    ToastNotifications.Show(deleted
                        ? $"Profile '{name}' deleted."
                        : $"Cannot delete profile '{name}'.", deleted ? ToastType.Success : ToastType.Error);
                    RefreshList();
                }
            );
        }
    }
}
