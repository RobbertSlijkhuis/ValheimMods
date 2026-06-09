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
        private Button m_viewersTabButton;

        // Tabs
        private ProfilesTab m_profilesTab = new ProfilesTab();
        private RedeemsTab m_redeemsTab = new RedeemsTab();
        private ViewersTab m_viewersTab = new ViewersTab();

        private GameObject m_profilesTabRoot;
        private GameObject m_redeemsTabRoot;
        private GameObject m_viewersTabRoot;

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
                width: 1200f,
                height: 720f,
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
                m_auth?.wizshBoneGUI.SignalReopenSettings();
                CloseSettings();
                m_auth?.wizshBoneGUI.CloseGUI();
            });
            m_viewersTabRoot = m_viewersTab.Create(panel, CreateScrollableContainer);

            ShowTab(m_profilesTabRoot, m_profilesTabButton);

            GameObject closeButtonObj = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 0f),
                position: new Vector2(150f, 50f),
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
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(140f, -90f),
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
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(325f, -90f),
                width: 180f,
                height: 40f
            );
            redeemsTabBtn.SetActive(true);
            m_redeemsTabButton = redeemsTabBtn.GetComponent<Button>();
            m_redeemsTabButton.onClick.AddListener(() => ShowTab(m_redeemsTabRoot, m_redeemsTabButton));

            GameObject viewersTabBtn = GUIManager.Instance.CreateButton(
                text: "Viewers",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(510f, -90f),
                width: 180f,
                height: 40f
            );
            viewersTabBtn.SetActive(true);
            m_viewersTabButton = viewersTabBtn.GetComponent<Button>();
            m_viewersTabButton.onClick.AddListener(() => ShowTab(m_viewersTabRoot, m_viewersTabButton));
        }

        private void ShowTab(GameObject tabToShow, Button activeButton)
        {
            m_profilesTabRoot.SetActive(false);
            m_redeemsTabRoot.SetActive(false);
            m_viewersTabRoot.SetActive(false);

            tabToShow.SetActive(true);
            RefreshActiveTab();

            SetTabButtonColor(m_profilesTabButton, m_profilesTabRoot.activeSelf);
            SetTabButtonColor(m_redeemsTabButton, m_redeemsTabRoot.activeSelf);
            SetTabButtonColor(m_viewersTabButton, m_viewersTabRoot.activeSelf);
        }

        private void RefreshActiveTab()
        {
            if (m_profilesTabRoot != null && m_profilesTabRoot.activeSelf)
                m_profilesTab.Refresh();

            if (m_redeemsTabRoot != null && m_redeemsTabRoot.activeSelf)
                m_redeemsTab.Refresh();

            if (m_viewersTabRoot != null && m_viewersTabRoot.activeSelf)
                m_viewersTab.Refresh();
        }

        private void SetTabButtonColor(Button button, bool isActive)
        {
            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = isActive ? TabActiveColor : m_tabDefaultColor;
        }

        private GameObject CreateScrollableContainer(string name, GameObject parent, float topOffset)
        {
            return ScrollableView.CreateStretched(
                parent: parent,
                name: name,
                offsetMin: new Vector2(50f, 90f),
                offsetMax: new Vector2(-50f, topOffset),
                backgroundColor: new Color(0f, 0f, 0f, 0.5f),
                autoHideScrollbar: true
            );
        }

        public bool IsVisible => panel != null && panel.activeSelf;
    }
}