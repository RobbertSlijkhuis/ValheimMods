using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class WizshBoneGUI
    {
        private GameObject panel;
        private TwitchAuth auth;
        private TwitchCustomRewards customRewards;
        private TwitchChatting chatting;

        public UnityEvent onLogin = new UnityEvent();
        public UnityEvent onToggleRedeems = new UnityEvent();
        public UnityEvent onToggleChatting = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();
        public UnityEvent onOpenRedeemHistory = new UnityEvent();

        private readonly float buttonSwitchWidth = 200f;
        private readonly float buttonSwitchHeight = 60f;
        private readonly float buttonSwitchPosX = 120f;
        private readonly float descSwitchWidth = 230f;
        private readonly float descSwitchHeight = 60f;

        // Mutable element references
        private Text m_loginStatusText;
        private Text m_loginButtonText;
        private GameObject m_redeemsSection;
        private Text m_redeemsSectionStatusText;
        private Text m_redeemsSectionButtonText;
        private GameObject m_chattingSection;
        private Text m_chattingSectionStatusText;
        private Text m_chattingSectionButtonText;
        private WizshBoneSettingsGUI m_settingsGUI;
        private WizshBoneRedeemHistoryGUI m_redeemHistoryGUI;

        public void ShowGUI()
        {
            if (GUIManager.Instance == null)
            {
                Jotunn.Logger.LogError("GUIManager instance is null");
                return;
            }

            if (!GUIManager.CustomGUIFront)
            {
                Jotunn.Logger.LogError("GUIManager CustomGUI is null");
                return;
            }

            auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            if (m_settingsGUI == null)
                m_settingsGUI = new WizshBoneSettingsGUI(auth);

            if (m_redeemHistoryGUI == null)
                m_redeemHistoryGUI = new WizshBoneRedeemHistoryGUI(customRewards);

            panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 480,
                height: 520,
                draggable: false
            );
            panel.SetActive(false);
            CreateGUI();

            panel.SetActive(true);
            GUIManager.BlockInput(true);
        }

        public void CloseGUI()
        {
            onClose.Invoke();
            panel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        public void UpdateGUI()
        {
            if (panel == null)
                return;

            // Update login section
            m_loginStatusText.text = GetLoginStatusMessage();
            m_loginStatusText.color = auth.m_userInfo != null
                ? GUIManager.Instance.ValheimYellow
                : GUIManager.Instance.ValheimBeige;
            m_loginButtonText.text = GetLoginButtonText();

            // Show/hide and update conditional sections
            bool loggedIn = auth.m_userInfo != null;
            m_redeemsSection.SetActive(loggedIn);
            m_chattingSection.SetActive(loggedIn);

            if (loggedIn)
            {
                m_redeemsSectionStatusText.text = customRewards.m_enabled
                    ? "Redeems are currently enabled"
                    : "Redeems are currently disabled";
                m_redeemsSectionStatusText.color = customRewards.m_enabled
                    ? GUIManager.Instance.ValheimYellow
                    : GUIManager.Instance.ValheimBeige;
                m_redeemsSectionButtonText.text = customRewards.m_enabled
                    ? "Disable Redeems"
                    : "Enable Redeems";

                m_chattingSectionStatusText.color = chatting.m_enabled
                    ? GUIManager.Instance.ValheimYellow
                    : GUIManager.Instance.ValheimBeige;
                m_chattingSectionButtonText.text = chatting.m_enabled
                    ? "Disable in-game chat messages"
                    : "Enable in-game chat messages";
            }
        }

        private void CreateGUI()
        {
            GUIManager.Instance.CreateText(
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
                width: 360,
                height: 30f,
                addContentSizeFitter: false
            );

            CreateTwitchSection();
            CreateRedeemsSection();
            CreateChattingSection();

            //GUIManager.Instance.CreateText(
            //    text: $"Redeems can be configured in \"{WizshBoneTwitchIntegration.redeemsConfigPath.Substring(0, 24)} {WizshBoneTwitchIntegration.redeemsConfigPath.Substring(24)}\". They can be reloaded during gameplay with the \"ReloadTwitchRedeems\" command.",
            //    parent: panel.transform,
            //    anchorMin: new Vector2(0.5f, 0f),
            //    anchorMax: new Vector2(0.5f, 0f),
            //    position: new Vector2(10f, 140f),
            //    font: GUIManager.Instance.AveriaSerifBold,
            //    fontSize: 14,
            //    color: GUIManager.Instance.ValheimBeige,
            //    outline: true,
            //    outlineColor: Color.black,
            //    width: 450,
            //    height: 60f,
            //    addContentSizeFitter: false
            //);

            GameObject cancelButtonObj = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-120f, 75f),
                width: 200f,
                height: 60f
            );
            cancelButtonObj.SetActive(true);
            cancelButtonObj.GetComponent<Button>().onClick.AddListener(CloseGUI);

            GameObject settingsButtonObj = GUIManager.Instance.CreateButton(
                text: "Settings",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(120f, 75f),
                width: 200f,
                height: 60f
            );
            settingsButtonObj.SetActive(true);
            settingsButtonObj.GetComponent<Button>().onClick.AddListener(OpenSettings);

            //GUIManager.Instance.CreateText(
            //    text: "Please report any issues on my Discord, the link can be found on my mod page or Twitch channel. Suggestions are also welcome!",
            //    parent: panel.transform,
            //    anchorMin: new Vector2(0.5f, 0f),
            //    anchorMax: new Vector2(0.5f, 0f),
            //    position: new Vector2(110f, 50f),
            //    font: GUIManager.Instance.AveriaSerifBold,
            //    fontSize: 14,
            //    color: GUIManager.Instance.ValheimBeige,
            //    outline: true,
            //    outlineColor: Color.black,
            //    width: 235,
            //    height: 120f,
            //    addContentSizeFitter: false
            //);

            GUIManager.Instance.CreateText(
                text: $"Created by: DeathWizsh, commisioned by: LoyalBones        v{WizshBoneTwitchIntegration.PluginVersion}",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(10f, 17f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 460,
                height: 30f,
                addContentSizeFitter: false
            );
        }

        private void CreateTwitchSection()
        {
            m_loginStatusText = GUIManager.Instance.CreateText(
                text: GetLoginStatusMessage(),
                parent: panel.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, -115f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: auth.m_userInfo != null ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject loginButtonObj = GUIManager.Instance.CreateButton(
                text: GetLoginButtonText(),
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -100f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            loginButtonObj.SetActive(true);
            loginButtonObj.GetComponent<Button>().onClick.AddListener(Login);
            m_loginButtonText = loginButtonObj.GetComponentInChildren<Text>();
        }

        private void CreateRedeemsSection()
        {
            m_redeemsSection = UIContainer.Create(panel, "RedeemsSection", startActive: false);

            m_redeemsSectionStatusText = GUIManager.Instance.CreateText(
                text: customRewards.m_enabled ? "Redeems are currently enabled" : "Redeems are currently disabled",
                parent: m_redeemsSection.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, -250f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: customRewards.m_enabled ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: customRewards.m_enabled ? "Disable Redeems" : "Enable Redeems",
                parent: m_redeemsSection.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -240f),
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
                position: new Vector2(buttonSwitchPosX, -310f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            historyButtonObj.SetActive(true);
            historyButtonObj.GetComponent<Button>().onClick.AddListener(OpenRedeemHistory);

            m_redeemsSection.SetActive(auth.m_userInfo != null);
        }

        private void CreateChattingSection()
        {
            m_chattingSection = UIContainer.Create(panel, "ChattingSection", startActive: false);

            m_chattingSectionStatusText = GUIManager.Instance.CreateText(
                text: "Random creatures can show chat messages from viewers",
                parent: m_chattingSection.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, -180f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: chatting.m_enabled ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: descSwitchWidth,
                height: descSwitchHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: chatting.m_enabled ? "Disable in-game chat messages" : "Enable in-game chat messages",
                parent: m_chattingSection.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -170f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            enableButtonObj.SetActive(true);
            enableButtonObj.GetComponent<Button>().onClick.AddListener(ToggleChatting);
            m_chattingSectionButtonText = enableButtonObj.GetComponentInChildren<Text>();

            m_chattingSection.SetActive(auth.m_userInfo != null);
        }

        private void Login()
        {
            if (auth.m_loggedIn)
            {
                auth.Logout();
                UpdateGUI();
            }
            else
            {
                onLogin.Invoke();
            }
        }

        private void ToggleRedeems()
        {
            onToggleRedeems.Invoke();
            UpdateGUI();
        }

        private void ToggleChatting()
        {
            onToggleChatting.Invoke();
            UpdateGUI();
        }

        private void OpenSettings()
        {
            m_settingsGUI.ShowSettings();
        }

        private void OpenRedeemHistory()
        {
            m_redeemHistoryGUI.ShowGUI();
        }

        private string GetLoginButtonText()
        {
            if (auth.m_loggedIn)
                return "Logout";

            if (auth.m_waitingForCode)
                return "Open Browser Again";

            return "Twitch Login";
        }

        private string GetLoginStatusMessage()
        {
            if (auth.m_userInfo != null)
                return $"Welcome {auth.m_userInfo.displayName}";

            if (auth.m_loggedIn && auth.m_userInfo == null)
                return "Fetching user info...";

            if (auth.m_waitingForCode)
                return "Waiting for authorization,\nCHECK YOUR BROWSER!";

            return "Welcome! Please login into Twitch";
        }

        public bool IsAnyGUIVisible =>
            (panel != null && panel.activeSelf) ||
            (m_settingsGUI != null && m_settingsGUI.IsVisible) ||
            (m_redeemHistoryGUI != null && m_redeemHistoryGUI.IsVisible);
    }
}
