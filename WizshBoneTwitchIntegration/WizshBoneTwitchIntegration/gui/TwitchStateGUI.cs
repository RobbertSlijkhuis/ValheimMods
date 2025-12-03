using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class TwitchStateGUI
    {
        private GameObject panel;
        private TwitchAuth twitchAuth;
        private TwitchChatting twitchChatting;

        public UnityEvent onLogin = new UnityEvent();
        public UnityEvent onEnableRedeems = new UnityEvent();
        public UnityEvent onEnableChatting = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

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

            twitchAuth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 480,
                //height: 380,
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

            GameObject loginTextObj = GUIManager.Instance.CreateText(
                text: twitchAuth.retrievedUserInfo ? $"Welcome {twitchAuth.displayName}" : "Please login into Twitch",
                parent: panel.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-105f, -105f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: twitchAuth.retrievedUserInfo ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 225,
                height: 30f,
                addContentSizeFitter: false
            );

            GameObject loginButtonObj = GUIManager.Instance.CreateButton(
                text: "Twitch Login",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(110f, -100f),
                width: 225f,
                height: 60f
            );
            loginButtonObj.SetActive(true);
            Button loginButton = loginButtonObj.GetComponent<Button>();
            loginButton.onClick.AddListener(Login);

            if (twitchAuth.retrievedUserInfo)
            {
                CreateEnableRedeemsButton();
                CreateEnableChattingButton();
            }

            GUIManager.Instance.CreateText(
                // text: "Please report any issues on my Discord, the link is on the mod page! Suggestions are also welcome!",
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
                // text: $"Redeems can be configured in \"{WizshBoneTwitchIntegration.redeemsConfigPath}\". They can be reloaded during gameplay with the \"ReloadTwitchRedeems\" command.",
                text: "Please report any issues on my Discord, the link can be found on my mod page or Twitch channel! Suggestions are also welcome!",
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
                text: "Created by: DeathWizsh, commisioned by: LoyalBones",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(5f, 17f),
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
            onLogin.Invoke();
        }

        private void EnableRedeems()
        {
            onEnableRedeems.Invoke();
            UpdateGUI();
        }

        private void EnableChatting()
        {
            onEnableChatting.Invoke();
            UpdateGUI();
        }

        public void CreateEnableRedeemsButton()
        {
            GUIManager.Instance.CreateText(
                text: twitchAuth.isEnabledRedeems ? "Redeems are currently enabled" : "Redeems are currently disabled",
                parent: panel.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-105f, -245f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: twitchAuth.isEnabledRedeems ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 225,
                height: 60f,
                addContentSizeFitter: false
            );

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: twitchAuth.isEnabledRedeems ? "Disable Redeems" : "Enable Redeems",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(110f, -240f),
                width: 225f,
                height: 60f
            );
            enableButtonObj.SetActive(true);
            Button enableButton = enableButtonObj.GetComponent<Button>();
            enableButton.onClick.AddListener(EnableRedeems);
        }

        public void CreateEnableChattingButton()
        {
            GUIManager.Instance.CreateText(
                text: "Random creatures can show chat messages from viewers",
                parent: panel.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-105f, -175f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: twitchChatting.isEnabled ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 225,
                height: 60f,
                addContentSizeFitter: false
            );

            GameObject enableButtonObj = GUIManager.Instance.CreateButton(
                text: twitchChatting.isEnabled ? "Disable in-game chat messages" : "Enable in-game chat messages",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(110f, -170f),
                width: 225f,
                height: 60f
            );
            enableButtonObj.SetActive(true);
            Button enableButton = enableButtonObj.GetComponent<Button>();
            enableButton.onClick.AddListener(EnableChatting);
        }
    }
}
