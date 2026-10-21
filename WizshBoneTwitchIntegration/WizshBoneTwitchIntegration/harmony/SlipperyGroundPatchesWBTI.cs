using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class SlipperyGroundPatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "UpdateBodyFriction")]
        public static void UpdateBodyFriction_Postfix(Character __instance)
        {
            try
            {
                SlipperyGroundHelper.ApplyBodyFriction(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SlipperyGround UpdateBodyFriction_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "UpdateWalking")]
        public static void UpdateWalking_Prefix(Character __instance)
        {
            try
            {
                SlipperyGroundHelper.Swap(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SlipperyGround UpdateWalking_Prefix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "UpdateWalking")]
        public static void UpdateWalking_Postfix(Character __instance)
        {
            try
            {
                SlipperyGroundHelper.Restore(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SlipperyGround UpdateWalking_Postfix: " + e);
            }
        }
    }
}
