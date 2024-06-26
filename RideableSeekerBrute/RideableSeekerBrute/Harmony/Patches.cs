﻿using HarmonyLib;
using UnityEngine;

namespace RideableSeekerBrute
{
    // START TEST SECTION
    [HarmonyPatch(typeof(Player), nameof(Player.AttachStop))]
    class Dismount_Patch
    {
        static bool Prefix(ref Player __instance)
        {
            if (__instance != null)
            {
                Jotunn.Logger.LogWarning("AttachStop has been called!");
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
    class TestSetControls_Patch
    {
        static bool Prefix(ref Player __instance, Vector3 movedir, bool attack, bool attackHold, bool secondaryAttack, bool secondaryAttackHold, bool block, bool blockHold, bool jump, bool crouch, bool run, bool autoRun, bool dodge = false)
        {
            if (__instance != null)
            {
                if ((__instance.IsAttached() || __instance.InEmote()) && (movedir != Vector3.zero || attack || secondaryAttack || block || blockHold || jump || crouch) && __instance.GetDoodadController() == null)
                {
                    Jotunn.Logger.LogWarning("IsAttached() || InEmote() check");
                }
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Sadle), nameof(Sadle.OnUseStop))]
    class TestOnUseStop_Patch
    {
        static bool Prefix(ref Sadle __instance)
        {
            if (__instance != null)
            {
                if (__instance.m_nview.IsValid() && __instance.m_attachPoint != null)
                {
                    Jotunn.Logger.LogWarning("OnUseStop check");
                }
            }

            return true;
        }
    }    
    
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateDoodadControls))]
    class TestUpdateDoodadControls_Patch
    {
        static bool Prefix(ref Player __instance)
        {
            if (__instance != null)
            {
                if (__instance.m_doodadController == null)
                {
                    return true;
                }

                if (Vector3.Distance(__instance.m_doodadController.GetPosition(), __instance.transform.position) > __instance.m_maxInteractDistance)
                {
                    Jotunn.Logger.LogWarning("Distance is greater than m_maxInteractDistance in UpdateDoodadControls");
                    Jotunn.Logger.LogWarning(Vector3.Distance(__instance.m_doodadController.GetPosition(), __instance.transform.position));
                    Jotunn.Logger.LogWarning(__instance.m_maxInteractDistance);
                    Jotunn.Logger.LogWarning(Vector3.Distance(__instance.m_doodadController.GetPosition(), __instance.transform.position) > __instance.m_maxInteractDistance);
                    return true;
                }
            }

            return true;
        }
    }
    // END TEST SECTION

    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.SetFollowTarget))]
    class WakeUpSleepy_patch
    {
        static bool Prefix(ref MonsterAI __instance)
        {
            // Certain creatures start as sleeping, like the Stone Golem
            // or the Abomination. When you log in they are in their
            // sleeping (underground) position and cannot be ridden
            // or cannot follow. In order to wake it up you either need
            // to do damage to them or have an enemy nearby. This patch
            // will prevent them from ever sleeping

            if (__instance != null) 
            {
                if (__instance.IsSleeping() && __instance.m_character.m_tamed)
                    __instance.Wakeup();
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.SetSaddle))]
    class SetSaddle_Patch
    {
        static bool Prefix(ref Tameable __instance, bool enabled)
        {
            // The Tameable component disables its parent GameObject when there
            // is no saddle on the creature. Since our saddle is mounted on a
            // different GameObject we need to disable that one aswell. A custom
            // component "SaddleAnchor" holds the Transform data of that GameObject

            if (__instance != null && __instance.m_saddle != null)
            {
                SaddleAnchor saddleAnchorComp = __instance.GetComponent<SaddleAnchor>();

                if (saddleAnchorComp?.m_saddle_anchor != null)
                {
                    saddleAnchorComp.m_saddle_anchor.gameObject.SetActive(enabled);
                }
            }

            return true;
        }
    }

    //[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
    //class ActAsAnimal_Patch
    //{
    //    float m_timeToSafe = 10;

    //    static bool Prefix(ref MonsterAI __instance, float dt)
    //    {
    //        // Creatures require MonsterAI to be tamable. For animals like the
    //        // Hare, Deer etc. we want to keep their Animal behaviour. To do achieve
    //        // that we override the UpdateAI method with the AnimalAI behaviour,
    //        // and add the things from MonterAI that allows us to tame, set follow etc.
    //        // We can recognize in the MonsterAI if it was converted from an AnimalAI
    //        // with the m_fleeIfLowHealth field when that is set to -89.

    //        MonsterAI instance = __instance;

    //        if (instance.m_fleeIfLowHealth != -89)
    //            return true;

    //        if ((bool)instance.m_tamable && (bool)instance.m_tamable.m_saddle && instance.m_tamable.m_saddle.UpdateRiding(dt))
    //        {
    //            return false;
    //        }

    //        if (!instance.m_nview.IsOwner() || (instance.m_afraidOfFire && instance.AvoidFire(dt, null, superAfraid: true)))
    //            return false;

    //        float m_timeToSafe = 10;
    //        Humanoid humanoid = instance.m_character as Humanoid;
    //        instance.m_updateTargetTimer -= dt;

    //        if (instance.m_updateTargetTimer <= 0f)
    //        {
    //            instance.m_updateTargetTimer = (Character.IsCharacterInRange(instance.transform.position, 32f) ? 2f : 10f);
    //            Character character = instance.FindEnemy();
    //            if ((bool)character)
    //            {
    //                instance.m_targetCreature = character;
    //            }
    //        }

    //        if ((bool)instance.m_targetCreature && instance.m_targetCreature.IsDead())
    //        {
    //            instance.m_targetCreature = null;
    //        }

    //        if ((bool)instance.m_targetCreature)
    //        {
    //            bool num = instance.CanSenseTarget(instance.m_targetCreature);
    //            instance.SetTargetInfo(instance.m_targetCreature.GetZDOID());
    //            if (num)
    //            {
    //                instance.SetAlerted(alert: true);
    //            }
    //        }
    //        else
    //        {
    //            instance.SetTargetInfo(ZDOID.None);
    //        }

    //        if ((bool)instance.m_targetCreature && instance.m_character.IsTamed())
    //        {
    //            if (instance.GetPatrolPoint(out var point))
    //            {
    //                if (Vector3.Distance(instance.m_targetCreature.transform.position, point) > instance.m_alertRange)
    //                {
    //                    instance.m_targetCreature = null;
    //                }
    //            }
    //            else if ((bool)instance.m_follow && Vector3.Distance(instance.m_targetCreature.transform.position, instance.m_follow.transform.position) > instance.m_alertRange)
    //            {
    //                instance.m_targetCreature = null;
    //            }
    //        }

    //        if (instance.IsAlerted())
    //        {
    //            instance.m_timeSinceSensedTargetCreature += dt;
    //            if (instance.m_timeSinceSensedTargetCreature > m_timeToSafe)
    //            {
    //                instance.m_targetCreature = null;
    //                instance.SetAlerted(alert: false);
    //            }
    //        }

    //        if ((!instance.IsAlerted() || (instance.m_targetStatic == null && instance.m_targetCreature == null)) && instance.UpdateConsumeItem(humanoid, dt))
    //        {
    //            if (instance.m_aiStatus != null)
    //            {
    //                instance.m_aiStatus = "Consume item";
    //            }

    //            return false;
    //        }

    //        if (instance.m_targetStatic == null && instance.m_targetCreature == null)
    //        {
    //            if ((bool)instance.m_follow)
    //            {
    //                instance.Follow(instance.m_follow, dt);
    //                if (instance.m_aiStatus != null)
    //                {
    //                    instance.m_aiStatus = "Follow";
    //                }

    //                return false;
    //            }
    //        }

    //        if ((bool)instance.m_targetCreature)
    //        {
    //            instance.Flee(dt, instance.m_targetCreature.transform.position);
    //            instance.m_targetCreature.OnTargeted(sensed: false, alerted: false);
    //        }
    //        else
    //        {
    //            instance.IdleMovement(dt);
    //        }

    //        return false;
    //    }
    //}
}
