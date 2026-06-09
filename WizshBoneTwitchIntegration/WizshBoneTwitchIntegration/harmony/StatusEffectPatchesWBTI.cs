using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class StatusEffectPatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
        public static void TryRunCommand_Postfix(string text)
        {
            try
            {
                if (!text.Trim().StartsWith("clearstatus", System.StringComparison.OrdinalIgnoreCase))
                    return;

                if (Game.instance == null)
                    return;

                StatusEffectManager manager = Game.instance.gameObject.GetComponent<StatusEffectManager>();
                manager?.ClearAll();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in TryRunCommand_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(Player __instance)
        {
            try
            {
                if (Player.m_localPlayer == null || Player.m_localPlayer.GetPlayerID() != __instance.GetPlayerID())
                    return;

                StatusEffectManager manager = Game.instance.gameObject.GetComponent<StatusEffectManager>();

                if (manager == null)
                    return;

                manager?.ReApplyPending(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnSpawned_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatusEffect), "Stop")]
        public static void StatusEffectStop_Postfix(StatusEffect __instance)
        {
            try
            {
                if (Game.instance == null)
                    return;

                StatusEffectManager manager = Game.instance.gameObject.GetComponent<StatusEffectManager>();

                if (manager == null || !manager.IsTracked(__instance.NameHash()))
                    return;

                if (__instance.IsDone())
                {
                    // Effect expired naturally — remove from tracking
                    manager.UntrackStatusEffect(__instance.NameHash());
                    Jotunn.Logger.LogInfo($"StatusEffect '{__instance.name}' expired, removed from tracking.");
                }
                else
                {
                    // Stopped prematurely (player died) — snapshot remaining time for respawn
                    manager.SnapshotForRespawn(__instance.NameHash(), __instance.name, __instance.GetRemaningTime());
                    Jotunn.Logger.LogInfo($"StatusEffect '{__instance.name}' snapshotted with {__instance.GetRemaningTime():F1}s remaining.");
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in StatusEffectStop_Postfix: " + e);
            }
        }
    }
}
