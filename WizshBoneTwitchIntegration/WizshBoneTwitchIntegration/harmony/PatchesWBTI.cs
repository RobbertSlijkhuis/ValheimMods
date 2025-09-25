using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class PatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref PlayerController __instance)
        {
            try
            {
                Jotunn.Logger.LogWarning("Adding twitch components to player");
                __instance.gameObject.AddComponent<TwitchChat>();
                __instance.gameObject.AddComponent<TwitchCustomRewards>();
                __instance.gameObject.AddComponent<TwitchAuth>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add Twitch components in OnSpawned_Postfix: " + e);
            }
        }
    }
}
