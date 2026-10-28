using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    /// <summary>
    /// Patch methods only - all state and logic lives in <see cref="DeathSoundHelper"/>.
    /// </summary>
    [HarmonyPatch]
    public class DeathSoundPatchesWBTI
    {
        // Player.OnDeath() (protected, patched by string name) returns early for non-owners, so this
        // only ever runs for the client that owns the dying Player; the local-player check is a second guard.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnDeath")]
        public static void PlayerOnDeath_Postfix(Player __instance)
        {
            try
            {
                if (__instance == Player.m_localPlayer)
                    DeathSoundHelper.PlayRandom();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in PlayerOnDeath_Postfix (death sound): " + e);
            }
        }
    }
}
