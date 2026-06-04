using Glimuleikar.Configs;
using HarmonyLib;
using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class HarpoonPatchesWBTI
    {
        private static bool pullNeeded = false;
        private static float pullElapsed = 0f;
        private static Vector3 pullStartPosition;
        private static Vector3 pullTargetPosition;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SE_Harpooned), "Setup")]
        public static void SE_Harpooned_Setup_Postfix(SE_Harpooned __instance)
        {
            Jotunn.Logger.LogWarning("SE_Harpooned_Setup_Postfix fired.");
            Jotunn.Logger.LogWarning($"m_character: {__instance.m_character}, m_attacker: {__instance.m_attacker}");
            pullNeeded = false;
            pullElapsed = 0f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SE_Harpooned), "UpdateStatusEffect")]
        public static void SE_Harpooned_UpdateStatusEffect_Postfix(SE_Harpooned __instance, float dt)
        {
            try
            {
                Character target = __instance.m_character;
                Character attacker = __instance.m_attacker;

                if (target == null || attacker == null)
                    return;

                if (!target.m_nview.IsOwner())
                    return;

                if (!pullNeeded)
                {
                    float maxDistance = PluginConfig.configHarpoonMaxRopeLength.Value;
                    Vector3 attackerPos = attacker.transform.position;
                    Vector3 targetPos = target.transform.position;
                    float currentDistance = Vector3.Distance(attackerPos, targetPos);

                    if (currentDistance <= maxDistance)
                        return;

                    pullStartPosition = targetPos;
                    pullTargetPosition = attackerPos + (targetPos - attackerPos).normalized * maxDistance;
                    pullNeeded = true;
                    pullElapsed = 0f;

                    Jotunn.Logger.LogWarning($"Starting pull from {currentDistance} to {maxDistance} over {PluginConfig.configHarpoonPullDuration.Value}s.");
                }

                if (pullNeeded)
                {
                    pullElapsed += dt;
                    float t = Mathf.Clamp01(pullElapsed / PluginConfig.configHarpoonPullDuration.Value);
                    Vector3 newPosition = Vector3.Lerp(pullStartPosition, pullTargetPosition, t);

                    target.m_body.MovePosition(newPosition);

                    if (t >= 1f)
                    {
                        __instance.m_baseDistance = PluginConfig.configHarpoonMaxRopeLength.Value;
                        pullNeeded = false;
                        Jotunn.Logger.LogWarning("Pull complete.");
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SE_Harpooned_UpdateStatusEffect_Postfix: " + e);
            }
        }
    }
}