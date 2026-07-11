using System;
using System.Collections;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.extensions;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using static SpawnAbility;
using static Trap;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class SpawnAbilityExtension
    {
        public static IEnumerator Spawn2(this SpawnAbility spawnAbility, CustomRewardEvent customRewardEvent, SpawnAbilityData spawnAbilityData, CreatureData creatureData)
        {
            if (spawnAbility.m_initialSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(spawnAbility.m_initialSpawnDelay);
            }

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            int toSpawn = UnityEngine.Random.Range(spawnAbility.m_minToSpawn, spawnAbility.m_maxToSpawn);
            Skills skills = (spawnAbility.m_owner ? spawnAbility.m_owner.GetSkills() : null);

            // SpawnSystem.GetNrOfInstances falls back to GameObject.FindGameObjectsWithTag("spawned")
            // for non-BaseAI prefabs (e.g. traps) - a full scene-wide scan. Track counts locally per
            // prefab instead of re-scanning the scene on every single spawn iteration.
            Dictionary<GameObject, int> instanceCounts = new Dictionary<GameObject, int>();
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
                        bool indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

                        if (!TransformHelper.TryGetGroundHeight(spawnPoint, vector.y, indoor, out float height, spawnAbility.m_getSolidHeightMargin))
                        {
                            continue;
                        }

                        spawnPoint.y = height;
                    }

                    spawnPoint.y += spawnAbility.m_spawnGroundOffset;
                    if (Mathf.Abs(spawnPoint.y - vector.y) > 100f)
                    {
                        continue;
                    }
                }

                if (TwitchSafeZone.IsPointInSafeZone(spawnPoint))
                {
                    //Jotunn.Logger.LogWarning("Spawnpoint in safezone, skipping...");
                    continue;
                }

                GameObject prefab;

                if (spawnAbilityData.isBiomeList)
                {
                    Heightmap.Biome biome = Player.m_localPlayer.GetCurrentBiome();

                    try
                    {
                        switch (biome)
                        {
                            case Heightmap.Biome.Meadows:
                                prefab = spawnAbility.m_spawnPrefab[0];
                                break;
                            case Heightmap.Biome.BlackForest:
                                prefab = spawnAbility.m_spawnPrefab[1];
                                break;
                            case Heightmap.Biome.Swamp:
                                prefab = spawnAbility.m_spawnPrefab[2];
                                break;
                            case Heightmap.Biome.Mountain:
                            case Heightmap.Biome.DeepNorth:
                                prefab = spawnAbility.m_spawnPrefab[3];
                                break;
                            case Heightmap.Biome.Plains:
                                prefab = spawnAbility.m_spawnPrefab[4];
                                break;
                            case Heightmap.Biome.Mistlands:
                                prefab = spawnAbility.m_spawnPrefab[5];
                                break;
                            case Heightmap.Biome.AshLands:
                                prefab = spawnAbility.m_spawnPrefab[6];
                                break;
                            case Heightmap.Biome.Ocean:
                                prefab = spawnAbility.m_spawnPrefab[7];
                                break;
                            default:
                                prefab = spawnAbility.m_spawnPrefab[0];
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Jotunn.Logger.LogError("Could not pick biome spefific prefab: " + e);
                        prefab = spawnAbility.m_spawnPrefab[0];
                    }
                }
                else
                    prefab = spawnAbility.m_spawnPrefab[UnityEngine.Random.Range(0, spawnAbility.m_spawnPrefab.Length)];

                if (spawnAbility.m_maxSpawned > 0)
                {
                    if (!instanceCounts.TryGetValue(prefab, out int existingCount))
                    {
                        existingCount = SpawnSystem.GetNrOfInstances(prefab);
                        instanceCounts[prefab] = existingCount;
                    }

                    if (existingCount >= spawnAbility.m_maxSpawned)
                    {
                        if (spawnAbility.m_owner is Player player)
                        {
                            player.Message(MessageHud.MessageType.Center, spawnAbility.m_maxSummonReached);
                        }

                        continue;
                    }
                }

                if (customRewards.m_playerIsInSafeZone)
                    yield break;

                spawnAbility.m_preSpawnEffects.Create(spawnPoint, Quaternion.identity);
                if (spawnAbility.m_preSpawnDelay > 0f)
                    yield return new WaitForSeconds(spawnAbility.m_preSpawnDelay);

                Quaternion spawnRotation = spawnAbilityData.randomRotation
                    ? Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)
                    : Quaternion.identity;

                GameObject gameObject = ZNetViewHelper.Instantiate(prefab, spawnPoint, spawnRotation);

                if (spawnAbility.m_maxSpawned > 0)
                    instanceCounts[prefab] = instanceCounts[prefab] + 1;

                // Remove any safe zones that may be on the spawned prefab (e.g. boat rain)
                foreach (TwitchSafeZone safeZone in gameObject.GetComponentsInChildren<TwitchSafeZone>(true))
                    UnityEngine.Object.Destroy(safeZone);

                ZNetView component = gameObject.GetComponent<ZNetView>();
                Projectile component2 = gameObject.GetComponent<Projectile>();

                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();
                Humanoid humanoid2 = gameObject.GetComponent<Humanoid>();
                ImpactEffect impactEffect = gameObject.GetComponentInChildren<ImpactEffect>(true);
                Aoe aoe = gameObject.GetComponentInChildren<Aoe>(true);
                Aoe aoeRod = null;
                Trap trap = gameObject.GetComponentInChildren<Trap>();
                Piece piece = gameObject.GetComponentInChildren<Piece>();

                if (gameObject.name == "lightningAOE(Clone)")
                {
                    Aoe[] aoes = gameObject.GetComponentsInChildren<Aoe>();
                    aoeRod = aoes[0];
                    aoe = aoes[1];
                }

                if (monsterAI != null && humanoid2 != null)
                {
                    TwitchCreaturePersistentData creaturePersistentData = gameObject.GetComponent<TwitchCreaturePersistentData>() ?? gameObject.AddComponent<TwitchCreaturePersistentData>();
                    creaturePersistentData.SetData(customRewardEvent.RedeemerName, creatureData, customRewardEvent.CustomRewardTitle, false);

                    TwitchCreatureClaim creatureClaim = gameObject.GetComponent<TwitchCreatureClaim>() ?? gameObject.AddComponent<TwitchCreatureClaim>();
                    creatureClaim.ReInit(customRewardEvent.RedeemerName, creatureData);
                }

                if (monsterAI == null && impactEffect != null)
                {
                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

                    if (spawnAbilityData.dropVelocity != 0f)
                    {
                        if (rigidbody != null)
                            rigidbody.AddForce(new Vector3(0f, (float)spawnAbilityData.dropVelocity, 0f) * rigidbody.mass * rigidbody.mass);
                        else
                            Jotunn.Logger.LogWarning("Rigidbody missing, cannot apply dropVelocity.");
                    }

                    if (spawnAbilityData.duration > 0)
                    {
                        TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>() ?? gameObject.AddComponent<TwitchPersistentDestruction>();
                        persistentDestruction.SetStarted(spawnAbilityData.duration, spawnAbilityData.noSpawnEffect ? null : WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium, spawnAbilityData.breakOnDestroy);
                    }

                    impactEffect.m_damagePlayers = true;

                    DamageData resetDamage = new DamageData();
                    resetDamage.blunt = 50f;
                    resetDamage.chop = 30f;
                    impactEffect.StartCoroutine(impactEffect.ResetShowerSettings(resetDamage));
                }

                if (trap != null)
                {
                    trap.RequestStateChange(TrapState.Unarmed);

                    if (spawnAbilityData.duration > 0)
                    {
                        TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>() ?? gameObject.AddComponent<TwitchPersistentDestruction>();
                        persistentDestruction.SetStarted(spawnAbilityData.duration, spawnAbilityData.noSpawnEffect ? null : WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall);
                    }

                    trap.StartCoroutine(trap.ArmTrapAfterDelay(1f));
                }

                if (monsterAI == null && aoe != null)
                {
                    if (aoeRod != null)
                    {
                        TimedDestruction timedDestruction = aoeRod.gameObject.AddComponent<TimedDestruction>();
                        timedDestruction.m_timeout = 0.5f;
                        timedDestruction.Trigger();

                        aoeRod.m_useTriggers = true;
                    }
                }

                if (piece != null)
                {
                    gameObject.AddComponent<TwitchPiecePersistentData>().SetData(customRewardEvent, spawnAbilityData);

                    if (spawnAbilityData.duration > 0)
                    {
                        TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>() ?? gameObject.AddComponent<TwitchPersistentDestruction>();
                        persistentDestruction.SetStarted(spawnAbilityData.duration, spawnAbilityData.noSpawnEffect ? null : WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall, spawnAbilityData.breakOnDestroy);
                    }

                    Door door = gameObject.GetComponent<Door>();
                    if (door != null && spawnAbilityData.doorInterval > 0)
                        gameObject.AddComponent<TwitchDoorPersistentData>().Initialize(spawnAbilityData.doorInterval);

                    // TwitchWindmillPersistentData is baked onto the "Windmill_WBTI" prefab itself
                    // (see WizshBoneTwitchIntegration.SetupPieces) rather than added here, since the
                    // vanilla Windmill component it used to key off of no longer exists on that clone.
                    TwitchWindmillPersistentData windmillData = gameObject.GetComponent<TwitchWindmillPersistentData>();
                    if (windmillData != null && spawnAbilityData.windmillRotationSpeed > 0f)
                        windmillData.Initialize(spawnAbilityData.windmillRotationSpeed);
                }

                if ((bool)component2)
                {
                    spawnAbility.SetupProjectile(component2, targetPosition);
                }

                if (spawnAbilityData.damage != null)
                    gameObject.AddComponent<TwitchPersistentDamage>().SetData(customRewardEvent, spawnAbilityData.damage);

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

                if (!spawnAbilityData.noSpawnEffect)
                    spawnAbility.m_spawnEffects.Create(spawnPoint, Quaternion.identity);

                if (spawnAbility.m_spawnDelay > 0f && (i + 1) % spawnAbilityData.batchSize == 0)
                {
                    yield return new WaitForSeconds(spawnAbility.m_spawnDelay);
                }
            }

            if (!spawnAbility.m_spawnOnAwake)
            {
                ZNetViewHelper.Destroy(spawnAbility.gameObject);
            }
        }
    }
}
