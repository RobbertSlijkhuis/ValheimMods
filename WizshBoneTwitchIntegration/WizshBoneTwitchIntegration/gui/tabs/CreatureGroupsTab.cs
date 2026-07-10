using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class CreatureGroupsTab
    {
        private GameObject m_root;

        // List view
        private GameObject m_listView;
        private Text m_groupsLabel;
        private GameObject m_listContainer;
        private Text m_listFeedbackText;
        private string m_searchText = "";

        // Create view
        private GameObject m_createView;
        private Text m_createViewTitle;
        private Text m_createFeedbackText;
        private GameObject m_editorContainer;
        private Button m_confirmButton;
        private Button m_toggleCreatureViewBtn;
        private bool m_showRawCreatureData;
        private CreatureGroupData m_editingOriginal;

        // Working group - list field is written into directly by ObjectEditor
        private CreatureGroupData m_newGroup = new CreatureGroupData();
        private readonly ObjectEditor m_objectEditor = new ObjectEditor(startX: -341f, startY: -20f, fieldWidth: 320f);

        private CreateScrollableContainerDelegate m_createScrollable;

        // Working copy only flushed to disk on Save
        private List<CreatureGroupData> m_workingGroups = new List<CreatureGroupData>();
        private readonly HashSet<string> m_unsavedNames = new HashSet<string>();
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        private GameObject m_createScrollContent;
        private float m_editorContainerYPos;

        private const float ItemHeight        = 40f;
        private const float ItemSpacing       = 5f;
        private const float ListTopPadding    = 15f;
        private const float ActionButtonWidth = 80f;

        private const float ColNameX    = -350f;
        private const float ColNameW    = 280f;
        private const float ColSummaryX = 60f;
        private const float ColSummaryW = 500f;
        private const float BtnEditX    = 380f;
        private const float BtnDeleteX  = 460f;

        private const float NewGroupBtnWidth      = 160f;
        private const float SaveBtnWidth          = 80f;
        private const float ToggleRawViewBtnWidth = 170f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_createScrollable = createScrollable;

            m_root = UIContainer.Create(parent, "CreatureGroupsTab");

            m_confirmDialog.Init();

            CreateListView(createScrollable);
            CreateCreateView();

            ShowListView();

            return m_root;
        }

        public void Refresh()
        {
            m_workingGroups = new List<CreatureGroupData>(RedeemHelper.creatureGroups ?? new List<CreatureGroupData>());
            m_groupsLabel.text = "Creature Groups:";
            RefreshList();
        }

        // =====================================================================
        // List view
        // =====================================================================

        private void CreateListView(CreateScrollableContainerDelegate createScrollable)
        {
            var options = new TabListLayoutOptions
            {
                TitleText = "Creature Groups:",

                ShowSearchBar   = true,
                OnSearchChanged = OnSearchChanged,

                HeaderButtons = new List<HeaderButtonSpec>
                {
                    new HeaderButtonSpec("+ New Group", NewGroupBtnWidth, ShowCreateView),
                    new HeaderButtonSpec("Save", SaveBtnWidth, OnSave, textColor: new Color(0.2f, 0.8f, 0.2f)),
                },

                MainContainerName = "CreatureGroupList",
            };

            TabListLayoutResult result = TabListLayout.Create(m_root, "ListView", createScrollable, options);

            m_listView        = result.ListView;
            m_groupsLabel      = result.TitleLabel;
            m_listFeedbackText = result.FeedbackText;
            m_listContainer    = result.MainContainer;
        }

        private void RefreshList()
        {
            TabUIHelper.ClearContainer(m_listContainer);

            if (m_workingGroups == null || m_workingGroups.Count == 0)
            {
                m_listFeedbackText.text = "No creature groups found.";

                RectTransform emptyRt = m_listContainer.GetComponent<RectTransform>();
                emptyRt.sizeDelta = new Vector2(emptyRt.sizeDelta.x, ItemHeight);
                return;
            }

            m_listFeedbackText.text = "";
            float yOffset = -(ListTopPadding + ItemHeight / 2f);

            foreach (CreatureGroupData group in m_workingGroups)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && (group.group == null || group.group.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;

                CreatureGroupData captured = group;
                bool isUnsaved = m_unsavedNames.Contains(group.group);
                Color labelColor = isUnsaved ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige;

                GameObject row = new GameObject("GroupRow");
                row.transform.SetParent(m_listContainer.transform, false);

                RectTransform rowRt = row.AddComponent<RectTransform>();
                rowRt.anchorMin        = new Vector2(0f, 1f);
                rowRt.anchorMax        = new Vector2(1f, 1f);
                rowRt.pivot            = new Vector2(0.5f, 0.5f);
                rowRt.anchoredPosition = new Vector2(0f, yOffset);
                rowRt.sizeDelta        = new Vector2(0f, ItemHeight);

                Image rowBackground = row.AddComponent<Image>();
                var revealOnHover = new List<GameObject>();

                Text nameText = GUIManager.Instance.CreateText(
                    text: group.group,
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(ColNameX, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColNameW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                nameText.alignment = TextAnchor.MiddleLeft;

                string summaryLabel = group.list != null && group.list.Count > 0
                    ? $"{group.list.Count} creature(s): " + string.Join(", ", group.list.Select(c => FieldUIBuilder.GetCreatureDisplayName(c.prefabName)))
                    : "0 creatures";

                Text summaryText = GUIManager.Instance.CreateText(
                    text: summaryLabel,
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(ColSummaryX, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColSummaryW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                summaryText.alignment = TextAnchor.MiddleLeft;

                GameObject editBtn = GUIManager.Instance.CreateButton(
                    text: "Edit",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnEditX, 0f),
                    width: ActionButtonWidth,
                    height: ItemHeight
                );
                editBtn.GetComponentInChildren<Text>().color = Color.cyan;
                editBtn.GetComponent<Button>().onClick.AddListener(() => ShowEditView(captured));
                revealOnHover.Add(editBtn);

                GameObject deleteBtn = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(BtnDeleteX, 0f),
                    width: ItemHeight,
                    height: ItemHeight
                );
                deleteBtn.SetActive(true);
                deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteGroup(captured));

                row.AddComponent<RowHoverReveal>().Init(rowBackground, revealOnHover);

                yOffset -= ItemHeight + ItemSpacing;
            }

            RectTransform contentRt = m_listContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ItemHeight / 2f);
        }

        private void OnDeleteGroup(CreatureGroupData group)
        {
            m_confirmDialog.Show(
                title:       "Delete Creature Group",
                description: $"Are you sure you want to delete '{group.group}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    m_workingGroups.Remove(group);
                    m_unsavedNames.Remove(group.group);
                    m_listFeedbackText.text = $"'{group.group}' removed. Press Save to apply.";
                    RefreshList();
                },
                confirmText: "Delete",
                cancelText:  "Cancel"
            );
        }

        private void OnSave()
        {
            string path = ProfileManager.GetActiveRedeemPath();
            string backupPath = path + ".bak";
            HashSet<string> unsavedNamesBackup = new HashSet<string>(m_unsavedNames);

            try
            {
                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();

                ExtraConfigHelper.WriteRedeemsConfig(path, m_workingGroups, data.redeems);

                RedeemHelper.Reload();
                m_unsavedNames.Clear();
                m_listFeedbackText.text = "Saved!";
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Failed to save creature groups, restoring backup: {ex}");

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    RedeemHelper.Reload();
                }

                m_unsavedNames.Clear();
                foreach (string name in unsavedNamesBackup)
                    m_unsavedNames.Add(name);

                m_listFeedbackText.text = "Save failed! Restored previous creature groups file.";
            }

            RefreshList();
        }

        // =====================================================================
        // Create view
        // =====================================================================

        private void CreateCreateView()
        {
            m_createView = UIContainer.Create(m_root, "CreateView");

            m_createViewTitle = TabUIHelper.CreateTabTitle("New Creature Group", m_createView, new Vector2(-200f, -153f));

            m_createFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_createView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-100f, -158f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject backBtn = GUIManager.Instance.CreateButton(
                text: "< Back",
                parent: m_createView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(165f, -145f),
                width: 120f,
                height: 40f
            );
            backBtn.SetActive(true);
            backBtn.GetComponent<Button>().onClick.AddListener(ShowListView);

            GameObject confirmBtnObj = GUIManager.Instance.CreateButton(
                text: "+ Add",
                parent: m_createView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(290f, -145f),
                width: 120f,
                height: 40f
            );
            confirmBtnObj.SetActive(true);
            m_confirmButton = confirmBtnObj.GetComponent<Button>();
            m_confirmButton.onClick.AddListener(OnConfirm);

            GameObject scrollContent = m_createScrollable("CreateViewContent", m_createView, -175f);
            m_createScrollContent = scrollContent;

            float yPos = -20f;

            // Toggles between a curated subset of CreatureData fields per list entry
            // (CreatureSimpleView) and every field - always relevant here since the group's
            // entire editable content is its creature list.
            GameObject toggleRow = TabUIHelper.CreateStaticContainer("ToggleRow", scrollContent, yPos);

            Text toggleLabelComp = GUIManager.Instance.CreateText(
                text: "Creature fields",
                parent: toggleRow.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-341f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 210f,
                height: 36f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            toggleLabelComp.alignment = TextAnchor.MiddleLeft;

            GameObject toggleBtnObj = GUIManager.Instance.CreateButton(
                text: "Show full data",
                parent: toggleRow.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-341f + 210f / 2f + 10f + ToggleRawViewBtnWidth / 2f, 0f),
                width: ToggleRawViewBtnWidth,
                height: 36f
            );
            toggleBtnObj.SetActive(true);
            m_toggleCreatureViewBtn = toggleBtnObj.GetComponent<Button>();
            m_toggleCreatureViewBtn.onClick.AddListener(OnToggleCreatureView);

            toggleRow.GetComponent<RectTransform>().sizeDelta = new Vector2(1050f, 36f);

            yPos -= 70f;
            m_editorContainerYPos = yPos;

            m_editorContainer = TabUIHelper.CreateStaticContainer("EditorContainer", scrollContent, yPos);

            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, 0f);

            m_createView.SetActive(false);
        }

        private void RebuildEditorFields()
        {
            TabUIHelper.ClearContainer(m_editorContainer);
            float height = m_objectEditor.Build(m_editorContainer, m_newGroup, simpleMode: !m_showRawCreatureData);
            m_editorContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(1050f, height + 20f);
            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, height);
        }

        private void OnToggleCreatureView()
        {
            m_showRawCreatureData = !m_showRawCreatureData;
            m_toggleCreatureViewBtn.GetComponentInChildren<Text>().text = m_showRawCreatureData
                ? "Show simple fields"
                : "Show full data";
            RebuildEditorFields();
        }

        private void OnConfirm()
        {
            if (string.IsNullOrEmpty(m_newGroup.group))
            {
                m_createFeedbackText.text = "Group name is required.";
                return;
            }

            if (m_editingOriginal != null)
            {
                int index = m_workingGroups.IndexOf(m_editingOriginal);
                if (index >= 0)
                    m_workingGroups[index] = m_newGroup;

                m_unsavedNames.Add(m_newGroup.group);
                ShowListView();
                m_listFeedbackText.text = $"'{m_newGroup.group}' updated (unsaved). Press Save to persist.";
            }
            else
            {
                if (m_workingGroups.Exists(g => g.group == m_newGroup.group))
                {
                    m_createFeedbackText.text = $"A creature group named '{m_newGroup.group}' already exists.";
                    return;
                }

                m_workingGroups.Add(m_newGroup);
                m_unsavedNames.Add(m_newGroup.group);
                ShowListView();
                m_listFeedbackText.text = $"'{m_newGroup.group}' added (unsaved). Press Save to persist.";
            }
        }

        // =====================================================================
        // View switching
        // =====================================================================

        private void ShowListView()
        {
            m_editingOriginal = null;
            RefreshList();
            m_createView.SetActive(false);
            m_listView.SetActive(true);
        }

        private void ShowCreateView()
        {
            m_editingOriginal                                   = null;
            m_newGroup                                           = new CreatureGroupData();
            m_showRawCreatureData                                = false;
            m_createFeedbackText.text                            = "";
            m_createViewTitle.text                               = "New Creature Group";
            m_confirmButton.GetComponentInChildren<Text>().text = "+ Add";
            m_toggleCreatureViewBtn.GetComponentInChildren<Text>().text = "Show full data";

            RebuildEditorFields();

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        private void ShowEditView(CreatureGroupData group)
        {
            m_editingOriginal                                   = group;
            m_newGroup                                           = group.DeepClone<CreatureGroupData>();
            m_showRawCreatureData                                = false;
            m_createFeedbackText.text                            = "";
            m_createViewTitle.text                               = "Edit Creature Group";
            m_confirmButton.GetComponentInChildren<Text>().text = "Save";
            m_toggleCreatureViewBtn.GetComponentInChildren<Text>().text = "Show full data";

            RebuildEditorFields();

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            RefreshList();
        }
    }
}
