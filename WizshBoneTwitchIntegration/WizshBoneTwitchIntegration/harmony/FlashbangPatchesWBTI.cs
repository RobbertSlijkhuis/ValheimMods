using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class FlashbangPatchesWBTI
    {
        // Player.OnDeath() already early-returns unless m_nview.IsOwner(), so it only ever runs for
        // the local player's own character - but check explicitly for consistency with the other
        // OnSpawned patch below and every other local-player guard in this codebase.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnDeath")]
        public static void OnDeath_Postfix(Player __instance)
        {
            try
            {
                if (Player.m_localPlayer == null || Player.m_localPlayer.GetPlayerID() != __instance.GetPlayerID())
                    return;

                FlashBangHelper.ClearUI();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnDeath_Postfix: " + e);
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

                FlashBangHelper.ClearUI();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnSpawned_Postfix: " + e);
            }
        }
    }
}
