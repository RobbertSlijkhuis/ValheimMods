using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EffectList;

namespace RideableSeekerBrute
{
    internal class TameableManager
    {
        /**
         * Add the required components to a prefab to make the monster tameable
         */
        public void MakeTameable(GameObject prefab, TameableConfig tameableConfig)
        {
            try
            {
                if (prefab.GetComponent<MonsterAI>() == null && prefab.GetComponent<AnimalAI>() == null)
                {
                    Jotunn.Logger.LogWarning("No AnimalAI component found on " + prefab.name + ", cannot convert to MonsterAI. Skipping " + prefab.name);
                    return;
                }
                else if (prefab.GetComponent<MonsterAI>() == null)
                {
                    Jotunn.Logger.LogWarning("Found AnimalAI component on " + prefab.name + ", converting to MonsterAI...");
                    ConvertAnimalAIToMonsterAI(prefab);
                }

                if (prefab.GetComponent<Humanoid>() == null && prefab.GetComponent<Character>() == null)
                {
                    Jotunn.Logger.LogWarning("No Character component found on " + prefab.name + ", cannot convert to Humanoid. Skipping " + prefab.name);
                    return;
                }
                else if (prefab.GetComponent<Humanoid>() == null)
                { 
                    Jotunn.Logger.LogWarning("Found Character component on " + prefab.name + ", converting to Humanoid...");
                    ConvertCharacterToHumanoid(prefab);
                }

                if (prefab.GetComponent<Tameable>() == null)
                {
                    Jotunn.Logger.LogWarning("No Tameable component found on " + prefab.name + ", creating one...");
                    prefab.AddComponent<Tameable>();
                }

                MonsterAI monsterComp = prefab.GetComponent<MonsterAI>();

                if (tameableConfig.consumeItems.Count > 0)
                    monsterComp.m_consumeItems = tameableConfig.consumeItems;

                monsterComp.m_consumeRange = tameableConfig.consumeRange;
                monsterComp.m_consumeSearchRange = tameableConfig.consumeSearchRange;
                monsterComp.m_consumeSearchInterval = tameableConfig.consumeSearchInterval;

                Tameable tameableComp = prefab.GetComponent<Tameable>();
                tameableComp.m_commandable = tameableConfig.commandable;
                tameableComp.m_fedDuration = tameableConfig.feedDuration;
                tameableComp.m_levelUpFactor = 1;
                tameableComp.m_tamingTime = tameableConfig.tamingTime;

                if (tameableComp.m_tamedEffect.m_effectPrefabs.Length == 0)
                {
                    EffectData tamedEffect = new EffectData();
                    tamedEffect.m_prefab = PrefabManager.Instance.GetPrefab("fx_creature_tamed");
                    tamedEffect.m_enabled = true;
                    tamedEffect.m_variant = -1;
                    List<EffectData> tamedEffectList = new List<EffectData> { tamedEffect };
                    tameableComp.m_tamedEffect.m_effectPrefabs = tamedEffectList.ToArray();
                }

                if (tameableComp.m_sootheEffect.m_effectPrefabs.Length == 0)
                {

                    EffectData soothedEffect = new EffectData();
                    soothedEffect.m_prefab = PrefabManager.Instance.GetPrefab("vfx_creature_soothed");
                    soothedEffect.m_enabled = true;
                    soothedEffect.m_variant = -1;
                    List<EffectData> soothedEffectList = new List<EffectData> { soothedEffect };
                    tameableComp.m_sootheEffect.m_effectPrefabs = soothedEffectList.ToArray();
                }

                if (tameableComp.m_petEffect.m_effectPrefabs.Length == 0)
                {
                    EffectData petEffect = new EffectData();
                    petEffect.m_prefab = PrefabManager.Instance.GetPrefab("fx_lox_pet");
                    petEffect.m_enabled = true;
                    petEffect.m_variant = -1;
                    List<EffectData> petEffectList = new List<EffectData> { petEffect };
                    tameableComp.m_petEffect.m_effectPrefabs = petEffectList.ToArray();
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Error while adding Tameable component, " + error.Message);
            }
        }

        /**
         * Converts the AnimalAI component to a MonsterAI component, add/remove the components
         */
        private void ConvertAnimalAIToMonsterAI(GameObject prefab)
        {
            prefab.AddComponent<MonsterAI>();
            MonsterAI monsterComp = prefab.GetComponent<MonsterAI>();
            AnimalAI animalAI = prefab.GetComponent<AnimalAI>();

            // General
            monsterComp.m_viewRange = animalAI.m_viewRange;
            monsterComp.m_viewAngle = animalAI.m_viewAngle;
            monsterComp.m_hearRange = animalAI.m_hearRange;
            monsterComp.m_mistVision = animalAI.m_mistVision;
            monsterComp.m_alertedEffects = animalAI.m_alertedEffects;
            monsterComp.m_idleSound = animalAI.m_idleSound;
            monsterComp.m_idleSoundInterval = animalAI.m_idleSoundInterval;
            monsterComp.m_idleSoundChance = animalAI.m_idleSoundChance;
            monsterComp.m_pathAgentType = animalAI.m_pathAgentType;
            monsterComp.m_moveMinAngle = animalAI.m_moveMinAngle;
            monsterComp.m_smoothMovement = animalAI.m_smoothMovement;
            monsterComp.m_serpentMovement = animalAI.m_serpentMovement;
            monsterComp.m_serpentTurnRadius = animalAI.m_serpentTurnRadius;
            monsterComp.m_jumpInterval = animalAI.m_jumpInterval;

            // Random
            monsterComp.m_randomCircleInterval = animalAI.m_randomCircleInterval;
            monsterComp.m_randomMoveInterval = animalAI.m_randomMoveInterval;
            monsterComp.m_randomMoveRange = animalAI.m_randomMoveRange;

            // Flying
            monsterComp.m_randomFly = animalAI.m_randomFly;
            monsterComp.m_chanceToTakeoff = animalAI.m_chanceToTakeoff;
            monsterComp.m_chanceToLand = animalAI.m_chanceToLand;
            monsterComp.m_groundDuration = animalAI.m_groundDuration;
            monsterComp.m_airDuration = animalAI.m_airDuration;
            monsterComp.m_maxLandAltitude = animalAI.m_maxLandAltitude;
            monsterComp.m_takeoffTime = animalAI.m_takeoffTime;
            monsterComp.m_flyAltitudeMin = animalAI.m_flyAltitudeMin;
            monsterComp.m_flyAltitudeMax = animalAI.m_flyAltitudeMax;
            monsterComp.m_limitMaxAltitude = animalAI.m_limitMaxAltitude;

            // Other
            monsterComp.m_avoidFire = animalAI.m_avoidFire;
            monsterComp.m_afraidOfFire = animalAI.m_afraidOfFire;
            monsterComp.m_avoidWater = animalAI.m_avoidWater;
            monsterComp.m_aggravatable = animalAI.m_aggravatable;
            monsterComp.m_spawnMessage = animalAI.m_spawnMessage;
            monsterComp.m_deathMessage = animalAI.m_deathMessage;
            monsterComp.m_alertedMessage = animalAI.m_alertedMessage;

            // MonsterAI
            monsterComp.m_attackPlayerObjects = false;
            // This will allow the Harmony ActAsAnimal_Patch to override the AI behaviour
            monsterComp.m_fleeIfLowHealth = -89;

            // Destroy the AnimalAI component because we no longer need that
            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<AnimalAI>());
        }

