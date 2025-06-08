using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class PatchesMMF
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        public static void SetPicked_Postfix(ref Pickable __instance, ref bool picked)
        {
            try
            {
                if (__instance == null || !picked)
                    return;

                Transform footTrans = __instance.transform.Find("foot/particles");

                if (footTrans == null)
                    return;

                footTrans.gameObject.GetComponent<ParticleSystem>().Stop();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not stop particles in SetPicked_Postfix: "+ e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "ShouldRespawn")]
        public static void ShouldRespawn_Postfix(ref Pickable __instance, ref bool __result)
        {
            try
            {
                if (__instance == null)
                    return;

                Transform footTrans = __instance.transform.Find("foot/particles");

                if (footTrans == null)
                    return;

                if (__result)
                {
                    footTrans.gameObject.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    footTrans.gameObject.GetComponent<ParticleSystem>().Stop();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not start/stop particles in ShouldRespawn_Postfix: " + e);
            }
        }
    }
}
