using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class TargetingPatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StaticTarget), nameof(StaticTarget.IsPriorityTarget))]
        public static void IsPriorityTarget_Postfix(StaticTarget __instance, ref bool __result)
        {
            try
            {
                if (__result && IndestructibleHelper.ShouldProtect(__instance.gameObject))
                    __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in IsPriorityTarget_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StaticTarget), nameof(StaticTarget.IsRandomTarget))]
        public static void IsRandomTarget_Postfix(StaticTarget __instance, ref bool __result)
        {
            try
            {
                if (__result && IndestructibleHelper.ShouldProtect(__instance.gameObject))
                    __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in IsRandomTarget_Postfix: " + e);
            }
        }
    }
}