        /**
         * Converts the Character component to a Humanoid component, add/remove the components
         */
        private void ConvertCharacterToHumanoid(GameObject prefab)
        {
            prefab.AddComponent<Humanoid>();
            Humanoid humanComp = prefab.GetComponent<Humanoid>();
            Character charComp = prefab.GetComponent<Character>();

            // General
            humanComp.m_eye = charComp.m_eye;
            humanComp.m_nViewOverride = charComp.m_nViewOverride;
            humanComp.m_name = charComp.m_name;
            humanComp.m_group = charComp.m_group;
            humanComp.m_faction = charComp.m_faction;
            humanComp.m_boss = charComp.m_boss;
            humanComp.m_dontHideBossHud = charComp.m_dontHideBossHud;
            humanComp.m_bossEvent = charComp.m_bossEvent;
            humanComp.m_defeatSetGlobalKey = charComp.m_defeatSetGlobalKey;

            // Ground
            humanComp.m_crouchSpeed = charComp.m_crouchSpeed;
            humanComp.m_walkSpeed = charComp.m_walkSpeed;
            humanComp.m_speed = charComp.m_speed;
            humanComp.m_turnSpeed = charComp.m_turnSpeed;
            humanComp.m_runSpeed = charComp.m_runSpeed;
            humanComp.m_runTurnSpeed = charComp.m_runTurnSpeed;

            // Flying
            humanComp.m_flying = charComp.m_flying;
            humanComp.m_flySlowSpeed = charComp.m_flySlowSpeed;
            humanComp.m_flyFastSpeed = charComp.m_flyFastSpeed;
            humanComp.m_flyTurnSpeed = charComp.m_flyTurnSpeed;
            humanComp.m_acceleration = charComp.m_acceleration;
            humanComp.m_jumpForce = charComp.m_jumpForce;
            humanComp.m_jumpForceForward = charComp.m_jumpForceForward;
            humanComp.m_jumpForceTiredFactor = charComp.m_jumpForceTiredFactor;
            humanComp.m_airControl = charComp.m_airControl;

            // Swimming
            humanComp.m_canSwim = charComp.m_canSwim;
            humanComp.m_swimDepth = charComp.m_swimDepth;
            humanComp.m_swimSpeed = charComp.m_swimSpeed;
            humanComp.m_swimTurnSpeed = charComp.m_swimTurnSpeed;
            humanComp.m_swimAcceleration = charComp.m_swimAcceleration;

            // Tolerate
            humanComp.m_tolerateWater = charComp.m_tolerateWater;
            humanComp.m_tolerateSmoke = charComp.m_tolerateSmoke;
            humanComp.m_tolerateTar = charComp.m_tolerateTar;

            // Other
            humanComp.m_health = charComp.m_health;
            humanComp.m_damageModifiers = charComp.m_damageModifiers;
            humanComp.m_weakSpots = charComp.m_weakSpots;
            humanComp.m_staggerWhenBlocked = charComp.m_staggerWhenBlocked;
            humanComp.m_staggerDamageFactor = charComp.m_staggerDamageFactor;
            humanComp.m_groundTilt = charComp.m_groundTilt;
            humanComp.m_groundTiltSpeed = charComp.m_groundTiltSpeed;
            humanComp.m_jumpStaminaUsage = charComp.m_jumpStaminaUsage;
            humanComp.m_disableWhileSleeping = charComp.m_disableWhileSleeping;

            // Destroy the Character component because we no longer need that
            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<Character>());
        }
    }

    internal class TameableConfig
    {
        public bool commandable = false;
        public List<ItemDrop> consumeItems = new List<ItemDrop>();
        public float consumeRange = 1;
        public float consumeSearchInterval = 10;
        public float consumeSearchRange = 10;
        public float feedDuration = 600;
        public float tamingTime = 1800;

        public void AddConsumeItem(ItemDrop itemDrop)
        {
            this.consumeItems.Add(itemDrop);
        }
    }
}
