using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        private Dropdown m_typeDropdown;
        private Text m_createFeedbackText;
        private GameObject m_standardFieldsContainer;
        private GameObject m_editorContainer;
        private Button m_confirmButton;
        private RedeemData m_editingOriginal;
        private bool m_isReadOnly;

        // Working redeem — sub-data objects are written into directly by ObjectEditor
        private RedeemData m_newRedeem = new RedeemData();
        private readonly ObjectEditor m_objectEditor = new ObjectEditor(startX: -220f, startY: -20f, fieldWidth: 200f);

        // Stored so CreateCreateView can create a scrollable editor container
        private CreateScrollableContainerDelegate m_createScrollable;

        // Working copy — only flushed to disk on Save
        private List<RedeemData> m_workingRedeems = new List<RedeemData>();
        private readonly HashSet<string> m_unsavedTitles = new HashSet<string>();

        private static readonly string[] RedeemTypes = new[]
        {
            RedeemType.Undefined,
            RedeemType.Detonate,
            RedeemType.Flashbang,
            RedeemType.SpawnAbility,
            RedeemType.SpawnCreature,
            RedeemType.SpawnMist,
            RedeemType.SpawnWeather,
            RedeemType.StatusEffect,
            RedeemType.StatusEffectRandom,
            RedeemType.SurpriseChest,
            RedeemType.TerrainEdit,
        };

        private const float ButtonHeight        = 40f;
        private const float ButtonSpacing       = 5f;
        private const float ItemHeight          = 40f;
        private const float ListTopPadding      = 15f;
        private const float HeaderTopPadding    = 45f;
        private const float ActionButtonWidth   = 80f;

        private const float ColTitleX  = -230f;
        private const float ColTitleW  = 200f;
        private const float ColTypeX   = -55f;
        private const float ColTypeW   = 140f;
        private const float ColCostX   = 70f;
        private const float ColCostW   = 100f;

        private const float BtnTestX   = 165f;
        private const float BtnEditX   = 250f;
        private const float BtnDeleteX = 315f;

        private const float ListLeftEdgeX        = -350f;
        private const float ContentTopY          = -(110f + HeaderTopPadding);
        private const float RedeemLabelY         = ContentTopY - 53f;
        private const float ScrollTopOffset      = ContentTopY - 71f;
        private const float SearchWidth          = 400f;
        private const float NewRedeemBtnWidth    = 160f;
        private const float SaveBtnWidth         = 80f;
        private const float SearchCenterX        = ListLeftEdgeX + SearchWidth / 2f;
        private const float NewRedeemBtnCenterX  = SearchCenterX + SearchWidth / 2f + ButtonSpacing + NewRedeemBtnWidth / 2f;
        private const float SaveBtnCenterX       = NewRedeemBtnCenterX + NewRedeemBtnWidth / 2f + ButtonSpacing + SaveBtnWidth / 2f;

        private const float TypeLabelX    = -220f;
        private const float TypeLabelW    = 130f;
        private const float TypeDropdownW = 200f;
        private const float TypeDropdownX = TypeLabelX + TypeLabelW / 2f + 10f + TypeDropdownW / 2f;

        private System.Action m_onCloseRequested;

        // Create view title — stored to allow "New Redeem" / "Edit Redeem" / "View Redeem" switching
        private Text m_createViewTitle;
        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        // Stored to allow scroll content resize on type change
        private GameObject m_createScrollContent;
        private float m_editorContainerYPos;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable, System.Action onCloseRequested)
        {
            m_onCloseRequested = onCloseRequested;
            m_createScrollable = createScrollable;

            m_root = UIContainer.Create(parent, "RedeemsTab");

            m_confirmDialog.Init();

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
                    // Synced profile — show read-only view, no delete
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

        private void OnSave()
        {
            string path = ProfileManager.GetActiveRedeemPath();

            ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();

            List<Dictionary<string, object>> redeems = new List<Dictionary<string, object>>();
            foreach (RedeemData redeem in m_workingRedeems)
                redeems.Add(redeem.ToDictionary());

            Dictionary<string, object> output = new Dictionary<string, object>
            {
                { "redeems", redeems }
            };

            if (data.creatureGroups != null && data.creatureGroups.Count > 0)
                output["creatureGroups"] = data.creatureGroups;

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using (StreamWriter writer = new StreamWriter(path, append: false))
                serializer.Serialize(writer, output);

            RedeemHelper.Reload();
            m_unsavedTitles.Clear();
            m_listFeedbackText.text = "Saved!";
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

            GameObject dropdownObj = GUIManager.Instance.CreateDropDown(
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(TypeDropdownX, yPos),
                fontSize: FieldUIBuilder.FieldFontSize,
                width: TypeDropdownW,
                height: 36f
            );
            m_typeDropdown = dropdownObj.GetComponent<Dropdown>();
            FieldUIBuilder.FixDropdownItemHeight(m_typeDropdown, FieldUIBuilder.InputHeight);
            m_typeDropdown.ClearOptions();
            m_typeDropdown.AddOptions(new List<string>(RedeemTypes));
            m_typeDropdown.value = 0;
            m_typeDropdown.RefreshShownValue();
            m_typeDropdown.onValueChanged.AddListener(_ => OnTypeChanged());

            yPos -= 70f;
            m_editorContainerYPos = yPos;

            m_editorContainer = TabUIHelper.CreateStaticContainer("EditorContainer", scrollContent, yPos);

            // Initial scroll content height — editor is empty at start
            TabUIHelper.UpdateScrollContentHeight(m_createScrollContent, m_editorContainerYPos, 0f);

            m_createView.SetActive(false);
        }

        private void OnTypeChanged()
        {
            m_newRedeem.type = RedeemTypes[m_typeDropdown.value];
            TabUIHelper.ClearContainer(m_editorContainer);

            float editorHeight = 0f;

            if (m_newRedeem.type == RedeemType.Detonate)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.detonateData);
            else if (m_newRedeem.type == RedeemType.Flashbang)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.flashbangData);
            else if (m_newRedeem.type == RedeemType.SpawnMist)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.mistData);
            else if (m_newRedeem.type == RedeemType.TerrainEdit)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.terrainEditData);
            else if (m_newRedeem.type == RedeemType.SpawnWeather)
                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.weatherData);
            else if (m_newRedeem.type == RedeemType.StatusEffect || m_newRedeem.type == RedeemType.StatusEffectRandom)
            {
                if (m_newRedeem.statusEffectData == null || m_newRedeem.statusEffectData.Count == 0)
                    m_newRedeem.statusEffectData.Add(new StatusEffectData());

                editorHeight = m_objectEditor.Build(m_editorContainer, m_newRedeem.statusEffectData[0]);
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
            m_standardFieldsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(700f, height + 40f);
        }

        private void OnTestRedeem(RedeemData redeem)
        {
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
            m_typeDropdown.value          = 0;
            m_typeDropdown.interactable   = true;
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
            m_typeDropdown.interactable   = false;
            m_confirmButton.gameObject.SetActive(true);
            m_confirmButton.GetComponentInChildren<Text>().text = "Save";

            int typeIndex = System.Array.IndexOf(RedeemTypes, redeem.type);
            m_typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;

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
            m_createFeedbackText.text     = "Read-only — this profile is synced.";
            m_createViewTitle.text        = "View Redeem";
            m_typeDropdown.interactable   = false;
            m_confirmButton.gameObject.SetActive(false);

            int typeIndex = System.Array.IndexOf(RedeemTypes, redeem.type);
            m_typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;

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

            foreach (Dropdown dropdown in container.GetComponentsInChildren<Dropdown>(true))
                dropdown.interactable = interactable;
        }

        private void OnSearchChanged(string value)
        {
            m_searchText = value;
            RefreshList();
        }
    }
}