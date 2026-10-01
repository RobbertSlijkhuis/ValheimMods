using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Exceptions;
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
        public List<string> m_bannedUsers = new List<string>();
        public bool m_playerIsInSafeZone = false;
        public bool m_enabled = false;
        public string m_alias;
        public string m_quickTestRedeem = "Timestop";
        public static string m_refundMessage;

        public readonly List<CustomRewardEvent> m_redeemHistory = new List<CustomRewardEvent>();
        private const int m_redeemHistoryMaxSize = 500;

        public readonly BulkRedeemResolveHelper m_bulkResolveHelper = new BulkRedeemResolveHelper();

        public void Awake()
        {
            try
            {
                m_chat = gameObject.GetComponent<TwitchChat>();
                m_bannedUsers = ExtraConfigHelper.ReadBannedUsersFromFile();
                RedeemHelper.ReadRedeems();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCustomRewards.Awake failed: " + e);
            }
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

        public List<RedeemData> GetRedeemList()
        {
            return RedeemHelper.redeems;
        }

        public void SetEnableRedeems(bool value)
        {
            m_enabled = value;

            if (value)
                SetRewards();
            else
                ClearRewards();
        }

        public bool HasUnresolvedRedeems()
        {
            return m_enabled
                && !ProfileSettingsHelper.Current.autoResolveRedeems
                && m_redeemHistory.Exists(item => item.Status == CustomRewardRedemptionState.Unfulfilled);
        }

        public bool IsLoggedIn => m_auth != null && m_auth.m_loggedIn;

        public bool HasUnresolvedRedeemsFor(string redeemTitle)
        {
            return m_enabled
                && !ProfileSettingsHelper.Current.autoResolveRedeems
                && m_redeemHistory.Exists(item =>
                    item.CustomRewardTitle == redeemTitle
                    && item.Status == CustomRewardRedemptionState.Unfulfilled);
        }

        public void Update()
        {
            try
            {
                if (m_customRewardEvents == null || !m_auth.m_loggedIn)
                    return;

                CustomRewardEvent customRewardEvent;
                m_customRewardEvents.MaybeResult.TryGetNextEvent(out customRewardEvent);
                HandleRedeem(customRewardEvent);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in custom rewards update: " + e);
            }
        }

        public void HandleRedeem(CustomRewardEvent customRewardEvent)
        {
            try
            {
                if (customRewardEvent == null)
                    return;

                if (customRewardEvent.Status == CustomRewardRedemptionState.Fulfilled || customRewardEvent.Status == CustomRewardRedemptionState.Canceled)
                    return;

                m_redeemHistory.Add(customRewardEvent);

                if (m_redeemHistory.Count > m_redeemHistoryMaxSize)
                    m_redeemHistory.RemoveAt(0);

                if (m_alias != null && (customRewardEvent.RedeemerName == m_auth?.m_userInfo?.displayName || WizshBoneTwitchIntegration.useRedeemCommand))
                {
                    customRewardEvent.RedeemerName = m_alias;
                }


                m_refundMessage = $"Your redeem {customRewardEvent.CustomRewardTitle} of {customRewardEvent.CustomRewardCost} points has been refunded!";

                // customRewardEvent.CustomRewardTitle is whatever was actually pushed to Twitch -
                // the Twitch-facing (prefixed) title, not the raw RedeemData.title - so match
                // against RedeemManager.GetFullTitle() rather than item.title directly.
                RedeemData redeem = GetRedeemList().Find(item => RedeemManager.GetFullTitle(item) == customRewardEvent.CustomRewardTitle);
                if (redeem == null)
                {
                    //m_chat.Send($"Could not find redeem! Your redeem {customRewardEvent.CustomRewardTitle} of {customRewardEvent.CustomRewardCost} points has been refunded!");
                    //throw new RedeemException("Could not find redeem", ExceptionType.Error);
                    throw new RedeemException($"Could not find redeem with the name: {customRewardEvent.CustomRewardTitle}, probaly not part of the mod. Aborting...", ExceptionType.Warning);
                }

                if (m_bannedUsers.Contains(customRewardEvent.RedeemerName.ToLower()))
                    throw new RedeemException($"{customRewardEvent.RedeemerName} is banned", ExceptionType.Warning);

                if (m_playerIsInSafeZone && !redeem.ignoreWard)
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {m_refundMessage}");
                    throw new RedeemException("Player is in safe zone", ExceptionType.Warning);
                }

                if (Game.IsPaused() || Menu.IsVisible())
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, either the game is currently paused or the streamer is busy in the menu! {m_refundMessage}");
                    throw new RedeemException("Game is paused or menu is visible", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.IsSleeping())
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is currently sleeping and can't react! {m_refundMessage}");
                    throw new RedeemException("Player is sleeping", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.IsTeleporting())
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is currently teleporting and can't react! {m_refundMessage}");
                    throw new RedeemException("Player is teleporting", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.InInterior())
                {
                    if (redeem.type == RedeemType.TerrainEdit || RedeemType.SpawnAbilityFamily.Contains(redeem.type))
                    {
                        m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! {m_refundMessage}");
                        throw new RedeemException("Player is in dungeon and redeem is not allowed", ExceptionType.Warning);
                    }
                }

                if (redeem.type == RedeemType.SpawnCreature)
                    CreatureHelper.HandleSpawnCreatureRedeem(redeem, customRewardEvent, m_chat);

                if (RedeemType.SpawnAbilityFamily.Contains(redeem.type))
                {
                    if (redeem.spawnAbilityData == null)
                        throw new RedeemException("Could not find data for SpawnAbility", ExceptionType.Error);

                    SpawnAbilityHelper.SpawnAbility(redeem.type, redeem.spawnAbilityData, customRewardEvent, m_chat);
                }

                if (redeem.type == RedeemType.Detonate)
                {
                    if (redeem.detonateData == null)
                    {
                        throw new RedeemException("Could not find data for detonate", ExceptionType.Error);
                    }

                    DetonateHelper.Enqueue(this, redeem.detonateData, customRewardEvent);
                }

                if (redeem.type == RedeemType.Flashbang)
                {
                    if (redeem.flashbangData == null)
                    {
                        throw new RedeemException("Could not find data for flashbang", ExceptionType.Error);
                    }

                    FlashBangHelper.Enqueue(this, redeem.flashbangData, customRewardEvent);
                }

                if (redeem.type == RedeemType.Mist)
                {
                    if (redeem.mistData == null)
                        throw new RedeemException("could not find data for Mist", ExceptionType.Error);

                    MistHelper.SpawnMist(redeem.mistData, customRewardEvent);
                }

                if (redeem.type == RedeemType.StatusEffect)
                {
                    if (redeem.statusEffectData == null)
                        throw new RedeemException("Could not find data for StatusEffect", ExceptionType.Error);

                    StatusEffectHelper.Apply(redeem.statusEffectData, customRewardEvent);
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
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, redeem.chestData.announceMessage));

                    StartCoroutine(SpawnSurpriseChestWithDelay(chestPrefab, redeem, customRewardEvent));
                }

                if (redeem.type == RedeemType.TerrainEdit)
                {
                    if (redeem.terrainEditData == null)
                    {
                        throw new RedeemException("Could not find terrain edit data for TerrainEdit", ExceptionType.Error);
                    }

                    TerrainEditHelper.ApplyTerrainEdit(redeem.terrainEditData, customRewardEvent);
                }

                if (redeem.type == RedeemType.TimeStop)
                {
                    if (redeem.timeStopData == null)
                        throw new RedeemException("Could not find data for TimeStop", ExceptionType.Error);

                    TimeStopHelper.Apply(redeem.timeStopData, customRewardEvent);
                }

                if (redeem.type == RedeemType.Weather)
                {
                    if (redeem.weatherData == null)
                        throw new RedeemException("could not find data for Weather", ExceptionType.Error);

                    WeatherHelper.SpawnWeather(redeem.weatherData, customRewardEvent);
                }

                if (ProfileSettingsHelper.Current.autoResolveRedeems)
                {
                    if (Player.m_localPlayer != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{customRewardEvent.CustomRewardTitle} fullfilled");

                    customRewardEvent.Status = CustomRewardRedemptionState.Fulfilled;

                    // A test/command-triggered redeem (or any redeem processed before we're
                    // actually logged in) has no real reward ID behind it - resolving it would
                    // either be a pointless real API call or, if Twitch.API hasn't been
                    // initialized yet, throw trying to bootstrap one from scratch.
                    if (IsLoggedIn)
                        Twitch.API.ResolveCustomReward(customRewardEvent, CustomRewardRedemptionState.Fulfilled);
                }

                if (WizshBoneTwitchIntegration.useRedeemCommand)
                    WizshBoneTwitchIntegration.useRedeemCommand = false;
            }
            catch (RedeemException e)
            {
                HandleRedeemException(e, customRewardEvent);
            }
        }

        private void HandleRedeemException(RedeemException e, CustomRewardEvent customRewardEvent)
        {
            if (e.type == ExceptionType.Error)
                Jotunn.Logger.LogError("Something went wrong while handling the redeem: " + e);
            else
                Jotunn.Logger.LogWarning("Could not complete redeem: " + e.Message);

            if (WizshBoneTwitchIntegration.useRedeemCommand)
                WizshBoneTwitchIntegration.useRedeemCommand = false;

            if (Player.m_localPlayer != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{customRewardEvent.CustomRewardTitle} canceled");

            customRewardEvent.Status = CustomRewardRedemptionState.Canceled;

            // See the matching guard in HandleRedeem's success path: resolving against the real
            // Twitch API only makes sense once we're actually logged in - otherwise it's either a
            // pointless call for a locally-fabricated test event, or throws trying to bootstrap a
            // Twitch.API instance that has no business existing yet (e.g. a DllNotFoundException
            // from the native SDK library never having been loaded).
            if (IsLoggedIn)
                Twitch.API.ResolveCustomReward(customRewardEvent, CustomRewardRedemptionState.Canceled);
        }

        public bool IsPlayerInSafeZone(CustomRewardEvent customRewardEvent, bool ignoreWard = false)
        {
            if (m_playerIsInSafeZone && !ignoreWard)
            {
                m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {m_refundMessage}");
                HandleRedeemException(new RedeemException("Player is in safe zone", ExceptionType.Warning), customRewardEvent);
                return true;
            }

            return false;
        }

        public IEnumerator SpawnSurpriseChestWithDelay(GameObject prefab, RedeemData redeem, CustomRewardEvent customRewardEvent)
        {
            yield return new WaitForSeconds(3f);

            if (!IsPlayerInSafeZone(customRewardEvent, redeem.ignoreWard))
                SurpriseChestHelper.SpawnSupriseChest(prefab, redeem.chestData, customRewardEvent);
        }

        public void SetRewards(List<RedeemData> redeems = null, bool isEnabled = true)
        {
            List<CustomRewardDefinition> listRewards = new List<CustomRewardDefinition>();

            if (redeems == null)
                redeems = GetRedeemList();

            redeems.RemoveAll(item => item.type == RedeemType.Undefined);

            foreach (RedeemData redeem in redeems)
            {
                if (ProgressionHelper.IsAllowedByGlobalKeys(redeem.globalKeyAdd, redeem.globalKeyRemove))
                {
                    // Capped here too (not just in the wizard) so a hand-edited profile.yaml can't
                    // send Twitch a cooldown it rejects.
                    int cooldownSeconds = CooldownHelper.ClampSeconds(redeem.cooldown);

                    listRewards.Add(new CustomRewardDefinition()
                    {
                        BackgroundColor = redeem.backgroundColor,
                        GlobalCooldownSeconds = cooldownSeconds,
                        IsGlobalCooldownEnabled = cooldownSeconds > 0,
                        MaxPerStream = redeem.maxPerStream,
                        IsMaxPerStreamEnabled = redeem.maxPerStream > 0,
                        MaxPerUserPerStream = redeem.maxPerUserPerStream,
                        IsMaxPerUserPerStreamEnabled = redeem.maxPerUserPerStream > 0,
                        IsUserInputRequired = redeem.userInput,
                        Cost = ApplyDiscount(redeem.points),
                        Prompt = redeem.description,
                        // The Twitch-facing (prefixed) title - RedeemData.title itself is stored
                        // prefix-free, see RedeemManager.GetFullTitle.
                        Title = RedeemManager.GetFullTitle(redeem),
                        IsEnabled = redeem.enabled && isEnabled,
                    });
                }
            }

            Twitch.API.ReplaceCustomRewards(listRewards.ToArray());
        }

        /// <summary>
        /// Applies <see cref="ProfileSettingsData.redeemsDiscountPercent"/> to a redeem's configured
        /// point cost before it's sent to Twitch. A 0-cost redeem is left as 0 (never floored to 1),
        /// but any nonzero cost is never rounded down to 0 by the discount - that would silently
        /// turn a paid redeem into a free one.
        /// </summary>
        private static int ApplyDiscount(int points)
        {
            if (points <= 0)
                return points;

            int discountPercent = Mathf.Clamp(ProfileSettingsHelper.Current.redeemsDiscountPercent, 0, 100);
            return Mathf.Max(1, Mathf.RoundToInt(points * (100 - discountPercent) / 100f));
        }

        public TaskAwaiter ClearRewards()
        {
            List<CustomRewardDefinition> list = new List<CustomRewardDefinition>();
            GameTask gameTask = Twitch.API.ReplaceCustomRewards(list.ToArray());
            return gameTask.GetAwaiter();
        }

        public bool ReloadRewards()
        {
            bool result = RedeemHelper.Reload();

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
