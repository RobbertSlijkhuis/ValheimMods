using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class TrapPatchesWBTI
    {
        // Trap.OnTriggerEnter skips the Player/MonsterAI early-out for any other collider,
        // so a door sweeping over a trap would trigger it. Bail out when the entering
        // collider belongs to a WBTI-spawned door.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trap), "OnTriggerEnter")]
        public static bool OnTriggerEnter_Prefix(Collider collider)
        {
            try
            {
                if (collider == null)
                    return true;

                if (collider.GetComponentInParent<TwitchDoorPersistentData>() != null)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] TrapPatchesWBTI.OnTriggerEnter_Prefix: {e}");
                return true;
            }
        }
    }
}
