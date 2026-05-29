using Jotunn.Managers;
using System.Collections.Generic;
using System.IO;
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
        private Text m_activeProfileLabel;
        private GameObject m_redeemListContainer;
        private Text m_listFeedbackText;

        // Create view
        private GameObject m_createView;
        private InputField m_titleInput;
        private InputField m_pointsInput;
        private InputField m_descriptionInput;
        private Dropdown m_typeDropdown;
        private Text m_createFeedbackText;
        private GameObject m_editorContainer;

        // Working redeem — sub-data objects are written into directly by ObjectEditor
        private RedeemData m_newRedeem = new RedeemData();
        private readonly ObjectEditor m_objectEditor = new ObjectEditor(startX: -80f, startY: -20f, fieldWidth: 200f);

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

        private const float ButtonHeight  = 40f;
        private const float ButtonSpacing = 5f;
        private const float ItemHeight    = 40f;

        private System.Action m_onCloseRequested;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable, System.Action onCloseRequested)
        {
            m_onCloseRequested  = onCloseRequested;
            m_createScrollable  = createScrollable;

            m_root = CreateContainer("RedeemsTab", parent);

            CreateListView(createScrollable);
            CreateCreateView();

            ShowListView();

            return m_root;
        }

        public void Refresh()
        {
            m_workingRedeems = new List<RedeemData>(RedeemHelper.redeems);
            m_activeProfileLabel.text = $"Active profile: {ProfileManager.ActiveProfile}";
            RefreshList();
        }

        // =====================================================================
        // List view
        // =====================================================================

        private void CreateListView(CreateScrollableContainerDelegate createScrollable)
        {
            m_listView = CreateContainer("ListView", m_root);

            m_activeProfileLabel = GUIManager.Instance.CreateText(
                text: $"Active profile: {ProfileManager.ActiveProfile}",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-100f, -120f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            // Add New button
            GameObject addNewBtn = GUIManager.Instance.CreateButton(
                text: "+ New Redeem",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(260f, -120f),
                width: 160f,
                height: 36f
            );
            addNewBtn.SetActive(true);
            addNewBtn.GetComponent<Button>().onClick.AddListener(ShowCreateView);

            // Save button
            GameObject saveBtn = GUIManager.Instance.CreateButton(
                text: "Save",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(350f, -120f),
                width: 80f,
                height: 36f
            );
            saveBtn.SetActive(true);
            saveBtn.GetComponentInChildren<Text>().color = new Color(0.2f, 0.8f, 0.2f);
            saveBtn.GetComponent<Button>().onClick.AddListener(OnSave);

            GUIManager.Instance.CreateText(
                text: "Redeems:",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-310f, -155f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 160f,
                height: 20f,
                addContentSizeFitter: false
            );

            m_listFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(50f, -155f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            m_redeemListContainer = createScrollable("RedeemList", m_listView, -173f);
        }

        private void RefreshList()
        {
            ClearContainer(m_redeemListContainer);

            float yOffset = -(ItemHeight / 2f);

            foreach (RedeemData redeem in m_workingRedeems)
            {
                RedeemData captured = redeem;
                bool isUnsaved = m_unsavedTitles.Contains(redeem.title);

                GUIManager.Instance.CreateText(
                    text: $"{redeem.title}  [{redeem.type}]  {redeem.points} pts",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(-50f, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 13,
                    color: isUnsaved ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: ItemHeight,
                    addContentSizeFitter: false
                );

                GameObject deleteBtn = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(260f, yOffset),
                    width: ButtonHeight,
                    height: ButtonHeight
                );
                deleteBtn.SetActive(true);
                deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteRedeem(captured));

                GameObject testBtn = GUIManager.Instance.CreateButton(
                    text: "Test",
                    parent: m_redeemListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(310f, yOffset),
                    width: ButtonHeight,
                    height: ButtonHeight
                );
                testBtn.SetActive(true);
                testBtn.GetComponentInChildren<Text>().color = Color.yellow;
                testBtn.GetComponent<Button>().onClick.AddListener(() => OnTestRedeem(captured));

                yOffset -= ItemHeight + ButtonSpacing;
            }

            RectTransform contentRt = m_redeemListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ItemHeight / 2f);
        }

        private void OnDeleteRedeem(RedeemData redeem)
        {
            m_workingRedeems.Remove(redeem);
            m_unsavedTitles.Remove(redeem.title);
            RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);
            m_listFeedbackText.text = $"'{redeem.title}' removed. Press Save to apply.";
            RefreshList();
        }

        private void OnSave()
        {
            string path = ProfileManager.GetActiveRedeemPath();

            ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();
            data.redeems = m_workingRedeems;

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using (StreamWriter writer = new StreamWriter(path, append: false))
                serializer.Serialize(writer, data);

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
            m_createView = CreateContainer("CreateView", m_root);

            // Create a single scroll container for ALL content in the create view
            GameObject scrollContent = m_createScrollable("CreateViewContent", m_createView, -140f);

            float yPos = -20f;

            GUIManager.Instance.CreateText(
                text: "New Redeem",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 18,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: 25f,
                addContentSizeFitter: false
            ).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            yPos -= 40f;

            // Common fields
            GUIManager.Instance.CreateText(
                text: "Title",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-290f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 180f,
                height: 20f,
                addContentSizeFitter: false
            );
            m_titleInput = CreateInputField(scrollContent, new Vector2(-290f, yPos - 30f), new Vector2(180f, 36f));

            GUIManager.Instance.CreateText(
                text: "Points",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-80f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 120f,
                height: 20f,
                addContentSizeFitter: false
            );
            m_pointsInput = CreateInputField(scrollContent, new Vector2(-80f, yPos - 30f), new Vector2(120f, 36f));

            GUIManager.Instance.CreateText(
                text: "Description",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(80f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 180f,
                height: 20f,
                addContentSizeFitter: false
            );
            m_descriptionInput = CreateInputField(scrollContent, new Vector2(80f, yPos - 30f), new Vector2(180f, 36f));

            GUIManager.Instance.CreateText(
                text: "Type",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(280f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 160f,
                height: 20f,
                addContentSizeFitter: false
            );

            GameObject dropdownObj = GUIManager.Instance.CreateDropDown(
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(280f, yPos - 30f),
                fontSize: 12,
                width: 160f,
                height: 36f
            );
            m_typeDropdown = dropdownObj.GetComponent<Dropdown>();
            m_typeDropdown.ClearOptions();
            m_typeDropdown.AddOptions(new List<string>(RedeemTypes));
            m_typeDropdown.value = 0;
            m_typeDropdown.RefreshShownValue();
            m_typeDropdown.onValueChanged.AddListener(_ => OnTypeChanged());

            yPos -= 70f;

            // Container for type-specific fields (non-scrollable now, part of parent scroll)
            m_editorContainer = CreateStaticContainer("EditorContainer", scrollContent, yPos);

            yPos -= 400f; // Reserve space for editor content

            // Buttons
            GameObject backBtn = GUIManager.Instance.CreateButton(
                text: "< Back",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-160f, yPos),
                width: 120f,
                height: 40f
            );
            backBtn.SetActive(true);
            backBtn.GetComponent<Button>().onClick.AddListener(ShowListView);

            GameObject addBtn = GUIManager.Instance.CreateButton(
                text: "+ Add",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(160f, yPos),
                width: 120f,
                height: 40f
            );
            addBtn.SetActive(true);
            addBtn.GetComponent<Button>().onClick.AddListener(OnAddRedeem);

            yPos -= 60f;

            m_createFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: scrollContent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, yPos),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 600f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            // Update scroll content size
            RectTransform scrollContentRt = scrollContent.GetComponent<RectTransform>();
            scrollContentRt.sizeDelta = new Vector2(scrollContentRt.sizeDelta.x, Mathf.Abs(yPos) + 40f);

            m_createView.SetActive(false);
        }

        // Add this helper method
        private static GameObject CreateStaticContainer(string name, GameObject parent, float yPos)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta = new Vector2(700f, 400f);

            return container;
        }

        private void OnTypeChanged()
        {
            ClearContainer(m_editorContainer);

            string selectedType = RedeemTypes[m_typeDropdown.value];

            if (selectedType == RedeemType.Detonate)
                m_objectEditor.Build(m_editorContainer, m_newRedeem.detonateData);
            else if (selectedType == RedeemType.Flashbang)
                m_objectEditor.Build(m_editorContainer, m_newRedeem.flashbangData);
            else if (selectedType == RedeemType.SpawnMist)
                m_objectEditor.Build(m_editorContainer, m_newRedeem.mistData);
            else if (selectedType == RedeemType.TerrainEdit)
                m_objectEditor.Build(m_editorContainer, m_newRedeem.terrainEditData);
            else if (selectedType == RedeemType.SpawnWeather)
                m_objectEditor.Build(m_editorContainer, m_newRedeem.weatherData);
        }

        private void OnAddRedeem()
        {
            string title = m_titleInput.text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                m_createFeedbackText.text = "Title is required.";
                return;
            }

            if (m_workingRedeems.Exists(r => r.title == title))
            {
                m_createFeedbackText.text = $"A redeem named '{title}' already exists.";
                return;
            }

            if (!string.IsNullOrEmpty(m_pointsInput.text))
                int.TryParse(m_pointsInput.text, out m_newRedeem.points);

            m_newRedeem.title       = title;
            m_newRedeem.description = m_descriptionInput.text.Trim();
            m_newRedeem.type        = RedeemTypes[m_typeDropdown.value];

            m_workingRedeems.Add(m_newRedeem);
            m_unsavedTitles.Add(title);
            RedeemHelper.redeems = new List<RedeemData>(m_workingRedeems);

            ShowListView();
            m_listFeedbackText.text = $"'{title}' added (unsaved). Press Save to persist.";
        }

        private void OnTestRedeem(RedeemData redeem)
        {
            m_onCloseRequested?.Invoke();

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            CustomRewardEvent rewardEvent = new CustomRewardEvent
            {
                RedeemerName       = "DevWizsh",
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
            RefreshList();
            m_createView.SetActive(false);
            m_listView.SetActive(true);
        }

        private void ShowCreateView()
        {
            m_newRedeem = new RedeemData();

            m_createFeedbackText.text = "";
            m_titleInput.text         = "";
            m_pointsInput.text        = "";
            m_descriptionInput.text   = "";
            m_typeDropdown.value      = 0;
            ClearContainer(m_editorContainer);

            m_listView.SetActive(false);
            m_createView.SetActive(true);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static GameObject CreateContainer(string name, GameObject parent)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot     = new Vector2(0.5f, 0.5f);

            container.SetActive(false);
            return container;
        }

        private static InputField CreateInputField(GameObject parent, Vector2 position, Vector2 size)
        {
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(parent.transform, false);

            RectTransform rt = inputObj.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = position;

            inputObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            InputField inputField = inputObj.AddComponent<InputField>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputField.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5f, 2f);
            textRt.offsetMax = new Vector2(-5f, -2f);
            Text text = textObj.AddComponent<Text>();
            text.font            = GUIManager.Instance.AveriaSerifBold;
            text.fontSize        = 12;
            text.color           = Color.white;
            text.supportRichText = false;

            inputField.textComponent = text;
            inputField.text          = "";
            inputObj.SetActive(true);
            return inputField;
        }

        private static void ClearContainer(GameObject container)
        {
            foreach (Transform child in container.transform)
                GameObject.Destroy(child.gameObject);
        }
    }
}