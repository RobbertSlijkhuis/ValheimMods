using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    /// <summary>
    /// Patch methods only - the logic lives in <see cref="AnimationSoundHelper"/>.
    /// </summary>
    [HarmonyPatch]
    public class AnimationSoundPatchesWBTI
    {
        // Returning false skips the vanilla Effect call, so the event's sound is never instantiated.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnimationEffect), nameof(AnimationEffect.Effect))]
        public static bool AnimationEffectEffect_Prefix(AnimationEffect __instance, UnityEngine.AnimationEvent e)
        {
            try
            {
                return !AnimationSoundHelper.ShouldSkipEffect(__instance, e);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("AnimationEffectEffect_Prefix failed: " + ex);
                return true;
            }
        }
    }
}
