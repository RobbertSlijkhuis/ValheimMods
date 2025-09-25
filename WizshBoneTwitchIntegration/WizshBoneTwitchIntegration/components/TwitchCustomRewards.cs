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
        GameTask<EventStream<CustomRewardEvent>> m_customRewardEvents;
        public Redeems m_redeems = new Redeems();
        public bool isPlayerInSafeZone = false;

        public void SubscribeToRedeemEvents()
        {
            if (m_customRewardEvents != null)
                return;

            m_customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
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

                RedeemData redeem = m_redeems.list.Find(item => item.title == currentRewardEvent.CustomRewardTitle);
                if (redeem == null)
                    return;

                if (isPlayerInSafeZone)
                {
                    Jotunn.Logger.LogWarning("Player is in safe zone, canceling redeem...");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    return;
                }

                if (redeem.type == RedeemType.SpawnCreature)
                {
                    foreach (SpawnCreatureData creature in redeem.spawnCreatureData)
                    {
                        if (creature.count > 1)
                            RedeemHelper.SpawnCreatures(new SpawnOptions(creature.prefabName, Player.m_localPlayer.transform, creature, currentRewardEvent));
                        else
                            RedeemHelper.SpawnCreature(new SpawnOptions(creature.prefabName, Player.m_localPlayer.transform, creature, currentRewardEvent));
                    }
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.SpawnShower)
                {
                    GameObject shower = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.FishRainScript, transform.position, transform.rotation);
                    SpawnAbility spawnComp = shower.GetComponent<SpawnAbility>();
                    spawnComp.m_owner = Player.m_localPlayer;
                    shower.transform.SetParent(Player.m_localPlayer.transform);
                    shower.SetActive(true);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.ShrinkPlayer)
                {
                    ShrinkPlayer();
                    Invoke(nameof(UnshrinkPlayer), PluginConfig.configMiniMeDuration.Value);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (redeem.type == RedeemType.RandomStatusEffect)
                {
                    int hash = RedeemHelper.GetRandomStatusEffect();
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(hash);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in custom rewards: "+ e);
            }
        }

        private void ShrinkPlayer()
        {
            Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.MiniMe);
            RedeemHelper.SetPlayerSpeed(0.75f);
            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, new Vector3(0.45f, 0.45f, 0.45f), 1.5f));
        }

        private void UnshrinkPlayer()
        {
            RedeemHelper.ResetPlayerSpeed(Player.m_localPlayer);
            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, new Vector3(1f, 1f, 1f), 1.5f));
        }

        public void SetRewards()
        {
            Jotunn.Logger.LogWarning("SetRewards()");
            List<CustomRewardDefinition> list = new List<CustomRewardDefinition>();

            foreach (RedeemData redeem in m_redeems.list)
            {

                list.Add(new CustomRewardDefinition()
                {
                    BackgroundColor = redeem.backgroundColor,
                    Cost = redeem.cost,
                    // IsUserInputRequired = redeem.talks,
                    IsUserInputRequired = false,
                    Title = redeem.title,
                });
            }

            Twitch.API.ReplaceCustomRewards(list.ToArray());
        }

        public void ClearRewards()
        {
            Jotunn.Logger.LogWarning("ClearRewards()");
            CustomRewardDefinition Cleared = new CustomRewardDefinition()
            {
                Cost = 0,
                IsEnabled = false,
                Title = "",
            };

            Twitch.API.ReplaceCustomRewards(Cleared);
        }

        public string ToRFC3339String(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
