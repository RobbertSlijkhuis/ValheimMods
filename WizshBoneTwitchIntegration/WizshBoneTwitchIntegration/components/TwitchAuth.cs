using Jotunn.Managers;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchApi;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchAuth : MonoBehaviour
    {
        private TwitchChat m_chat;
        public TwitchCustomRewards m_customRewards;
        public TwitchUserInfo m_userInfo;
        public bool m_loggedIn = false;
        private Coroutine m_trackRoutine;
        // We log out this long before the real expiry, so the redeems can still be removed from Twitch with a valid token.
        private const int LogoutBeforeExpiryMinutes = 5;
        // Twitch wants a login that is kept to be validated when it starts and then every hour.
        private const int ValidateEveryMinutes = 60;
        // Polls in a row that get no answer from Twitch before the login gives up (about half a minute).
        private const int MaxUnreachablePolls = 6;
        private const int UserLookupAttempts = 3;
        private const int UserLookupRetryMilliseconds = 2000;
        // How long a logout or quit waits for the token to be revoked on Twitch's side before going on without it.
        private const float RevokeWaitSeconds = 3f;
        private const string RemovingRewardsMessage = "Removing redeems from Twitch, this could take a moment";
        private const string RewardsNotRemovedMessage = "Your redeems could not be confirmed removed from Twitch. Any left are cleaned up the next time you enable redeems.";
        // Minutes-remaining marks (until that logout) at which the player is warned, once each per login (descending).
        private static readonly int[] WarnAtMinutesRemaining = { 15, 10, 5, 1 };
        private int m_lastWarnedMark = int.MaxValue;
        // Set once the pre-expiry logout has started, so the 60s tick doesn't start it twice.
        private bool m_limitLogoutStarted;
        // Set by the deliberate logout paths below so OnSessionEnded can tell them apart from the
        // login being ended by Twitch (token expired or revoked) and only warn about the latter.
        private bool m_logoutRequested;
        // Set while a logout or quit waits for the redeems to be removed from Twitch (or for the player's answer).
        private bool m_logoutInProgress;
        public bool m_waitingForCode = false;

        // The login in progress. Every start of a login, and every end of a session, takes a new number, so a step
        // that was waiting (for Twitch, or for the player to authorize) sees that it was replaced and stops.
        private int m_loginGeneration;
        private bool m_loginInFlight;
        private TwitchOAuthClient.DeviceCode m_device;
        private DateTime m_deviceExpiresAtUtc;
        private DateTime m_lastValidatedUtc;
        private bool m_validating;
        // The revoke of the last deliberate logout; the logout and quit steps give it a moment to finish.
        private Task m_revokeTask;

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

        // A login that is still waiting (for Twitch, or for the player) must not finish on a component that is gone.
        // Destroyed while still logged in means the game connection was lost: Game.Logout() skips every logout path
        // above, so the login is ended here instead - on Twitch's side too, and nothing of it is carried to the next
        // world, which logs in again.
        private void OnDestroy()
        {
            m_loginGeneration++;
            GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;

            if (!m_loggedIn)
                return;

            m_loggedIn = false;
            _ = TwitchOAuthClient.RevokeAsync(TwitchSession.AccessToken);
            TwitchSession.Clear();
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

        /// <summary>
        /// The login button. Starts the login in the browser; while that waits for the player it opens the page again.
        /// </summary>
        public void InvokeAuth()
        {
            if (m_loggedIn)
                return;

            // Waiting for the player to authorize: the button opens the page of the same code again.
            if (m_waitingForCode && m_device != null && DateTime.UtcNow < m_deviceExpiresAtUtc)
            {
                Application.OpenURL(m_device.VerificationUri);
                return;
            }

            // A press while the code or the account is still being fetched would start a second login.
            if (m_loginInFlight)
                return;

            _ = LoginAsync();
        }

        // Runs on the main thread: it is started from InvokeAuth and resumes there after every await.
        private async Task LoginAsync()
        {
            int generation = ++m_loginGeneration;
            m_loginInFlight = true;

            try
            {
                m_waitingForCode = false;
                m_device = null;

                TwitchOAuthClient.DeviceCode device = await TwitchOAuthClient.RequestDeviceCodeAsync(TwitchApp.LoginScopes);

                if (generation != m_loginGeneration)
                    return;

                if (device == null)
                {
                    ToastNotifications.Show("Could not start the Twitch login. Check your connection and try again.", ToastType.Warning);
                    return;
                }

                m_device = device;
                m_deviceExpiresAtUtc = DateTime.UtcNow.AddSeconds(device.ExpiresInSeconds);
                m_waitingForCode = true;
                Application.OpenURL(device.VerificationUri);

                TwitchOAuthClient.PollResult granted = await WaitForAuthorizationAsync(device, generation);

                if (generation != m_loginGeneration)
                    return;

                m_waitingForCode = false;
                m_device = null;

                // Not authorized (the reason was shown): back to the login button.
                if (granted == null)
                    return;

                TwitchSession.Set(granted.AccessToken, granted.ExpiresInSeconds);
                await CompleteLoginAsync(generation);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while logging in to Twitch: " + e);

                if (generation == m_loginGeneration)
                {
                    m_waitingForCode = false;
                    m_device = null;
                }
            }
            finally
            {
                if (generation == m_loginGeneration)
                    m_loginInFlight = false;
            }
        }

        // Polls until the player has authorized, the code runs out, or the login is replaced. null when it did not
        // end with a token (the player was told why, except when the login was replaced).
        private async Task<TwitchOAuthClient.PollResult> WaitForAuthorizationAsync(TwitchOAuthClient.DeviceCode device, int generation)
        {
            int interval = device.IntervalSeconds;
            int unreachablePolls = 0;

            while (DateTime.UtcNow < m_deviceExpiresAtUtc)
            {
                await Task.Delay(interval * 1000);

                if (generation != m_loginGeneration)
                    return null;

                TwitchOAuthClient.PollResult result = await TwitchOAuthClient.PollAsync(device.Code, TwitchApp.LoginScopes);

                if (generation != m_loginGeneration)
                    return null;

                // Twitch answered (even if only "not yet"): the connection is fine.
                if (result.Status != TwitchOAuthClient.PollStatus.Unreachable)
                    unreachablePolls = 0;

                switch (result.Status)
                {
                    case TwitchOAuthClient.PollStatus.Granted:
                        return result;

                    case TwitchOAuthClient.PollStatus.SlowDown:
                        interval += 5;
                        break;

                    // Waiting would only look like waiting for the player: say it when Twitch stays out of reach.
                    case TwitchOAuthClient.PollStatus.Unreachable:
                        Jotunn.Logger.LogWarning($"[WBTI] Could not reach Twitch to check the login: {result.Message}");

                        if (++unreachablePolls >= MaxUnreachablePolls)
                        {
                            ToastNotifications.Show("Could not reach Twitch to finish the login. Check your connection and press Twitch Login to try again.", ToastType.Warning);
                            return null;
                        }

                        break;

                    case TwitchOAuthClient.PollStatus.Failed:
                        Jotunn.Logger.LogWarning($"[WBTI] The Twitch login was not completed: {result.Message}");
                        ToastNotifications.Show("The Twitch login was not completed. Press Twitch Login to try again.", ToastType.Warning);
                        return null;
                }
            }

            Jotunn.Logger.LogWarning("[WBTI] The Twitch login was not authorized in time.");
            ToastNotifications.Show("The Twitch login timed out. Press Twitch Login to try again.", ToastType.Warning);
            return null;
        }

        // The token is in TwitchSession: look up who it belongs to and start everything that needs the login.
        private async Task CompleteLoginAsync(int generation)
        {
            m_lastValidatedUtc = DateTime.UtcNow;
            m_lastWarnedMark = int.MaxValue;
            m_limitLogoutStarted = false;
            m_logoutRequested = false;
            m_loggedIn = true;

            // A failed lookup is tried again a few times: giving up ends (and revokes) a login that is fine.
            TwitchUserInfo user = null;

            for (int attempt = 0; attempt < UserLookupAttempts && user == null; attempt++)
            {
                if (attempt > 0)
                    await Task.Delay(UserLookupRetryMilliseconds);

                // Logged out (or logging in again) in the meantime: that already ended this session.
                if (generation != m_loginGeneration || !m_loggedIn)
                    return;

                user = await TwitchUserClient.GetMyUserAsync();
            }

            if (generation != m_loginGeneration || !m_loggedIn)
                return;

            if (user == null)
            {
                ToastNotifications.Show("Could not read your Twitch account. Press Twitch Login to try again.", ToastType.Warning);
                DropSession(deliberate: true);
                return;
            }

            m_userInfo = user;

            // The channel name is only known now, and claim ownership (TwitchChatting.ClaimChannel)
            // depends on it - number the claims this client already holds / can adopt.
            gameObject.GetComponent<TwitchChatting>()?.RefreshAllDisplayIndexes();

            FinishLogin();
        }

        private void FinishLogin()
        {
            // Tracking first: it holds the expiry warnings, the logout before the token ends (which removes the redeems)
            // and the hourly validation, so a step below that throws must not leave a login without them.
            StartTracking();

            try
            {
                m_chat.LogIn();
                m_customRewards.SubscribeToRedeemEvents();

                if (ProfileSettingsHelper.Current.enableRedeemsOnLogin)
                    m_customRewards.SetEnableRedeems(true);   // looks up the channel's other rewards as part of the push
                else
                    m_customRewards.RefreshChannelRewardCounts();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while loggin in: " + e);
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
                // A login always has its expiry in the session; none means it just ended.
                DateTime? expiresAt = TwitchSession.ExpiresAtUtc;

                if (!expiresAt.HasValue)
                    return;

                double minutesRemaining = expiresAt.Value.AddMinutes(-LogoutBeforeExpiryMinutes).Subtract(DateTime.UtcNow).TotalMinutes;

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

                if (!m_validating && DateTime.UtcNow - m_lastValidatedUtc >= TimeSpan.FromMinutes(ValidateEveryMinutes))
                    _ = ValidateSessionAsync();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.TrackAuthRepeating failed: " + e);
            }
        }

        // Asks Twitch whether the login still stands. It stops standing without its time running out when the player
        // disconnects the app on twitch.tv, which nothing else would notice. The next check is an hour after an answer;
        // when Twitch could not be reached it is tried again on the next tick. Never throws.
        private async Task ValidateSessionAsync()
        {
            m_validating = true;

            try
            {
                string token = TwitchSession.AccessToken;
                int generation = m_loginGeneration;

                if (string.IsNullOrEmpty(token))
                    return;

                TwitchOAuthClient.ValidateResult result = await TwitchOAuthClient.ValidateAsync(token);

                // Logged out, logged in again or destroyed while the request was out: the answer is about a login that
                // is over, and must not end the session of whatever replaced it.
                if (generation != m_loginGeneration || !m_loggedIn || TwitchSession.AccessToken != token)
                    return;

                if (result.State != TwitchOAuthClient.TokenState.Unknown)
                    m_lastValidatedUtc = DateTime.UtcNow;

                if (result.State == TwitchOAuthClient.TokenState.Invalid)
                {
                    Jotunn.Logger.LogWarning("[WBTI] Twitch no longer accepts this login.");
                    OnSessionEnded();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchAuth.ValidateSessionAsync failed: " + e);
            }
            finally
            {
                m_validating = false;
            }
        }

        // Reached LogoutBeforeExpiryMinutes before the real expiry: disable redeems and remove them from Twitch while the token is still valid, and
        // only then log out. Going through LogOutOfTwitch marks it deliberate, so OnSessionEnded doesn't show a second message.
        private void LogoutAtTokenLimit()
        {
            m_limitLogoutStarted = true;
            Jotunn.Logger.LogWarning("[WBTI] Twitch login reached its time limit, removing redeems and logging out.");
            ShowCenterMessage("You have been logged out from Twitch (login expired). Redeems and chat are inactive until you log in again.", 15);

            m_customRewards.m_enabled = false;
            StartCoroutine(WaitForClear(m_customRewards.ClearRewards(), OnLimitRewardsCleared));
        }

        private void OnLimitRewardsCleared(bool removed)
        {
            if (!removed)
                ShowCenterMessage(RewardsNotRemovedMessage, 15);

            m_customRewards.UnSubscribeFromRedeemEvents();
            LogOutOfTwitch();
        }

        // Waits for ClearRewards to finish and tells onDone whether every reward is confirmed gone. It always
        // ends: each Twitch request has its own time limit, and the clear stops at the first one that gets no answer.
        private IEnumerator WaitForClear(Task<bool> clear, Action<bool> onDone)
        {
            while (!clear.IsCompleted)
                yield return null;

            bool removed = !clear.IsFaulted && clear.Result;

            if (!removed)
                Jotunn.Logger.LogWarning("[WBTI] Not all redeems could be confirmed removed from Twitch before logging out.");

            onDone(removed);
        }

        // Removes the redeems from Twitch and only then calls proceed(removed). When some could not be removed the
        // game doesn't carry on by itself: the player picks Retry or continuing anyway (named by anywayText).
        private void ClearBeforeExit(string anywayText, Action<bool> proceed)
        {
            // Already logged out (the pre-expiry logout finished first): the redeems are gone and the token is wiped,
            // so a clear would only fail and ask the player to retry something that can't work.
            if (!m_loggedIn)
            {
                proceed(true);
                return;
            }

            m_logoutProgressHUD.Show(this, RemovingRewardsMessage);
            StartCoroutine(WaitForClear(m_customRewards.ClearRewards(), removed => OnExitClearDone(removed, anywayText, proceed)));
        }

        private void OnExitClearDone(bool removed, string anywayText, Action<bool> proceed)
        {
            if (removed)
            {
                proceed(true);
                return;
            }

            m_logoutProgressHUD.Hide();
            WizshBoneGUI gui = gameObject.GetComponent<WizshBoneGUI>();

            bool asked = gui != null && gui.ShowExitConfirm(
                "Redeems not removed",
                "Your redeems could not be removed from Twitch. Retry, or continue and have them cleaned up the next time you enable redeems.",
                onConfirm:   () => ClearBeforeExit(anywayText, proceed),
                onCancel:    () =>
                {
                    // The steps after this one update the message on this panel, which only works while it is up.
                    m_logoutProgressHUD.Show(this, anywayText + "...");
                    proceed(false);
                },
                confirmText: "Retry",
                cancelText:  anywayText);

            // Nothing to ask with (the GUI isn't up): carry on, the redeems are cleaned up at the next enable.
            if (!asked)
                proceed(false);
        }

        // The login is over: reset everything that depended on it. Reached from a deliberate logout (DropSession) and,
        // with m_logoutRequested false, when Twitch ended the login by itself (it expired or was revoked).
        private void OnSessionEnded()
        {
            if (!m_loggedIn)
                return;

            // Whatever is still waiting on this login (the account lookup) must not finish it.
            m_loginGeneration++;
            m_loginInFlight = false;

            m_loggedIn = false;
            m_waitingForCode = false;
            m_device = null;
            m_userInfo = null;
            TwitchSession.Clear();

            if (m_customRewards)
            {
                m_customRewards.ClearChannelRewardCounts();
                m_customRewards.UnSubscribeFromRedeemEvents();

                // Every deliberate logout removes the rewards from Twitch first, so the UI mustn't
                // keep showing redeems as enabled (a re-login without enable-on-login would never
                // push them again). After the login was ended by Twitch the rewards are still live on
                // Twitch and keep being received after the next login, so the flag stays as it is.
                if (m_logoutRequested)
                    m_customRewards.m_enabled = false;
            }

            if (!m_logoutRequested)
            {
                // The login was ended by Twitch: stop the per-minute poll and close the chat connection
                // that was using the same token.
                StopTracking();
                if (m_chat)
                    m_chat.Disconnect();
                Jotunn.Logger.LogWarning("[WBTI] Twitch session ended unexpectedly (access token expired or revoked).");
                ShowCenterMessage("You have been logged out from Twitch (login expired). Redeems and chat are inactive until you log in again.", 15);
            }

            m_logoutRequested = false;
        }

        private static void ShowCenterMessage(string message, int seconds = 10)
        {
            if (Player.m_localPlayer != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, message, seconds);
        }

        public void Logout()
        {
            // A second click while the rewards are still being removed would start a second clear.
            if (m_logoutInProgress)
                return;

            m_logoutInProgress = true;
            ClearBeforeExit("Logout anyway", OnLogoutRewardsCleared);
        }

        private void OnLogoutRewardsCleared(bool removed)
        {
            m_logoutProgressHUD.Hide();
            m_logoutInProgress = false;

            if (!removed)
                ShowCenterMessage(RewardsNotRemovedMessage, 15);

            m_customRewards.UnSubscribeFromRedeemEvents();
            LogOutOfTwitch();
        }

        // Shared by every deliberate logout.
        private void LogOutOfTwitch()
        {
            // Another logout got here first (e.g. the pre-expiry one while the player chose Logout or Quit).
            if (!m_loggedIn)
                return;

            if (m_chat)
                m_chat.Disconnect();

            StopTracking();
            DropSession(deliberate: true);
        }

        // Ends the login. A deliberate one also revokes the token on Twitch's side (best effort: the login is over
        // either way) so it cannot be used anymore; m_logoutRequested keeps OnSessionEnded from reporting it as an expiry.
        private void DropSession(bool deliberate)
        {
            m_logoutRequested = deliberate;

            if (deliberate)
                m_revokeTask = TwitchOAuthClient.RevokeAsync(TwitchSession.AccessToken);

            OnSessionEnded();
        }

        public void LogoutBackToMainMenu()
        {
            // Reset by DelayedLogout just before the vanilla logout runs.
            if (m_logoutInProgress)
                return;

            m_logoutInProgress = true;
            ClearBeforeExit("Logout anyway", OnLogoutBackToMainMenu);
        }

        private void OnLogoutBackToMainMenu(bool removed)
        {
            LogOutOfTwitch();
            StartCoroutine(DelayedLogout(removed));
        }

        private IEnumerator DelayedLogout(bool removed)
        {
            yield return ShowLogoutStep("Logging out...", removed);
            m_logoutProgressHUD.Hide();

            // The patch on OnLogoutYes starts this logout again while we are still logged in; the guard must be
            // off by then, or that call would return and the vanilla logout would never run.
            m_logoutInProgress = false;
            Menu.instance.OnLogoutYes();
        }

        public void LogoutQuitApplication()
        {
            // Reset by DelayedQuit just before the vanilla quit runs.
            if (m_logoutInProgress)
                return;

            m_logoutInProgress = true;
            ClearBeforeExit("Quit anyway", OnQuitRewardsCleared);
        }

        private void OnQuitRewardsCleared(bool removed)
        {
            LogOutOfTwitch();
            StartCoroutine(DelayedQuit(removed));
        }

        private IEnumerator DelayedQuit(bool removed)
        {
            yield return ShowLogoutStep("Quitting game...", removed);

            // Same as DelayedLogout: the patch on OnQuitYes must not find the guard still set.
            m_logoutInProgress = false;
            Menu.instance.OnQuitYes();
        }

        // The last HUD message before the game goes on. When the redeems couldn't be removed it says so and stays
        // long enough to read.
        private IEnumerator ShowLogoutStep(string message, bool rewardsRemoved)
        {
            m_logoutProgressHUD.UpdateMessage(rewardsRemoved ? message : RewardsNotRemovedMessage + " " + message);
            yield return new WaitForSecondsRealtime(rewardsRemoved ? 1f : 4f);

            // A quit right after must not cut the revoke off: the token would stay valid on Twitch for hours.
            float until = Time.realtimeSinceStartup + RevokeWaitSeconds;

            while (m_revokeTask != null && !m_revokeTask.IsCompleted && Time.realtimeSinceStartup < until)
                yield return null;
        }
    }
}
