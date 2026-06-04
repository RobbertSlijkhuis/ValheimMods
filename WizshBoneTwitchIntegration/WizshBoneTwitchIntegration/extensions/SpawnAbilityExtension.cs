using System;
using System.Collections;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
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

            Collider[] objects = Array.Empty<Collider>();
            List<Collider> safezoneColliders = new List<Collider>();

            if (!spawnAbilityData.rescanForSafezones)
            {
                objects = Physics.OverlapSphere(spawnAbility.transform.position, spawnAbility.m_spawnRadius, LayerMask.GetMask("character_trigger"));
                foreach (Collider collider in objects)
                {
                    if (collider.gameObject.GetComponentInChildren<TwitchSafeZone>())
                    {
                        safezoneColliders.Add(collider);
                    }
                }
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

                if (spawnAbilityData.rescanForSafezones)
                {
                    objects = Physics.OverlapSphere(spawnAbility.transform.position, spawnAbility.m_spawnRadius, LayerMask.GetMask("character_trigger"));
                    safezoneColliders = new List<Collider>();

                    foreach (Collider collider in objects)
                    {
                        if (collider.gameObject.GetComponentInChildren<TwitchSafeZone>())
                        {
                            safezoneColliders.Add(collider);
                        }
                    }
                }

                if (safezoneColliders.Count > 0)
                {
                    bool skip = false;
                    Vector3 closest;

                    foreach (var collider in safezoneColliders)
                    {
                        closest = collider.ClosestPoint(spawnPoint);

                        if (closest == spawnPoint)
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                    {
                        //Jotunn.Logger.LogWarning("Spawnpoint in safezone, skipping...");
                        continue;
                    }
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

                if (spawnAbility.m_maxSpawned > 0 && SpawnSystem.GetNrOfInstances(prefab) >= spawnAbility.m_maxSpawned)
                {
                    if (spawnAbility.m_owner is Player player)
                    {
                        player.Message(MessageHud.MessageType.Center, spawnAbility.m_maxSummonReached);
                    }

                    continue;
                }

                if (customRewards.m_playerIsInSafeZone)
                    yield break;

                spawnAbility.m_preSpawnEffects.Create(spawnPoint, Quaternion.identity);
                if (spawnAbility.m_preSpawnDelay > 0f)
                    yield return new WaitForSeconds(spawnAbility.m_preSpawnDelay);

                GameObject gameObject = UnityEngine.Object.Instantiate(prefab, spawnPoint, Quaternion.Euler(0f, UnityEngine.Random.value * (float)Math.PI * 2f, 0f));

                // Remove any safe zones that may be on the spawned prefab (e.g. boat rain)
                foreach (TwitchSafeZone safeZone in gameObject.GetComponentsInChildren<TwitchSafeZone>(true))
                    UnityEngine.Object.Destroy(safeZone);

                ZNetView component = gameObject.GetComponent<ZNetView>();
                Projectile component2 = gameObject.GetComponent<Projectile>();

                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();
                Humanoid humanoid2 = gameObject.GetComponent<Humanoid>();
                ImpactEffect impactEffect = gameObject.GetComponent<ImpactEffect>();
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
                    TwitchCreaturePersistentData creaturePersistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

                    if (creaturePersistentData != null)
                        creaturePersistentData.SetData(customRewardEvent.RedeemerName, creatureData, customRewardEvent.CustomRewardTitle, false);
                    else
                        Jotunn.Logger.LogWarning("Creature does not have persistent data somehow!");
                }

                if (monsterAI == null && impactEffect != null)
                {
                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

                    if (spawnAbilityData.damage != null)
                    {
                        if (spawnAbilityData.damage.basedOnMaxHealthAndArmor)
                            impactEffect.m_damages = DamageHelper.CalculateDamageBasedOnMaxHealthAndArmor(spawnAbilityData.damage);
                        else
                            impactEffect.m_damages = DamageHelper.ConvertToDamageTypes(spawnAbilityData.damage);
                    }

                    if (spawnAbilityData.dropVelocity != 0f)
                    {
                        if (rigidbody != null)
                            rigidbody.AddForce(new Vector3(0f, (float)spawnAbilityData.dropVelocity, 0f) * rigidbody.mass * rigidbody.mass);
                        else
                            Jotunn.Logger.LogWarning("Rigidbody missing, cannot apply dropVelocity.");
                    }

                    TwitchAllowDamage preventDamage = impactEffect.gameObject.AddComponent<TwitchAllowDamage>();
                    preventDamage.Init(spawnAbilityData.damageShips, spawnAbilityData.damageStructures);

                    if (spawnAbilityData.duration > 0)
                    {
                        TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>();
                        if (persistentDestruction != null)
                            persistentDestruction.SetStarted(spawnAbilityData.duration, spawnAbilityData.noSpawnEffect ? null : WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium);
                        else
                            Jotunn.Logger.LogWarning("TwitchPersistentDestruction missing on impactEffect object.");
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

                    if (piece != null)
                        piece.m_resources = new Piece.Requirement[0];

                    if (spawnAbilityData.duration > 0)
                    {
                        TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>();
                        if (persistentDestruction != null)
                            persistentDestruction.SetStarted(spawnAbilityData.duration, spawnAbilityData.noSpawnEffect ? null : WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall);
                        else
                            Jotunn.Logger.LogWarning("TwitchPersistentDestruction missing on trap object.");
                    }

                    trap.StartCoroutine(trap.ArmTrapAfterDelay(1f));
                }

                if (monsterAI == null && aoe != null)
                {
                    if (spawnAbilityData.damage != null)
                    {
                        HitData.DamageTypes damages = spawnAbilityData.damage.basedOnMaxHealthAndArmor
                            ? DamageHelper.CalculateDamageBasedOnMaxHealthAndArmor(spawnAbilityData.damage)
                            : DamageHelper.ConvertToDamageTypes(spawnAbilityData.damage);
                        aoe.m_damage = damages;

                        TwitchPersistentDamage persistentDamage = gameObject.GetComponent<TwitchPersistentDamage>();

                        if (persistentDamage != null)
                            persistentDamage.SetData(damages);
                    }

                    TwitchAllowDamage preventDamage = aoe.gameObject.AddComponent<TwitchAllowDamage>();
                    preventDamage.Init(spawnAbilityData.damageShips, spawnAbilityData.damageStructures);

                    if (aoeRod != null)
                    {
                        TimedDestruction timedDestruction = aoeRod.gameObject.AddComponent<TimedDestruction>();
                        timedDestruction.m_timeout = 0.5f;
                        timedDestruction.Trigger();

                        TwitchAllowDamage preventDamageRod = aoeRod.gameObject.AddComponent<TwitchAllowDamage>();
                        preventDamageRod.Init(spawnAbilityData.damageShips, spawnAbilityData.damageStructures);
                        aoeRod.m_useTriggers = true;
                    }
                }

                if (piece != null)
                {
                    TwitchPiecePersistentData piecePersistentData = gameObject.GetComponent<TwitchPiecePersistentData>();
                    if (piecePersistentData != null)
                        piecePersistentData.SetData(customRewardEvent, spawnAbilityData);
                    else
                        Jotunn.Logger.LogWarning("TwitchPiecePersistentData missing on piece object.");
                }

                if ((bool)component2)
                {
                    if (spawnAbilityData.damage != null)
                    {
                        if (spawnAbilityData.damage.basedOnMaxHealthAndArmor)
                            component2.m_damage = DamageHelper.CalculateDamageBasedOnMaxHealthAndArmor(spawnAbilityData.damage);
                        else
                            component2.m_damage = DamageHelper.ConvertToDamageTypes(spawnAbilityData.damage);
                    }

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

                if (!spawnAbilityData.noSpawnEffect)
                    spawnAbility.m_spawnEffects.Create(spawnPoint, Quaternion.identity);

                if (spawnAbility.m_spawnDelay > 0f)
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
