using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Data;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchCustomRewards : MonoBehaviour
    {
        private TwitchAuth m_auth;
        private TwitchChat m_chat;
        private GameTask<EventStream<CustomRewardEvent>> m_customRewardEvents;
        private Redeems m_redeems = new Redeems();
        private List<string> m_bannedUsers = new List<string>();
        public bool m_playerIsInSafeZone = false;
        public bool m_enabled = false;

        public void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
        }

        public void SubscribeToRedeemEvents()
        {
            m_auth = gameObject.GetComponent<TwitchAuth>();

            if (m_customRewardEvents != null || !m_auth.m_loggedIn)
                return;

            m_customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        }

        public List<RedeemEntry> GetRedeemList()
        {
            return m_redeems.list;
        }

        public void SetEnableRedeems(bool value)
        {
            m_enabled = value;

            if (value)
                SetRewards();
            else
                ClearRewards();
        }

        public void Update()
        {
            try
            {
                if (m_customRewardEvents == null || !m_auth.m_loggedIn)
                    return;

                CustomRewardEvent currentRewardEvent;
                m_customRewardEvents.MaybeResult.TryGetNextEvent(out currentRewardEvent);
                HandleRedeem(currentRewardEvent);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in custom rewards update: " + e);
            }
        }

        public void HandleRedeem(CustomRewardEvent currentRewardEvent)
        {
            try
            {
                if (currentRewardEvent == null)
                    return;

                if (currentRewardEvent == null || currentRewardEvent.Status == CustomRewardRedemptionState.Fulfilled || currentRewardEvent.Status == CustomRewardRedemptionState.Canceled)
                    return;

                Jotunn.Logger.LogWarning($"{currentRewardEvent.RedeemerName} has bought {currentRewardEvent.CustomRewardTitle} for {currentRewardEvent.CustomRewardCost}!");
                Jotunn.Logger.LogWarning($"Time: {currentRewardEvent.RedeemedAt}");
                Jotunn.Logger.LogWarning($"Status: {currentRewardEvent.Status}");

                RedeemEntry redeem = m_redeems.list.Find(item => item.title == currentRewardEvent.CustomRewardTitle);
                if (redeem == null)
                {
                    Jotunn.Logger.LogError($"Could not find redeem! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    return;
                }

                if (m_playerIsInSafeZone && !redeem.ignoreWard)
                {
                    Jotunn.Logger.LogWarning("Player is in safe zone, canceling redeem...");
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    return;
                }

                if (Player.m_localPlayer.transform.localPosition.y >= 4000)
                {
                    bool cancelRedeem = false;

                    if (redeem.type == RedeemType.TerrainRemove || redeem.type == RedeemType.SpawnHallucination || redeem.type == RedeemType.SpawnShower)
                        cancelRedeem = true;

                    if (redeem.creatures != null)
                    {
                        List<string> notAllowedList = new List<string>();
                        notAllowedList.Add("abomination");
                        notAllowedList.Add("bat");
                        notAllowedList.Add("bjorn");
                        notAllowedList.Add("deathsquito");
                        notAllowedList.Add("gjall");
                        notAllowedList.Add("goblinbrute");
                        notAllowedList.Add("golem");
                        notAllowedList.Add("hatchling");
                        notAllowedList.Add("lox");
                        notAllowedList.Add("seekerbrute");
                        notAllowedList.Add("troll");
                        notAllowedList.Add("unbjorn");

                        foreach (SpawnCreatureData creature in redeem.creatures)
                        {
                            if (notAllowedList.Contains(creature.prefabName.ToLower()))
                            {
                                cancelRedeem = true;
                                break;
                            }
                        }
                    }

                    if (cancelRedeem)
                    {
                        Jotunn.Logger.LogWarning("Player is in dungeon, canceling redeem...");
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                        return;
                    }
                }

                if (redeem.type == RedeemType.SpawnCreature)
                {
                    foreach (SpawnCreatureData creature in redeem.creatures)
                    {
                        if (redeem.userInput)
                            creature.talkMessage = m_chat.GetLastMessageOfUser(currentRewardEvent.RedeemerName)?.message;

                        if (creature.amount > 1)
                            RedeemHelper.SpawnCreatures(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent, redeem.ignoreWard));
                        else
                            RedeemHelper.SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent, redeem.ignoreWard));
                    }

                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.SpawnHallucination)
                {
                    RedeemHelper.hallucinationCount = 0;
                    InvokeRepeating(nameof(StartHallucinations), 0f, 20f);
                    return;
                }

                if (redeem.type == RedeemType.SpawnShower)
                {
                    GameObject shower = Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.FishRainScript, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                    SpawnAbility spawnComp = shower.GetComponent<SpawnAbility>();
                    spawnComp.m_owner = Player.m_localPlayer;
                    shower.transform.SetParent(Player.m_localPlayer.transform);
                    shower.SetActive(true);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.PlayerShrink)
                {
                    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                    TwitchStatusEffect statusEffect = new TwitchStatusEffect("MiniMe", PluginConfig.configMiniMeDuration.Value);
                    statusEffect.blockedByStatusEffects.Add("BigMe");
                    statusEffect.onStart = customStatusEffect.ShrinkPlayer;
                    statusEffect.onEnd = customStatusEffect.ResetPlayer;
                    bool success = customStatusEffect.AddStatusEffect(statusEffect);

                    if (success)
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    else
                    {
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the selected StatusEffect or it is blocked by a different one! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    }
                    return;
                }

                if (redeem.type == RedeemType.PlayerGrow)
                {
                    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                    TwitchStatusEffect statusEffect = new TwitchStatusEffect("BigMe", PluginConfig.configMiniMeDuration.Value);
                    statusEffect.blockedByStatusEffects.Add("MiniMe");
                    statusEffect.onStart = customStatusEffect.GrowPlayer;
                    statusEffect.onEnd = customStatusEffect.ResetPlayer;
                    bool success = customStatusEffect.AddStatusEffect(statusEffect);

                    if (success)
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    else
                    {
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the selected StatusEffect or it is blocked by a different one! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                    }
                    return;
                }

                if (redeem.type == RedeemType.StatusEffect)
                {
                    Jotunn.Logger.LogWarning("STATUS EFFECT");
                    if (redeem.statusEffects == null || redeem.statusEffects.Count == 0)
                    {
                        Jotunn.Logger.LogWarning("Could not find a status effect to apply, canceling redeem!");
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                        return;
                    }

                    foreach (string statusEffect in redeem.statusEffects)
                    {
                        Jotunn.Logger.LogWarning("name: " + statusEffect);
                        int hash = StatusEffectType.GetByString(statusEffect);
                        Jotunn.Logger.LogWarning("Hash: " + hash);

                        if (hash == -1)
                        {
                            Jotunn.Logger.LogWarning("Not a valid status effect to apply, skipping...");
                            continue;
                        }

                        Player.m_localPlayer.GetSEMan().AddStatusEffect(hash);
                    }

                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.StatusEffectRandom)
                {
                    int hash = StatusEffectHelper.GetRandomStatusEffect();
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(hash);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.SurpriseChest)
                {
                    if (redeem.chest == null)
                    {
                        Jotunn.Logger.LogWarning("Could not find a chest options to apply, canceling redeem!");
                        Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                        return;
                    }

                    GameObject chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestIron;

                    if (redeem.chest.type == ChestType.Gold)
                        chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestGold;

                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Surprise Chest inbound!");
                    StartCoroutine(InitSurpriseChestWithDelay(chestPrefab, redeem.chest));
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.TerrainRemove)
                {
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.NoFallDamage);
                    GameObject dig = Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }

                if (redeem.type == RedeemType.ExplodeFish)
                {
                    RedeemHelper.DetonateFish();
                    Invoke(nameof(DestroyFish), 0.5f);
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                    return;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while handling the redeem, refunding... and the error: " + e);
                Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
            }
        }

        private void StartHallucinations()
        {
            if (RedeemHelper.hallucinationCount > 6)
                CancelInvoke(nameof(StartHallucinations));
            else
                RedeemHelper.SpawnHallucination();
        }

        private void DestroyFish()
        {
            foreach (GameObject fish in RedeemHelper.fishList)
            {
                ZNetView netView = fish.GetComponent<ZNetView>();
                netView.Destroy();
                Destroy(fish);
            }

            RedeemHelper.fishList.Clear();
        }

        public IEnumerator InitSurpriseChestWithDelay(GameObject prefab, ChestData chestData)
        {
            yield return new WaitForSeconds(3f);

            RedeemHelper.SpawnSupriseChest(prefab, chestData);

        }

        public void SetRewards(List<RedeemEntry> redeems = null)
        {
            Jotunn.Logger.LogWarning("SetRewards()");
            List<CustomRewardDefinition> listRewards = new List<CustomRewardDefinition>();

            if (redeems == null)
                redeems = m_redeems.list;

            redeems.RemoveAll(item => item.type == RedeemType.Undefined);

            foreach (RedeemEntry redeem in redeems)
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
