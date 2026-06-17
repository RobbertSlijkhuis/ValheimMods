using HarmonyLib;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class TimeStopPatchesWBTI
    {
        // Block player input while frozen so the movement system can't fight the frozen rigidbody.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
        public static bool PlayerController_FixedUpdate_Prefix()
        {
            return !TimeStopHelper.IsPlayerFrozen;
        }
    }
}
