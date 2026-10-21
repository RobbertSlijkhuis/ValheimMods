using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Gui;
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
        private const int m_redeemHistoryMaxSize = 1000;
        // Warns when this many redeems in the history are still unfulfilled. The history drops its
        // oldest entry once it passes the max size, and a dropped unresolved entry can no longer be
        // completed or refunded from the history.
        private const int m_redeemHistoryWarnSize = 750;

        // History entries whose title matched no redeem in the active profile. We can't tell a
        // redeem that isn't ours from one that is ours but no longer in the profile, so they are
        // left unresolved in the history for the user to decide, and don't count as unresolved.
        private readonly HashSet<CustomRewardEvent> m_unknownRedeems = new HashSet<CustomRewardEvent>();

        public readonly BulkRedeemResolveHelper m_bulkResolveHelper = new BulkRedeemResolveHelper();

        // Twitch's reward cap is shared with rewards we didn't create (the streamer's own, other tools').
        // How many of those the channel had at the last Helix lookup; null when unknown (logged out, or
        // the lookup failed). Our own rewards are replaced on every push, so they aren't counted here.
        public int? OtherChannelRewards { get; private set; }

        /// <summary>
        /// Raised on the main thread whenever <see cref="OtherChannelRewards"/> changes (a lookup finished,
        /// or logout cleared it), so an open Redeems tab can update itself. Static because the UI outlives
        /// this component (it lives on <c>Game.instance</c>, which is replaced per world).
        /// </summary>
        public static event Action ChannelRewardCountsChanged;

        private const float ChannelLookupTimeoutSeconds = 10f;
        private Coroutine m_pushRoutine;

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

        // Game.FixedUpdate calls Game.Logout() directly on a lost connection, which skips the
        // Menu.OnLogoutYes patch (and with it every logout path that disposes the stream), while the
        // SDK's login and subscription outlive this component.
        public void OnDestroy()
        {
            UnSubscribeFromRedeemEvents();
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
            // Dropping the reference isn't enough: an undisposed EventStream stays subscribed and keeps
            // buffering every event until the GC finalizes it.
            GameTask<EventStream<CustomRewardEvent>> stream = m_customRewardEvents;
            m_customRewardEvents = null;

            if (stream == null)
                return;

            try
            {
                if (stream.IsCompleted)
                    stream.MaybeResult?.Dispose();
                else
                    // Still connecting (MaybeResult is null until it's done), so close it once it is.
                    stream.Task.ContinueWith(
                        task => { if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) task.Result?.Dispose(); },
                        System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not close the redeem event stream: {e.Message}");
            }
        }

        public List<RedeemData> GetRedeemList()
        {
            return RedeemHelper.redeems;
        }

        public void SetEnableRedeems(bool value)
        {
            if (value)
            {
                // Nothing can be pushed to Twitch while logged out, so don't claim redeems are on.
                // The user info is needed too: without its channel id the reward-limit lookup is
                // skipped and the push goes out unchecked.
                if (m_auth == null)
                    m_auth = gameObject.GetComponent<TwitchAuth>();

                if (!IsLoggedIn)
                {
                    Jotunn.Logger.LogWarning("[WBTI] Redeems were not enabled: not logged in to Twitch.");
                    return;
                }

                if (m_auth.m_userInfo == null)
                {
                    Jotunn.Logger.LogWarning("[WBTI] Redeems were not enabled: the Twitch user info is not loaded yet.");
                    return;
                }
            }

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
                && m_redeemHistory.Exists(item =>
                    item.Status == CustomRewardRedemptionState.Unfulfilled
                    && !IsUnknownRedeem(item));
        }

        private int CountUnfulfilledInHistory()
        {
            int count = 0;

            foreach (CustomRewardEvent item in m_redeemHistory)
            {
                if (item.Status == CustomRewardRedemptionState.Unfulfilled)
                    count++;
            }

            return count;
        }

        public bool IsUnknownRedeem(CustomRewardEvent customRewardEvent)
        {
            return m_unknownRedeems.Contains(customRewardEvent);
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

                // MaybeResult is null until the subscription has finished connecting.
                EventStream<CustomRewardEvent> stream = m_customRewardEvents.MaybeResult;

                if (stream == null)
                    return;

                CustomRewardEvent customRewardEvent;
                stream.TryGetNextEvent(out customRewardEvent);
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
                {
                    // Drop the oldest already-resolved entry first so a still-pending redeem is only
                    // lost when every entry is pending. The search skips the entry just added.
                    int dropIndex = m_redeemHistory.FindIndex(0, m_redeemHistory.Count - 1,
                        item => item.Status != CustomRewardRedemptionState.Unfulfilled);

                    if (dropIndex < 0)
                        dropIndex = 0;

                    m_unknownRedeems.Remove(m_redeemHistory[dropIndex]);
                    m_redeemHistory.RemoveAt(dropIndex);
                }

                // Fires each time the number of still-unfulfilled redeems climbs to the threshold;
                // fulfilled/canceled ones no longer count. An add raises that number by at most 1,
                // so every upward crossing lands exactly on it.
                if (CountUnfulfilledInHistory() == m_redeemHistoryWarnSize)
                {
                    string warning = $"Redeem history is getting full ({m_redeemHistoryWarnSize}/{m_redeemHistoryMaxSize})";
                    Jotunn.Logger.LogWarning($"[WBTI] {warning}");

                    if (Player.m_localPlayer != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, warning);
                }

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
                    // Not refunded or resolved on purpose: see m_unknownRedeems.
                    m_unknownRedeems.Add(customRewardEvent);
                    Jotunn.Logger.LogWarning($"[WBTI] Redeem '{customRewardEvent.CustomRewardTitle}' is not in the active profile, leaving it unresolved in the history.");

                    if (WizshBoneTwitchIntegration.useRedeemCommand)
                        WizshBoneTwitchIntegration.useRedeemCommand = false;

                    return;
                }

                if (m_bannedUsers.Contains(customRewardEvent.RedeemerName.ToLower()))
                    throw new RedeemException($"{customRewardEvent.RedeemerName} is banned", ExceptionType.Warning);

                // Every check and helper below dereferences Player.m_localPlayer, which is null
                // while dead/respawning/loading - refund instead of letting that NRE get swallowed
                // by Update() and leave the redeem neither fulfilled nor refunded.
                if (Player.m_localPlayer == null)
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer isn't in the world right now! {m_refundMessage}");
                    throw new RedeemException("No local player", ExceptionType.Warning);
                }

                if (m_playerIsInSafeZone && !redeem.ignoreWard)
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {m_refundMessage}");
                    throw new RedeemException("Player is in safe zone", ExceptionType.Warning);
                }

                if (ProfileSettingsHelper.Current.rejectWhenPaused && (Game.IsPaused() || Menu.IsVisible()))
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, either the game is currently paused or the streamer is busy in the menu! {m_refundMessage}");
                    throw new RedeemException("Game is paused or menu is visible", ExceptionType.Warning);
                }

                if (ProfileSettingsHelper.Current.rejectWhenSleeping && Player.m_localPlayer.IsSleeping())
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is currently sleeping and can't react! {m_refundMessage}");
                    throw new RedeemException("Player is sleeping", ExceptionType.Warning);
                }

                if (ProfileSettingsHelper.Current.rejectWhenTeleporting && Player.m_localPlayer.IsTeleporting())
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

                // The surprise chest is the one redeem that isn't done yet here: it re-checks the safe
                // zone after a delay and can still cancel, so it finishes itself afterwards (see
                // SpawnSurpriseChestWithDelay) - resolving it now would mark it Fulfilled on Twitch
                // before that cancel/refund could happen.
                if (redeem.type != RedeemType.SurpriseChest)
                    FinishRedeem(customRewardEvent, redeem);

                if (WizshBoneTwitchIntegration.useRedeemCommand)
                    WizshBoneTwitchIntegration.useRedeemCommand = false;
            }
            catch (RedeemException e)
            {
                HandleRedeemException(e, customRewardEvent);
            }
        }

        /// <summary>
        /// The point where a redeem counts as carried out: counts it on the leaderboard, then
        /// auto-resolves it if that's enabled. Reached once every check has passed - a manual
        /// refund afterwards doesn't undo the leaderboard entry.
        /// </summary>
        private void FinishRedeem(CustomRewardEvent customRewardEvent, RedeemData redeem)
        {
            LeaderboardHelper.RecordRedeem(customRewardEvent, redeem);

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
            {
                // Same last-chance cancel/refund as the safe-zone check above, while the redeem isn't
                // fulfilled yet: if the dungeon filter and the Max spawned limits leave nothing for
                // the chest to spawn, refund instead of handing out an empty chest.
                if (!SurpriseChestHelper.HasEligibleLoot(redeem.chestData))
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the surprise chest has nothing it can spawn right now! {m_refundMessage}");
                    HandleRedeemException(new RedeemException("Surprise chest has no eligible loot", ExceptionType.Warning), customRewardEvent);
                    yield break;
                }

                SurpriseChestHelper.SpawnSupriseChest(prefab, redeem.chestData, customRewardEvent);
                FinishRedeem(customRewardEvent, redeem);
            }
        }

        /// <summary>
        /// Whether a redeem is sent to Twitch. Disabled redeems are left off entirely, since even a hidden
        /// reward takes one of the channel's <see cref="TwitchRewardLimitHelper.MaxRewards"/> slots.
        /// </summary>
        private static bool IsPushedToTwitch(RedeemData redeem)
        {
            return redeem.type != RedeemType.Undefined
                && redeem.enabled
                && ProgressionHelper.IsAllowedByGlobalKeys(redeem.globalKeyAdd, redeem.globalKeyRemove);
        }

        /// <summary>How many Twitch slots the active profile takes up right now.</summary>
        public int CountPushedRedeems()
        {
            return GetRedeemList().Count(IsPushedToTwitch);
        }

        public void SetRewards(List<RedeemData> redeems = null, bool isEnabled = true)
        {
            List<CustomRewardDefinition> listRewards = new List<CustomRewardDefinition>();

            if (redeems == null)
                redeems = GetRedeemList();

            redeems.RemoveAll(item => item.type == RedeemType.Undefined);

            foreach (RedeemData redeem in redeems)
            {
                if (IsPushedToTwitch(redeem))
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
                        IsEnabled = isEnabled,
                    });
                }
            }

            if (m_pushRoutine != null)
                StopCoroutine(m_pushRoutine);

            m_pushRoutine = StartCoroutine(PushRewards(listRewards.ToArray(), isEnabled));
        }

        /// <summary>
        /// Sends the rewards to Twitch, after checking they fit next to the channel's other rewards. The
        /// push is skipped (with an error toast) when it would go over Twitch's limit; if the lookup itself
        /// fails the push goes ahead, and a rejection from Twitch is reported instead of silently lost.
        /// </summary>
        private IEnumerator PushRewards(CustomRewardDefinition[] rewards, bool isEnabled)
        {
            yield return StartCoroutine(LookupChannelRewards());

            // Redeems were switched off (ClearRewards) while the lookup ran.
            if (isEnabled && !m_enabled)
            {
                m_pushRoutine = null;
                yield break;
            }

            int others = OtherChannelRewards ?? 0;

            if (rewards.Length + others > TwitchRewardLimitHelper.MaxRewards)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Redeems not pushed: {rewards.Length} redeems + {others} other Twitch rewards is over the limit of {TwitchRewardLimitHelper.MaxRewards}.");
                ToastNotifications.Show(
                    $"Redeems not sent to Twitch: {rewards.Length} redeems plus {others} of your other rewards is over Twitch's limit of {TwitchRewardLimitHelper.MaxRewards}. Delete some Twitch rewards or disable redeems.",
                    ToastType.Error);
                m_pushRoutine = null;
                yield break;
            }

            GameTask push = Twitch.API.ReplaceCustomRewards(rewards);

            while (!push.IsCompleted)
                yield return null;

            if (push.Task.IsFaulted)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Twitch rejected the redeems: {push.Task.Exception?.GetBaseException()}");
                ToastNotifications.Show("Twitch rejected the redeems. You may be over its limit of 50 rewards - check the log.", ToastType.Error);
            }

            m_pushRoutine = null;
        }

        /// <summary>
        /// Re-reads how many other rewards (not ours) the channel has; <see cref="ChannelRewardCountsChanged"/>
        /// fires when the answer is in. Does nothing when logged out.
        /// </summary>
        public void RefreshChannelRewardCounts()
        {
            if (!IsLoggedIn)
                return;

            StartCoroutine(LookupChannelRewards());
        }

        /// <summary>Forgets the last lookup (logout): the number belongs to the channel that was logged in.</summary>
        public void ClearChannelRewardCounts()
        {
            OtherChannelRewards = null;
            ChannelRewardCountsChanged?.Invoke();
        }

        private IEnumerator LookupChannelRewards()
        {
            System.Threading.Tasks.Task<TwitchRewardLimitHelper.ChannelRewardCounts> lookup =
                TwitchRewardLimitHelper.FetchAsync(m_auth?.m_userInfo?.channelId);
            float deadline = Time.realtimeSinceStartup + ChannelLookupTimeoutSeconds;

            while (!lookup.IsCompleted && Time.realtimeSinceStartup < deadline)
                yield return null;

            // Unknown (null) when the lookup failed or timed out, rather than a stale number.
            OtherChannelRewards = lookup.IsCompleted ? lookup.Result?.Others : null;
            ChannelRewardCountsChanged?.Invoke();
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
            // A push still waiting on its lookup must not land after the clear.
            if (m_pushRoutine != null)
            {
                StopCoroutine(m_pushRoutine);
                m_pushRoutine = null;
            }

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
