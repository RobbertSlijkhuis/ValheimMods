using Jotunn.Managers;
using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Models.Views;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;
using static SpawnAbility;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SpawnAbilityHelper : MonoBehaviour
    {
        public static bool debug;

        // Each narrowed View is the single source of truth for its redeem type's hardcoded
        // prefab/forced defaults - constructing (and discarding) one here applies that
        // normalization at redemption time too, not just when the in-game editor is open.
        // RedeemType.SpawnAbility (the raw/generic type) has no narrowed View, so no entry.
        private static readonly Dictionary<string, Action<SpawnAbilityData>> Normalizers = new Dictionary<string, Action<SpawnAbilityData>>
        {
            { RedeemType.Door,     data => new DoorView(data) },
            { RedeemType.Windmill, data => new WindmillView(data) },
            { RedeemType.Smite,    data => new SmiteView(data) },
            { RedeemType.Rain,     data => new RainView(data) },
            { RedeemType.LogRain,  data => new LogRainView(data) },
            { RedeemType.Meteor,   data => new MeteorView(data) },
            { RedeemType.Trap,     data => new TrapView(data) },
            { RedeemType.Root,     data => new RootView(data) },
        };

        public static void SpawnAbility(string type, SpawnAbilityData spawnAbilityData, CustomRewardEvent customRewardEvent, TwitchChat m_chat)
        {
            if (Normalizers.TryGetValue(type, out Action<SpawnAbilityData> normalize))
                normalize(spawnAbilityData);

            // SAPHONETTE-CLEANUP: remove this call once the bit is over. See
            // SpecialRedeemHelper.ApplyLogRainOverride for the rest of the hack. Gated on
            // LogRainOverrideEnabled so "!toggle karl" can fall back to LogRain's normal configured
            // behavior mid-stream without a rebuild.
            if (type == RedeemType.LogRain && SpecialRedeemHelper.LogRainOverrideEnabled)
                SpecialRedeemHelper.ApplyLogRainOverride(spawnAbilityData);

            GameObject showerPrefab;

            if (spawnAbilityData.prefabName == null)
                showerPrefab = WizshBoneTwitchIntegration.Instance.prefabs.FishRainScript;
            else
                showerPrefab = PrefabManager.Instance.GetPrefab(spawnAbilityData.prefabName);

            GameObject shower = GameObject.Instantiate(showerPrefab, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            SpawnAbility spawnAbility = shower.GetComponent<SpawnAbility>();
            shower.transform.SetParent(Player.m_localPlayer.transform);

            if (spawnAbility == null)
                throw new RedeemException("Could not find spawn ability on shower prefab", ExceptionType.Error);

            spawnAbility.m_setMaxInstancesFromWeaponLevel = false;
            spawnAbility.m_maxSummonReached = "You have reached the maximum of spawns";
            spawnAbility.m_snapToTerrain = spawnAbilityData.snapToterrain;

            if (spawnAbilityData.isOwner)
                spawnAbility.m_owner = Player.m_localPlayer;

            if (spawnAbilityData.accuracy != null)
                spawnAbility.m_projectileAccuracy = (float)spawnAbilityData.accuracy;

            if (spawnAbilityData.groundOffset != null)
                spawnAbility.m_spawnGroundOffset = (float)spawnAbilityData.groundOffset;

            if (spawnAbilityData.initialSpawnDelay != null)
                spawnAbility.m_initialSpawnDelay = (float)spawnAbilityData.initialSpawnDelay;

            if (spawnAbilityData.maxTargetRange != null)
                spawnAbility.m_maxTargetRange = (int)spawnAbilityData.maxTargetRange;

            if (spawnAbilityData.maxSpawned != null)
                spawnAbility.m_maxSpawned = (int)spawnAbilityData.maxSpawned;

            if (spawnAbilityData.maxToSpawn != null)
                spawnAbility.m_maxToSpawn = (int)spawnAbilityData.maxToSpawn;

            if (spawnAbilityData.minToSpawn != null)
                spawnAbility.m_minToSpawn = (int)spawnAbilityData.minToSpawn;

            if (spawnAbilityData.randomDirection != null)
                spawnAbility.m_randomDirection = (bool)spawnAbilityData.randomDirection;

            if (spawnAbilityData.randomAngleMax != null)
                spawnAbility.m_randomAngleMax = (float)spawnAbilityData.randomAngleMax;

            if (spawnAbilityData.randomAngleMin != null)
                spawnAbility.m_randomAngleMin = (float)spawnAbilityData.randomAngleMin;

            if (spawnAbilityData.randomYRotation != null)
                spawnAbility.m_randomYRotation = (bool)spawnAbilityData.randomYRotation;

            if (spawnAbilityData.spawnDelay != null)
                spawnAbility.m_spawnDelay = (float)spawnAbilityData.spawnDelay;

            if (spawnAbilityData.spawnRadius != null)
                spawnAbility.m_spawnRadius = (float)spawnAbilityData.spawnRadius;

            if (spawnAbilityData.targetType != null)
                spawnAbility.m_targetType = (TargetType)SpawnAbilityTargetType.ConvertToTargetType(spawnAbilityData.targetType);

            if (spawnAbilityData.velocity != null)
                spawnAbility.m_projectileVelocity = (float)spawnAbilityData.velocity;

            if (spawnAbilityData.velocityMax != null)
                spawnAbility.m_projectileVelocityMax = (float)spawnAbilityData.velocityMax;

            if (spawnAbilityData.spawns != null && spawnAbilityData.spawns.Count > 0)
            {
                List<GameObject> spawns = new List<GameObject>();

                foreach (string spawn in spawnAbilityData.spawns)
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

            SpawnAbility.TargetType? targetType = SpawnAbilityTargetType.ConvertToTargetType(spawnAbilityData.targetType);

            if (targetType != null)
                spawnAbility.m_targetType = (SpawnAbility.TargetType)targetType;
            else
                Jotunn.Logger.LogWarning("SpawnAbility target type is null");

            // Rolled here (rather than inside Spawn2) so the maxSpawned check below can cancel
            // the whole redeem up front if there isn't room for the full amount, instead of
            // letting it start and silently skip spawns once the cap is hit partway through.
            int toSpawn = UnityEngine.Random.Range(spawnAbility.m_minToSpawn, spawnAbility.m_maxToSpawn);

            if (spawnAbilityData.maxSpawned != null && spawnAbilityData.maxSpawned > 0)
            {
                foreach (GameObject prefab in spawnAbility.m_spawnPrefab)
                {
                    if (SpawnSystem.GetNrOfInstances(prefab) + toSpawn > spawnAbilityData.maxSpawned)
                    {
                        m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, there isn't enough room left for the full spawn amount! {TwitchCustomRewards.m_refundMessage}");
                        throw new RedeemException("Not enough space left for this redeem", ExceptionType.Warning);
                    }
                }
            }

            if (spawnAbilityData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, spawnAbilityData.announceMessage), 3000);

            // SAPHONETTE-CLEANUP: see SpecialRedeemHelper.BuildTalkCreatureData.
            CreatureData creatureData = SpecialRedeemHelper.BuildTalkCreatureData(spawnAbilityData);
            m_chat.StartCoroutine(spawnAbility.Spawn2(toSpawn, customRewardEvent, spawnAbilityData, creatureData));
        }
    }
}
