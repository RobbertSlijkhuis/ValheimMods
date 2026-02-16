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
        private GameTask<AuthenticationInfo> m_authInfo;
        private GameTask<AuthState> m_authState;
        private string m_scopes = $"{TwitchOAuthScope.Bits.Read.Scope} {TwitchOAuthScope.Channel.ManageRedemptions.Scope} {TwitchOAuthScope.User.ReadSubscriptions.Scope}";
        public TwitchUserInfo m_userInfo;
        public bool m_loggedIn = false;
        private DateTime m_loggedinInTime;
        private int m_logOutTime = 225;
        // private int m_logOutTime = 1;
        public bool m_waitingForCode = false;

        public WizshBoneGUI wizshBoneGUI;

        public void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
            m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();

            wizshBoneGUI = new WizshBoneGUI();
            wizshBoneGUI.onLogin.AddListener(InvokeAuth);
            wizshBoneGUI.onToggleRedeems.AddListener(ToggleRedeems);
            wizshBoneGUI.onToggleChatting.AddListener(ToggleChatting);
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
                if (m_authInfo == null)
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
                wizshBoneGUI.UpdateGUI();
                CancelInvoke(nameof(InitLoginProcess));
                InvokeRepeating(nameof(TrackAuthRepeating), 0, 60f);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while loggin in: " + e);
                CancelInvoke(nameof(InitLoginProcess));
                CancelInvoke(nameof(TrackAuthRepeating));
            }
        }

        public void TrackAuthRepeating()
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(m_loggedinInTime);
            Jotunn.Logger.LogWarning(timeSpan.TotalMinutes);

            if (timeSpan.TotalMinutes > m_logOutTime)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You are about to be logged out from Twitch, do you want to refresh login?", 10);

            //StartCoroutine(TrackAuthState());
        }

        //public IEnumerator TrackAuthState()
        //{
        //    GetBitsLeaderboard();

        //    yield return new WaitForSeconds(5f);

        //    GetAuthState();
        //}

        public void GetAuthState()
        {
            try
            {
                m_authState = Twitch.API.GetAuthState();

                if (m_authState == null || m_authState.MaybeResult == null)
                {
                    Jotunn.Logger.LogError("Current auth state is null");
                    return;
                }

                Jotunn.Logger.LogWarning(m_authState.MaybeResult.Status);

                if (m_authState.MaybeResult.Status == AuthStatus.LoggedIn && !m_loggedIn)
                {
                    if (m_loggedIn)
                        return;

                    m_loggedinInTime = DateTime.Now;
                    m_loggedIn = true;
                    m_waitingForCode = false;

                    if (PluginConfig.configEnableRedeemsOnLogin.Value)
                        m_customRewards.SetEnableRedeems(true);

                    wizshBoneGUI.UpdateGUI();
                    return;
                }

                if (m_authState.MaybeResult.Status == AuthStatus.LoggedOut)
                {
                    if (!m_loggedIn)
                        return;

                    m_loggedIn = false;
                    m_waitingForCode = false;
                    m_authInfo = null;
                    m_userInfo = null;
                    m_customRewards.SetEnableRedeems(false);
                    wizshBoneGUI.UpdateGUI();

                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "YOU ARE NO LONGER LOGGED IN INTO TWITCH!", 3);
                    return;
                }

                if (m_authState.MaybeResult.Status == AuthStatus.WaitingForCode)
                {
                    if (m_waitingForCode)
                        return;

                    TwitchOAuthScope tscopes = new TwitchOAuthScope(m_scopes);
                    AuthenticationInfo authInfo = Twitch.API.GetAuthenticationInfo(tscopes).MaybeResult;

                    if (authInfo == null)
                        throw new Exception("auth information is null while waiting for code");

                    Application.OpenURL($"{authInfo.Uri}");
                    m_waitingForCode = true;
                    wizshBoneGUI.UpdateGUI();
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
                    throw new Exception("UserInfo is null");

                m_userInfo = new TwitchUserInfo();
                m_userInfo.broadcasterType = userInfo.BroadcasterType;
                m_userInfo.displayName = userInfo.DisplayName;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while getting user info: " + e);
                CancelInvoke(nameof(InitLoginProcess));
            }
        }

        public void GetAuthInformation()
        {
            if (m_authInfo == null)
            {
                TwitchOAuthScope tscopes = new TwitchOAuthScope(m_scopes);
                m_authInfo = Twitch.API.GetAuthenticationInfo(tscopes);
            }
        }

        public void Logout()
        {
            Twitch.API.LogOut();
            m_customRewards.UnSubscribeFromRedeemEvents();
            GetAuthState();
            CancelInvoke(nameof(TrackAuthRepeating));
        }

        public void GetBitsLeaderboard()
        {
            Jotunn.Logger.LogWarning("Checking logged in status...");
            Twitch.API.GetBitsLeaderboard();
        }
    }
}
