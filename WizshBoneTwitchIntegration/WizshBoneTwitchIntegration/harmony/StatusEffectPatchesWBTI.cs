using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class StatusEffectPatchesWBTI
    {
        

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                if (Player.m_localPlayer.GetPlayerID() != __instance.GetPlayerID())
                {
                    Jotunn.Logger.LogWarning($"Not local player! {Player.m_localPlayer.GetPlayerID()} - {__instance.GetPlayerID()}");
                    return;
                }

                TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                customStatusEffect.ReApplyStatusEffects();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnSpawned_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatusEffect), "Stop")]
        public static void Stop_Postfix(StatusEffect __instance)
        {
            try
            {
                int nameHash = __instance.NameHash();
                TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                StatusEffectData statusEffect = customStatusEffect.GetStatusEffects().Find(item => item.nameHash == nameHash);

                if (statusEffect == null)
                    return;

                if (__instance.IsDone())
                {
                    //Jotunn.Logger.LogWarning("Removing StatusEffect: " + statusEffect.name);
                    customStatusEffect.RemoveStatusEffect(statusEffect, false);
                }
                else if (statusEffect.persistsThroughDeath)
                {
                    //Jotunn.Logger.LogWarning("Setting StatusEffect remaining time: " + statusEffect.name + ", " + __instance.GetRemaningTime());
                    statusEffect.durationRemaining = __instance.GetRemaningTime();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Stop_Postfix: " + e);
            }
        }
    }
}
