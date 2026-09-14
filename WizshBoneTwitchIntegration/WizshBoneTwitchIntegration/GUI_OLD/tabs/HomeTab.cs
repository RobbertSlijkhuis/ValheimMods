using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Landing tab of <see cref="WizshBoneSettingsGUI"/> - Twitch login, and the "Redeems enabled"
    /// / "Chatting enabled" toggles. Ported from the old standalone WizshBoneGUI outer panel, which
    /// used to sit in front of the Settings panel; F4 now opens Settings directly onto this tab.
    /// </summary>
    internal class HomeTab
    {
        private GameObject m_root;
        private TwitchAuth m_auth;
        private TwitchCustomRewards m_customRewards;
        private TwitchChatting m_chatting;
        private Action m_onOpenHistory;

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        private readonly float buttonSwitchWidth = 200f;
        private readonly float buttonSwitchHeight = 60f;
        private readonly float buttonSwitchPosX = 120f;
        private readonly float descSwitchWidth = 230f;
        private readonly float descSwitchHeight = 60f;

        private const float TitleY = -153f;

        private const float LoginStatusY = -220f;
        private const float LoginButtonY = -205f;

        private const float ChattingStatusY = -330f;
        private const float ChattingButtonY = -315f;

        private const float RedeemsStatusY = -405f;
        private const float RedeemsButtonY = -390f;
        private const float RedeemsHistoryButtonY = -460f;

        // Mutable element references
        private Text m_loginStatusText;
        private Text m_loginButtonText;
        private GameObject m_chattingSection;
        private Text m_chattingSectionStatusText;
        private Text m_chattingSectionButtonText;
        private GameObject m_redeemsSection;
        private Text m_redeemsSectionStatusText;
        private Text m_redeemsSectionButtonText;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable, TwitchAuth auth, Action onOpenHistory)
        {
            m_auth = auth;
            m_onOpenHistory = onOpenHistory;
            m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            m_chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            m_root = UIContainer.Create(parent, "HomeTab");

            m_confirmDialog.Init();

            TabUIHelper.CreateTabTitle("Home:", m_root, new Vector2(-200f, TitleY));

            CreateTwitchSection();
            CreateChattingSection();
            CreateRedeemsSection();

            return m_root;
        }

        public void Refresh()
        {
            m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            m_chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            // Update login section
            m_loginStatusText.text = GetLoginStatusMessage();
            m_loginStatusText.color = m_auth.m_userInfo != null
                ? GUIManager.Instance.ValheimYellow
                : GUIManager.Instance.ValheimBeige;
            m_loginButtonText.text = GetLoginButtonText();

            // Show/hide and update conditional sections
            bool loggedIn = m_auth.m_userInfo != null;
            m_chattingSection.SetActive(loggedIn);
            m_redeemsSection.SetActive(loggedIn);

            if (loggedIn)
            {
                m_chattingSectionStatusText.color = m_chatting.m_enabled
                    ? GUIManager.Instance.ValheimYellow
                    : GUIManager.Instance.ValheimBeige;
                m_chattingSectionButtonText.text = m_chatting.m_enabled
                    ? "Disable in-game chat messages"
                    : "Enable in-game chat messages";

                m_redeemsSectionStatusText.text = m_customRewards.m_enabled
                    ? "Redeems are currently enabled"
                    : "Redeems are currently disabled";
                m_redeemsSectionStatusText.color = m_customRewards.m_enabled
                    ? GUIManager.Instance.ValheimYellow
                    : GUIManager.Instance.ValheimBeige;
                m_redeemsSectionButtonText.text = m_customRewards.m_enabled
                    ? "Disable Redeems"
                    : "Enable Redeems";
            }
        }

        private void CreateTwitchSection()
        {
            m_loginStatusText = GUIManager.Instance.CreateText(
                text: GetLoginStatusMessage(),
                parent: m_root.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, LoginStatusY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: m_auth.m_userInfo != null ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject loginButtonObj = GUIManager.Instance.CreateButton(
                text: GetLoginButtonText(),
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, LoginButtonY),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            loginButtonObj.SetActive(true);
            loginButtonObj.GetComponent<Button>().onClick.AddListener(Login);
            m_loginButtonText = loginButtonObj.GetComponentInChildren<Text>();
        }

        private void CreateRedeemsSection()
        {
            m_redeemsSection = UIContainer.Create(m_root, "RedeemsSection", startActive: false);

            m_redeemsSectionStatusText = GUIManager.Instance.CreateText(
                text: m_customRewards.m_enabled ? "Redeems are currently enabled" : "Redeems are currently disabled",
                parent: m_redeemsSection.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, RedeemsStatusY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: m_customRewards.m_enabled ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: m_customRewards.m_enabled ? "Disable Redeems" : "Enable Redeems",
                parent: m_redeemsSection.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, RedeemsButtonY),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            enableButtonObj.SetActive(true);
            enableButtonObj.GetComponent<Button>().onClick.AddListener(ToggleRedeems);
            m_redeemsSectionButtonText = enableButtonObj.GetComponentInChildren<Text>();

            GameObject historyButtonObj = GUIManager.Instance.CreateButton(
                text: "Redeem History",
                parent: m_redeemsSection.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, RedeemsHistoryButtonY),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            historyButtonObj.SetActive(true);
            historyButtonObj.GetComponent<Button>().onClick.AddListener(() => m_onOpenHistory?.Invoke());

            m_redeemsSection.SetActive(m_auth.m_userInfo != null);
        }

        private void CreateChattingSection()
        {
            m_chattingSection = UIContainer.Create(m_root, "ChattingSection", startActive: false);

            m_chattingSectionStatusText = GUIManager.Instance.CreateText(
                text: "Random creatures can show chat messages from viewers",
                parent: m_chattingSection.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, ChattingStatusY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: m_chatting.m_enabled ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: m_chatting.m_enabled ? "Disable in-game chat messages" : "Enable in-game chat messages",
                parent: m_chattingSection.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, ChattingButtonY),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            enableButtonObj.SetActive(true);
            enableButtonObj.GetComponent<Button>().onClick.AddListener(ToggleChatting);
            m_chattingSectionButtonText = enableButtonObj.GetComponentInChildren<Text>();

            m_chattingSection.SetActive(m_auth.m_userInfo != null);
        }

        private void Login()
        {
            if (m_auth.m_loggedIn)
            {
                if (m_customRewards.HasUnresolvedRedeems())
                {
                    m_confirmDialog.Show(
                        title:       "Logout",
                        description: "Auto-resolve is off. Pending redeems won't be refunded automatically. Log out anyway?",
                        onConfirm:   () =>
                        {
                            m_auth.Logout();
                            Refresh();
                        },
                        confirmText: "Log Out",
                        cancelText:  "Open History",
                        onCancel:    () =>
                        {
                            m_onOpenHistory?.Invoke();
                        }
                    );
                    return;
                }

                m_auth.Logout();
                Refresh();
            }
            else
            {
                m_auth.InvokeAuth();
            }
        }

        private void ToggleRedeems()
        {
            if (m_customRewards.HasUnresolvedRedeems())
            {
                m_confirmDialog.Show(
                    title:       "Disable Redeems",
                    description: "Auto-resolve is off. Pending redeems won't be refunded automatically. Are you sure?",
                    onConfirm:   () =>
                    {
                        m_auth.ToggleRedeems();
                        Refresh();
                    },
                    confirmText: "Disable",
                    cancelText:  "Open history",
                    onCancel: () =>
                    {
                        m_onOpenHistory?.Invoke();
                    }
                );
                return;
            }

            m_auth.ToggleRedeems();
            Refresh();
        }

        private void ToggleChatting()
        {
            m_auth.ToggleChatting();
            Refresh();
        }

        private string GetLoginButtonText()
        {
            if (m_auth.m_loggedIn)
                return "Logout";

            if (m_auth.m_waitingForCode)
                return "Open Browser Again";

            return "Twitch Login";
        }

        private string GetLoginStatusMessage()
        {
            if (m_auth.m_userInfo != null)
                return $"Welcome {m_auth.m_userInfo.displayName}";

            if (m_auth.m_loggedIn && m_auth.m_userInfo == null)
                return "Fetching user info...";

            if (m_auth.m_waitingForCode)
                return "Waiting for authorization,\nCHECK YOUR BROWSER!";

            return "Welcome! Please login into Twitch";
        }
    }
}
