using System;
using System.Collections.Generic;
using System.Globalization;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Data;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchCustomRewards : MonoBehaviour
    {
        private TwitchChat m_chat;
        GameTask<EventStream<CustomRewardEvent>> m_customRewardEvents;
        public Redeems m_redeems = new Redeems();
        public bool m_playerIsInSafeZone = false;
        public bool isEnabled = false;

        private void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
        }

        public void SubscribeToRedeemEvents()
        {
            if (m_customRewardEvents != null)
                return;

            m_customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        }

        public void SetEnableRedeems(bool value)
        {
            isEnabled = value;

            if (value)
                SetRewards();
            else
                ClearRewards();
        }

        // Update is called once per frame
        public void Update()
        {
            try
            {
                if (m_customRewardEvents == null)
                    return;

                CustomRewardEvent currentRewardEvent;
                m_customRewardEvents.MaybeResult.TryGetNextEvent(out currentRewardEvent);

                if (currentRewardEvent == null || currentRewardEvent.Status == CustomRewardRedemptionState.Fulfilled || currentRewardEvent.Status == CustomRewardRedemptionState.Canceled)
                    return;

                // Do something
                Jotunn.Logger.LogWarning($"{currentRewardEvent.RedeemerName} has bought {currentRewardEvent.CustomRewardTitle} for {currentRewardEvent.CustomRewardCost}!");
                Jotunn.Logger.LogWarning($"Time: {currentRewardEvent.RedeemedAt}");
                Jotunn.Logger.LogWarning($"Status: {currentRewardEvent.Status}");

                RedeemEntry redeem = m_redeems.list.Find(item => item.title == currentRewardEvent.CustomRewardTitle);
                if (redeem == null)
                    return;

                if (m_playerIsInSafeZone)
                {
                    Jotunn.Logger.LogWarning("Player is in safe zone, canceling redeem...");
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} point has been refunded!");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    return;
                }

                if (Player.m_localPlayer.transform.localPosition.y >= 4000)
                {
                    List<string> listOfTitles = new List<string>();
                    listOfTitles.Add("troll");
                    listOfTitles.Add("golem");
                    listOfTitles.Add("lox");
                    listOfTitles.Add("bat");
                    listOfTitles.Add("abomination");
                    listOfTitles.Add("country");

                    foreach (string title in listOfTitles)
                    {
                        if (currentRewardEvent.CustomRewardTitle.ToLower().Contains(title))
                        {
                            Jotunn.Logger.LogWarning("Player is in dungeon, canceling redeem...");
                            m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} point has been refunded!");
                            Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                            return;
                        }
                    }
                }

                if (redeem.type == RedeemType.SpawnCreature)
                {
                    foreach (SpawnCreatureData creature in redeem.creatures)
                    {
                        if (redeem.userInput)
                            creature.talkMessage = m_chat.GetLastMessageOfUser(currentRewardEvent.RedeemerName)?.message;

                        if (creature.amount > 1)
                            RedeemHelper.SpawnCreatures(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent));
                        else
                            RedeemHelper.SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent));
                    }
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.SpawnHallucination)
                {
                    RedeemHelper.hallucinationCount = 0;
                    InvokeRepeating(nameof(StartHallucinations), 0f, 20f);
                }

                if (redeem.type == RedeemType.SpawnShower)
                {
                    GameObject shower = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.FishRainScript, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                    SpawnAbility spawnComp = shower.GetComponent<SpawnAbility>();
                    spawnComp.m_owner = Player.m_localPlayer;
                    shower.transform.SetParent(Player.m_localPlayer.transform);
                    shower.SetActive(true);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.PlayerGrow)
                {
                    GrowPlayer();
                    Invoke(nameof(ResetPlayer), PluginConfig.configMiniMeDuration.Value);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.PlayerShrink)
                {
                    ShrinkPlayer();
                    Invoke(nameof(ResetPlayer), PluginConfig.configMiniMeDuration.Value);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.StatusEffectRandom)
                {
                    int hash = RedeemHelper.GetRandomStatusEffect();
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(hash);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.TerrainRemove)
                {
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.NoFallDamage);
                    GameObject dig = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.ExplodeFish)
                {
                    RedeemHelper.DetonateFish();
                    Invoke(nameof(DestroyFish), 0.5f);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in custom rewards: "+ e);
            }
        }

        private void StartHallucinations()
        {
            if (RedeemHelper.hallucinationCount > 6)
                StopHallucinations();
            else
                RedeemHelper.SpawnHallucination();
        }

        private void StopHallucinations()
        {
            CancelInvoke(nameof(StartHallucinations));
        }

        private void DestroyFish()
        {
            foreach (GameObject fish in RedeemHelper.fishList)
            {
                ZNetView netView = fish.GetComponent<ZNetView>();
                netView.Destroy();
                GameObject.Destroy(fish);
            }

            RedeemHelper.fishList.Clear();
        }

        private void GrowPlayer()
        {
            Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.BigMe);
            RedeemHelper.SetPlayerSpeed(1.25f);
            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, new Vector3(1.45f, 1.45f, 1.45f), 1.5f));
        }

        private void ShrinkPlayer()
        {
            Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.MiniMe);
            RedeemHelper.SetPlayerSpeed(0.75f);
            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, new Vector3(0.45f, 0.45f, 0.45f), 1.5f));
        }

        private void ResetPlayer()
        {
            RedeemHelper.ResetPlayerSpeed(Player.m_localPlayer);
            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, new Vector3(1f, 1f, 1f), 1.5f));
        }

        public void SetRewards(List<RedeemEntry> listData = null)
        {
            Jotunn.Logger.LogWarning("SetRewards()");
            List<CustomRewardDefinition> listRewards = new List<CustomRewardDefinition>();

            if (listData == null)
                listData = m_redeems.list;

            listData.RemoveAll(item => item.type == RedeemType.Undefined);

            foreach (RedeemEntry redeem in m_redeems.list)
            {
                if (redeem.globalKey == null || ZoneSystem.instance.GetGlobalKey(redeem.globalKey))
                {
                    listRewards.Add(new CustomRewardDefinition()
                    {
                        BackgroundColor = redeem.backgroundColor,
                        GlobalCooldownSeconds = redeem.cooldown,
                        IsGlobalCooldownEnabled = redeem.cooldown > 0,
                        IsUserInputRequired = redeem.userInput,
                        Cost = redeem.points,
                        Title = redeem.title,
                    });
                }
            }

            Twitch.API.ReplaceCustomRewards(listRewards.ToArray());
        }

        public async void ClearRewards(bool applicationQuit = false)
        {
            Jotunn.Logger.LogWarning("ClearRewards()");
            List<CustomRewardDefinition> list = new List<CustomRewardDefinition>();
            await Twitch.API.ReplaceCustomRewards(list.ToArray());

            if (applicationQuit)
                Invoke(nameof(Quit), 10f);
        }

        public bool ReloadRewards()
        {
            bool result = m_redeems.Reload();
            SetRewards();
            return result;
        }

        private void Quit()
        {
            Jotunn.Logger.LogWarning("Quitting NOW!");
            Application.Quit();
        }

        public string ToRFC3339String(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
