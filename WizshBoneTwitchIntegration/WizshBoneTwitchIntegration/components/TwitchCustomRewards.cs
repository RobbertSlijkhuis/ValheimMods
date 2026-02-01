using Jotunn.Managers;
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

                if (m_alias != null && currentRewardEvent.RedeemerName == m_auth.m_userInfo.displayName)
                {
                    Jotunn.Logger.LogWarning($"{m_auth.m_userInfo.displayName} pretending to be {m_alias}");
                    currentRewardEvent.RedeemerName = m_alias;
                }

                Jotunn.Logger.LogWarning($"{currentRewardEvent.RedeemerName} has bought {currentRewardEvent.CustomRewardTitle} for {currentRewardEvent.CustomRewardCost}!");
                Jotunn.Logger.LogWarning($"Time: {currentRewardEvent.RedeemedAt}");
                Jotunn.Logger.LogWarning($"Status: {currentRewardEvent.Status}");

                string refundAutoResolveOn = $"Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!";
                string refundAutoResolveOff = $"Please notify the streamer to refund your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points!";

                RedeemEntry redeem = m_redeems.list.Find(item => item.title == currentRewardEvent.CustomRewardTitle);
                if (redeem == null)
                {
                    //m_chat.Send($"Could not find redeem! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                    //throw new RedeemException("Could not find redeem", ExceptionType.Error);
                    Jotunn.Logger.LogWarning($"Could not find redeem with the name: {currentRewardEvent.CustomRewardTitle}, probaly not part of the mod. Aborting...");
                }

                if (m_bannedUsers.Contains(currentRewardEvent.RedeemerName.ToLower()))
                    throw new RedeemException("Redeemer is banned", ExceptionType.Warning);

                if (Game.IsPaused() || Menu.IsVisible())
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, either the game is currently paused or the streamer is busy in the menu! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                    throw new RedeemException("Game is paused or menu is visible", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.IsSleeping())
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is currently sleeping and can't react! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                    throw new RedeemException("Player is sleeping", ExceptionType.Warning);
                }

                if (m_playerIsInSafeZone && !redeem.ignoreWard)
                {
                    m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a Twitch safe zone! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                    throw new RedeemException("Player is in safe zone", ExceptionType.Warning);
                }

                if (Player.m_localPlayer.InInterior())
                {
                    string dungeonType = EnvMan.instance.GetCurrentEnvironment().m_name;
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

                        switch (dungeonType)
                        {
                            case nameof(DungeonType.FrostCave):
                            case nameof(DungeonType.HowlingCavern):
                                notAllowedList.Remove("golem");
                                break;
                            case nameof(DungeonType.InfestedMine):
                                notAllowedList.Remove("seekerbrute");
                                notAllowedList.Remove("golem");
                                break;
                            case nameof(DungeonType.Queen):
                                notAllowedList.Remove("bat");
                                notAllowedList.Remove("deathsquito");
                                notAllowedList.Remove("gjall");
                                notAllowedList.Remove("golem");
                                notAllowedList.Remove("seekerbrute");
                                break;
                        }

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
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                        throw new RedeemException("Player is in dungeon and redeem is not allowed", ExceptionType.Warning);
                    }
                }

                if (redeem.type == RedeemType.SpawnCreature)
                {
                    if (redeem.creatures == null)
                        throw new RedeemException("Could not find creature data for SpawnCreature", ExceptionType.Error);

                    foreach (SpawnCreatureData creature in redeem.creatures)
                    {
                        if (redeem.userInput)
                            creature.talkMessage = m_chat.GetLastMessageOfUser(currentRewardEvent.RedeemerName)?.message;

                        if (creature.amount > 1)
                            RedeemHelper.SpawnCreatures(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent, redeem.ignoreWard));
                        else
                            RedeemHelper.SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, currentRewardEvent, redeem.ignoreWard));
                    }
                }

                if (redeem.type == RedeemType.SpawnHallucination)
                {
                    RedeemHelper.hallucinationCount = 0;
                    InvokeRepeating(nameof(StartHallucinations), 0f, 20f);
                }

                if (redeem.type == RedeemType.SpawnShower)
                {
                    if (redeem.shower == null)
                        throw new RedeemException("Could not find shower data for SpawnShower", ExceptionType.Error);

                    GameObject showerPrefab;

                    if (redeem.shower.prefabName == null)
                        showerPrefab = WizshBoneTwitchIntegration.Instance.prefabs.FishRainScript;
                    else
                        showerPrefab = PrefabManager.Instance.GetPrefab(redeem.shower.prefabName);

                    GameObject shower = Instantiate(showerPrefab, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                    SpawnAbility spawnAbility = shower.GetComponent<SpawnAbility>();
                    shower.transform.SetParent(Player.m_localPlayer.transform);

                    if (spawnAbility == null)
                        throw new RedeemException("Could not find spawn ability on shower prefab", ExceptionType.Error);

                    spawnAbility.m_setMaxInstancesFromWeaponLevel = false;
                    spawnAbility.m_maxSummonReached = "You have reached the maximum of spawns";

                    if (redeem.shower.isOwner)
                        spawnAbility.m_owner = Player.m_localPlayer;

                    if (redeem.shower.accuracy != null)
                        spawnAbility.m_projectileAccuracy = (float)redeem.shower.accuracy;

                    if (redeem.shower.groundOffset != null)
                        spawnAbility.m_spawnGroundOffset = (float)redeem.shower.groundOffset;

                    if (redeem.shower.initialSpawnDelay != null)
                        spawnAbility.m_initialSpawnDelay = (float)redeem.shower.initialSpawnDelay;

                    if (redeem.shower.maxTargetRange != null)
                        spawnAbility.m_maxTargetRange = (int)redeem.shower.maxTargetRange;

                    if (redeem.shower.maxSpawned != null)
                        spawnAbility.m_maxSpawned = (int)redeem.shower.maxSpawned;

                    if (redeem.shower.maxToSpawn != null)
                        spawnAbility.m_maxToSpawn = (int)redeem.shower.maxToSpawn;

                    if (redeem.shower.minToSpawn != null)
                        spawnAbility.m_minToSpawn = (int)redeem.shower.minToSpawn;

                    if (redeem.shower.randomDirection != null)
                        spawnAbility.m_randomDirection = (bool)redeem.shower.randomDirection;

                    if (redeem.shower.randomAngleMax != null)
                        spawnAbility.m_randomAngleMax = (float)redeem.shower.randomAngleMax;

                    if (redeem.shower.randomAngleMin != null)
                        spawnAbility.m_randomAngleMin = (float)redeem.shower.randomAngleMin;

                    if (redeem.shower.randomYRotation != null)
                        spawnAbility.m_randomYRotation = (bool)redeem.shower.randomYRotation;

                    if (redeem.shower.spawnDelay != null)
                        spawnAbility.m_spawnDelay = (float)redeem.shower.spawnDelay;

                    if (redeem.shower.spawnRadius != null)
                        spawnAbility.m_spawnRadius = (float)redeem.shower.spawnRadius;

                    if (redeem.shower.velocity != null)
                        spawnAbility.m_projectileVelocity = (float)redeem.shower.velocity;

                    if (redeem.shower.velocityMax != null)
                        spawnAbility.m_projectileVelocityMax = (float)redeem.shower.velocityMax;

                    if (redeem.shower.spawns != null && redeem.shower.spawns.Count > 0)
                    {
                        List<GameObject> spawns = new List<GameObject>();

                        foreach (string spawn in redeem.shower.spawns)
                        {
                            GameObject prefab = PrefabManager.Instance.GetPrefab(spawn);

                            if (prefab == null)
                            {
                                Jotunn.Logger.LogWarning($"Could not find prefab {spawn} for SpawnAbility, skipping...");
                                continue;
                            }

                            spawns.Add(prefab);
                        }

                        spawnAbility.m_spawnPrefab = spawns.ToArray();
                    }

                    SpawnAbility.TargetType? targetType = SpawnAbilityTargetType.ConvertToTargetType(redeem.shower.targetType);

                    if (targetType != null)
                        spawnAbility.m_targetType = (SpawnAbility.TargetType)targetType;
                    else
                        Jotunn.Logger.LogWarning("SpawnAbility target type is null");

                    if (redeem.shower.maxSpawned != null && redeem.shower.maxSpawned > 0)
                    {
                        foreach (GameObject prefab in spawnAbility.m_spawnPrefab)
                        {
                            if (SpawnSystem.GetNrOfInstances(prefab) >= redeem.shower.maxSpawned)
                            {
                                m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, there is already a maximum number of spawns! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                                throw new RedeemException("Already on max spawned for this redeeem", ExceptionType.Warning);
                            }
                        }
                    }

                    if (redeem.shower.announceMessage != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, redeem.shower.announceMessage);

                    SpawnCreatureData creatureData = new SpawnCreatureData();
                    StartCoroutine(spawnAbility.Spawn2(currentRewardEvent, redeem.shower, creatureData));
                }

                if (redeem.type == RedeemType.SpawnMist)
                {
                    if (redeem.mist == null)
                        throw new RedeemException("could not find mist data for SpawnMist", ExceptionType.Error);

                    RedeemHelper.SpawnMist(redeem.mist);

                    if (redeem.mist.announceMessage != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, redeem.mist.announceMessage);
                }

                //if (redeem.type == RedeemType.PlayerShrink)
                //{
                //    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                //    StatusEffectData statusEffect = new StatusEffectData("MiniMe", PluginConfig.configMiniMeDuration.Value);
                //    statusEffect.blockedBy.Add("BigMe");
                //    statusEffect.onStart = customStatusEffect.ShrinkPlayer;
                //    statusEffect.onEnd = customStatusEffect.ResetPlayer;
                //    bool success = customStatusEffect.AddStatusEffect(statusEffect);

                //    if (!success)
                //    {
                //        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the selected StatusEffect or it is blocked by a different one! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                //        throw new RedeemException("Player already has StatusEffect", ExceptionType.Warning);
                //    }
                //}

                //if (redeem.type == RedeemType.PlayerGrow)
                //{
                //    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                //    StatusEffectData statusEffect = new StatusEffectData("BigMe", PluginConfig.configMiniMeDuration.Value);
                //    statusEffect.blockedBy.Add("MiniMe");
                //    statusEffect.onStart = customStatusEffect.GrowPlayer;
                //    statusEffect.onEnd = customStatusEffect.ResetPlayer;
                //    bool success = customStatusEffect.AddStatusEffect(statusEffect);

                //    if (!success)
                //    {
                //        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the selected StatusEffect or it is blocked by a different one! Your redeem {currentRewardEvent.CustomRewardTitle} of {currentRewardEvent.CustomRewardCost} points has been refunded!");
                //        throw new RedeemException("Player already has StatusEffect", ExceptionType.Warning);
                //    }
                //}

                if (redeem.type == RedeemType.StatusEffect)
                {
                    if (redeem.statusEffects == null || redeem.statusEffects.Count == 0)
                        throw new RedeemException("Could not find a status effect to apply", ExceptionType.Error);

                    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                    List<string> availableStatusEffects = StatusEffectHelper.GetAvailableStatusEffects();

                    foreach (StatusEffectData statusEffect in redeem.statusEffects)
                    {
                        statusEffect.Init();

                        if (!availableStatusEffects.Contains(statusEffect.name))
                        {
                            Jotunn.Logger.LogWarning("Not a valid status effect to apply, skipping...");
                            continue;
                        }

                        if (!statusEffect.renew && Player.m_localPlayer.GetSEMan().HaveStatusEffect(statusEffect.nameHash))
                        {
                            m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the {statusEffect.name} StatusEffect! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                            throw new RedeemException($"Player already has the {statusEffect.name} status effect", ExceptionType.Warning);
                        }

                        if (statusEffect.name == "PlayerShrink")
                        {
                            Jotunn.Logger.LogWarning("Found shrink");
                            statusEffect.onStart = customStatusEffect.PlayerShrink;
                            statusEffect.onEnd = customStatusEffect.PlayerSizeReset;
                        }
                        else if (statusEffect.name == "PlayerGrow")
                        {
                            Jotunn.Logger.LogWarning("Found grow");
                            statusEffect.onStart = customStatusEffect.PlayerGrow;
                            statusEffect.onEnd = customStatusEffect.PlayerSizeReset;
                        }
                        else if (statusEffect.name == "WindInBack")
                        {
                            Jotunn.Logger.LogWarning("Found WindInback");
                            statusEffect.onStart = customStatusEffect.WindInTheBack;
                        }

                        bool success = customStatusEffect.AddStatusEffect(statusEffect);
                        Jotunn.Logger.LogWarning("AddStatusEffect: " + success);
                    }
                }

                if (redeem.type == RedeemType.StatusEffectRandom)
                {
                    if (redeem.statusEffects == null || redeem.statusEffects.Count == 0)
                        throw new RedeemException("Could not find a status effect to apply", ExceptionType.Error);

                    TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                    List<string> availableStatusEffects = StatusEffectHelper.GetAvailableStatusEffects();
                    StatusEffectData random = StatusEffectHelper.GetRandomStatusEffect(redeem.statusEffects);
                    Jotunn.Logger.LogWarning(random.name);
                    Jotunn.Logger.LogWarning(random.renew);
                    random.Init();

                    if (!availableStatusEffects.Contains(random.name))
                    {
                        Jotunn.Logger.LogWarning("Not a valid status effect to apply, skipping...");
                        throw new RedeemException($"{random.name} is not a valid status effect", ExceptionType.Warning);
                    }

                    if (!random.renew && Player.m_localPlayer.GetSEMan().HaveStatusEffect(random.nameHash))
                    {
                        m_chat.Send($"Sorry @{currentRewardEvent.RedeemerName}, the streamer already has the {random.name} StatusEffect! {(PluginConfig.configAutoResolveRedeems.Value ? refundAutoResolveOn : refundAutoResolveOff)}");
                        throw new RedeemException($"Player already has the {random.name} status effect", ExceptionType.Warning);
                    }

                    if (random.name == "PlayerShrink")
                    {
                        Jotunn.Logger.LogWarning("Found shrink");
                        random.onStart = customStatusEffect.PlayerShrink;
                        random.onEnd = customStatusEffect.PlayerSizeReset;
                    }
                    else if (random.name == "PlayerGrow")
                    {
                        Jotunn.Logger.LogWarning("Found grow");
                        random.onStart = customStatusEffect.PlayerGrow;
                        random.onEnd = customStatusEffect.PlayerSizeReset;
                    }
                    else if (random.name == "WindInBack")
                    {
                        Jotunn.Logger.LogWarning("Found WindInback");
                        random.onStart = customStatusEffect.WindInTheBack;
                    }

                    bool success = customStatusEffect.AddStatusEffect(random);
                    Jotunn.Logger.LogWarning("AddRandomStatusEffect: " + success);
                }

                if (redeem.type == RedeemType.SurpriseChest)
                {
                    if (redeem.chest == null)
                    {
                        throw new RedeemException("Could not find chest data for SurpriseChest", ExceptionType.Error);
                    }

                    GameObject chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestIron;

                    if (redeem.chest.type == ChestType.Gold)
                        chestPrefab = WizshBoneTwitchIntegration.Instance.prefabs.ChestGold;

                    if (redeem.chest.announceMessage != null)
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, redeem.chest.announceMessage);

                    StartCoroutine(InitSurpriseChestWithDelay(chestPrefab, redeem.chest));
                }

                if (redeem.type == RedeemType.TerrainRemove)
                {
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.NoFallDamage);
                    GameObject dig = Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
                }

                if (redeem.type == RedeemType.ExplodeFish)
                {
                    RedeemHelper.DetonateFish();
                    Invoke(nameof(DestroyFish), 0.5f);
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
                if ((redeem.globalKeyAdd == "" && (redeem.globalKeyRemove == "" || !ZoneSystem.instance.GetGlobalKey(redeem.globalKeyRemove))) || (ZoneSystem.instance.GetGlobalKey(redeem.globalKeyAdd) && !ZoneSystem.instance.GetGlobalKey(redeem.globalKeyRemove)))
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

            if (m_auth && m_auth.m_loggedIn)
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
