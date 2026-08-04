using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ProfilesTab
    {
        private GameObject m_root;
        private GameObject m_profileListContainer;
        private Text m_profileFeedbackText;
        private string m_searchText = "";
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();
        private readonly ProfileNameDialog m_profileNameDialog = new ProfileNameDialog();

        private const float ItemHeight     = 40f;
        private const float ItemSpacing    = 5f;
        private const float ListTopPadding = 15f;

        private const float ColNameX = -370f;
        private const float ColNameW = 260f;

        private const float ColCountX = -155f;
        private const float ColCountW = 140f;

        // Row action buttons chain left-to-right: Copy, Import, Export, Sync, Edit, then
        // either Reload (active row) or Delete (every other row) share the same rightmost slot.
        private const float CopyBtnWidth      = 70f;
        private const float ImportBtnWidth    = 80f;
        private const float ExportBtnWidth    = 80f;
        private const float SyncBtnWidth      = 80f;
        private const float EditBtnWidth      = 80f;
        private const float DeleteReloadWidth = 40f;

        private const float BtnCopyX   = -40f;
        private const float BtnImportX = BtnCopyX + CopyBtnWidth / 2f + ItemSpacing + ImportBtnWidth / 2f;
        private const float BtnExportX = BtnImportX + ImportBtnWidth / 2f + ItemSpacing + ExportBtnWidth / 2f;
        private const float BtnSyncX   = BtnExportX + ExportBtnWidth / 2f + ItemSpacing + SyncBtnWidth / 2f;
        private const float BtnEditX   = BtnSyncX + SyncBtnWidth / 2f + ItemSpacing + EditBtnWidth / 2f;
        private const float BtnDeleteReloadX = BtnEditX + EditBtnWidth / 2f + ItemSpacing + DeleteReloadWidth / 2f;

        // Spans the whole Copy..Delete/Reload button cluster, for the "Actions" column header.
        private const float ActionsClusterLeft  = BtnCopyX - CopyBtnWidth / 2f;
        private const float ActionsClusterRight = BtnDeleteReloadX + DeleteReloadWidth / 2f;
        private const float ColActionsX = (ActionsClusterLeft + ActionsClusterRight) / 2f;
        private const float ColActionsW = ActionsClusterRight - ActionsClusterLeft;

        private const float NewProfileBtnWidth = 100f;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OpenFileName
        {
            public int    lStructSize;
            public System.IntPtr hwndOwner;
            public System.IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFilter;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrCustomFilter;
            public int    nMaxCustFilter;
            public int    nFilterIndex;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFile;
            public int    nMaxFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFileTitle;
            public int    nMaxFileTitle;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrInitialDir;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrTitle;
            public int    Flags;
            public short  nFileOffset;
            public short  nFileExtension;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpstrDefExt;
            public System.IntPtr lCustData;
            public System.IntPtr lpfnHook;
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

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_root = UIContainer.Create(parent, "ProfilesTab");

            m_confirmDialog.Init();
            m_profileNameDialog.Init();

            var options = new TabListLayoutOptions
            {
                TitleText         = "Profiles:",
                SearchPlaceholder = "Search profiles...",

                ShowSearchBar   = true,
                OnSearchChanged = OnSearchChanged,

                HeaderButtons = new List<HeaderButtonSpec>
                {
                    new HeaderButtonSpec("+ New", NewProfileBtnWidth, OnCreateProfile),
                },

                ColumnHeaders = new List<ColumnHeaderSpec>
                {
                    new ColumnHeaderSpec("Name", ColNameX, ColNameW, TextAnchor.MiddleLeft),
                    new ColumnHeaderSpec("Redeem amount", ColCountX, ColCountW, TextAnchor.MiddleCenter),
                    new ColumnHeaderSpec("Actions", ColActionsX, ColActionsW, TextAnchor.MiddleCenter),
                },

                MainContainerName = "ProfileList",
            };

            TabListLayoutResult result = TabListLayout.Create(m_root, "ListView", createScrollable, options);
            result.ListView.SetActive(true);

            m_profileFeedbackText  = result.FeedbackText;
            m_profileListContainer = result.MainContainer;

            return m_root;
        }

        public void Refresh()
        {
            RefreshList();
        }

        private void RefreshList()
        {
            TabUIHelper.ClearContainer(m_profileListContainer);

            List<string> profiles = ProfileManager.GetProfiles();
            float yOffset = -(ListTopPadding + ItemHeight / 2f);

            foreach (string profile in profiles)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && profile.IndexOf(m_searchText, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                string profileName = profile;
                bool isActive = profile == ProfileManager.ActiveProfile;
                bool isSynced = ProfileManager.IsSyncedProfile(profile);

                GameObject row = new GameObject("ProfileRow");
                row.transform.SetParent(m_profileListContainer.transform, false);

                RectTransform rowRt = row.AddComponent<RectTransform>();
                rowRt.anchorMin        = new Vector2(0f, 1f);
                rowRt.anchorMax        = new Vector2(1f, 1f);
                rowRt.pivot            = new Vector2(0.5f, 0.5f);
                rowRt.anchoredPosition = new Vector2(0f, yOffset);
                rowRt.sizeDelta        = new Vector2(0f, ItemHeight);

                Image rowBackground = row.AddComponent<Image>();
                var revealOnHover = new List<GameObject>();

                Text nameText = GUIManager.Instance.CreateText(
                    text: profile,
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(ColNameX, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: isActive ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColNameW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                nameText.alignment = TextAnchor.MiddleLeft;

                var redeemConfig = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(profile));
                int redeemCount = redeemConfig?.redeems?.Count ?? 0;

                Text countText = GUIManager.Instance.CreateText(
                    text: $"{redeemCount} redeems",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(ColCountX, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColCountW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                countText.alignment = TextAnchor.MiddleCenter;

                // The row itself is clickable (selects this profile) - purely functional, no visual
                // transition of its own; RowHoverReveal (added below) owns the hover-darken look.
                Button rowSelectButton = row.AddComponent<Button>();
                rowSelectButton.targetGraphic = rowBackground;
                rowSelectButton.transition = Selectable.Transition.None;
                if (!isActive)
                    rowSelectButton.onClick.AddListener(() => OnSelectProfile(profileName));

                GameObject copyBtn = GUIManager.Instance.CreateButton(
                    text: "Copy",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnCopyX, 0f),
                    width: CopyBtnWidth,
                    height: ItemHeight
                );
                copyBtn.GetComponentInChildren<Text>().color = Color.yellow;
                TabUIHelper.AddBorder(copyBtn, Color.yellow);
                copyBtn.GetComponent<Button>().onClick.AddListener(() => OnRowCopy(profileName));
                revealOnHover.Add(copyBtn);

                GameObject importBtn = GUIManager.Instance.CreateButton(
                    text: "Import",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnImportX, 0f),
                    width: ImportBtnWidth,
                    height: ItemHeight
                );
                importBtn.GetComponentInChildren<Text>().color = Color.yellow;
                TabUIHelper.AddBorder(importBtn, Color.yellow);
                importBtn.GetComponent<Button>().onClick.AddListener(() => OnRowImport(profileName));
                revealOnHover.Add(importBtn);

                GameObject exportBtn = GUIManager.Instance.CreateButton(
                    text: "Export",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnExportX, 0f),
                    width: ExportBtnWidth,
                    height: ItemHeight
                );
                exportBtn.GetComponentInChildren<Text>().color = Color.yellow;
                TabUIHelper.AddBorder(exportBtn, Color.yellow);
                exportBtn.GetComponent<Button>().onClick.AddListener(() => OnRowExport(profileName));
                revealOnHover.Add(exportBtn);

                GameObject syncBtn = GUIManager.Instance.CreateButton(
                    text: "Sync",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnSyncX, 0f),
                    width: SyncBtnWidth,
                    height: ItemHeight
                );
                syncBtn.GetComponentInChildren<Text>().color = Color.yellow;
                TabUIHelper.AddBorder(syncBtn, Color.yellow);
                syncBtn.GetComponent<Button>().interactable = !isSynced;
                syncBtn.GetComponent<Button>().onClick.AddListener(() => OnRowSync(profileName));
                revealOnHover.Add(syncBtn);

                // The Reload/Delete slot stays permanently visible (not added to revealOnHover) so
                // a row isn't empty-looking until hovered - Edit joins Copy/Import/Export/Sync above
                // as hover-gated.
                GameObject editBtn = GUIManager.Instance.CreateButton(
                    text: "Edit",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnEditX, 0f),
                    width: EditBtnWidth,
                    height: ItemHeight
                );
                editBtn.GetComponentInChildren<Text>().color = Color.cyan;
                TabUIHelper.AddBorder(editBtn, Color.cyan);
                editBtn.GetComponent<Button>().interactable = !isSynced;
                editBtn.GetComponent<Button>().onClick.AddListener(() => OnRenameProfile(profileName));
                revealOnHover.Add(editBtn);

                if (isActive)
                {
                    GameObject reloadBtn = GUIManager.Instance.CreateButton(
                        text: "↺",
                        parent: row.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(BtnDeleteReloadX, 0f),
                        width: DeleteReloadWidth,
                        height: ItemHeight
                    );
                    reloadBtn.SetActive(true);
                    reloadBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimOrange;
                    TabUIHelper.AddBorder(reloadBtn, GUIManager.Instance.ValheimOrange);
                    reloadBtn.GetComponent<Button>().onClick.AddListener(() => OnReloadProfile(profileName));
                }
                else
                {
                    GameObject deleteBtn = GUIManager.Instance.CreateButton(
                        text: "X",
                        parent: row.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(BtnDeleteReloadX, 0f),
                        width: DeleteReloadWidth,
                        height: ItemHeight
                    );
                    deleteBtn.SetActive(true);
                    deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                    TabUIHelper.AddBorder(deleteBtn, Color.red);
                    deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteProfile(profileName));
                }

                row.AddComponent<RowHoverReveal>().Init(rowBackground, revealOnHover);

                yOffset -= ItemHeight + ItemSpacing;
            }

            RectTransform contentRt = m_profileListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ItemHeight / 2f);
        }

        private void EnsureActive(string name)
        {
            if (name == ProfileManager.ActiveProfile)
                return;

            ProfileManager.SelectProfile(name);
            Refresh();
        }

        private void OnRowCopy(string name)
        {
            EnsureActive(name);
            OnCopyProfile();
        }

        private void OnRowImport(string name)
        {
            EnsureActive(name);
            OnImportProfile();
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
            m_profileNameDialog.Show(
                mode:          ProfileNameDialogMode.Create,
                suggestedName: "",
                onConfirm:     HandleCreateConfirm
            );
        }

        private string HandleCreateConfirm(string name)
        {
            bool created = ProfileManager.CreateProfile(name, out string error);
            if (!created)
                return error;

            m_profileFeedbackText.text = $"Profile '{name}' created!";
            Refresh();
            return null;
        }

        private void OnCopyProfile()
        {
            string suggestedName = ProfileManager.GetUniqueProfileName(ProfileManager.ActiveProfile);

            m_profileNameDialog.Show(
                mode:          ProfileNameDialogMode.Copy,
                suggestedName: suggestedName,
                onConfirm:     HandleCopyConfirm
            );
        }

        private string HandleCopyConfirm(string newName)
        {
            bool copied = ProfileManager.CopyProfileTo(newName, out string error);
            if (!copied)
                return error;

            m_profileFeedbackText.text = $"Copied '{ProfileManager.ActiveProfile}' to '{newName}'.";
            Refresh();
            return null;
        }

        private void OnImportProfile()
        {
            string downloadsPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            OpenFileName ofn = new OpenFileName();
            ofn.lStructSize     = Marshal.SizeOf(ofn);
            ofn.lpstrFilter     = "YAML Files (*.yaml)\0*.yaml\0All Files (*.*)\0*.*\0";
            ofn.lpstrFile       = new string('\0', 260);
            ofn.nMaxFile        = 260;
            ofn.lpstrTitle      = "Select a redeems.yaml to import";
            ofn.lpstrInitialDir = downloadsPath;
            ofn.Flags           = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;

            if (!GetOpenFileName(ref ofn))
                return;

            try
            {
                string selectedFile = ofn.lpstrFile.TrimEnd('\0');
                string suggestedName = ProfileManager.ResolveImportProfileName(selectedFile);

                m_profileNameDialog.Show(
                    mode:          ProfileNameDialogMode.Import,
                    suggestedName: suggestedName,
                    onConfirm:     newName => HandleImportConfirm(selectedFile, newName)
                );
            }
            catch (System.Exception e)
            {
                m_profileFeedbackText.text = "Import failed: " + e.Message;
                Jotunn.Logger.LogError("Profile import failed: " + e);
            }
        }

        private string HandleImportConfirm(string selectedFile, string targetName)
        {
            try
            {
                bool imported = ProfileManager.ImportProfileTo(selectedFile, targetName, out bool profileCreated, out string error);
                if (!imported)
                    return error;

                m_profileFeedbackText.text = profileCreated
                    ? $"Created and imported profile '{targetName}'."
                    : $"Updated profile '{targetName}' with imported file.";
                Refresh();
                return null;
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Profile import failed: " + e);
                return "Import failed: " + e.Message;
            }
        }

        private void OnExportProfile()
        {
            string downloadsPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            string defaultFileName = $"{ProfileManager.ActiveProfile}_redeems.yaml";
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
                m_profileFeedbackText.text = exported
                    ? $"Exported '{ProfileManager.ActiveProfile}' to {Path.GetFileName(destPath)}."
                    : "Export failed: profile file not found.";
            }
            catch (System.Exception e)
            {
                m_profileFeedbackText.text = "Export failed: " + e.Message;
                Jotunn.Logger.LogError("Profile export failed: " + e);
            }
        }

        private void OnSyncProfile()
        {
            if (ZNet.instance == null || ZNet.instance.GetPeers().Count == 0)
            {
                m_profileFeedbackText.text = "Sync failed: no players connected.";
                return;
            }

            if (ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile))
            {
                m_profileFeedbackText.text = "Only the original owner can sync this profile.";
                return;
            }

            ProfileSyncHelper.SendActiveProfileToAll();
            m_profileFeedbackText.text = $"Profile '{ProfileManager.ActiveProfile}' synced to all online players.";
        }

        private void OnSelectProfile(string name)
        {
            ProfileManager.SelectProfile(name);
            Refresh();
        }

        private void OnReloadProfile(string name)
        {
            TwitchCustomRewards customRewards = Game.instance?.gameObject?.GetComponent<TwitchCustomRewards>();

            bool reloaded = customRewards != null
                ? customRewards.ReloadRewards()
                : RedeemHelper.Reload();

            reloaded &= ProfileSettingsHelper.Reload();

            m_profileFeedbackText.text = reloaded
                ? $"Profile '{name}' reloaded."
                : $"Failed to reload profile '{name}'.";
        }

        private void OnRenameProfile(string name)
        {
            m_profileNameDialog.Show(
                mode:          ProfileNameDialogMode.Rename,
                suggestedName: name,
                onConfirm:     newName => HandleRenameConfirm(name, newName)
            );
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

                m_profileFeedbackText.text = $"Renamed '{oldName}' to '{newName}'.";
                Refresh();
                return null;
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Profile rename failed: " + e);
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
                    m_profileFeedbackText.text = deleted
                        ? $"Profile '{name}' deleted."
                        : $"Cannot delete profile '{name}'.";
                    Refresh();
                }
            );
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            RefreshList();
        }
    }
}
