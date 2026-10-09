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
        private string m_scopes = $"{TwitchOAuthScope.Bits.Read.Scope} {TwitchOAuthScope.Channel.ManageRedemptions.Scope} {TwitchOAuthScope.User.ReadSubscriptions.Scope} {TwitchTokenCapture.ChatScopes}";
        public TwitchUserInfo m_userInfo;
        public bool m_loggedIn = false;
        // UTC so a clock/daylight-saving change mid-session can't skew the countdown.
        private DateTime m_loggedinInTime;
        private Coroutine m_trackRoutine;
        private Coroutine m_loginRoutine;
        private bool m_loginPolling;
        // Fallback only, used when the token response's expires_in wasn't captured (TwitchTokenCapture.ExpiresAtUtc is null).
        private const int FallbackTokenLifetimeMinutes = 240;
        // private const int FallbackTokenLifetimeMinutes = 16;
        // We log out this long before the real expiry, so the redeems can still be removed from Twitch with a valid token.
        private const int LogoutBeforeExpiryMinutes = 5;
        // Minutes-remaining marks (until that logout) at which the player is warned, once each per login (descending).
        private static readonly int[] WarnAtMinutesRemaining = { 15, 10, 5, 1 };
        private int m_lastWarnedMark = int.MaxValue;
        // Set once the pre-expiry logout has started, so the 60s tick doesn't start it twice.
        private bool m_limitLogoutStarted;
        // Set by the deliberate logout paths below so GetAuthState can tell them apart from the
        // session dropping on its own (token expiry) and only warn about the latter.
        private bool m_logoutRequested;
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
            m_waitingForCode = false;
            StartLoginPolling();
        }

        // Realtime coroutine rather than InvokeRepeating (scaled time), so the login is still picked up
        // while a single-player menu pauses the game. The flag - not just StopCoroutine - ends the loop,
        // because InitLoginProcess stops its own polling from inside the loop.
        private void StartLoginPolling()
        {
            StopLoginPolling();
            m_loginPolling = true;
            m_loginRoutine = StartCoroutine(LoginLoop());
        }

        private void StopLoginPolling()
        {
            m_loginPolling = false;

            if (m_loginRoutine == null)
                return;

            StopCoroutine(m_loginRoutine);
            m_loginRoutine = null;
        }

        private IEnumerator LoginLoop()
        {
            while (m_loginPolling)
            {
                InitLoginProcess();

                if (!m_loginPolling)
                    break;

                yield return new WaitForSecondsRealtime(1f);
            }
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

                StopLoginPolling();
                StartTracking();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while loggin in: " + e);
                StopLoginPolling();
                StopTracking();
            }
        }

        // Realtime rather than InvokeRepeating (scaled time), which stops while a single-player menu pauses the game.
        private void StartTracking()
        {
            StopTracking();
            m_trackRoutine = StartCoroutine(TrackAuthLoop());
        }

        private void StopTracking()
        {
            if (m_trackRoutine == null)
                return;

            StopCoroutine(m_trackRoutine);
            m_trackRoutine = null;
        }

        private IEnumerator TrackAuthLoop()
        {
            while (true)
            {
                TrackAuthRepeating();
                yield return new WaitForSecondsRealtime(60f);
            }
        }

        public void TrackAuthRepeating()
        {
            try
            {
                DateTime expiresAt = TwitchTokenCapture.ExpiresAtUtc ?? m_loggedinInTime.AddMinutes(FallbackTokenLifetimeMinutes);
                double minutesRemaining = expiresAt.AddMinutes(-LogoutBeforeExpiryMinutes).Subtract(DateTime.UtcNow).TotalMinutes;

                if (minutesRemaining <= 0)
                {
                    if (!m_limitLogoutStarted)
                        LogoutAtTokenLimit();

                    return;
                }

                // Smallest mark we've now passed but not yet warned about, so each mark fires once.
                int dueMark = int.MaxValue;
                foreach (int mark in WarnAtMinutesRemaining)
                {
                    if (minutesRemaining <= mark && mark < m_lastWarnedMark)
                        dueMark = mark;
                }

                if (dueMark != int.MaxValue)
                {
                    m_lastWarnedMark = dueMark;
                    int minutes = Math.Max(1, (int)Math.Ceiling(minutesRemaining));
                    ShowCenterMessage($"You will be logged out from Twitch in {minutes} minute{(minutes == 1 ? "" : "s")}!");
                }

                StartCoroutine(TrackAuthState());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.TrackAuthRepeating failed: " + e);
            }
        }

        // Reached LogoutBeforeExpiryMinutes before the real expiry: disable redeems and remove them from Twitch while the token is still valid, and
        // only then log out. Going through LogOutOfTwitch marks it deliberate, so GetAuthState doesn't show a second message.
        private void LogoutAtTokenLimit()
        {
            m_limitLogoutStarted = true;
            Jotunn.Logger.LogWarning("[WBTI] Twitch login reached its time limit, removing redeems and logging out.");
            ShowCenterMessage("You have been logged out from Twitch (login expired). Redeems and chat are inactive until you log in again.", 15);

            m_customRewards.m_enabled = false;
            TaskAwaiter awaiter = m_customRewards.ClearRewards();
            awaiter.OnCompleted(OnLimitRewardsCleared);
        }

        private void OnLimitRewardsCleared()
        {
            m_customRewards.UnSubscribeFromRedeemEvents();
            LogOutOfTwitch();
        }

        public IEnumerator TrackAuthState()
        {
            GetBitsLeaderboard();

            yield return new WaitForSecondsRealtime(5f);

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

                if (m_authState.MaybeResult.Status == AuthStatus.LoggedIn && !m_loggedIn)
                {
                    m_loggedinInTime = DateTime.UtcNow;
                    m_lastWarnedMark = int.MaxValue;
                    m_limitLogoutStarted = false;
                    m_logoutRequested = false;
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
                    TwitchTokenCapture.Clear();

                    if (!m_logoutRequested)
                    {
                        // The session dropped by itself (token expired): stop the per-minute poll and
                        // close the chat connection that was using the same token.
                        StopTracking();
                        if (m_chat)
                            m_chat.Disconnect();
                        Jotunn.Logger.LogWarning("[WBTI] Twitch session ended unexpectedly (access token expired).");
                        ShowCenterMessage("You have been logged out from Twitch (login expired). Redeems and chat are inactive until you log in again.", 15);
                    }

                    m_logoutRequested = false;
                    return;
                }

                if (m_authState.MaybeResult.Status == AuthStatus.WaitingForCode)
                {
                    if (m_waitingForCode)
                    {
                        if ((DateTime.UtcNow - m_waitingForCodeSince).TotalSeconds > 60)
                            ResetLoginProcess();

                        return;
                    }

                    TwitchOAuthScope tscopes = new TwitchOAuthScope(m_scopes);
                    AuthenticationInfo authInfo = Twitch.API.GetAuthenticationInfo(tscopes).MaybeResult;

                    if (authInfo == null)
                        throw new Exception("auth information is null while waiting for code");

                    Application.OpenURL($"{authInfo.Uri}");
                    m_waitingForCode = true;
                    m_waitingForCodeSince = DateTime.UtcNow;
                    return;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while getting auth state: " + e);
                StopLoginPolling();
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
                m_userInfo.loginName = userInfo.LoginName;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while getting user info: " + e);
                StopLoginPolling();
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

        private static void ShowCenterMessage(string message, int seconds = 10)
        {
            if (Player.m_localPlayer != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, message, seconds);
        }

        private void ResetLoginProcess()
        {
            StopLoginPolling();
            m_waitingForCode = false;
            m_authInfo = null;
            Twitch.API.LogOut();
        }

        public void Logout()
        {
            m_customRewards.ClearRewards();
            m_customRewards.UnSubscribeFromRedeemEvents();

            LogOutOfTwitch();
        }

        // Shared by every deliberate logout. m_logoutRequested keeps GetAuthState from reporting it as an expiry.
        private void LogOutOfTwitch()
        {
            m_logoutRequested = true;

            if (m_chat)
                m_chat.Disconnect();

            Twitch.API.LogOut();
            StopTracking();
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
            LogOutOfTwitch();
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
            LogOutOfTwitch();
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
            Twitch.API.GetBitsLeaderboard();
        }
    }
}
