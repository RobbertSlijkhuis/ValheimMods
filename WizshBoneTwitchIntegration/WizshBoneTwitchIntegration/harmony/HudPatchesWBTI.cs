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
        // Game.SpawnPlayer calls OnSpawned on the local player only (after SetLocalPlayer), on the
        // first spawn and on every respawn - HudHelper keeps the flag set once it has fired.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(Player __instance)
        {
            try
            {
                if (__instance == Player.m_localPlayer)
                    HudHelper.MarkPlayerSpawned();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in HudPatchesWBTI.OnSpawned_Postfix: " + e);
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

        // Patches SetText (the plain local call the rename UI makes) rather than the RPC_SetName
        // handler it triggers - RPC_SetName only runs on whichever peer currently owns the tame's
        // ZDO (Tameable.SetText uses the single-target InvokeRPC overload, routed to m_zdo.GetOwner()),
        // which isn't guaranteed to be the renaming player. SetText runs synchronously on the
        // renaming player's own client instead, before the RPC is even sent.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tameable), "SetText")]
        public static bool SetText_Prefix(ref Tameable __instance, ref string text)
        {
            try
            {
                // Checked before "claim:" - "unclaim:" contains that substring too, so this must
                // come first or it'd be misread as a claim for a garbled "un<name>" username.
                if (text.Contains("unclaim:"))
                {
                    __instance.gameObject.GetComponent<TwitchCreatureClaim>()?.Release();

                    // Skip the vanilla rename entirely - Release()'s own teardown already restores
                    // the true original name/tamed-name, so there's nothing left to rename to.
                    return false;
                }

                if (!text.Contains("claim:"))
                    return true;

                TwitchCreatureClaim creatureClaim = __instance.gameObject.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim == null)
                    creatureClaim = __instance.gameObject.AddComponent<TwitchCreatureClaim>();

                text = text.Replace("claim:", "");
                creatureClaim.Init(text);
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetText_Prefix: " + e);
                return true;
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
