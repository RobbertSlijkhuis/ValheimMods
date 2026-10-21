using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchApi;
using WizshBoneTwitchIntegration.TwitchApi.Models;
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
        // the lookup failed). Our own rewards are managed by the sync, so they aren't counted here.
        public int? OtherChannelRewards { get; private set; }

        /// <summary>
        /// Raised on the main thread whenever <see cref="OtherChannelRewards"/> changes (a lookup finished,
        /// or logout cleared it), so an open Redeems tab can update itself. Static because the UI outlives
        /// this component (it lives on <c>Game.instance</c>, which is replaced per world).
        /// </summary>
        public static event Action ChannelRewardCountsChanged;

        private const float ChannelLookupTimeoutSeconds = 10f;
        private const float SyncDelaySeconds = 1.5f;
        private Coroutine m_delayedSync;

        // Everything that changes the rewards on Twitch goes through one worker (RunWorkerAsync) that does one thing
        // at a time. Callers only note what they want - a sync to the current state, or a clear - and the worker picks
        // it up as soon as the step that is running is done, so two actions never overlap however fast they follow
        // each other. Only the worker touches Twitch; it runs on the main thread between its awaits.
        private bool m_workerRunning;
        private bool m_syncRequested;
        private ClearRequest m_clearRequest;

        // Lets a running sync stop after its current request when something newer is waiting; the worker then
        // starts over from the rewards as they are on Twitch by then.
        private CancellationTokenSource m_passCancel;

        private class ClearRequest
        {
            public string ChannelId;
            public string Token;
            public readonly TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
        }

        // Redeem id -> id of its reward on Twitch, as far as the last sync knows. This is the link between a redeem
        // and its reward that survives a rename: the sync finds the reward by it (and edits it instead of replacing
        // it), a redemption is matched to its redeem by it, and the history lookup uses it. Only lives for the session:
        // the rewards are removed at logout, so there is nothing to remember across one.
        private readonly Dictionary<string, string> m_rewardIds = new Dictionary<string, string>();

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
            // No more syncs for a component that is gone; a clear that was asked for still runs.
            m_syncRequested = false;
            m_passCancel?.Cancel();

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
                StartCoroutine(WarnIfNotCleared(ClearRewards()));
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

        /// <summary>By title only, for the legacy GUI_OLD, which has no redeem to ask about.</summary>
        public bool HasUnresolvedRedeemsFor(string redeemTitle)
        {
            return m_enabled
                && !ProfileSettingsHelper.Current.autoResolveRedeems
                && m_redeemHistory.Exists(item =>
                    item.CustomRewardTitle == redeemTitle
                    && item.Status == CustomRewardRedemptionState.Unfulfilled);
        }

        /// <summary>
        /// Whether redemptions of <paramref name="redeem"/> are still waiting to be completed or refunded. Found by the
        /// redeem's reward, so redemptions made under an earlier title (before a rename) count too. Only a redeem
        /// without a known reward is looked up by its title, since a title can be reused by another redeem later.
        /// </summary>
        public bool HasUnresolvedRedeemsFor(RedeemData redeem)
        {
            if (!m_enabled || ProfileSettingsHelper.Current.autoResolveRedeems)
                return false;

            if (m_rewardIds.TryGetValue(redeem.id ?? "", out string rewardId))
            {
                return m_redeemHistory.Exists(item =>
                    item.Status == CustomRewardRedemptionState.Unfulfilled && item.CustomRewardId == rewardId);
            }

            string title = RedeemManager.GetFullTitle(redeem);

            return m_redeemHistory.Exists(item =>
                item.Status == CustomRewardRedemptionState.Unfulfilled && RedeemManager.SameTitle(item.CustomRewardTitle, title));
        }

        /// <summary>
        /// The redeem a redemption belongs to: by the id of the reward it was made on, which still works while Twitch
        /// shows an old title (a rename that is not synced yet), and otherwise by its title.
        /// </summary>
        private RedeemData FindRedeemFor(CustomRewardEvent customRewardEvent)
        {
            if (!string.IsNullOrEmpty(customRewardEvent.CustomRewardId))
            {
                foreach (KeyValuePair<string, string> link in m_rewardIds)
                {
                    if (link.Value != customRewardEvent.CustomRewardId)
                        continue;

                    RedeemData byId = GetRedeemList().Find(item => item.id == link.Key);

                    if (byId != null)
                        return byId;
                }
            }

            // customRewardEvent.CustomRewardTitle is whatever was actually pushed to Twitch - the Twitch-facing
            // (prefixed) title, not the raw RedeemData.title - so match against RedeemManager.GetFullTitle() rather
            // than item.title directly. Capitals are ignored: the reward may have been retitled in a different case
            // on twitch.tv since the last sync.
            return GetRedeemList().Find(item => RedeemManager.SameTitle(RedeemManager.GetFullTitle(item), customRewardEvent.CustomRewardTitle));
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

                RedeemData redeem = FindRedeemFor(customRewardEvent);
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
        /// Whether a redeem is sent to Twitch. Disabled redeems, and ones their global keys don't allow
        /// right now, are left off entirely (and removed if they were there), since even a hidden reward
        /// takes one of the channel's <see cref="TwitchRewardLimitHelper.MaxRewards"/> slots.
        /// </summary>
        private static bool IsPushedToTwitch(RedeemData redeem, string assumeKey = null)
        {
            return redeem.type != RedeemType.Undefined
                && redeem.enabled
                && ProgressionHelper.IsAllowedByGlobalKeys(redeem.globalKeyAdd, redeem.globalKeyRemove, assumeKey);
        }

        /// <summary>How many Twitch slots the active profile takes up right now.</summary>
        public int CountPushedRedeems()
        {
            return GetRedeemList().Count(r => IsPushedToTwitch(r));
        }

        /// <summary>
        /// How many Twitch slots the active profile would take if the world gained <paramref name="globalKey"/>
        /// (e.g. after the next boss). Needs a loaded world: global keys only exist there.
        /// </summary>
        public int CountPushedRedeemsAfter(string globalKey)
        {
            return GetRedeemList().Count(r => IsPushedToTwitch(r, globalKey));
        }

        /// <summary>
        /// True while the last push was skipped because the redeems plus the channel's other rewards don't fit
        /// under Twitch's limit. Nothing was changed on Twitch then. <see cref="RetryPush"/> tries again.
        /// </summary>
        public bool PushBlockedByLimit { get; private set; }

        /// <summary>Pushes again, e.g. after freeing up slots on twitch.tv. Does nothing unless redeems are on.</summary>
        public void RetryPush()
        {
            if (!IsLoggedIn || !m_enabled)
                return;

            SetRewards();
        }

        private void SetPushBlocked(bool blocked)
        {
            if (PushBlockedByLimit == blocked)
                return;

            PushBlockedByLimit = blocked;

            // Stays on screen until the redeems fit again (a successful Retry, or redeems switched off).
            StatusMessages.SetRedeemsOverLimit(blocked);
            ChannelRewardCountsChanged?.Invoke();
        }

        /// <summary>
        /// Asks for the rewards on Twitch to be made to match the profile as it is by the time the worker gets to
        /// it. Returns at once; asking again while a sync is running makes it stop early and start over.
        /// </summary>
        public void SetRewards()
        {
            m_syncRequested = true;
            m_passCancel?.Cancel();
            EnsureWorker();
        }

        /// <summary>
        /// <see cref="SetRewards"/> once no further call has come in for <see cref="SyncDelaySeconds"/> - for a value that
        /// is typed letter by letter. Does nothing when redeems are off or the login is gone by then.
        /// </summary>
        public void SetRewardsSoon()
        {
            if (m_delayedSync != null)
                StopCoroutine(m_delayedSync);

            m_delayedSync = StartCoroutine(SetRewardsAfterDelay());
        }

        private IEnumerator SetRewardsAfterDelay()
        {
            yield return new WaitForSecondsRealtime(SyncDelaySeconds);

            m_delayedSync = null;

            if (IsLoggedIn && m_enabled)
                SetRewards();
        }

        private void EnsureWorker()
        {
            if (m_workerRunning)
                return;

            m_workerRunning = true;
            _ = RunWorkerAsync();
        }

        // Runs until nothing is left to do. A clear always goes before a sync that was asked for after it.
        private async Task RunWorkerAsync()
        {
            try
            {
                while (m_clearRequest != null || m_syncRequested)
                {
                    if (m_clearRequest != null)
                    {
                        ClearRequest request = m_clearRequest;
                        m_clearRequest = null;
                        bool removed = await ClearOnTwitchAsync(request);

                        // The rewards are gone, so no redeem has a reward to be linked to. Only the worker changes this map,
                        // in its own turn: a sync that was still finishing when the clear was asked for wrote its rewards
                        // into it, and they were still on Twitch until now.
                        m_rewardIds.Clear();

                        request.Done.TrySetResult(removed);
                        continue;
                    }

                    m_syncRequested = false;

                    // Redeems may have been switched off, or the login lost, since the sync was asked for.
                    if (!m_enabled || !IsLoggedIn)
                        continue;

                    await SyncPassAsync();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] The Twitch reward worker stopped: {e}");
            }
            finally
            {
                m_workerRunning = false;
            }
        }

        // The rewards the profile wants on Twitch right now.
        private List<TwitchRewardSync.Desired> BuildWanted()
        {
            List<RedeemData> redeems = GetRedeemList();

            redeems.RemoveAll(item => item.type == RedeemType.Undefined);

            List<TwitchRewardSync.Desired> wanted = new List<TwitchRewardSync.Desired>();

            foreach (RedeemData redeem in redeems)
            {
                if (!IsPushedToTwitch(redeem))
                    continue;

                wanted.Add(new TwitchRewardSync.Desired
                {
                    RedeemId = redeem.id,

                    // The Twitch-facing (prefixed) title - RedeemData.title itself is stored
                    // prefix-free, see RedeemManager.GetFullTitle.
                    Title = RedeemManager.GetFullTitle(redeem),
                    Prompt = redeem.description,
                    Cost = ApplyDiscount(redeem.points),
                    BackgroundColor = redeem.backgroundColor,
                    UserInput = redeem.userInput,
                    MaxPerStream = redeem.maxPerStream,
                    MaxPerUserPerStream = redeem.maxPerUserPerStream,
                    // Capped here too (not just in the wizard) so a hand-edited profile.yaml can't
                    // send Twitch a cooldown it rejects.
                    CooldownSeconds = CooldownHelper.ClampSeconds(redeem.cooldown),
                });
            }

            return wanted;
        }

        /// <summary>
        /// One sync: makes the rewards on Twitch match the profile, after checking they fit next to the
        /// channel's other rewards. It is skipped (with an error toast) when it would go over Twitch's
        /// limit; if the lookup itself fails it goes ahead, and a rejection from Twitch is reported
        /// instead of silently lost.
        /// </summary>
        private async Task SyncPassAsync()
        {
            CancellationTokenSource cancelSource = new CancellationTokenSource();
            m_passCancel = cancelSource;
            CancellationToken cancel = cancelSource.Token;

            try
            {
                List<TwitchRewardSync.Desired> wanted = BuildWanted();

                // Our rewards as the lookup read them, handed to the sync so it doesn't read them a second time.
                TwitchRewardLimitHelper.ChannelRewardCounts counts = await LookupChannelRewardsAsync();

                // Redeems were switched off or something newer was asked for while the lookup ran.
                if (!m_enabled || cancel.IsCancellationRequested)
                    return;

                await PushAsync(wanted, counts, cancel);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Updating the Twitch rewards failed: {e}");
                ToastNotifications.Show("Updating the redeems on Twitch failed - check the log.", ToastType.Error);
            }
            finally
            {
                m_passCancel = null;
                cancelSource.Dispose();
            }
        }

        // counts is this pass's own lookup (null when it failed): not OtherChannelRewards, which other lookups can overwrite.
        private async Task PushAsync(List<TwitchRewardSync.Desired> wanted, TwitchRewardLimitHelper.ChannelRewardCounts counts, CancellationToken cancel)
        {
            int others = counts?.Others ?? 0;

            int over = wanted.Count + others - TwitchRewardLimitHelper.MaxRewards;

            if (over > 0)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Redeems not pushed: {wanted.Count} redeems + {others} other Twitch rewards is over the limit of {TwitchRewardLimitHelper.MaxRewards}.");
                string message = $"Redeems were not updated on Twitch: {wanted.Count} redeems plus {others} of your other rewards is over Twitch's limit of {TwitchRewardLimitHelper.MaxRewards}. " +
                                 $"Free up {over} slot(s) on Twitch or disable {over} redeem(s), then press Retry in the Redeems tab.";

                // The mod window is usually closed when this happens (e.g. a boss kill): SetPushBlocked puts the
                // "Redeems need fixing" message on screen, which stays until it is fixed.
                ToastNotifications.Show(message, ToastType.Error);
                SetPushBlocked(true);
                return;
            }

            SetPushBlocked(false);

            TwitchRewardSync.Result result = await TwitchRewardSync.SyncAsync(
                m_auth?.m_userInfo?.channelId, wanted, new Dictionary<string, string>(m_rewardIds), cancel, counts?.OwnRewards);

            if (result.LookupFailed)
            {
                ToastNotifications.Show("Redeems not sent to Twitch: your channel's rewards could not be read - check the log.", ToastType.Error);
                return;
            }

            // A finished sync knows the reward of every redeem that is on Twitch, so it replaces what was known (the
            // rewards it removed drop out); a cancelled one only adds what it found.
            if (!result.Cancelled)
                m_rewardIds.Clear();

            foreach (KeyValuePair<string, string> link in result.RewardIds)
                m_rewardIds[link.Key] = link.Value;

            if (result.Cancelled)
                return;

            Jotunn.Logger.LogWarning($"[WBTI] Twitch rewards updated: {result}.");

            if (result.Failed > 0)
                ToastNotifications.Show($"Twitch rejected {result.Failed} change(s) to your redeems - check the log.", ToastType.Error);

            if (result.Skipped > 0)
                ToastNotifications.Show($"{result.Skipped} redeem(s) were not sent to Twitch because their title is empty or matches another redeem's - check the log.", ToastType.Error);
        }

        /// <summary>
        /// Re-reads how many other rewards (not ours) the channel has; <see cref="ChannelRewardCountsChanged"/>
        /// fires when the answer is in. Does nothing when logged out.
        /// </summary>
        public void RefreshChannelRewardCounts()
        {
            if (!IsLoggedIn)
                return;

            _ = LookupChannelRewardsAsync();
        }

        /// <summary>Forgets the last lookup (logout): the number belongs to the channel that was logged in.</summary>
        public void ClearChannelRewardCounts()
        {
            OtherChannelRewards = null;
            ChannelRewardCountsChanged?.Invoke();
        }

        // Null when the lookup failed or timed out.
        private async Task<TwitchRewardLimitHelper.ChannelRewardCounts> LookupChannelRewardsAsync()
        {
            TwitchRewardLimitHelper.ChannelRewardCounts counts = null;

            try
            {
                Task<TwitchRewardLimitHelper.ChannelRewardCounts> lookup =
                    TwitchRewardLimitHelper.FetchAsync(m_auth?.m_userInfo?.channelId);

                if (await Task.WhenAny(lookup, Task.Delay(TimeSpan.FromSeconds(ChannelLookupTimeoutSeconds))) == lookup)
                    counts = lookup.Result;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the channel's Twitch rewards: {e.Message}");
            }

            // Unknown (null) when the lookup failed or timed out, rather than a stale number.
            OtherChannelRewards = counts?.Others;
            ChannelRewardCountsChanged?.Invoke();
            return counts;
        }

        /// <summary>
        /// Applies <see cref="ProfileSettingsData.redeemsDiscountPercent"/> to a redeem's configured
        /// point cost before it's sent to Twitch. A 0-cost redeem isn't discounted (<c>TwitchRewardSync</c>
        /// raises it to Twitch's minimum of 1 when it sends it), and any nonzero cost is never rounded
        /// down to 0 by the discount - that would silently turn a paid redeem into a free one.
        /// </summary>
        private static int ApplyDiscount(int points)
        {
            if (points <= 0)
                return points;

            int discountPercent = Mathf.Clamp(ProfileSettingsHelper.Current.redeemsDiscountPercent, 0, 100);
            return Mathf.Max(1, Mathf.RoundToInt(points * (100 - discountPercent) / 100f));
        }

        /// <summary>
        /// Removes every reward this app made from the channel. The task resolves to whether all of them
        /// are gone, and never faults, so a caller can wait for it (see <c>TwitchAuth.WaitForClear</c>) and
        /// carry on whatever Twitch answers; failures are logged.
        /// </summary>
        public Task<bool> ClearRewards()
        {
            // Off before the delete starts: a sync asked for meanwhile (boss kill, redeem save) would
            // recreate what is being removed. Every caller leaves redeems off afterwards anyway.
            m_enabled = false;

            // A sync that was asked for but hasn't started is dropped, and a running one stops early.
            m_syncRequested = false;
            m_passCancel?.Cancel();
            SetPushBlocked(false);

            // A clear that is still waiting for its turn covers this one too. The channel and token are taken now:
            // the logout that follows wipes the stored token before the deletes are done.
            if (m_clearRequest == null)
            {
                m_clearRequest = new ClearRequest
                {
                    ChannelId = m_auth?.m_userInfo?.channelId,
                    Token = TwitchTokenCapture.AccessToken,
                };
            }

            Task<bool> done = m_clearRequest.Done.Task;
            EnsureWorker();
            return done;
        }

        // For the clear that switching redeems off starts: nobody waits for it, so a failure is shown here.
        private IEnumerator WarnIfNotCleared(Task<bool> clear)
        {
            while (!clear.IsCompleted)
                yield return null;

            if (!clear.Result)
                ToastNotifications.Show("Not all redeems could be removed from Twitch. Any left are cleaned up the next time you enable redeems.", ToastType.Warning);
        }

        // Never throws: the answer is whether every reward is gone.
        private async Task<bool> ClearOnTwitchAsync(ClearRequest request)
        {
            try
            {
                return await TwitchRewardSync.ClearAsync(request.ChannelId, request.Token);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not remove the redeems from Twitch: {e.Message}");
                return false;
            }
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
