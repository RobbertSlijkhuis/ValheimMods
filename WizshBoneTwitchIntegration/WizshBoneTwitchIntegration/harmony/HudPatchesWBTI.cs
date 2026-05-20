using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;
using static EnemyHud;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class HudPatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetHoverText")]
        public static void GetHoverText_Postfix(ref Character __instance, ref string __result)
        {
            try
            {
                TwitchCreatureInteract creatureInteract = __instance.gameObject.GetComponent<TwitchCreatureInteract>();

                if (creatureInteract != null)
                    __result = creatureInteract.GetHoverText();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetHoverText_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextInput), "RequestText")]
        public static void RequestText_Postfix(ref TextInput __instance, TextReceiver sign, string topic, ref int charLimit)
        {
            try
            {
                if (sign.ToString().Contains("(Tameable)") && topic == "$hud_rename")
                    __instance.m_inputField.characterLimit = 60;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in RequestText_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tameable), "RPC_SetName")]
        public static void RPC_SetName_Prefix(ref Tameable __instance, long sender, ref string name, string authorId)
        {
            try
            {
                if (!name.Contains("claim:"))
                    return;

                TwitchCreatureClaim creatureClaim = __instance.gameObject.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim == null)
                    creatureClaim = __instance.gameObject.AddComponent<TwitchCreatureClaim>();

                name = name.Replace("claim:", "");
                creatureClaim.Init(name);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in RPC_SetName_Prefix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        public static void UpdateHuds_Postfix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
        {
            try
            {
                RectTransform hudBase = __instance.transform.Find("HudRoot/HudBase") as RectTransform;
                RectTransform level4Trans = hudBase.transform.Find("level_custom_4") as RectTransform;

                if (level4Trans == null)
                    HudHelper.GenerateLevels(hudBase);

                foreach (KeyValuePair<Character, HudData> hud in __instance.m_huds)
                {
                    HudData value = hud.Value;

                    if (value == null || value.m_level3 == null)
                        continue;

                    int level = value.m_character.GetLevel();

                    RectTransform hudLevel2 = value.m_level3.parent.Find("level_custom_2") as RectTransform;
                    RectTransform hudLevel3 = value.m_level3.parent.Find("level_custom_3") as RectTransform;
                    RectTransform hudLevel4 = value.m_level3.parent.Find("level_custom_4") as RectTransform;
                    RectTransform hudLevel5 = value.m_level3.parent.Find("level_custom_5") as RectTransform;
                    RectTransform hudLevel6 = value.m_level3.parent.Find("level_custom_6") as RectTransform;
                    RectTransform hudLevel7 = value.m_level3.parent.Find("level_custom_7") as RectTransform;
                    RectTransform hudLevel8 = value.m_level3.parent.Find("level_custom_8") as RectTransform;
                    RectTransform hudLevel9 = value.m_level3.parent.Find("level_custom_9") as RectTransform;
                    RectTransform hudLevel10 = value.m_level3.parent.Find("level_custom_10") as RectTransform;

                    if (hudLevel2 != null)
                        hudLevel2.gameObject.SetActive(level >= 4);

                    if (hudLevel3 != null)
                        hudLevel3.gameObject.SetActive(level >= 4);

                    if (hudLevel4 != null)
                        hudLevel4.gameObject.SetActive(level >= 4);

                    if (hudLevel5 != null)
                        hudLevel5.gameObject.SetActive(level >= 5);

                    if (hudLevel6 != null)
                        hudLevel6.gameObject.SetActive(level >= 6);

                    if (hudLevel7 != null)
                        hudLevel7.gameObject.SetActive(level >= 7);

                    if (hudLevel8 != null)
                        hudLevel8.gameObject.SetActive(level >= 8);

                    if (hudLevel9 != null)
                        hudLevel9.gameObject.SetActive(level >= 9);

                    if (hudLevel10 != null)
                        hudLevel10.gameObject.SetActive(level == 10);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in UpdateHuds_Postfix: " + e);
            }
        }
    }
}
