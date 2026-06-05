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
        private InputField m_profileNameInput;
        private GameObject m_profileListContainer;
        private Text m_profileFeedbackText;
        private Button m_syncButton;
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        private const float ButtonWidth       = 330f;
        private const float ButtonHeight      = 40f;
        private const float ActiveButtonHeight = 56f;
        private const float ButtonSpacing     = 5f;
        private const float ButtonCenterX     = -35f;

        private const float ListTopPadding   = 15f;
        private const float HeaderTopPadding = 30f;
        private const float ContentTopY      = -(108f + HeaderTopPadding);

        private const float ScrollViewLeft    = ButtonCenterX - ButtonWidth / 2f;
        private const float InputFieldCenterX = ScrollViewLeft + ButtonWidth / 2f;
        private const float ActionBtnWidth    = 80f;
        private const float CreateBtnCenterX  = ScrollViewLeft + ButtonWidth + ButtonSpacing + ActionBtnWidth / 2f;

        private const float ScrollViewRight  = 350f;
        private const float BottomBtnHeight  = 60f;
        private const float BottomBtnY       = 50f;
        private const float SyncBtnBottomX   = ScrollViewRight - ActionBtnWidth / 2f;
        private const float ExportBtnBottomX = SyncBtnBottomX - ActionBtnWidth - ButtonSpacing;
        private const float ImportBtnBottomX = ExportBtnBottomX - ActionBtnWidth - ButtonSpacing;
        private const float CopyBtnBottomX   = ImportBtnBottomX - ActionBtnWidth - ButtonSpacing;

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

            TabUIHelper.CreateTabTitle("New profile name:", m_root, new Vector2(InputFieldCenterX, ContentTopY), width: ButtonWidth);

            m_profileNameInput = FieldUIBuilder.CreateInputField(m_root, new Vector2(InputFieldCenterX, ContentTopY - 30f), ButtonWidth);

            GameObject createBtn = GUIManager.Instance.CreateButton(
                text: "Create",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(CreateBtnCenterX, ContentTopY - 30f),
                width: ActionBtnWidth,
                height: ButtonHeight
            );
            createBtn.SetActive(true);
            createBtn.GetComponent<Button>().onClick.AddListener(OnCreateProfile);

            GameObject copyBtn = GUIManager.Instance.CreateButton(
                text: "Copy",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(CopyBtnBottomX, BottomBtnY),
                width: ActionBtnWidth,
                height: BottomBtnHeight
            );
            copyBtn.SetActive(true);
            copyBtn.GetComponent<Button>().onClick.AddListener(OnCopyProfile);

            GameObject importBtn = GUIManager.Instance.CreateButton(
                text: "Import",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(ImportBtnBottomX, BottomBtnY),
                width: ActionBtnWidth,
                height: BottomBtnHeight
            );
            importBtn.SetActive(true);
            importBtn.GetComponent<Button>().onClick.AddListener(OnImportProfile);

            GameObject exportBtn = GUIManager.Instance.CreateButton(
                text: "Export",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(ExportBtnBottomX, BottomBtnY),
                width: ActionBtnWidth,
                height: BottomBtnHeight
            );
            exportBtn.SetActive(true);
            exportBtn.GetComponent<Button>().onClick.AddListener(OnExportProfile);

            GameObject syncBtnObj = GUIManager.Instance.CreateButton(
                text: "Sync",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(SyncBtnBottomX, BottomBtnY),
                width: ActionBtnWidth,
                height: BottomBtnHeight
            );
            syncBtnObj.SetActive(true);
            m_syncButton = syncBtnObj.GetComponent<Button>();
            m_syncButton.onClick.AddListener(OnSyncProfile);

            m_profileFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(ScrollViewLeft + 300f, ContentTopY - 60f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 600f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            TabUIHelper.CreateTabTitle("Available profiles:", m_root, new Vector2(-110f, ContentTopY - 87f), width: 180f);

            m_profileListContainer = createScrollable("ProfileList", m_root, ContentTopY - 107f);

            return m_root;
        }

        public void Refresh()
        {
            TabUIHelper.ClearContainer(m_profileListContainer);

            if (m_syncButton != null)
                m_syncButton.interactable = !ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile);

            List<string> profiles = ProfileManager.GetProfiles();
            float yOffset = -(ListTopPadding + ButtonHeight / 2f);

            foreach (string profile in profiles)
            {
                bool isActive = profile == ProfileManager.ActiveProfile;
                string profileName = profile;
                float btnHeight = isActive ? ActiveButtonHeight : ButtonHeight;

                yOffset -= isActive ? (ActiveButtonHeight - ButtonHeight) / 2f : 0f;

                GameObject btn = GUIManager.Instance.CreateButton(
                    text: profile,
                    parent: m_profileListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(ButtonCenterX, yOffset),
                    width: ButtonWidth,
                    height: btnHeight
                );
                btn.SetActive(true);
                btn.GetComponentInChildren<Text>().color = isActive
                    ? GUIManager.Instance.ValheimOrange
                    : GUIManager.Instance.ValheimBeige;

                if (isActive)
                {
                    GameObject reloadBtn = GUIManager.Instance.CreateButton(
                        text: "↺",
                        parent: m_profileListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(ButtonCenterX + ButtonWidth / 2f + ButtonHeight / 2f + ButtonSpacing, yOffset),
                        width: ButtonHeight,
                        height: btnHeight
                    );
                    reloadBtn.SetActive(true);
                    reloadBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimOrange;
                    reloadBtn.GetComponent<Button>().onClick.AddListener(() => OnReloadProfile(profileName));
                }
                else
                {
                    btn.GetComponent<Button>().onClick.AddListener(() => OnSelectProfile(profileName));

                    GameObject deleteBtn = GUIManager.Instance.CreateButton(
                        text: "X",
                        parent: m_profileListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(ButtonCenterX + ButtonWidth / 2f + ButtonHeight / 2f + ButtonSpacing, yOffset),
                        width: ButtonHeight,
                        height: ButtonHeight
                    );
                    deleteBtn.SetActive(true);
                    deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                    deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteProfile(profileName));
                }

                yOffset -= btnHeight + ButtonSpacing;
                if (isActive) yOffset += (ActiveButtonHeight - ButtonHeight) / 2f;
            }

            RectTransform contentRt = m_profileListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ButtonHeight / 2f);
        }

        private void OnCreateProfile()
        {
            if (m_profileNameInput == null)
                return;

            string name = m_profileNameInput.text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                m_profileFeedbackText.text = "Please enter a profile name.";
                return;
            }

            bool created = ProfileManager.CreateProfile(name);
            m_profileFeedbackText.text = created
                ? $"Profile '{name}' created!"
                : $"Profile '{name}' already exists.";

            m_profileNameInput.text = "";
            Refresh();
        }

        private void OnCopyProfile()
        {
            bool copied = ProfileManager.CopyProfile(out string newName);
            m_profileFeedbackText.text = copied
                ? $"Copied '{ProfileManager.ActiveProfile}' to '{newName}'."
                : "Copy failed: profile file not found.";

            Refresh();
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
                bool imported = ProfileManager.ImportProfile(selectedFile, out string targetProfile, out bool profileCreated);

                if (!imported)
                {
                    m_profileFeedbackText.text = "Import failed: selected file not found.";
                    return;
                }

                m_profileFeedbackText.text = profileCreated
                    ? $"Created and imported profile '{targetProfile}'."
                    : $"Updated profile '{targetProfile}' with imported file.";

                Refresh();
            }
            catch (System.Exception e)
            {
                m_profileFeedbackText.text = "Import failed: " + e.Message;
                Jotunn.Logger.LogError("Profile import failed: " + e);
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

            m_profileFeedbackText.text = reloaded
                ? $"Profile '{name}' reloaded."
                : $"Failed to reload profile '{name}'.";
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
    }
}