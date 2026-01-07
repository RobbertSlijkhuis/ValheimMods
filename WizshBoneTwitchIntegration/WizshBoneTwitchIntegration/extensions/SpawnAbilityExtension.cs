using System;
using System.Collections;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using static SpawnAbility;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class SpawnAbilityExtension
    {
        public static IEnumerator Spawn2(this SpawnAbility spawnAbility, CustomRewardEvent currentRewardEvent, SpawnAbilityData spawnAbilityData, SpawnCreatureData creatureData)
        {
            if (spawnAbility.m_initialSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(spawnAbility.m_initialSpawnDelay);
            }

            int toSpawn = UnityEngine.Random.Range(spawnAbility.m_minToSpawn, spawnAbility.m_maxToSpawn);
            Skills skills = (spawnAbility.m_owner ? spawnAbility.m_owner.GetSkills() : null);
            int num3;
            for (int i = 0; i < toSpawn; num3 = i + 1, i = num3)
            {
                Vector3 targetPosition = spawnAbility.transform.position;
                bool foundSpawnPoint = false;
                int tries = ((spawnAbility.m_targetType != TargetType.RandomPathfindablePosition) ? 1 : 5);
                for (int j = 0; j < tries; j++)
                {
                    bool flag;
                    foundSpawnPoint = (flag = spawnAbility.FindTarget(out targetPosition, i, toSpawn));
                    if (flag)
                    {
                        break;
                    }

                    if (spawnAbility.m_targetType == TargetType.RandomPathfindablePosition)
                    {
                        if (j == tries - 1)
                        {
                            Terminal.LogWarning($"SpawnAbility failed to pathfindable target after {tries} tries, defaulting to transform position.");
                            targetPosition = spawnAbility.transform.position;
                            foundSpawnPoint = true;
                        }
                        else
                        {
                            Terminal.Log("SpawnAbility failed to pathfindable target, waiting before retry.");
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                }

                if (!foundSpawnPoint)
                {
                    Terminal.LogWarning("SpawnAbility failed to find spawn point, aborting spawn.");
                    continue;
                }

                Vector3 spawnPoint = targetPosition;
                if (spawnAbility.m_targetType != TargetType.RandomPathfindablePosition)
                {
                    Vector3 vector = (spawnAbility.m_spawnAtTarget ? targetPosition : spawnAbility.transform.position);
                    Vector2 vector2 = UnityEngine.Random.insideUnitCircle * spawnAbility.m_spawnRadius;
                    if (spawnAbility.m_circleSpawn)
                    {
                        vector2 = spawnAbility.GetCirclePoint(i, toSpawn) * spawnAbility.m_spawnRadius;
                    }

                    spawnPoint = vector + new Vector3(vector2.x, 0f, vector2.y);
                    if (spawnAbility.m_snapToTerrain)
                    {
                        ZoneSystem.instance.GetSolidHeight(spawnPoint, out var height, spawnAbility.m_getSolidHeightMargin);
                        spawnPoint.y = height;
                    }

                    spawnPoint.y += spawnAbility.m_spawnGroundOffset;
                    if (Mathf.Abs(spawnPoint.y - vector.y) > 100f)
                    {
                        continue;
                    }
                }

                GameObject prefab = spawnAbility.m_spawnPrefab[UnityEngine.Random.Range(0, spawnAbility.m_spawnPrefab.Length)];
                if (spawnAbility.m_maxSpawned > 0 && SpawnSystem.GetNrOfInstances(prefab) >= spawnAbility.m_maxSpawned)
                {
                    if (spawnAbility.m_owner is Player player)
                    {
                        player.Message(MessageHud.MessageType.Center, spawnAbility.m_maxSummonReached);
                    }

                    continue;
                }

                spawnAbility.m_preSpawnEffects.Create(spawnPoint, Quaternion.identity);
                if (spawnAbility.m_preSpawnDelay > 0f)
                {
                    yield return new WaitForSeconds(spawnAbility.m_preSpawnDelay);
                }

                Terminal.Log("SpawnAbility spawning a " + prefab.name);
                GameObject gameObject = UnityEngine.Object.Instantiate(prefab, spawnPoint, Quaternion.Euler(0f, UnityEngine.Random.value * (float)Math.PI * 2f, 0f));
                ZNetView component = gameObject.GetComponent<ZNetView>();
                Projectile component2 = gameObject.GetComponent<Projectile>();
                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();
                Humanoid humanoid1 = gameObject.GetComponent<Humanoid>();
                ImpactEffect impactEffect = gameObject.GetComponent<ImpactEffect>();

                if (monsterAI != null && humanoid1 != null)
                {
                    TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

                    if (persistentData != null)
                        persistentData.SetData(currentRewardEvent.RedeemerName, creatureData, false);
                    else
                        Jotunn.Logger.LogWarning("Creature does not have persistent data somehow!");
                }

                if (monsterAI == null && impactEffect != null)
                {
                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

                    if (spawnAbilityData.damage != null)
                        impactEffect.m_damages = spawnAbilityData.damage.ConvertToDamageTypes();

                    if (spawnAbilityData.dropVelocity != 0f)
                        rigidbody.AddForce(new Vector3(0f, (float)spawnAbilityData.dropVelocity, 0f) * rigidbody.mass * rigidbody.mass);

                    impactEffect.StartCoroutine(impactEffect.ResetShowerSettings());
                }

                if ((bool)component2)
                {
                    spawnAbility.SetupProjectile(component2, targetPosition);
                }

                if (spawnAbility.m_randomYRotation)
                {
                    gameObject.transform.Rotate(Vector3.up, UnityEngine.Random.Range(-180, 180));
                }

                if ((bool)skills)
                {
                    if (spawnAbility.m_copySkill != 0 && spawnAbility.m_copySkillToRandomFactor > 0f)
                    {
                        component.GetZDO().Set(ZDOVars.s_randomSkillFactor, 1f + skills.GetSkillLevel(spawnAbility.m_copySkill) * spawnAbility.m_copySkillToRandomFactor);
                    }

                    if (spawnAbility.m_levelUpSettings.Count > 0)
                    {
                        Character component3 = gameObject.GetComponent<Character>();
                        if ((object)component3 != null)
                        {
                            for (int num = spawnAbility.m_levelUpSettings.Count - 1; num >= 0; num--)
                            {
                                LevelUpSettings levelUpSettings = spawnAbility.m_levelUpSettings[num];
                                if (skills.GetSkillLevel(levelUpSettings.m_skill) >= (float)levelUpSettings.m_skillLevel)
                                {
                                    component3.SetLevel(levelUpSettings.m_setLevel);
                                    int num2 = (spawnAbility.m_setMaxInstancesFromWeaponLevel ? spawnAbility.m_weapon.m_quality : levelUpSettings.m_maxSpawns);
                                    if (num2 > 0)
                                    {
                                        component.GetZDO().Set(ZDOVars.s_maxInstances, num2);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                if (spawnAbility.m_commandOnSpawn)
                {
                    Tameable component4 = gameObject.GetComponent<Tameable>();
                    if ((object)component4 != null && spawnAbility.m_owner is Humanoid humanoid)
                    {
                        component4.Command(humanoid, message: false);
                        if (humanoid == Player.m_localPlayer)
                        {
                            Game.instance.IncrementPlayerStat(PlayerStatType.SkeletonSummons);
                        }
                    }
                }

                if (spawnAbility.m_wakeUpAnimation)
                {
                    gameObject.GetComponent<ZSyncAnimation>()?.SetBool("wakeup", value: true);
                }

                BaseAI component5 = gameObject.GetComponent<BaseAI>();
                if (component5 != null)
                {
                    if (spawnAbility.m_alertSpawnedCreature)
                    {
                        component5.Alert();
                    }

                    BaseAI baseAI = spawnAbility.m_owner.GetBaseAI();
                    if (component5.m_aggravatable && (bool)baseAI && baseAI.m_aggravatable)
                    {
                        component5.SetAggravated(baseAI.IsAggravated(), BaseAI.AggravatedReason.Damage);
                    }

                    if (spawnAbility.m_passiveAggressive)
                    {
                        component5.m_passiveAggresive = true;
                    }
                }

                spawnAbility.SetupAoe(gameObject.GetComponent<Character>(), spawnPoint);
                spawnAbility.m_spawnEffects.Create(spawnPoint, Quaternion.identity);
                if (spawnAbility.m_spawnDelay > 0f)
                {
                    yield return new WaitForSeconds(spawnAbility.m_spawnDelay);
                }
            }

            if (!spawnAbility.m_spawnOnAwake)
            {
                UnityEngine.Object.Destroy(spawnAbility.gameObject);
            }
        }
    }
}
