using GrapplingHarpoonHook.Helpers;
using HarmonyLib;
using System;

namespace GrapplingHarpoonHook.Harmony
{
    [HarmonyPatch]
    internal class ProjectilePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Projectile), "Setup")]
        public static void Setup_Postfix(Projectile __instance, ItemDrop.ItemData item)
        {
            try
            {
                AttackHelper.RestoreHarpoonEffect(__instance, item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Setup_Postfix: " + e);
            }
        }
    }
}
