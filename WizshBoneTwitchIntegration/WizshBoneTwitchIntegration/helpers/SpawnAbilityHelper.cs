using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;
using static SpawnAbility;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SpawnAbilityHelper : MonoBehaviour
    {
        public static bool debug;

        public static void SpawnAbility(SpawnAbilityData spawnAbilityData, CustomRewardEvent customRewardEvent, TwitchChat m_chat)
        {
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

            if (spawnAbilityData.maxSpawned != null && spawnAbilityData.maxSpawned > 0)
            {
                foreach (GameObject prefab in spawnAbility.m_spawnPrefab)
                {
                    if (SpawnSystem.GetNrOfInstances(prefab) >= spawnAbilityData.maxSpawned)
                    {
                        m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, there is already a maximum number of spawns! {(PluginConfig.configAutoResolveRedeems.Value ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                        throw new RedeemException("Already on max spawned for this redeeem", ExceptionType.Warning);
                    }
                }
            }

            if (spawnAbilityData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, spawnAbilityData.announceMessage), 3000);

            CreatureData creatureData = new CreatureData();
            m_chat.StartCoroutine(spawnAbility.Spawn2(customRewardEvent, spawnAbilityData, creatureData));
        }

        /// <summary>
        /// Calculate the damage based on player max health and armor
        /// </summary>
        /// <param name="spawnAbilityData"></param>
        /// <returns></returns>
        public static HitData.DamageTypes CalculateDamageBasedOnMaxHealthAndArmor(SpawnAbilityData spawnAbilityData)
        {
            HitData.DamageTypes damages = DamageHelper.ConvertToDamageTypes(spawnAbilityData.damage);
            float armor = Player.m_localPlayer.GetBodyArmor();
            float maxHealth = Player.m_localPlayer.GetMaxHealth();
            float totalDamage = damages.GetTotalDamage();
            float maxDamage = maxHealth * spawnAbilityData.damage.maxHealthPercentage;

            if (debug)
            {
                Jotunn.Logger.LogWarning("maxHealth: " + maxHealth);
                Jotunn.Logger.LogWarning("totalDamage: " + totalDamage);
                Jotunn.Logger.LogWarning("maxDamage: " + maxDamage);
                Jotunn.Logger.LogWarning("multiplier: " + maxDamage / totalDamage);
            }

            damages.Modify(maxDamage / totalDamage);

            if (debug)
            {
                Jotunn.Logger.LogWarning("newDamage: " + damages.GetTotalDamage());
                Jotunn.Logger.LogWarning("Armor: " + armor);
                Jotunn.Logger.LogWarning("newDamage with armor: " + HitData.DamageTypes.ApplyArmor(damages.GetTotalDamage(), armor));
            }

            damages.IncreaseEqually(armor * spawnAbilityData.damage.armorPercentage);

            if (debug)
            {
                Jotunn.Logger.LogWarning("newDamage with armor offset: " + damages.GetTotalDamage());
            }

            return damages;
        }
    }
}
