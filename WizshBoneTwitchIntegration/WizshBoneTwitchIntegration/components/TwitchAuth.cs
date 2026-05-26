using Jotunn.Managers;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
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
        public TwitchCustomRewards m_customRewards;
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
        public WizshBoneHUD wizshBoneHUD;

        public void Awake()
        {
            try
            {
                m_chat = gameObject.GetComponent<TwitchChat>();
                m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();

                wizshBoneGUI = new WizshBoneGUI();
                wizshBoneGUI.onLogin.AddListener(InvokeAuth);
                wizshBoneGUI.onToggleRedeems.AddListener(ToggleRedeems);
                wizshBoneGUI.onToggleChatting.AddListener(ToggleChatting);

                wizshBoneHUD = new WizshBoneHUD();
                GUIManager.OnCustomGUIAvailable += OnGUIAvailable;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.Awake failed: " + e);
            }
        }

        private void OnGUIAvailable()
        {
            try
            {
                wizshBoneHUD.ShowHUD();
                GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.OnGUIAvailable failed: " + e);
            }
        }

        public void InvokeAuth()
        {
            CancelInvoke(nameof(InitLoginProcess));
            m_waitingForCode = false;
            InvokeRepeating(nameof(InitLoginProcess), 0f, 0.3f);
        }

        private void UpdateAllGUI()
        {
            wizshBoneGUI.UpdateGUI();
            wizshBoneHUD.UpdateHUD();
        }

        public void ToggleRedeems()
        {
            m_customRewards.SetEnableRedeems(!m_customRewards.m_enabled);
            UpdateAllGUI();
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

                if (PluginConfig.configEnableRedeemsOnLogin.Value)
                    m_customRewards.SetEnableRedeems(true);

                UpdateAllGUI();
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
            try
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(m_loggedinInTime);
                Jotunn.Logger.LogWarning(timeSpan.TotalMinutes);

                if (timeSpan.TotalMinutes > m_logOutTime)
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You will be logged out from Twitch in 15 minutes!", 10);

                StartCoroutine(TrackAuthState());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.TrackAuthRepeating failed: " + e);
            }
        }

        public IEnumerator TrackAuthState()
        {
            GetBitsLeaderboard();

            yield return new WaitForSeconds(5f);

            try
            {
                GetAuthState();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.TrackAuthState failed: " + e);
            }
        }

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
                    UpdateAllGUI();
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
                    UpdateAllGUI();
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
                    UpdateAllGUI();
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
            m_customRewards.ClearRewards();
            m_customRewards.UnSubscribeFromRedeemEvents();

            Twitch.API.LogOut();
            CancelInvoke(nameof(TrackAuthRepeating));
            GetAuthState();
        }

        public void LogoutBackToMainMenu()
        {
            TaskAwaiter awaiter = m_customRewards.ClearRewards();
            awaiter.OnCompleted(OnLogoutBackToMainMenu);
        }

        private void OnLogoutBackToMainMenu()
        {
            Twitch.API.LogOut();
            CancelInvoke(nameof(TrackAuthRepeating));
            GetAuthState();
            Jotunn.Logger.LogWarning("Logged out.., recalling: OnLogoutYes");
            Menu.instance.OnLogoutYes();
        }

        public void LogoutQuitApplication()
        {
            TaskAwaiter awaiter = m_customRewards.ClearRewards();
            awaiter.OnCompleted(ShowQuitMessage);
        }

        public void ShowQuitMessage()
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Quiting game now...", 10);
            Twitch.API.LogOut();
            CancelInvoke(nameof(TrackAuthRepeating));
            GetAuthState();
            Jotunn.Logger.LogWarning("Logged out.., recalling: OnQuitYes");
            Menu.instance.OnQuitYes();
        }

        public void GetBitsLeaderboard()
        {
            Jotunn.Logger.LogWarning("Checking logged in status...");
            Twitch.API.GetBitsLeaderboard();
        }
    }
}
