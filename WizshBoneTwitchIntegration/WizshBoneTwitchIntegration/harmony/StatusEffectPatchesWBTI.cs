using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class StatusEffectPatchesWBTI
    {
        

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                Jotunn.Logger.LogInfo("Player spawned, applying status effects...");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnSpawned_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "OnDeath")]
        public static void OnDeath_Postfix(StatusEffect __instance)
        {
            try
            {
                Jotunn.Logger.LogInfo("Player died, removing status effects...");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnDeath_Prefix: " + e);
            }
        }
    }
}
