using System;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchAuth : MonoBehaviour
    {
        private TwitchChat m_chat;
        private TwitchCustomRewards m_customRewards;
        private GameTask<AuthenticationInfo> AuthInfoTask;
        private GameTask<AuthState> currentAuthState;

        public TwitchUserInfo m_userInfo;
        public bool m_loggedIn = false;
        public bool m_waitingForCode = false;

        public TwitchStateGUI loginGUI;

        void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
            m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();

            loginGUI = new TwitchStateGUI();
            loginGUI.onLogin.AddListener(InvokeAuth);
            loginGUI.onToggleRedeems.AddListener(ToggleRedeems);
            loginGUI.onToggleChatting.AddListener(ToggleChatting);
        }

        public void InvokeAuth()
        {
            InvokeRepeating(nameof(InitLoginProcess), 0f, 0.3f);
        }

        public void ToggleRedeems()
        {
            m_customRewards.SetEnableRedeems(!m_customRewards.m_enabled);
        }

        public void ToggleChatting()
        {
            TwitchChatting chatting = gameObject.GetComponent<TwitchChatting>();
            chatting.m_enabled = !chatting.m_enabled;
            PluginConfig.configChattingEnabled.Value = chatting.m_enabled;
        }

        public void InitLoginProcess()
        {
            try
            {
                if (AuthInfoTask == null)
                {
                    GetAuthInformation();
                    return;
                }

                if (!m_loggedIn)
                {
                    GetAuthState();
                    return;
                }
                else if (m_userInfo == null)
                {
                    GetUserInfo();
                    return;
                }

                m_chat.Connect();
                m_customRewards.SubscribeToRedeemEvents();
                loginGUI.UpdateGUI();
                CancelInvoke(nameof(InitLoginProcess));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while loggin in: " + e);
                CancelInvoke(nameof(InitLoginProcess));
            }
        }

        public void GetAuthState()
        {
            try
            {
                currentAuthState = Twitch.API.GetAuthState();

                if (currentAuthState == null)
                {
                    Jotunn.Logger.LogError("Current auth state is null");
                    return;
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.LoggedIn && !m_loggedIn)
                {
                    m_loggedIn = true;
                    m_waitingForCode = false;
                    m_customRewards.SetEnableRedeems(true);
                    loginGUI.UpdateGUI();
                    return;
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.LoggedOut)
                {

                    m_loggedIn = false;
                    m_waitingForCode = false;
                    m_customRewards.SetEnableRedeems(false);
                    loginGUI.UpdateGUI();
                    return;
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.WaitingForCode)
                {
                    AuthenticationInfo authInfo = Twitch.API.GetAuthenticationInfo(TwitchOAuthScope.Bits.Read).MaybeResult;

                    if (authInfo != null && !m_waitingForCode)
                    {
                        Application.OpenURL($"{authInfo.Uri}");
                        m_waitingForCode = true;
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while getting auth state: " + e);
                CancelInvoke(nameof(InitLoginProcess));
            }
        }

        public void GetUserInfo()
        {
            try
            {
                TwitchSDK.Interop.UserInfo userInfo = Twitch.API.GetMyUserInfo().MaybeResult;

                if (userInfo == null)
                {
                    Jotunn.Logger.LogError("UserInfo is null");
                    return;
                }

                if (userInfo != null)
                {
                    m_userInfo = new TwitchUserInfo();
                    m_userInfo.broadcasterType = userInfo.BroadcasterType;
                    m_userInfo.displayName = userInfo.DisplayName;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while getting user info: " + e);
                CancelInvoke(nameof(InitLoginProcess));
            }
        }

        public void GetAuthInformation()
        {
            if (AuthInfoTask == null)
            {
                // This example uses all scopes, we suggest you only request the scopes you actively need.
                // var scopes = TwitchOAuthScope.Bits.Read.Scope + " " + TwitchOAuthScope.Channel.ManageBroadcast.Scope + " " + TwitchOAuthScope.Channel.ManagePolls.Scope + " " + TwitchOAuthScope.Channel.ManagePredictions.Scope + " " + TwitchOAuthScope.Channel.ManageRedemptions.Scope + " " + TwitchOAuthScope.Channel.ReadHypeTrain.Scope + " " + TwitchOAuthScope.Clips.Edit.Scope + " " + TwitchOAuthScope.User.ReadSubscriptions.Scope;
                string scopes = $"{TwitchOAuthScope.Bits.Read.Scope} {TwitchOAuthScope.Channel.ManageRedemptions.Scope} {TwitchOAuthScope.User.ReadSubscriptions.Scope}";
                TwitchOAuthScope tscopes = new TwitchOAuthScope(scopes);
                AuthInfoTask = Twitch.API.GetAuthenticationInfo(tscopes);
            }
        }
    }
}
