using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;
using YamlDotNet.Core.Tokens;
using static EnemyHud;

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
        public static void Shutdown_Postfix()
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

                if (statusEffect == null)
                    return;

                if (__instance.IsDone())
                    customStatusEffect.RemoveStatusEffect(statusEffect, false);
                else
                    statusEffect.duration = __instance.GetRemaningTime();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Stop_Postfix: " + e);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Player), "OnDamaged")]
        //public static void OnDamaged_Postfix(Player __instance, HitData hit)
        //{
        //    try
        //    {
        //        if (__instance == null || hit == null)
        //            return;

        //        Jotunn.Logger.LogWarning("Player got hit!");
        //        Jotunn.Logger.LogWarning($"type: {hit.m_hitType}");
        //        Jotunn.Logger.LogWarning($"damage: {hit.m_damage}");
        //        Jotunn.Logger.LogWarning($"attacker: {hit.m_attacker}");
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError("Something went wrong in OnCollisionEnter_Postfix: " + e);
        //    }
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        //public static void UpdateHuds_Prefix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
        //{
        //    try
        //    {
        //        //Character character = (sadle ? sadle.GetCharacter() : null);
        //        //Character character2 = (player ? player.GetHoverCreature() : null);
        //        //Character character3 = null;

        //        Transform hudBase = __instance.transform.Find("HudRoot/HudBase");
        //        RectTransform level4Trans = hudBase.transform.Find("level_4") as RectTransform;

        //        if (level4Trans == null)
        //        {
        //            Jotunn.Logger.LogWarning("Setting up new levels...");
        //            RectTransform level3 = hudBase.transform.Find("level_3") as RectTransform;

        //            GameObject level4 = UnityEngine.Object.Instantiate(level3.gameObject, hudBase);
        //            level4.name = "level_4";
        //            level4.SetActive(false);

        //            GameObject star = level4.transform.Find("star").gameObject;
        //            GameObject newStar = UnityEngine.Object.Instantiate(star, level4.transform);
        //            newStar.name = "star (2)";
        //            RectTransform newStarTransform = newStar.transform as RectTransform;
        //            newStarTransform.localPosition = new Vector3(24f, 0f, 0f);

        //            level4Trans = level4.transform as RectTransform;
        //        }

        //        foreach (KeyValuePair<Character, HudData> hud in __instance.m_huds)
        //        {
        //            HudData value = hud.Value;
                    
        //            int level = value.m_character.GetLevel();
        //            // Jotunn.Logger.LogWarning("Level: " + level);

        //            //if ((bool)value.m_level2)
        //            //{
        //            //    value.m_level2.gameObject.SetActive(level == 2);
        //            //}

        //            //if ((bool)value.m_level3)
        //            //{
        //            //    value.m_level3.gameObject.SetActive(level == 3);
        //            //}


        //            if (level4Trans != null)
        //            {
        //                Jotunn.Logger.LogWarning("Level is greater then 3: " + (level == 4));
        //                level4Trans.gameObject.SetActive(level == 4);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError("Something went wrong in UpdateHuds_Prefix: " + e);
        //        return;
        //    }
        //}
    }
}
