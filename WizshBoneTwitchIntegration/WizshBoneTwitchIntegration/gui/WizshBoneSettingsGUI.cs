using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    // Delegate for creating scrollable containers
    internal delegate GameObject CreateScrollableContainerDelegate(string name, GameObject parent, float topOffset);

    internal class WizshBoneSettingsGUI
    {
        private GameObject panel;
        private readonly TwitchAuth m_auth;

        // Tab buttons
        private Button m_profilesTabButton;
        private Button m_redeemsTabButton;

        // Tabs
        private ProfilesTab m_profilesTab = new ProfilesTab();
        private RedeemsTab m_redeemsTab = new RedeemsTab();

        private GameObject m_profilesTabRoot;
        private GameObject m_redeemsTabRoot;

        private static readonly Color TabActiveColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private Color m_tabDefaultColor;

        public WizshBoneSettingsGUI(TwitchAuth auth)
        {
            m_auth = auth;
        }

        public void ShowSettings()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            if (panel != null)
            {
                panel.transform.SetAsLastSibling();
                RefreshActiveTab();
                panel.SetActive(true);
                return;
            }

            panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                width: 800f,
                height: 650f,
                draggable: false
            );

            panel.transform.SetAsLastSibling();
            CreateGUI();
            panel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (panel == null)
                return;

            panel.SetActive(false);
        }

        private void CreateGUI()
        {
            GameObject titleObj = GUIManager.Instance.CreateText(
                text: "Settings",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 24,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 30f,
                addContentSizeFitter: false
            );
            titleObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            CreateTabButtons();

            m_profilesTabRoot = m_profilesTab.Create(panel, CreateScrollableContainer);
            m_redeemsTabRoot = m_redeemsTab.Create(panel, CreateScrollableContainer, () =>
            {
                CloseSettings();
                m_auth?.wizshBoneGUI.CloseGUI();
            });

            ShowTab(m_profilesTabRoot, m_profilesTabButton);

            GameObject closeButtonObj = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, 40f),
                width: 200f,
                height: 60f
            );
            closeButtonObj.SetActive(true);
            closeButtonObj.GetComponent<Button>().onClick.AddListener(CloseSettings);
        }

        private void CreateTabButtons()
        {
            GameObject profilesTabBtn = GUIManager.Instance.CreateButton(
                text: "Profiles",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-200f, -90f),
                width: 180f,
                height: 40f
            );
            profilesTabBtn.SetActive(true);
            m_profilesTabButton = profilesTabBtn.GetComponent<Button>();
            m_tabDefaultColor = profilesTabBtn.GetComponent<Image>().color;
            m_profilesTabButton.onClick.AddListener(() => ShowTab(m_profilesTabRoot, m_profilesTabButton));

            GameObject redeemsTabBtn = GUIManager.Instance.CreateButton(
                text: "Redeems",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -90f),
                width: 180f,
                height: 40f
            );
            redeemsTabBtn.SetActive(true);
            m_redeemsTabButton = redeemsTabBtn.GetComponent<Button>();
            m_redeemsTabButton.onClick.AddListener(() => ShowTab(m_redeemsTabRoot, m_redeemsTabButton));
        }

        private void ShowTab(GameObject tabToShow, Button activeButton)
        {
            m_profilesTabRoot.SetActive(false);
            m_redeemsTabRoot.SetActive(false);

            tabToShow.SetActive(true);
            RefreshActiveTab();

            SetTabButtonColor(m_profilesTabButton, m_profilesTabRoot.activeSelf);
            SetTabButtonColor(m_redeemsTabButton, m_redeemsTabRoot.activeSelf);
        }

        private void RefreshActiveTab()
        {
            if (m_profilesTabRoot != null && m_profilesTabRoot.activeSelf)
                m_profilesTab.Refresh();

            if (m_redeemsTabRoot != null && m_redeemsTabRoot.activeSelf)
                m_redeemsTab.Refresh();
        }

        private void SetTabButtonColor(Button button, bool isActive)
        {
            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = isActive ? TabActiveColor : m_tabDefaultColor;
        }

        private GameObject CreateScrollableContainer(string name, GameObject parent, float topOffset)
        {
            GameObject scrollRoot = new GameObject(name + "ScrollView");
            scrollRoot.transform.SetParent(parent.transform, false);

            RectTransform scrollRt = scrollRoot.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(50f, 80f);
            scrollRt.offsetMax = new Vector2(-50f, topOffset);

            scrollRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);

            RectTransform viewportRt = viewport.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = new Vector2(-16f, 0f);

            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRt;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;

            scrollRect.content = contentRt;

            GameObject scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollRoot.transform, false);

            RectTransform scrollbarRt = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRt.anchorMin = new Vector2(1f, 0f);
            scrollbarRt.anchorMax = new Vector2(1f, 1f);
            scrollbarRt.pivot = new Vector2(1f, 0.5f);
            scrollbarRt.sizeDelta = new Vector2(16f, 0f);
            scrollbarRt.anchoredPosition = Vector2.zero;

            scrollbarObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(scrollbarObj.transform, false);

            RectTransform handleRt = handleObj.AddComponent<RectTransform>();
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.sizeDelta = Vector2.zero;

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            scrollbar.handleRect = handleRt;
            scrollbar.targetGraphic = handleImage;

            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            return content;
        }

        public bool IsVisible => panel != null && panel.activeSelf;
    }
}