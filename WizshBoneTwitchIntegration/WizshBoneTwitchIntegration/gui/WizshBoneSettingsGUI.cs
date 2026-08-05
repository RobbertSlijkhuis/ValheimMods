using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    // Delegate for creating scrollable containers
    internal delegate GameObject CreateScrollableContainerDelegate(string name, GameObject parent, float topOffset, float leftInset = 0f);

    internal class WizshBoneSettingsGUI
    {
        // Referenced by tabs (e.g. RedeemsTab's sidebar) and TabListLayout, which need to
        // independently reproduce the main scrollable container's extent - single source of
        // truth instead of duplicated magic numbers that can drift out of sync.
        internal const float PanelHeight = 720f;
        internal const float ContentBottomMargin = 90f;

        internal const float PanelWidth = 1200f;
        internal const float ContentSideMargin = 50f;
        internal const float ContentLeftEdgeX = -PanelWidth / 2f + ContentSideMargin;

        private GameObject panel;
        private readonly TwitchAuth m_auth;

        // Tab buttons
        private Button m_homeTabButton;
        private Button m_profilesTabButton;
        private Button m_redeemsTabButton;
        private Button m_rulesTabButton;
        private Button m_creatureGroupsTabButton;
        private Button m_viewersTabButton;

        // Tabs
        private HomeTab m_homeTab = new HomeTab();
        private ProfilesTab m_profilesTab = new ProfilesTab();
        private RedeemsTab m_redeemsTab = new RedeemsTab();
        private RulesTab m_rulesTab = new RulesTab();
        private CreatureGroupsTab m_creatureGroupsTab = new CreatureGroupsTab();
        private ViewersTab m_viewersTab = new ViewersTab();

        private GameObject m_homeTabRoot;
        private GameObject m_profilesTabRoot;
        private GameObject m_redeemsTabRoot;
        private GameObject m_rulesTabRoot;
        private GameObject m_creatureGroupsTabRoot;
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
                width: PanelWidth,
                height: PanelHeight,
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
                text: "WizshBone Twitch Integration",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 24,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 600f,
                height: 30f,
                addContentSizeFitter: false
            );
            titleObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            CreateTabButtons();

            Action onOpenHistory = () =>
            {
                CloseSettings();
                m_auth?.wizshBoneGUI.OpenRedeemHistory();
            };

            m_homeTabRoot = m_homeTab.Create(panel, CreateScrollableContainer, m_auth, onOpenHistory);
            m_profilesTabRoot = m_profilesTab.Create(panel, CreateScrollableContainer);
            m_redeemsTabRoot = m_redeemsTab.Create(panel, CreateScrollableContainer,
                onCloseRequested: () =>
                {
                    CloseSettings();
                    m_auth?.wizshBoneGUI.CloseGUI();
                },
                onOpenHistory: onOpenHistory);
            m_rulesTabRoot = m_rulesTab.Create(panel, CreateScrollableContainer);
            m_creatureGroupsTabRoot = m_creatureGroupsTab.Create(panel, CreateScrollableContainer);
            m_viewersTabRoot = m_viewersTab.Create(panel, CreateScrollableContainer);

            ShowTab(m_homeTabRoot, m_homeTabButton);

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
            closeButtonObj.GetComponent<Button>().onClick.AddListener(() => m_auth?.wizshBoneGUI.CloseGUI());
        }

        private void CreateTabButtons()
        {
            GameObject homeTabBtn = GUIManager.Instance.CreateButton(
                text: "Home",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(140f, -90f),
                width: 180f,
                height: 40f
            );
            homeTabBtn.SetActive(true);
            m_homeTabButton = homeTabBtn.GetComponent<Button>();
            m_tabDefaultColor = homeTabBtn.GetComponent<Image>().color;
            m_homeTabButton.onClick.AddListener(() => ShowTab(m_homeTabRoot, m_homeTabButton));

            GameObject profilesTabBtn = GUIManager.Instance.CreateButton(
                text: "Profiles",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(324f, -90f),
                width: 180f,
                height: 40f
            );
            profilesTabBtn.SetActive(true);
            m_profilesTabButton = profilesTabBtn.GetComponent<Button>();
            m_profilesTabButton.onClick.AddListener(() => ShowTab(m_profilesTabRoot, m_profilesTabButton));

            GameObject redeemsTabBtn = GUIManager.Instance.CreateButton(
                text: "Redeems",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(508f, -90f),
                width: 180f,
                height: 40f
            );
            redeemsTabBtn.SetActive(true);
            m_redeemsTabButton = redeemsTabBtn.GetComponent<Button>();
            m_redeemsTabButton.onClick.AddListener(() => ShowTab(m_redeemsTabRoot, m_redeemsTabButton));

            GameObject rulesTabBtn = GUIManager.Instance.CreateButton(
                text: "Settings",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(692f, -90f),
                width: 180f,
                height: 40f
            );
            rulesTabBtn.SetActive(true);
            m_rulesTabButton = rulesTabBtn.GetComponent<Button>();
            m_rulesTabButton.onClick.AddListener(() => ShowTab(m_rulesTabRoot, m_rulesTabButton));

            GameObject creatureGroupsTabBtn = GUIManager.Instance.CreateButton(
                text: "Creature groups",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(876f, -90f),
                width: 180f,
                height: 40f
            );
            creatureGroupsTabBtn.SetActive(true);
            m_creatureGroupsTabButton = creatureGroupsTabBtn.GetComponent<Button>();
            m_creatureGroupsTabButton.onClick.AddListener(() => ShowTab(m_creatureGroupsTabRoot, m_creatureGroupsTabButton));

            GameObject viewersTabBtn = GUIManager.Instance.CreateButton(
                text: "Viewers",
                parent: panel.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(1060f, -90f),
                width: 180f,
                height: 40f
            );
            viewersTabBtn.SetActive(true);
            m_viewersTabButton = viewersTabBtn.GetComponent<Button>();
            m_viewersTabButton.onClick.AddListener(() => ShowTab(m_viewersTabRoot, m_viewersTabButton));
        }

        private void ShowTab(GameObject tabToShow, Button activeButton)
        {
            m_homeTabRoot.SetActive(false);
            m_profilesTabRoot.SetActive(false);
            m_redeemsTabRoot.SetActive(false);
            m_rulesTabRoot.SetActive(false);
            m_creatureGroupsTabRoot.SetActive(false);
            m_viewersTabRoot.SetActive(false);

            tabToShow.SetActive(true);
            RefreshActiveTab();

            SetTabButtonColor(m_homeTabButton, m_homeTabRoot.activeSelf);
            SetTabButtonColor(m_profilesTabButton, m_profilesTabRoot.activeSelf);
            SetTabButtonColor(m_redeemsTabButton, m_redeemsTabRoot.activeSelf);
            SetTabButtonColor(m_rulesTabButton, m_rulesTabRoot.activeSelf);
            SetTabButtonColor(m_creatureGroupsTabButton, m_creatureGroupsTabRoot.activeSelf);
            SetTabButtonColor(m_viewersTabButton, m_viewersTabRoot.activeSelf);
        }

        private void RefreshActiveTab()
        {
            if (m_homeTabRoot != null && m_homeTabRoot.activeSelf)
                m_homeTab.Refresh();

            if (m_profilesTabRoot != null && m_profilesTabRoot.activeSelf)
                m_profilesTab.Refresh();

            if (m_redeemsTabRoot != null && m_redeemsTabRoot.activeSelf)
                m_redeemsTab.Refresh();

            if (m_rulesTabRoot != null && m_rulesTabRoot.activeSelf)
                m_rulesTab.Refresh();

            if (m_creatureGroupsTabRoot != null && m_creatureGroupsTabRoot.activeSelf)
                m_creatureGroupsTab.Refresh();

            if (m_viewersTabRoot != null && m_viewersTabRoot.activeSelf)
                m_viewersTab.Refresh();
        }

        /// <summary>
        /// Refreshes the Home tab's login/redeems/chatting status regardless of whether it's the
        /// currently active tab, so auth-state changes (e.g. login completing) are reflected live
        /// even while another tab is showing - mirrors the old WizshBoneGUI.UpdateGUI() behavior of
        /// refreshing whenever its panel existed.
        /// </summary>
        public void RefreshHomeTab()
        {
            if (m_homeTabRoot != null)
                m_homeTab.Refresh();
        }

        private void SetTabButtonColor(Button button, bool isActive)
        {
            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = isActive ? TabActiveColor : m_tabDefaultColor;
        }

        private GameObject CreateScrollableContainer(string name, GameObject parent, float topOffset, float leftInset = 0f)
        {
            return ScrollableView.CreateStretched(
                parent: parent,
                name: name,
                offsetMin: new Vector2(ContentSideMargin + leftInset, ContentBottomMargin),
                offsetMax: new Vector2(-ContentSideMargin, topOffset),
                backgroundColor: ScrollableView.DarkBackground,
                autoHideScrollbar: true
            );
        }

        public bool IsVisible => panel != null && panel.activeSelf;
    }
}