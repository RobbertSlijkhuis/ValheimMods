using System;
using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ViewersTab
    {
        private GameObject m_root;

        // List view
        private GameObject m_listView;
        private Text m_viewersLabel;
        private GameObject m_viewerListContainer;
        private Text m_listFeedbackText;
        private string m_searchText = "";

        // Create view
        private GameObject m_createView;
        private Text m_createViewTitle;
        private Text m_createFeedbackText;
        private GameObject m_editorContainer;
        private Button m_confirmButton;
        private ViewerEntry m_editingOriginal;

        private ViewerEntry m_newViewer = new ViewerEntry();
        private readonly ObjectEditor m_objectEditor = new ObjectEditor(startX: -341f, startY: -20f, fieldWidth: 320f);

        private CreateScrollableContainerDelegate m_createScrollable;

        private List<ViewerEntry> m_workingViewers = new List<ViewerEntry>();
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        private GameObject m_createScrollContent;
        private float m_editorContainerYPos;

        private const float ItemHeight          = 40f;
        private const float ItemSpacing         = 5f;
        private const float ListTopPadding      = 15f;
        private const float ActionButtonWidth   = 80f;

        private const float ColNameX    = -350f;
        private const float ColNameW    = 280f;
        private const float ColColor1X  = -60f;
        private const float ColColor1W  = 140f;
        private const float ColEffectsX = 180f;
        private const float ColEffectsW = 300f;
        private const float BtnEditX    = 380f;
        private const float BtnDeleteX  = 460f;

        // Spans the whole Edit..Delete button cluster, for the "Actions" column header.
        private const float ActionsClusterLeft   = BtnEditX - ActionButtonWidth / 2f;
        private const float ActionsClusterRight  = BtnDeleteX + ItemHeight / 2f;
        private const float ActionsClusterCenterX = (ActionsClusterLeft + ActionsClusterRight) / 2f;
        private const float ActionsClusterWidth   = ActionsClusterRight - ActionsClusterLeft;

        private const float NewViewerBtnWidth = 160f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_createScrollable = createScrollable;

            m_root = UIContainer.Create(parent, "ViewersTab");

            m_confirmDialog.Init();

            CreateListView(createScrollable);
            CreateCreateView();

            ShowListView();

            return m_root;
        }

        public void Refresh()
        {
            m_workingViewers = new List<ViewerEntry>(ExtraConfigHelper.ReadViewersConfig() ?? new List<ViewerEntry>());
            m_viewersLabel.text = "Viewers:";
            RefreshList();
        }

        // =====================================================================
        // List view
        // =====================================================================

        private void CreateListView(CreateScrollableContainerDelegate createScrollable)
        {
            var options = new TabListLayoutOptions
            {
                TitleText = "Viewers:",

                ShowSearchBar   = true,
                OnSearchChanged = OnSearchChanged,

                HeaderButtons = new List<HeaderButtonSpec>
                {
                    new HeaderButtonSpec("+ New Viewer", NewViewerBtnWidth, ShowCreateView),
                },

                ColumnHeaders = new List<ColumnHeaderSpec>
                {
                    new ColumnHeaderSpec("Name", ColNameX, ColNameW, TextAnchor.MiddleLeft),
                    new ColumnHeaderSpec("Color", ColColor1X, ColColor1W, TextAnchor.MiddleCenter),
                    new ColumnHeaderSpec("Effects", ColEffectsX, ColEffectsW, TextAnchor.MiddleLeft),
                    new ColumnHeaderSpec("Actions", ActionsClusterCenterX, ActionsClusterWidth, TextAnchor.MiddleCenter),
                },

                MainContainerName = "ViewerList",
            };

            TabListLayoutResult result = TabListLayout.Create(m_root, "ListView", createScrollable, options);

            m_listView            = result.ListView;
            m_viewersLabel        = result.TitleLabel;
            m_listFeedbackText    = result.FeedbackText;
            m_viewerListContainer = result.MainContainer;
        }

        private void RefreshList()
        {
            TabUIHelper.ClearContainer(m_viewerListContainer);

            if (m_workingViewers == null || m_workingViewers.Count == 0)
            {
                m_listFeedbackText.text = "No viewers found.";

                RectTransform emptyRt = m_viewerListContainer.GetComponent<RectTransform>();
                emptyRt.sizeDelta = new Vector2(emptyRt.sizeDelta.x, ItemHeight);
                return;
            }

            m_listFeedbackText.text = "";
            float yOffset = -(ListTopPadding + ItemHeight / 2f);

            foreach (ViewerEntry viewer in m_workingViewers)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && viewer.name.IndexOf(m_searchText, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                ViewerEntry captured = viewer;
                Color labelColor = GUIManager.Instance.ValheimBeige;

                GameObject row = new GameObject("ViewerRow");
                row.transform.SetParent(m_viewerListContainer.transform, false);

                RectTransform rowRt = row.AddComponent<RectTransform>();
                rowRt.anchorMin        = new Vector2(0f, 1f);
                rowRt.anchorMax        = new Vector2(1f, 1f);
                rowRt.pivot            = new Vector2(0.5f, 0.5f);
                rowRt.anchoredPosition = new Vector2(0f, yOffset);
                rowRt.sizeDelta        = new Vector2(0f, ItemHeight);

                Image rowBackground = row.AddComponent<Image>();
                var revealOnHover = new List<GameObject>();

                Text nameText = GUIManager.Instance.CreateText(
                    text: viewer.name,
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

                Color swatchColor = Color.white;
                if (!string.IsNullOrEmpty(viewer.color1))
                    ColorUtility.TryParseHtmlString(viewer.color1, out swatchColor);

                Text color1Text = GUIManager.Instance.CreateText(
                    text: string.IsNullOrEmpty(viewer.color1) ? "-" : viewer.color1,
                    parent: row.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(ColColor1X, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: swatchColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColColor1W,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                color1Text.alignment = TextAnchor.MiddleCenter;

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
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColEffectsW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                effectsText.alignment = TextAnchor.MiddleLeft;

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
                TabUIHelper.AddBorder(editBtn, Color.cyan);
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
                TabUIHelper.AddBorder(deleteBtn, Color.red);
                deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteViewer(captured));

                row.AddComponent<RowHoverReveal>().Init(rowBackground, revealOnHover);

                yOffset -= ItemHeight + ItemSpacing;
            }

            RectTransform contentRt = m_viewerListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ItemHeight / 2f);
        }

        private void OnDeleteViewer(ViewerEntry viewer)
        {
            m_confirmDialog.Show(
                title:       "Delete Viewer",
                description: $"Are you sure you want to delete '{viewer.name}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    m_workingViewers.Remove(viewer);
                    Save($"'{viewer.name}' removed.");
                },
                confirmText: "Delete",
                cancelText:  "Cancel"
            );
        }

        private void Save(string successMessage = "Saved!")
        {
            string path = WizshBoneTwitchIntegration.viewersPath;
            string backupPath = path + ".bak";

            try
            {
                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                List<Dictionary<string, object>> viewers = new List<Dictionary<string, object>>();

                foreach (ViewerEntry viewer in m_workingViewers)
                    viewers.Add(viewer.ToDictionary());

                Dictionary<string, object> output = new Dictionary<string, object>
                {
                    { "viewers", viewers }
                };

                ISerializer serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                using (StreamWriter writer = new StreamWriter(path, append: false))
                    serializer.Serialize(writer, output);

                RecolorHelper.ReloadViewersConfig();
                m_listFeedbackText.text = successMessage;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Failed to save viewers, restoring backup: {ex}");

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    RecolorHelper.ReloadViewersConfig();
                }

                m_listFeedbackText.text = "Save failed! Restored previous viewers file.";
            }

            RefreshList();
        }

        // =====================================================================
        // Create view
        // =====================================================================

        private void CreateCreateView()
        {
            m_createView = UIContainer.Create(m_root, "CreateView");

            m_createViewTitle = TabUIHelper.CreateTabTitle("New Viewer", m_createView, new Vector2(-200f, -153f));

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
            m_editorContainerYPos = yPos;

            m_editorContainer = TabUIHelper.CreateStaticContainer("EditorContainer", scrollContent, yPos);

            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, 0f);

            m_createView.SetActive(false);
        }

        private void RebuildEditorFields()
        {
            TabUIHelper.ClearContainer(m_editorContainer);
            float height = m_objectEditor.Build(m_editorContainer, m_newViewer);
            m_editorContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(1050f, height + 20f);
            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, height);
        }

        private void OnConfirm()
        {
            if (string.IsNullOrEmpty(m_newViewer.name))
            {
                m_createFeedbackText.text = "Name is required.";
                return;
            }

            if (m_editingOriginal != null)
            {
                int index = m_workingViewers.IndexOf(m_editingOriginal);
                if (index >= 0)
                    m_workingViewers[index] = m_newViewer;

                Save($"'{m_newViewer.name}' updated.");
                ShowListView();
            }
            else
            {
                if (m_workingViewers.Exists(v => v.name == m_newViewer.name))
                {
                    m_createFeedbackText.text = $"A viewer named '{m_newViewer.name}' already exists.";
                    return;
                }

                m_workingViewers.Add(m_newViewer);
                Save($"'{m_newViewer.name}' added.");
                ShowListView();
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
            m_editingOriginal                                           = null;
            m_newViewer                                                 = new ViewerEntry();
            m_createFeedbackText.text                                   = "";
            m_createViewTitle.text                                      = "New Viewer";
            m_confirmButton.GetComponentInChildren<Text>().text         = "+ Add";

            RebuildEditorFields();

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        private void ShowEditView(ViewerEntry viewer)
        {
            m_editingOriginal                                           = viewer;
            m_newViewer                                                 = viewer.DeepClone<ViewerEntry>();
            m_createFeedbackText.text                                   = "";
            m_createViewTitle.text                                      = "Edit Viewer";
            m_confirmButton.GetComponentInChildren<Text>().text         = "Save";

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