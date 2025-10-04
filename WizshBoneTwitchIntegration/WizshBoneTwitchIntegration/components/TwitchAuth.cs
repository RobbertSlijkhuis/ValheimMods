using System;
using System.Collections.Generic;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
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
            //Jotunn.Logger.LogWarning("Activated hallucinations");
            //var biome = Player.m_localPlayer.GetCurrentBiome();
            //Jotunn.Logger.LogWarning("Current biome: " + biome);

            //List<string> monsterList = new List<string>();

            //switch (biome)
            //{
            //    case Heightmap.Biome.Meadows:
            //        monsterList.Add("Neck");
            //        monsterList.Add("Greyling");
            //        monsterList.Add("Boar");
            //        break;
            //    case Heightmap.Biome.BlackForest:
            //        monsterList.Add("Greydwarf");
            //        monsterList.Add("Bjorn");
            //        monsterList.Add("Greydwarf_Shaman");
            //        monsterList.Add("Troll");
            //        monsterList.Add("Greydwarf_Elite");
            //        monsterList.Add("Bjorn");
            //        monsterList.Add("Skeleton");
            //        break;
            //    case Heightmap.Biome.Swamp:
            //        monsterList.Add("Draugr");
            //        monsterList.Add("BlobElite");
            //        monsterList.Add("Abomination");
            //        monsterList.Add("Blob");
            //        monsterList.Add("Draugr_Elite");
            //        monsterList.Add("Wraith");
            //        break;
            //    case Heightmap.Biome.Mountain:
            //        monsterList.Add("Wolf");
            //        monsterList.Add("Hatchling");
            //        monsterList.Add("StoneGolem");
            //        monsterList.Add("Wolf");
            //        monsterList.Add("Fenring");
            //        break;
            //    case Heightmap.Biome.Plains:
            //        monsterList.Add("Deathsquito");
            //        monsterList.Add("Goblin");
            //        monsterList.Add("Lox");
            //        monsterList.Add("Deathsquito");
            //        monsterList.Add("GoblinBrute");
            //        monsterList.Add("Unbjorn");
            //        break;
            //    case Heightmap.Biome.Mistlands:
            //        monsterList.Add("Seeker");
            //        monsterList.Add("Tick");
            //        monsterList.Add("SeekerBrute");
            //        monsterList.Add("Gjall");
            //        break;
            //    case Heightmap.Biome.AshLands:
            //        monsterList.Add("Charred_Melee");
            //        monsterList.Add("FallenValkyrie");
            //        monsterList.Add("Asksvin");
            //        monsterList.Add("Charred_Archer");
            //        monsterList.Add("BlobLava");
            //        monsterList.Add("Morgen");
            //        monsterList.Add("Charred_Twitcher");
            //        monsterList.Add("Volture");
            //        break;
            //}

            //int index = UnityEngine.Random.Range(0, monsterList.Count);
            //string monster = monsterList[index];

            //CustomRewardEvent customReward = new CustomRewardEvent();
            //SpawnCreatureData creature = new SpawnCreatureData(monster);
            //customReward.BroadcasterName = "DeadFizsh";
            //creature.isHallucination = true;
            //creature.position = SpawnPositionType.Random;
            //RedeemHelper.SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, customReward));

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
