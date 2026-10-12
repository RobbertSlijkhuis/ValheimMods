using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    /// <summary>
    /// Patch methods only - the logic lives in <see cref="SafeZoneProjectileHelper"/>.
    /// </summary>
    [HarmonyPatch]
    public class SafeZonePatchesWBTI
    {
        // Same hook point vanilla uses for ShieldGenerator.CheckProjectile.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Projectile), "FixedUpdate")]
        public static void ProjectileFixedUpdate_Postfix(Projectile __instance)
        {
            try
            {
                SafeZoneProjectileHelper.TryBlock(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("ProjectileFixedUpdate_Postfix failed: " + e);
            }
        }

        // Idle wandering: RandomMovement picks another point when this is false.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseAI), "IsValidRandomMovePoint", new Type[] { typeof(Vector3) })]
        public static void IsValidRandomMovePoint_Postfix(BaseAI __instance, Vector3 point, ref bool __result)
        {
            try
            {
                if (__result && SafeZoneAIHelper.ShouldAvoidPoint(__instance, point, SafeZoneAIHelper.WanderMargin))
                    __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SafeZone IsValidRandomMovePoint_Postfix: " + e);
            }
        }

        // A point inside a zone counts as unreachable: Flee picks another direction, and MonsterAI's
        // building-target and item-pickup checks skip it (see SafeZoneAIHelper). Only points actually
        // inside a zone are rejected (no edge margin).
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseAI), "HavePath", new Type[] { typeof(Vector3) })]
        public static void HavePath_Postfix(BaseAI __instance, Vector3 target, ref bool __result)
        {
            try
            {
                if (__result && SafeZoneAIHelper.ShouldAvoidPoint(__instance, target, 0f))
                    __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SafeZone HavePath_Postfix: " + e);
            }
        }

        // Catches attacks that never fly as a Projectile (long-range hitscan swings, Aoe going off
        // just outside the zone). Returning false skips the vanilla Damage call.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static bool CharacterDamage_Prefix(Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return true;

                return !SafeZoneDamageHelper.ShouldBlockHit(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SafeZone CharacterDamage_Prefix: " + e);
                return true;
            }
        }
    }
}
