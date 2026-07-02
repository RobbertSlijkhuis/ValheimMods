using System;
using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Models.Views;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class RedeemsTab
    {
        private GameObject m_root;

        // List view
        private GameObject m_listView;
        private Text m_redeemsLabel;
        private GameObject m_redeemListContainer;
        private Text m_listFeedbackText;
        private string m_searchText = "";
        private Button m_addNewBtn;
        private Button m_saveBtn;

        // Create view
        private GameObject m_createView;
        private SearchableDropdown m_typeDropdown;
        private SearchableDropdown m_variantDropdown;
        private GameObject m_variantRow;
        private Button m_toggleRawViewBtn;
        private bool m_showRawSpawnAbilityData;
        private Text m_createFeedbackText;
        private GameObject m_standardFieldsContainer;
        private GameObject m_editorContainer;
        private Button m_confirmButton;
        private RedeemData m_editingOriginal;
        private bool m_isReadOnly;

        // Working redeem � sub-data objects are written into directly by ObjectEditor
        private RedeemData m_newRedeem = new RedeemData();
        private readonly ObjectEditor m_objectEditor = new ObjectEditor(startX: -341f, startY: -20f, fieldWidth: 320f);

        // Stored so CreateCreateView can create a scrollable editor container
        private CreateScrollableContainerDelegate m_createScrollable;

        // Working copy only flushed to disk on Save
        private List<RedeemData> m_workingRedeems = new List<RedeemData>();
        private readonly HashSet<string> m_unsavedTitles = new HashSet<string>();

        private static readonly string[] RedeemTypes = new[]
        {
            RedeemType.Undefined,
            RedeemType.Detonate,
            RedeemType.Flashbang,
            RedeemType.Mist,
            RedeemType.SpawnAbility,
            RedeemType.SpawnCreature,
            RedeemType.StatusEffect,
            RedeemType.SurpriseChest,
            RedeemType.TerrainEdit,
            RedeemType.TimeStop,
            RedeemType.Weather,
        };

        // Only shown when type == RedeemType.SpawnAbility - picks which narrowed view
        // (if any) ObjectEditor renders for the shared SpawnAbilityData sub-object.
        private static readonly string[] RedeemVariants = new[]
        {
            RedeemVariant.None,
            RedeemVariant.Door,
            RedeemVariant.Windmill,
            RedeemVariant.Smite,
            RedeemVariant.Rain,
            RedeemVariant.Meteor,
            RedeemVariant.Trap,
            RedeemVariant.Root,
        };

        private const float ButtonHeight        = 40f;
        private const float ButtonSpacing       = 5f;
        private const float ItemHeight          = 40f;
        private const float ListTopPadding      = 15f;
        private const float HeaderTopPadding    = 45f;
        private const float ActionButtonWidth   = 80f;

        private const float ColToggleX = -455f;
        private const float ColToggleW = 44f;
        private const float ColTitleX  = -295f;
        private const float ColTitleW  = 220f;
        private const float ColTypeX   = -80f;
        private const float ColTypeW   = 160f;
        private const float ColCostX   = 80f;
        private const float ColCostW   = 120f;

        private const float BtnTestX   = 220f;
        private const float BtnEditX   = BtnTestX + ActionButtonWidth / 2f + ButtonSpacing + ActionButtonWidth / 2f;
        private const float CopyBtnWidth = 70f;
        private const float BtnCopyX   = BtnEditX + ActionButtonWidth / 2f + ButtonSpacing + CopyBtnWidth / 2f;
        private const float BtnDeleteX = BtnCopyX + CopyBtnWidth / 2f + ButtonSpacing + ButtonHeight / 2f;

        private const float ListLeftEdgeX        = -480f;
        private const float ContentTopY          = -(110f + HeaderTopPadding);
        private const float RedeemLabelY         = ContentTopY - 53f;
        private const float ScrollTopOffset      = ContentTopY - 71f;
        private const float SearchWidth          = 560f;
        private const float NewRedeemBtnWidth    = 160f;
        private const float SaveBtnWidth         = 80f;
        private const float SearchCenterX        = ListLeftEdgeX + SearchWidth / 2f;
        private const float NewRedeemBtnCenterX  = SearchCenterX + SearchWidth / 2f + ButtonSpacing + NewRedeemBtnWidth / 2f;
        private const float SaveBtnCenterX       = NewRedeemBtnCenterX + NewRedeemBtnWidth / 2f + ButtonSpacing + SaveBtnWidth / 2f;

        private const float TypeLabelX    = -341f;
        private const float TypeLabelW    = 210f;
        private const float TypeDropdownW = 320f;
        private const float TypeDropdownX = TypeLabelX + TypeLabelW / 2f + 10f + TypeDropdownW / 2f;
        private const float ToggleRawViewBtnWidth = 170f;

        private System.Action m_onCloseRequested;
        private System.Action m_onOpenHistory;

        // Create view title � stored to allow "New Redeem" / "Edit Redeem" / "View Redeem" switching
        private Text m_createViewTitle;
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();
        private readonly CopyRedeemDialog m_copyRedeemDialog = new CopyRedeemDialog();

        // Stored to allow scroll content resize on type change
        private GameObject m_createScrollContent;
        private float m_editorContainerYPos;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable, System.Action onCloseRequested, System.Action onOpenHistory = null)
        {
            m_onCloseRequested = onCloseRequested;
            m_onOpenHistory = onOpenHistory;
            m_createScrollable = createScrollable;

            m_root = UIContainer.Create(parent, "RedeemsTab");

            m_confirmDialog.Init();
            m_copyRedeemDialog.Init();

            CreateListView(createScrollable);
            CreateCreateView();

            ShowListView();

            return m_root;
        }

        public void Refresh()
        {
            m_workingRedeems = new List<RedeemData>(RedeemHelper.redeems);
            m_redeemsLabel.text = $"Redeems - {ProfileManager.ActiveProfile}:";

            bool isSynced = ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile);
            if (m_addNewBtn != null) m_addNewBtn.interactable = !isSynced;
            if (m_saveBtn != null)   m_saveBtn.interactable   = !isSynced;

            RefreshList();
        }

        // =====================================================================
        // List view
        // =====================================================================

        private void CreateListView(CreateScrollableContainerDelegate createScrollable)
        {
            m_listView = UIContainer.Create(m_root, "ListView");

            InputField searchField = FieldUIBuilder.CreateInputField(
                parent: m_listView,
                position: new Vector2(SearchCenterX, ContentTopY),
                width: SearchWidth
            );
            searchField.placeholder.GetComponent<Text>().text = "Search redeems...";
            searchField.onValueChanged.AddListener(OnSearchChanged);

            GameObject addNewBtnObj = GUIManager.Instance.CreateButton(
                text: "+ New Redeem",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(NewRedeemBtnCenterX, ContentTopY),
                width: NewRedeemBtnWidth,
                height: 36f
            );
            addNewBtnObj.SetActive(true);
            m_addNewBtn = addNewBtnObj.GetComponent<Button>();
            m_addNewBtn.onClick.AddListener(ShowCreateView);

            GameObject saveBtnObj = GUIManager.Instance.CreateButton(
                text: "Save",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(SaveBtnCenterX, ContentTopY),
                width: SaveBtnWidth,
                height: 36f
            );
            saveBtnObj.SetActive(true);
            saveBtnObj.GetComponentInChildren<Text>().color = new Color(0.2f, 0.8f, 0.2f);
            m_saveBtn = saveBtnObj.GetComponent<Button>();
            m_saveBtn.onClick.AddListener(OnSave);

            m_redeemsLabel = TabUIHelper.CreateTabTitle(
                $"Redeems - {ProfileManager.ActiveProfile}:",
                m_listView,
                new Vector2(-150f, RedeemLabelY),
                width: 400f
            );

            m_listFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(50f, RedeemLabelY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            m_redeemListContainer = createScrollable("RedeemList", m_listView, ScrollTopOffset);
        }

        private void RefreshList()
        {
            TabUIHelper.ClearContainer(m_redeemListContainer);

            bool isSynced = ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile);
            float yOffset = -(ListTopPadding + ItemHeight / 2f);

            foreach (RedeemData redeem in m_workingRedeems)
            {
                if (!string.IsNullOrEmpty(m_searchText)
                    && redeem.title.IndexOf(m_searchText, System.StringComparison.OrdinalIgnoreCase) < 0
                    && redeem.type.IndexOf(m_searchText, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                RedeemData captured = redeem;
                bool isUnsaved = m_unsavedTitles.Contains(redeem.title);
                Color labelColor = isUnsaved ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige;

                if (!isSynced)
                {
                    GameObject toggleBtn = GUIManager.Instance.CreateButton(
                        text: captured.enabled ? "On" : "Off",
                        parent: m_redeemListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(ColToggleX, yOffset),
                        width: ColToggleW,
                        height: ButtonHeight
                    );
                    toggleBtn.SetActive(true);
                    toggleBtn.GetComponentInChildren<Text>().color = captured.enabled
                        ? new Color(0.2f, 0.8f, 0.2f)
                        : new Color(0.8f, 0.2f, 0.2f);
                    toggleBtn.GetComponent<Button>().onClick.AddListener(() => OnToggleRedeem(captured));
                }

                Text titleText = GUIManager.Instance.CreateText(
                    text: redeem.title,
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(ColTitleX, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColTitleW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                titleText.alignment = TextAnchor.MiddleLeft;

                Text typeText = GUIManager.Instance.CreateText(
                    text: $"[{redeem.type}]",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(ColTypeX, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColTypeW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                typeText.alignment = TextAnchor.MiddleCenter;

                Text costText = GUIManager.Instance.CreateText(
                    text: $"{redeem.points} pts",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(ColCostX, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: labelColor,
                    outline: true,
                    outlineColor: Color.black,
                    width: ColCostW,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                costText.alignment = TextAnchor.MiddleRight;

                GameObject testBtn = GUIManager.Instance.CreateButton(
                    text: "Test",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(BtnTestX, yOffset),
                    width: ActionButtonWidth,
                    height: ButtonHeight
                );
                testBtn.SetActive(true);
                testBtn.GetComponentInChildren<Text>().color = Color.yellow;
                testBtn.GetComponent<Button>().onClick.AddListener(() => OnTestRedeem(captured));

                if (isSynced)
                {
                    // Synced profile � show read-only view, no delete
                    GameObject showBtn = GUIManager.Instance.CreateButton(
                        text: "Show",
                        parent: m_redeemListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(BtnEditX, yOffset),
                        width: ActionButtonWidth,
                        height: ButtonHeight
                    );
                    showBtn.SetActive(true);
                    showBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimBeige;
                    showBtn.GetComponent<Button>().onClick.AddListener(() => ShowViewOnlyView(captured));
                }
                else
                {
                    GameObject editBtn = GUIManager.Instance.CreateButton(
                        text: "Edit",
                        parent: m_redeemListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(BtnEditX, yOffset),
                        width: ActionButtonWidth,
                        height: ButtonHeight
                    );
                    editBtn.SetActive(true);
                    editBtn.GetComponentInChildren<Text>().color = Color.cyan;
                    editBtn.GetComponent<Button>().onClick.AddListener(() => ShowEditView(captured));
                }

                GameObject copyBtn = GUIManager.Instance.CreateButton(
                    text: "Copy",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(BtnCopyX, yOffset),
                    width: CopyBtnWidth,
                    height: ButtonHeight
                );
                copyBtn.SetActive(true);
                copyBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimBeige;
                copyBtn.GetComponent<Button>().onClick.AddListener(() => OnCopyRedeem(captured));

                if (!isSynced)
                {
                    GameObject deleteBtn = GUIManager.Instance.CreateButton(
                        text: "X",
                        parent: m_redeemListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(BtnDeleteX, yOffset),
                        width: ButtonHeight,
                        height: ButtonHeight
                    );
                    deleteBtn.SetActive(true);
                    deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                    deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteRedeem(captured));
                }

                yOffset -= ItemHeight + ButtonSpacing;
            }

            RectTransform contentRt = m_redeemListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ItemHeight / 2f);
        }

        private void OnDeleteRedeem(RedeemData redeem)
        {
            m_confirmDialog.Show(
                title:       "Delete Redeem",
                description: $"Are you sure you want to delete '{redeem.title}'?\nThis cannot be undone.",
                onConfirm:   () =>
                {
                    m_workingRedeems.Remove(redeem);
                    m_unsavedTitles.Remove(redeem.title);
                    RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);
                    m_listFeedbackText.text = $"'{redeem.title}' removed. Press Save to apply.";
                    RefreshList();
                },
                confirmText: "Delete",
                cancelText:  "Cancel"
            );
        }

        private void OnCopyRedeem(RedeemData redeem)
        {
            m_copyRedeemDialog.Show(
                redeemTitle:   redeem.title,
                profiles:      ProfileManager.GetProfiles(),
                defaultProfile: ProfileManager.ActiveProfile,
                suggestedName: $"{redeem.title} - copy",
                onConfirm:     (targetProfile, newTitle) => HandleCopyRedeemConfirm(redeem, targetProfile, newTitle)
            );
        }

        private string HandleCopyRedeemConfirm(RedeemData redeem, string targetProfile, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return "Please enter a name.";

            if (ProfileManager.IsSyncedProfile(targetProfile))
                return $"Cannot copy into '{targetProfile}' - it is a synced (read-only) profile.";

            if (targetProfile == ProfileManager.ActiveProfile)
            {
                if (m_workingRedeems.Exists(r => r.title == newTitle))
                    return $"A redeem named '{newTitle}' already exists in '{targetProfile}'.";

                RedeemData copy = redeem.DeepClone<RedeemData>();
                copy.title = newTitle;

                m_workingRedeems.Add(copy);
                m_unsavedTitles.Add(newTitle);
                RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);
                RefreshList();
                m_listFeedbackText.text = $"Copied '{redeem.title}' to '{newTitle}' (unsaved). Press Save to persist.";
                return null;
            }

            bool copied = ProfileManager.CopyRedeemToOtherProfile(redeem, targetProfile, newTitle, out string error);
            if (copied)
                m_listFeedbackText.text = $"Copied '{redeem.title}' to '{targetProfile}' as '{newTitle}'.";

            return copied ? null : error;
        }

        private void OnSave()
        {
            string path = ProfileManager.GetActiveRedeemPath();
            string backupPath = path + ".bak";
            HashSet<string> unsavedTitlesBackup = new HashSet<string>(m_unsavedTitles);

            try
            {
                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();

                ExtraConfigHelper.WriteRedeemsConfig(path, data.creatureGroups, m_workingRedeems);

                RedeemHelper.Reload();
                m_unsavedTitles.Clear();
                m_listFeedbackText.text = "Saved!";
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Failed to save redeems, restoring backup: {ex}");

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    RedeemHelper.Reload();
                }

                m_unsavedTitles.Clear();
                foreach (string title in unsavedTitlesBackup)
                    m_unsavedTitles.Add(title);

                m_listFeedbackText.text = "Save failed! Restored previous redeems file.";
            }

            RefreshList();
        }

        // =====================================================================
        // Create view
        // =====================================================================

        private void CreateCreateView()
        {
            m_createView = UIContainer.Create(m_root, "CreateView");

            m_createViewTitle = TabUIHelper.CreateTabTitle(
                "New Redeem",
                m_createView,
                new Vector2(-200f, -153f)
            );

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

            m_standardFieldsContainer = TabUIHelper.CreateStaticContainer("StandardFields", scrollContent, yPos);
            yPos -= 460f;

            Text typeLabelComp = GUIManager.Instance.CreateText(
                text: "Type",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(TypeLabelX, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: TypeLabelW,
                height: 36f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            typeLabelComp.alignment = TextAnchor.MiddleLeft;

            m_typeDropdown = new SearchableDropdown();
            m_typeDropdown.Build(scrollContent, new Vector2(TypeDropdownX, yPos), TypeDropdownW, 36f, new List<string>(RedeemTypes), RedeemTypes[0]);
            m_typeDropdown.OnValueChanged += _ => OnTypeChanged();

            yPos -= 70f;

            m_variantRow = TabUIHelper.CreateStaticContainer("VariantRow", scrollContent, yPos);

            Text variantLabelComp = GUIManager.Instance.CreateText(
                text: "Variant",
                parent: m_variantRow.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(TypeLabelX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: TypeLabelW,
                height: 36f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            variantLabelComp.alignment = TextAnchor.MiddleLeft;

            m_variantDropdown = new SearchableDropdown();
            m_variantDropdown.Build(m_variantRow, new Vector2(TypeDropdownX, 0f), TypeDropdownW, 36f, new List<string>(RedeemVariants), RedeemVariants[0]);
            m_variantDropdown.OnValueChanged += _ =>
            {
                m_newRedeem.variant = m_variantDropdown.Value;
                OnTypeChanged();
            };

            GameObject toggleRawViewBtnObj = GUIManager.Instance.CreateButton(
                text: "Show full data",
                parent: m_variantRow.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(TypeDropdownX + TypeDropdownW / 2f + ButtonSpacing + ToggleRawViewBtnWidth / 2f, 0f),
                width: ToggleRawViewBtnWidth,
                height: 36f
            );
            toggleRawViewBtnObj.SetActive(true);
            m_toggleRawViewBtn = toggleRawViewBtnObj.GetComponent<Button>();
            m_toggleRawViewBtn.onClick.AddListener(OnToggleRawView);

            m_variantRow.GetComponent<RectTransform>().sizeDelta = new Vector2(1050f, 36f);
            m_variantRow.SetActive(false);

            yPos -= 70f;
            m_editorContainerYPos = yPos;

            m_editorContainer = TabUIHelper.CreateStaticContainer("EditorContainer", scrollContent, yPos);

            // Initial scroll content height � editor is empty at start
            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, 0f);

            m_createView.SetActive(false);
        }

        private void OnTypeChanged()
        {
            m_newRedeem.type = m_typeDropdown.Value;
            TabUIHelper.ClearContainer(m_editorContainer);

            bool isSpawnAbility = m_newRedeem.type == RedeemType.SpawnAbility;
            m_variantRow.SetActive(isSpawnAbility);
            if (!isSpawnAbility)
                m_newRedeem.variant = RedeemVariant.None;

            bool hasVariant = isSpawnAbility && m_newRedeem.variant != RedeemVariant.None;
            m_toggleRawViewBtn.gameObject.SetActive(hasVariant);
            if (!hasVariant)
                m_showRawSpawnAbilityData = false;

            float editorHeight = 0f;

            if (m_newRedeem.type == RedeemType.Detonate)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.detonateData);
            else if (m_newRedeem.type == RedeemType.Flashbang)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.flashbangData);
            else if (m_newRedeem.type == RedeemType.Mist)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.mistData);
            else if (m_newRedeem.type == RedeemType.TerrainEdit)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.terrainEditData);
            else if (m_newRedeem.type == RedeemType.StatusEffect)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.statusEffectData);
            else if (m_newRedeem.type == RedeemType.TimeStop)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.timeStopData);
            else if (m_newRedeem.type == RedeemType.Weather)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.weatherData);
            else if (isSpawnAbility)
            {
                object spawnAbilityView;
                if (!hasVariant || m_showRawSpawnAbilityData)
                    spawnAbilityView = m_newRedeem.spawnAbilityData;
                else if (m_newRedeem.variant == RedeemVariant.Door)
                    spawnAbilityView = new DoorView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Windmill)
                    spawnAbilityView = new WindmillView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Smite)
                    spawnAbilityView = new SmiteView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Rain)
                    spawnAbilityView = new RainView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Meteor)
                    spawnAbilityView = new MeteorView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Trap)
                    spawnAbilityView = new TrapView(m_newRedeem.spawnAbilityData);
                else if (m_newRedeem.variant == RedeemVariant.Root)
                    spawnAbilityView = new RootView(m_newRedeem.spawnAbilityData);
                else
                    spawnAbilityView = m_newRedeem.spawnAbilityData;

                editorHeight = m_objectEditor.Build(m_editorContainer, spawnAbilityView);
            }

            RectTransform editorRt = m_editorContainer.GetComponent<RectTransform>();
            editorRt.sizeDelta = new Vector2(editorRt.sizeDelta.x, editorHeight + 20f);

            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, editorHeight);

            if (m_isReadOnly)
            {
                SetContainerInteractable(m_editorContainer, false);
                SetContainerInteractable(m_standardFieldsContainer, false);
            }
        }

        private void OnToggleRawView()
        {
            m_showRawSpawnAbilityData = !m_showRawSpawnAbilityData;
            m_toggleRawViewBtn.GetComponentInChildren<Text>().text = m_showRawSpawnAbilityData
                ? "Show variant fields"
                : "Show full data";
            OnTypeChanged();
        }

        private void OnConfirm()
        {
            if (string.IsNullOrEmpty(m_newRedeem.title))
            {
                m_createFeedbackText.text = "Title is required.";
                return;
            }

            if (m_editingOriginal != null)
            {
                int index = m_workingRedeems.IndexOf(m_editingOriginal);
                if (index >= 0)
                    m_workingRedeems[index] = m_newRedeem;

                m_unsavedTitles.Add(m_newRedeem.title);
                RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);
                ShowListView();
                m_listFeedbackText.text = $"'{m_newRedeem.title}' updated (unsaved). Press Save to persist.";
            }
            else
            {
                if (m_workingRedeems.Exists(r => r.title == m_newRedeem.title))
                {
                    m_createFeedbackText.text = $"A redeem named '{m_newRedeem.title}' already exists.";
                    return;
                }

                m_workingRedeems.Add(m_newRedeem);
                m_unsavedTitles.Add(m_newRedeem.title);
                RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);
                ShowListView();
                m_listFeedbackText.text = $"'{m_newRedeem.title}' added (unsaved). Press Save to persist.";
            }
        }

        private void RebuildStandardFields()
        {
            TabUIHelper.ClearContainer(m_standardFieldsContainer);
            float height = m_objectEditor.Build(m_standardFieldsContainer, m_newRedeem);
            m_standardFieldsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(1050f, height + 40f);
        }

        private void OnTestRedeem(RedeemData redeem)
        {
            // A background row's Test button is still clickable while these dialogs are open
            // (they have no full-screen blocker), so make sure "close everything to watch the
            // test" really closes everything - otherwise the dialog is left orphaned on screen
            // and its outstanding InputBlockGate.Push() keeps input blocked after Test runs.
            m_confirmDialog.Hide();
            m_copyRedeemDialog.Hide();

            m_onCloseRequested?.Invoke();

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            CustomRewardEvent rewardEvent = new CustomRewardEvent
            {
                RedeemerName       = "WizshBone",
                RedeemedAt         = System.DateTime.Now.ToShortDateString(),
                CustomRewardTitle  = redeem.title,
                CustomRewardCost   = redeem.points,
                Status             = CustomRewardRedemptionState.Unfulfilled
            };

            WizshBoneTwitchIntegration.useRedeemCommand = true;
            customRewards.HandleRedeem(rewardEvent);
        }

        // =====================================================================
        // View switching
        // =====================================================================

        private void ShowListView()
        {
            m_isReadOnly = false;
            RefreshList();
            m_createView.SetActive(false);
            m_listView.SetActive(true);
        }

        private void ShowCreateView()
        {
            m_isReadOnly                  = false;
            m_editingOriginal             = null;
            m_newRedeem                   = new RedeemData();
            m_createFeedbackText.text     = "";
            m_createViewTitle.text        = "New Redeem";
            m_typeDropdown.Value          = RedeemTypes[0];
            m_typeDropdown.Interactable   = true;
            m_variantDropdown.Value       = RedeemVariants[0];
            m_variantDropdown.Interactable = true;
            m_variantRow.SetActive(false);
            m_showRawSpawnAbilityData     = false;
            m_toggleRawViewBtn.GetComponentInChildren<Text>().text = "Show full data";
            m_confirmButton.gameObject.SetActive(true);
            m_confirmButton.GetComponentInChildren<Text>().text = "+ Add";

            RebuildStandardFields();
            TabUIHelper.ClearContainer(m_editorContainer);

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        private void ShowEditView(RedeemData redeem)
        {
            m_isReadOnly                  = false;
            m_editingOriginal             = redeem;
            m_newRedeem                   = redeem.DeepClone<RedeemData>();
            m_createFeedbackText.text     = "";
            m_createViewTitle.text        = "Edit Redeem";
            m_typeDropdown.Interactable   = false;
            m_variantDropdown.Interactable = false;
            m_showRawSpawnAbilityData     = false;
            m_toggleRawViewBtn.GetComponentInChildren<Text>().text = "Show full data";
            m_confirmButton.gameObject.SetActive(true);
            m_confirmButton.GetComponentInChildren<Text>().text = "Save";

            m_typeDropdown.Value = redeem.type;
            m_variantDropdown.Value = redeem.variant;

            RebuildStandardFields();
            OnTypeChanged();

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        private void ShowViewOnlyView(RedeemData redeem)
        {
            m_isReadOnly                  = true;
            m_editingOriginal             = null;
            m_newRedeem                   = redeem.DeepClone<RedeemData>();
            m_createFeedbackText.text     = "Read-only � this profile is synced.";
            m_createViewTitle.text        = "View Redeem";
            m_typeDropdown.Interactable   = false;
            m_variantDropdown.Interactable = false;
            m_showRawSpawnAbilityData     = false;
            m_toggleRawViewBtn.GetComponentInChildren<Text>().text = "Show full data";
            m_confirmButton.gameObject.SetActive(false);

            m_typeDropdown.Value = redeem.type;
            m_variantDropdown.Value = redeem.variant;

            RebuildStandardFields();
            OnTypeChanged();

            // Lock down all inputs after building
            SetContainerInteractable(m_standardFieldsContainer, false);
            SetContainerInteractable(m_editorContainer, false);

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static void SetContainerInteractable(GameObject container, bool interactable)
        {
            foreach (InputField input in container.GetComponentsInChildren<InputField>(true))
                input.interactable = interactable;

            foreach (Toggle toggle in container.GetComponentsInChildren<Toggle>(true))
                toggle.interactable = interactable;

            foreach (SearchableDropdownHandle dropdown in container.GetComponentsInChildren<SearchableDropdownHandle>(true))
                dropdown.SetInteractable(interactable);
        }

        private void OnToggleRedeem(RedeemData redeem)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            bool willDisable = redeem.enabled;
            if (willDisable && customRewards.HasUnresolvedRedeemsFor(redeem.title))
            {
                m_confirmDialog.Show(
                    title:       "Disable Redeem",
                    description: $"Auto-resolve is off. Pending '{redeem.title}' redeems won't be refunded automatically. Are you sure?",
                    onConfirm:   () =>
                    {
                        redeem.enabled = false;
                        OnSave();
                        if (customRewards.IsLoggedIn && customRewards.m_enabled)
                            customRewards.SetRewards();
                    },
                    confirmText: "Disable",
                    cancelText:  "Open history",
                    onCancel:    () => m_onOpenHistory?.Invoke()
                );
                return;
            }

            redeem.enabled = !redeem.enabled;
            OnSave();
            if (customRewards.IsLoggedIn && customRewards.m_enabled)
                customRewards.SetRewards();
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            RefreshList();
        }
    }
}