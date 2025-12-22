using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class PatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                Game.instance.gameObject.AddComponent<TwitchChat>();
                Game.instance.gameObject.AddComponent<TwitchCustomRewards>();
                Game.instance.gameObject.AddComponent<TwitchAuth>();
                Game.instance.gameObject.AddComponent<TwitchChatting>();
                Game.instance.gameObject.AddComponent<TwitchCustomStatusEffect>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add Twitch components in GameAwake_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Shutdown")]
        public static void Shutdown_Postfix(ref PlayerController __instance)
        {
            try
            {
                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                customRewards.ClearRewards();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on Shutdown_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
        public static void GlobalKeyAdd_Postfix(ref ZoneSystem __instance, string keyStr, bool canSaveToServerOptionKeys = true)
        {
            try
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                string loweredKey = keyStr.ToLower();

                if (!auth || !auth.m_loggedIn || (!loweredKey.Contains("defeated_") && !loweredKey.Contains("killed")))
                    return;

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                customRewards.SetRewards();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update rewards on GlobalKeyAdd_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetHoverText")]
        public static void GetHoverText_Postfix(ref Character __instance, ref string __result)
        {
            try
            {
                TwitchCreatureInteract creatureInteract = __instance.gameObject.GetComponent<TwitchCreatureInteract>();

                if (creatureInteract != null)
                {
                    __result = creatureInteract.GetHoverText();
                    return;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetHoverText_Postfix: " + e);
            }
        }

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
                TwitchStatusEffect statusEffect = customStatusEffect.GetStatusEffects().Find(item => item.nameHash == nameHash);
                Jotunn.Logger.LogWarning($"STOP: {nameHash} in Stop_Postfix");

                if (statusEffect == null)
                    return;

                Jotunn.Logger.LogWarning($"{nameHash} is custom StatusEffect!");

                if (__instance.IsDone())
                {
                    Jotunn.Logger.LogWarning("Is done, calling onEnd!");
                    customStatusEffect.RemoveStatusEffect(statusEffect, false);
                }
                else
                {
                    Jotunn.Logger.LogWarning("Updating remaining time!");
                    statusEffect.duration = __instance.GetRemaningTime();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Stop_Postfix: " + e);
            }
        }
    }
}
