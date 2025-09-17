using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.GUI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchAuth : MonoBehaviour
    {
        GameTask<AuthenticationInfo> AuthInfoTask;
        GameTask<AuthState> curAuthState;
        bool isLoginShown = false;
        bool isRewardsSetup = false;
        public bool isLoggedIn = false;

        public LoginGUI loginGUI;

        void Awake()
        {
            GetAuthInformation();
            // GUI
            loginGUI = new LoginGUI();
            loginGUI.onAccept.AddListener(OnAcceptSettings);
        }

        void Update()
        {
            UpdateAuthState();
        }

        public void InvokeLogin()
        {
            if (isLoggedIn)
                return;

            InvokeRepeating(nameof(UpdateAuthState), 0f, 0.3f);
        }

        public void UpdateAuthState()
        {
            if (isLoggedIn)
                return;

            curAuthState = Twitch.API.GetAuthState();

            if (curAuthState == null)
            {
                Jotunn.Logger.LogWarning("curAuthState is null");
                return;
            }

            if (curAuthState.MaybeResult.Status == AuthStatus.LoggedIn)
            {
                isLoggedIn = true;
                Jotunn.Logger.LogWarning("User logged in");

                if (!isRewardsSetup)
                    return;

                TwitchCustomRewards rewardsComp = gameObject.GetComponent<TwitchCustomRewards>();
                if (rewardsComp)
                {
                    rewards.SetSampleRewards();
                    //Jotunn.Logger.LogWarning("Rewards setup");
                    isRewardsSetup = true;
                }
            }
            if (curAuthState.MaybeResult.Status == AuthStatus.LoggedOut)
            {
                // user is logged out, do something
                // In this example you could also call GetAuthInformation() to retrigger login
                isLoggedIn = false;
                Jotunn.Logger.LogWarning("User logged out");
                TwitchCustomRewards rewards = gameObject.GetComponent<TwitchCustomRewards>();
                if (rewards != null && isRewardsSetup)
                {
                    rewards.ClearRewards();
                    //Jotunn.Logger.LogWarning("Rewards removed!");
                    isRewardsSetup = false;
                }
            }
            if (curAuthState.MaybeResult.Status == AuthStatus.WaitingForCode)
            {
                isLoggedIn = false;

                // Waiting for code
                var UserAuthInfo = Twitch.API.GetAuthenticationInfo(TwitchOAuthScope.Bits.Read).MaybeResult;

                if (UserAuthInfo == null)
                {
                    // User is still loading
                    //Jotunn.Logger.LogWarning("Loading...");
                }

                //Jotunn.Logger.LogWarning("Uri: " + UserAuthInfo.Uri);
                //Jotunn.Logger.LogWarning("Code: " + UserAuthInfo.UserCode);
                Jotunn.Logger.LogWarning("Asking for login: ");

                if (!isLoginShown)
                {
                    // We have reached the state where we can ask the user to login
                    Application.OpenURL($"{UserAuthInfo.Uri}");
                    isLoginShown = true;
                }
            }
        }

        // Triggered by something external, like a login button on a options menu screen
        public void GetAuthInformation()
        {
            // Check to see if the user is currently logged in or not.
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
