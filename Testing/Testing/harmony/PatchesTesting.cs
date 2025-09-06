using HarmonyLib;
using System;

namespace Testing.Harmony
{
    [HarmonyPatch]
    public class PatchesTesting
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void ApplyDamage_Postfix(ref Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return;

                Jotunn.Logger.LogWarning("Do something");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in ApplyDamage_Postfix: " + e);
            }
        }
    }
}
