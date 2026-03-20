using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Data;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Extensions;
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
        public Redeems m_redeems = new Redeems();
        public List<string> m_bannedUsers = new List<string>();
        public bool m_playerIsInSafeZone = false;
        public bool m_enabled = false;
        public string m_alias;
        public static string m_refundAutoResolveOn;
        public static string m_refundAutoResolveOff;

        public void Awake()
        {
            m_chat = gameObject.GetComponent<TwitchChat>();
            m_bannedUsers = ExtraConfigHelper.ReadBannedUsersFromFile();
        }

        public void SubscribeToRedeemEvents()
        {
            m_auth = gameObject.GetComponent<TwitchAuth>();

            if (m_customRewardEvents != null || !m_auth.m_loggedIn)
                return;

            m_customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        }

        public void UnSubscribeFromRedeemEvents()
        {
            m_customRewardEvents = null;
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

                if (m_alias != null && (currentRewardEvent.RedeemerName == m_auth?.m_userInfo?.displayName || WizshBoneTwitchIntegration.useRedeemCommand))
                {
                    Jotunn.Logger.LogWarning($"{m_auth?.m_userInfo?.displayName} pretending to be {m_alias}");
                    currentRewardEvent.RedeemerName = m_alias;
                }

                Jotunn.Logger.LogWarning($"{currentRewardEvent.RedeemerName} has bought {currentRewardEvent.CustomRewardTitle} for {currentRewardEvent.CustomRewardCost}!");
                Jotunn.Logger.LogWarning($"Time: {currentRewardEvent.RedeemedAt}");
                Jotunn.Logger.LogWarning($"Status: {currentRewardEvent.Status}");

                m_refundAutoResolveOn = $"Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!";
                m_refundAutoResolveOff = $"Please notify the streamer to refund your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points!";

                RedeemEntry redeem = m_redeems.list.Find(item => item.title == currentRewardEvent.CustomRewardTitle);
                if (redeem == null)
                {
                    //m_chat.Send($"Could not find redeem! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                    //throw new RedeemException("Could not find redeem", ExceptionType.Error);
                    throw new RedeemException($"Could not find redeem with the name: {currentRewardEvent.CustomRewardTitle}, probaly not part of the mod. Aborting...", ExceptionType.Warning);
                }

                if (m_bannedUsers.Contains(currentRewardEvent.RedeemerName.ToLower()))
                    throw new RedeemException($"{currentRewardEvent.RedeemerName} is banned", ExceptionType.Warning);

                if (m_playerIsInSafeZone && !redeem.ignoreWard)
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                    throw new RedeemException("Player is in safe zone", ExceptionType.Warning);
                }

                if (Game.IsPaused() || Menu.IsVisible())
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, either the game is currently paused or the streamer is busy in the menu! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                    throw new RedeemException("Game is paused or menu is visible", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.IsSleeping())
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is currently sleeping and can't react! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                    throw new RedeemException("Player is sleeping", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.IsTeleporting())
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is currently teleporting and can't react! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                    throw new RedeemException("Player is teleporting", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.InInterior())
                {
                    bool cancelRedeem = false;

                    cancelRedeem = CreatureHelper.CancelRedeemCauseOfDungeon(redeem.creatureData);

                    if (redeem.type == RedeemType.TerrainEdit || redeem.type == RedeemType.SpawnHallucination || redeem.type == RedeemType.SpawnAbility)
                        cancelRedeem = true;

                    if (cancelRedeem)
                    {
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                        throw new RedeemException("Player is in dungeon and redeem is not allowed", ExceptionType.Warning);
                    }
                }

                if (redeem.type == RedeemType.SpawnCreature)
                {
                    if (redeem.creatureData == null)
                        throw new RedeemException("Could not find creature data for SpawnCreature", ExceptionType.Error);

                    if (CreatureHelper.GetNrOfTwitchInstances(PluginConfig.configCreaturesMaxRadius.Value) >= PluginConfig.configCreaturesMaxAmount.Value)
                    {
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the maximum spawned creature limit has been reached! {(PluginConfig.configAutoResolveRedeems.Value ? m_refundAutoResolveOn : m_refundAutoResolveOff)}");
                        throw new RedeemException("To many spawned creatures", ExceptionType.Warning);
                    }

                    foreach (CreatureData creature in redeem.creatureData)
                    {
                        if (!ProgressionHelper.IsAllowedByGlobalKeys(creature.globalKeyAdd, creature.globalKeyRemove))
                            continue;

                        if (redeem.userInput)
                            creature.talkMessage = m_chat.GetLastMessageOfUser(currentRewardEvent.RedeemerName)?.message;

                        if (creature.amount > 0)
                            CreatureHelper.SpawnCreatures(creature, Player.m_localPlayer.transform, currentRewardEvent, redeem.ignoreWard);
                    }
                }

                if (redeem.type == RedeemType.SpawnHallucination)
                {
                    CreatureHelper.hallucinationCount = 0;
                    InvokeRepeating(nameof(CreatureHelper.StartHallucinations), 0f, 20f);
                }

                if (redeem.type == RedeemType.SpawnAbility)
                {
                    if (redeem.spawnAbilityData == null)
                        throw new RedeemException("Could not find data for SpawnAbility", ExceptionType.Error);

                    SpawnAbilityHelper.SpawnAbility(redeem.spawnAbilityData, currentRewardEvent, m_chat);
                }

                if (redeem.type == RedeemType.SpawnMist)
                {
                    if (redeem.mistData == null)
                        throw new RedeemException("could not find data for SpawnMist", ExceptionType.Error);

                    MistHelper.SpawnMist(redeem.mistData, currentRewardEvent);
                }

                if (redeem.type == RedeemType.StatusEffect)
                {
                    if (redeem.statusEffectData == null || redeem.statusEffectData.Count == 0)
                        throw new RedeemException("Could not find a status effect to apply", ExceptionType.Error);

                    StatusEffectHelper.ApplyStatusEffects(redeem.statusEffectData, currentRewardEvent, m_chat);
                }

                if (redeem.type == RedeemType.StatusEffectRandom)
                {
                    if (redeem.statusEffectData == null || redeem.statusEffectData.Count == 0)
                        throw new RedeemException("Could not find a status effect to apply", ExceptionType.Error);

                    StatusEffectHelper.ApplyRandomStatusEffects(redeem.statusEffectData, currentRewardEvent, m_chat);
                }

                if (redeem.type == RedeemType.SurpriseChest)
                {
                    if (redeem.chestData == null)
                    {
                        throw new RedeemException("Could not find chest data for SurpriseChest", ExceptionType.Error);
                    }

                    GameObject chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestIron;

                    if (redeem.chestData.type == ChestType.Gold)
                        chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestGold;

                    if (redeem.chestData.announceMessage != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", currentRewardEvent.RedeemerName, redeem.chestData.announceMessage));

                    StartCoroutine(InitSurpriseChestWithDelay(chestPrefab, redeem.chestData, currentRewardEvent));
                }

                if (redeem.type == RedeemType.TerrainEdit)
                {
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.NoFallDamage);
                    GameObject dig = Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                }

                if (redeem.type == RedeemType.FlashBang)
                {
                    if (redeem.flashbangData == null)
                    {
                        throw new RedeemException("Could not find data for flashbang", ExceptionType.Error);
                    }

                    StartCoroutine(FlashBangHelper.AttachFlashBang(redeem.flashbangData));
                }

                if (redeem.type == RedeemType.Detonate)
                {
                    if (redeem.detonateData == null)
                    {
                        throw new RedeemException("Could not find data for detonate", ExceptionType.Error);
                    }

                    DetonateHelper.DetonateFish(redeem.detonateData);
                }

                if (PluginConfig.configAutoResolveRedeems.Value)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{currentRewardEvent.CustomRewardTitle} fullfilled");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (WizshBoneTwitchIntegration.useRedeemCommand)
                    WizshBoneTwitchIntegration.useRedeemCommand = false;
            }
            catch (RedeemException e)
            {
                if (e.type == ExceptionType.Error)
                    Jotunn.Logger.LogError("Something went wrong while handling the redeem: " + e);
                else
                    Jotunn.Logger.LogWarning("Could not complete redeem: " + e.Message);

                if (WizshBoneTwitchIntegration.useRedeemCommand)
                    WizshBoneTwitchIntegration.useRedeemCommand = false;

                if (PluginConfig.configAutoResolveRedeems.Value)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{currentRewardEvent.CustomRewardTitle} canceled");
                    Twitch.API.ResolveCustomReward(currentRewardEvent, CustomRewardRedemptionState.Canceled);
                }
            }
        }

        public IEnumerator InitSurpriseChestWithDelay(GameObject prefab, SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            yield return new WaitForSeconds(3f);

            if (m_playerIsInSafeZone)
            {
                WizshBoneTwitchIntegration.useRedeemCommand = true;
                string refundAutoResolveOn = $"Your redeem {customRewardEvent.CustomRewardTitle} of {customRewardEvent.CustomRewardCost} points has been refunded!";
                string refundAutoResolveOff = $"Please notify the streamer to refund your redeem {customRewardEvent.CustomRewardTitle} of {customRewardEvent.CustomRewardCost} points!";

                m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");

                if (PluginConfig.configAutoResolveRedeems.Value)
                {
                    Twitch.API.ResolveCustomReward(customRewardEvent, CustomRewardRedemptionState.Canceled);
                }
                WizshBoneTwitchIntegration.useRedeemCommand = false;
            }
            else
            {
                RedeemHelper.SpawnSupriseChest(prefab, chestData, customRewardEvent);
            }
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
                if (ProgressionHelper.IsAllowedByGlobalKeys(redeem.globalKeyAdd, redeem.globalKeyRemove))
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

        public TaskAwaiter ClearRewards()
        {
            Jotunn.Logger.LogWarning("ClearRewards()");
            List<CustomRewardDefinition> list = new List<CustomRewardDefinition>();
            GameTask gameTask = Twitch.API.ReplaceCustomRewards(list.ToArray());
            return gameTask.GetAwaiter();
        }

        public bool ReloadRewards()
        {
            bool result = m_redeems.Reload();

            if (m_auth && m_auth.m_loggedIn && m_enabled)
                SetRewards();

            return result;
        }

        public string ToRFC3339String(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
