using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Top bar: Twitch login status dot+label, login/logout button, and "Created by DeathWizsh"
    /// + the plugin version on the far right. Login state text/logic ported verbatim from
    /// GUI_OLD/tabs/HomeTab.cs (GetLoginButtonText/GetLoginStatusMessage/Login), except the
    /// unresolved-redeems logout guard's Cancel button just dismisses here rather than routing to
    /// a history view (out of scope this round).
    /// </summary>
    internal class ShellTopBar
    {
        public const float Height = 60f;

        private static readonly Color ColorLoggedIn = new Color(0.18f, 0.8f, 0.18f);
        private static readonly Color ColorLoggedOut = new Color(0.8f, 0.18f, 0.18f);

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        private TwitchAuth m_auth;
        private TwitchCustomRewards m_customRewards;

        private Image m_statusDot;
        private Text m_statusLabel;
        private Text m_loginButtonText;

        /// <summary>
        /// Builds the top bar region as a child of <paramref name="panel"/>, to the right of the
        /// sidebar.
        /// </summary>
        public GameObject Create(GameObject panel, float sidebarWidth, TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            m_auth = auth;
            m_customRewards = customRewards;
            m_confirmDialog.Init();

            GameObject root = GuiHelper.CreateRegion(
                panel, "TopBar",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(sidebarWidth, -Height),
                offsetMax: Vector2.zero);
            GuiHelper.AddBackground(root, new Color(0f, 0f, 0f, 0.35f));

            m_statusDot = StatusDot.Create(
                root,
                anchorMin: new Vector2(0f, 0.5f),
                anchorMax: new Vector2(0f, 0.5f),
                position:  new Vector2(24f, 0f),
                diameter:  14f,
                color:     ColorLoggedOut);

            m_statusLabel = GUIManager.Instance.CreateText(
                text:                "",
                parent:              root.transform,
                anchorMin:           new Vector2(0f, 0.5f),
                anchorMax:           new Vector2(0f, 0.5f),
                position:            new Vector2(160f, 0f),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            13,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               260f,
                height:              34f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_statusLabel.alignment = TextAnchor.MiddleLeft;

            GameObject loginBtnObj = GUIManager.Instance.CreateButton(
                text:      "",
                parent:    root.transform,
                anchorMin: new Vector2(0f, 0.5f),
                anchorMax: new Vector2(0f, 0.5f),
                position:  new Vector2(430f, 0f),
                width:     140f,
                height:    36f
            );
            loginBtnObj.SetActive(true);
            loginBtnObj.GetComponent<Button>().onClick.AddListener(Login);
            m_loginButtonText = loginBtnObj.GetComponentInChildren<Text>();

            Text credit = GUIManager.Instance.CreateText(
                text:                "Created by DeathWizsh",
                parent:              root.transform,
                anchorMin:           new Vector2(1f, 0.5f),
                anchorMax:           new Vector2(1f, 0.5f),
                position:            new Vector2(-170f, 0f),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            12,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               200f,
                height:              24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            credit.alignment = TextAnchor.MiddleRight;

            Text version = GUIManager.Instance.CreateText(
                text:                WizshBoneTwitchIntegration.PluginVersion,
                parent:              root.transform,
                anchorMin:           new Vector2(1f, 0.5f),
                anchorMax:           new Vector2(1f, 0.5f),
                position:            new Vector2(-30f, 0f),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            12,
                color:               GUIManager.Instance.ValheimOrange,
                outline:             true,
                outlineColor:        Color.black,
                width:               60f,
                height:              24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            version.alignment = TextAnchor.MiddleRight;

            Refresh();

            return root;
        }

        /// <summary>
        /// Re-reads <see cref="TwitchAuth"/> state and updates the dot/label/button text. Called by
        /// <see cref="WizshBoneGUI"/> every frame while the shell is visible, rather than TwitchAuth
        /// pushing a callback - keeps components/TwitchAuth.cs untouched by this round (see the
        /// round 2 plan for the rationale).
        /// </summary>
        public void Refresh()
        {
            bool fullyLoggedIn = m_auth.m_userInfo != null;

            m_statusDot.color = fullyLoggedIn ? ColorLoggedIn : ColorLoggedOut;
            m_statusLabel.text = GetLoginStatusMessage();
            m_statusLabel.color = fullyLoggedIn ? GUIManager.Instance.ValheimYellow : GUIManager.Instance.ValheimBeige;
            m_loginButtonText.text = GetLoginButtonText();
        }

        private void Login()
        {
            if (m_auth.m_loggedIn)
            {
                if (m_customRewards != null && m_customRewards.HasUnresolvedRedeems())
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
                        cancelText:  "Cancel");
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
