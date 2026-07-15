using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class FlashbangPatchesWBTI
    {
        // Deliberately does NOT clear on death (unlike OnSpawned below) - dying mid-flashbang
        // leaves the overlay up, so a blinded player doesn't immediately know they died until
        // either the effect naturally fades or they respawn. See FlashBangHelper.AttachFlashBang's
        // `flash == null` guards for why an early ClearUI() elsewhere (e.g. OnSpawned below) is
        // still safe to leave in place even though this one was intentionally removed.
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
