using Jotunn.Managers;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
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
        private DateTime m_waitingForCodeSince;

        // LogoutProgressHUD is the one Gui.* panel that still lives here - Show/UpdateMessage/Hide
        // are called directly from this class's own logout/quit sequence below, unlike
        // WizshBoneHUD/ConfirmDialog which moved to WizshBoneGUI (see its own doc comment).
        public LogoutProgressHUD m_logoutProgressHUD = new LogoutProgressHUD();

        public void Awake()
        {
            try
            {
                m_chat = gameObject.GetComponent<TwitchChat>();
                m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();

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
                m_logoutProgressHUD.Init();
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
            InvokeRepeating(nameof(InitLoginProcess), 0f, 1f);
        }

        public void ToggleRedeems()
        {
            m_customRewards.SetEnableRedeems(!m_customRewards.m_enabled);
        }

        public void ToggleChatting()
        {
            TwitchChatting chatting = gameObject.GetComponent<TwitchChatting>();
            chatting.m_enabled = !chatting.m_enabled;
            ProfileSettingsHelper.Current.chattingEnabled = chatting.m_enabled;
            ProfileSettingsHelper.Save();
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

                if (ProfileSettingsHelper.Current.enableRedeemsOnLogin)
                    m_customRewards.SetEnableRedeems(true);

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
                    return;
                }

                if (m_authState.MaybeResult.Status == AuthStatus.WaitingForCode)
                {
                    if (m_waitingForCode)
                    {
                        if ((DateTime.Now - m_waitingForCodeSince).TotalSeconds > 60)
                            ResetLoginProcess();

                        return;
                    }

                    TwitchOAuthScope tscopes = new TwitchOAuthScope(m_scopes);
                    AuthenticationInfo authInfo = Twitch.API.GetAuthenticationInfo(tscopes).MaybeResult;

                    if (authInfo == null)
                        throw new Exception("auth information is null while waiting for code");

                    Application.OpenURL($"{authInfo.Uri}");
                    m_waitingForCode = true;
                    m_waitingForCodeSince = DateTime.Now;
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

        private void ResetLoginProcess()
        {
            Jotunn.Logger.LogWarning("[WBTI] TwitchAuth: Login timed out waiting for browser authorization, resetting.");
            CancelInvoke(nameof(InitLoginProcess));
            m_waitingForCode = false;
            m_authInfo = null;
            Twitch.API.LogOut();
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
            m_logoutProgressHUD.Show(this, "Removing redeems from Twitch, this could take a moment");
            TaskAwaiter awaiter = m_customRewards.ClearRewards();
            awaiter.OnCompleted(OnLogoutBackToMainMenu);
        }

        private void OnLogoutBackToMainMenu()
        {
            Twitch.API.LogOut();
            CancelInvoke(nameof(TrackAuthRepeating));
            GetAuthState();
            Jotunn.Logger.LogWarning("Logged out.., recalling: OnLogoutYes");
            StartCoroutine(DelayedLogout());
        }

        private IEnumerator DelayedLogout()
        {
            m_logoutProgressHUD.UpdateMessage("Logging out...");
            yield return new WaitForSecondsRealtime(1f);
            m_logoutProgressHUD.Hide();
            Menu.instance.OnLogoutYes();
        }

        public void LogoutQuitApplication()
        {
            m_logoutProgressHUD.Show(this, "Removing redeems from Twitch, this could take a moment");
            TaskAwaiter awaiter = m_customRewards.ClearRewards();
            awaiter.OnCompleted(ShowQuitMessage);
        }

        public void ShowQuitMessage()
        {
            Twitch.API.LogOut();
            CancelInvoke(nameof(TrackAuthRepeating));
            GetAuthState();
            Jotunn.Logger.LogWarning("Logged out.., recalling: OnQuitYes");
            StartCoroutine(DelayedQuit());
        }

        private IEnumerator DelayedQuit()
        {
            m_logoutProgressHUD.UpdateMessage("Quitting game...");
            yield return new WaitForSecondsRealtime(1f);
            Menu.instance.OnQuitYes();
        }

        public void GetBitsLeaderboard()
        {
            Jotunn.Logger.LogWarning("Checking logged in status...");
            Twitch.API.GetBitsLeaderboard();
        }
    }
}
