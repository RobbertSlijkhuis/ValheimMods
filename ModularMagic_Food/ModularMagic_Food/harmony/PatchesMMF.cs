using HarmonyLib;
using ModularMagic_Food.Helpers;
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
        // A fresh load reads the picked state from the ZDO without going through SetPicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "Awake")]
        public static void Awake_Postfix(Pickable __instance)
        {
            PickableHelper.SyncFootParticles(__instance, instant: true);
        }

        // Runs on every client for a pick and for a respawn
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        public static void SetPicked_Postfix(Pickable __instance)
        {
            PickableHelper.SyncFootParticles(__instance, instant: false);
        }
    }
}
