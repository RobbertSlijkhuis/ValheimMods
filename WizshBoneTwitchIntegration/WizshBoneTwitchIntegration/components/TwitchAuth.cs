using System;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchAuth : MonoBehaviour
    {
        private TwitchChat m_chat;
        private TwitchCustomRewards m_customRewards;
        private GameTask<AuthenticationInfo> AuthInfoTask;
        private GameTask<AuthState> currentAuthState;
        public GameTask<TwitchSDK.Interop.UserInfo> userInfo;
        private string twitchStatus;
        private bool isLoginShown = false;
        public bool isLoggedIn = false;
        public bool retrievedUserInfo = false;
        public string displayName;

        public TwitchStateGUI loginGUI;

        void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
            m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();
            loginGUI = new TwitchStateGUI();
            loginGUI.onLogin.AddListener(InvokeAuth);
            loginGUI.onEnableRedeems.AddListener(InvokeEnableRedeems);
            loginGUI.onEnableChatting.AddListener(InvokeEnableChatting);
        }

        public void InvokeAuth()
        {
            InvokeRepeating(nameof(InitComponents), 0f, 0.3f);
        }

        public void InvokeEnableRedeems()
        {
            m_customRewards.SetEnableRedeems(!m_customRewards.isEnabled);
        }

        public void InvokeEnableChatting()
        {
            TwitchChatting chatting = gameObject.GetComponent<TwitchChatting>();
            chatting.m_enabled = !chatting.m_enabled;
            PluginConfig.configChattingEnabled.Value = chatting.m_enabled;
        }

        public void InitComponents()
        {
            Jotunn.Logger.LogWarning("LOGIN");

            if (AuthInfoTask == null)
            {
                GetAuthInformation();
                return;
            }

            if (!isLoggedIn)
            {
                UpdateAuthState();
                return;
            }
            else
            {
                GetMyUserInfo();
            }

            if (!retrievedUserInfo)
                return;

            m_chat.Connect();
            m_customRewards.SubscribeToRedeemEvents();
            CancelInvoke(nameof(InitComponents));
            loginGUI.UpdateGUI();
        }

        public void UpdateAuthState()
        {
            try
            {
                currentAuthState = Twitch.API.GetAuthState();

                if (currentAuthState == null)
                {
                    Jotunn.Logger.LogError("curAuthState is null");
                    return;
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.LoggedIn)
                {
                    m_customRewards.SetEnableRedeems(true);
                    isLoggedIn = true;
                    twitchStatus = TwitchStatusType.LoggedIn;
                    loginGUI.UpdateGUI();
                    Jotunn.Logger.LogWarning("User logged in");
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.LoggedOut)
                {
                    m_customRewards.SetEnableRedeems(false);
                    isLoggedIn = false;
                    isLoginShown = false;
                    twitchStatus = TwitchStatusType.LoggedOut;
                    loginGUI.UpdateGUI();
                    Jotunn.Logger.LogWarning("User logged out");
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.WaitingForCode)
                {
                    var UserAuthInfo = Twitch.API.GetAuthenticationInfo(TwitchOAuthScope.Bits.Read).MaybeResult;

                    if (UserAuthInfo == null)
                    {
                        // User is still loading
                        //Jotunn.Logger.LogWarning("Loading...");
                    }

                    Jotunn.Logger.LogWarning("Asking for login: ");
                    twitchStatus = TwitchStatusType.WaitingForCode;

                    if (!isLoginShown)
                    {
                        Application.OpenURL($"{UserAuthInfo.Uri}");
                        isLoginShown = true;
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
                CancelInvoke(nameof(InitComponents));
            }
        }

        public void GetMyUserInfo()
        {
            try
            {
                userInfo = Twitch.API.GetMyUserInfo();

                if (userInfo == null)
                {
                    Jotunn.Logger.LogError("userInfo is null");
                    return;
                }

                if (userInfo.MaybeResult != null)
                {
                    displayName = userInfo.MaybeResult.DisplayName;
                    Jotunn.Logger.LogWarning(displayName);
                    retrievedUserInfo = true;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
                CancelInvoke(nameof(InitComponents));
            }
        }

        public void GetAuthInformation()
        {
            if (AuthInfoTask == null)
            {
                // This example uses all scopes, we suggest you only request the scopes you actively need.
                // var scopes = TwitchOAuthScope.Bits.Read.Scope + " " + TwitchOAuthScope.Channel.ManageBroadcast.Scope + " " + TwitchOAuthScope.Channel.ManagePolls.Scope + " " + TwitchOAuthScope.Channel.ManagePredictions.Scope + " " + TwitchOAuthScope.Channel.ManageRedemptions.Scope + " " + TwitchOAuthScope.Channel.ReadHypeTrain.Scope + " " + TwitchOAuthScope.Clips.Edit.Scope + " " + TwitchOAuthScope.User.ReadSubscriptions.Scope;
                var scopes = $"{TwitchOAuthScope.Bits.Read.Scope} {TwitchOAuthScope.Channel.ManageRedemptions.Scope} {TwitchOAuthScope.User.ReadSubscriptions.Scope}";
                TwitchOAuthScope tscopes = new TwitchOAuthScope(scopes);
                AuthInfoTask = Twitch.API.GetAuthenticationInfo(tscopes);
            }
        }
    }
}
