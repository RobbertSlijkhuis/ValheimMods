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

        float buttonSwitchWidth = 200f;
        float buttonSwitchHeight = 60f;
        float buttonSwitchPosX = 120f;

        float descSwitchWidth = 230f;
        float descSwitchHeight = 60f;

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

            panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 480,
                height: 450,
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
            foreach (Transform child in panel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            CreateGUI();
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

            CreateTwitchButton();

            if (auth.m_userInfo != null)
            {
                CreateEnableRedeemsButton();
                CreateEnableChattingButton();
            }

            GUIManager.Instance.CreateText(
                text: $"Redeems can be configured in \"{WizshBoneTwitchIntegration.redeemsConfigPath.Substring(0, 24)} {WizshBoneTwitchIntegration.redeemsConfigPath.Substring(24)}\". They can be reloaded during gameplay with the \"ReloadTwitchRedeems\" command.",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(10f, 140f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 450,
                height: 60f,
                addContentSizeFitter: false
            );

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
            Button cancelButton = cancelButtonObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(CloseGUI);

            GUIManager.Instance.CreateText(
                text: "Please report any issues on my Discord, the link can be found on my mod page or Twitch channel. Suggestions are also welcome!",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(110f, 50f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 235,
                height: 120f,
                addContentSizeFitter: false
            );

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

        private void Login()
        {
            if (auth.m_loggedIn)
                auth.Logout();
            else
                onLogin.Invoke();

            UpdateGUI();
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

        public void CreateTwitchButton()
        {
            GameObject loginTextObj = GUIManager.Instance.CreateText(
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
            );

            GameObject loginButtonObj = GUIManager.Instance.CreateButton(
                text: auth.m_loggedIn ? "Logout" : "Twitch Login",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -100f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            loginButtonObj.SetActive(true);
            Button loginButton = loginButtonObj.GetComponent<Button>();
            loginButton.onClick.AddListener(Login);
        }

        public void CreateEnableRedeemsButton()
        {
            GUIManager.Instance.CreateText(
                text: customRewards.m_enabled ? "Redeems are currently enabled" : "Redeems are currently disabled",
                parent: panel.transform,
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
            );

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: customRewards.m_enabled ? "Disable Redeems" : "Enable Redeems",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -240f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            enableButtonObj.SetActive(true);
            Button enableButton = enableButtonObj.GetComponent<Button>();
            enableButton.onClick.AddListener(ToggleRedeems);
        }

        public void CreateEnableChattingButton()
        {
            GUIManager.Instance.CreateText(
                text: "Random creatures can show chat messages from viewers",
                parent: panel.transform,
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
            );

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: chatting.m_enabled ? "Disable in-game chat messages" : "Enable in-game chat messages",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(buttonSwitchPosX, -170f),
                width: buttonSwitchWidth,
                height: buttonSwitchHeight
            );
            enableButtonObj.SetActive(true);
            Button enableButton = enableButtonObj.GetComponent<Button>();
            enableButton.onClick.AddListener(ToggleChatting);
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
    }
}
