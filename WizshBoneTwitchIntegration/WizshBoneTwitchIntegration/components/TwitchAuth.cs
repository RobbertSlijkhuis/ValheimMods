using System;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
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
        private string twitchStatus;
        private bool isLoginShown = false;
        public bool isLoggedIn = false;
        public bool isEnabled = false;

        public TwitchStateGUI loginGUI;

        void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
            m_customRewards = gameObject.GetComponent<TwitchCustomRewards>();
            loginGUI = new TwitchStateGUI();
            loginGUI.onLogin.AddListener(InvokeAuth);
            loginGUI.onEnable.AddListener(InvokeEnabled);
        }

        public void InvokeAuth()
        {
            InvokeRepeating(nameof(InitComponents), 0f, 0.3f);
        }

        public void InvokeEnabled()
        {
            SetEnabled(!isEnabled);
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

            m_customRewards.SubscribeToRedeemEvents();
            CancelInvoke(nameof(InitComponents));
        }

        public void SetEnabled(bool value)
        {
            isEnabled = value;

            if (m_customRewards == null)
                throw new Exception("Could not find custom rewards component!");
            if (value)
                m_customRewards.SetRewards();
            else
                m_customRewards.ClearRewards();
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
                    //AuthInfoTask.
                    //currentAuthState.Task.
                    Jotunn.Logger.LogWarning(AuthInfoTask.Exception);
                    Jotunn.Logger.LogWarning(currentAuthState.Exception);

                    SetEnabled(true);
                    isLoggedIn = true;
                    twitchStatus = TwitchStatusType.LoggedIn;
                    loginGUI.UpdateGUI();
                    Jotunn.Logger.LogWarning("User logged in");

                    m_chat.Connect();
                }

                if (currentAuthState.MaybeResult.Status == AuthStatus.LoggedOut)
                {
                    SetEnabled(false);
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
