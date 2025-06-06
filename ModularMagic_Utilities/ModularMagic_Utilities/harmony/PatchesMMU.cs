using HarmonyLib;
using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class PatchesMMU
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                LanternMMU comp = __instance.GetComponent<LanternMMU>();
                comp.SetPlayerStatus(__instance.GetPlayerID(), true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void AwakePlayerController_Postfix(ref PlayerController __instance)
        {
            try
            {
                __instance.gameObject.AddComponent<LanternMMU>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add lantern component in AwakePlayerController_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "SetVisualItem")]
        public static void SetVisualItem_Postfix(ref ItemStand __instance)
        {
            try
            {
                if (__instance == null && __instance.m_currentItemName == "")
                    return;

                Transform weatherZoneTrans = __instance.transform.parent.Find("weatherzone");

                if (weatherZoneTrans == null)
                    return;

                WeatherZone weatherZone = weatherZoneTrans.gameObject.GetComponent<WeatherZone>();

                if (weatherZone == null || weatherZone.isInitialised)
                    return;

                weatherZone.InitWeather(__instance.m_currentItemName);

            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set weather in SetVisualItem_Postfix: " + e);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "UseItem")]
        public static void UseItem_Postfix(ref ItemStand __instance, Humanoid user, ItemDrop.ItemData item)
        {
            try
            {
                if (__instance == null)
                    return;

                Transform weatherZoneTrans = __instance.gameObject.transform.parent.Find("weatherzone");

                if (weatherZoneTrans == null)
                    return;

                if (__instance.m_currentItemName != "")
                    return;

                weatherZoneTrans.gameObject.GetComponent<WeatherZone>().EnableWeather(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not enable weather in UseItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "Interact")]
        public static void Interact_Postfix(ref ItemStand __instance, Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (__instance == null)
                    return;

                Transform weatherZoneTrans = __instance.gameObject.transform.parent.Find("weatherzone");

                if (weatherZoneTrans == null)
                    return;

                weatherZoneTrans.gameObject.GetComponent<WeatherZone>().DisableWeather();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not disable weather in Interact_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                if (__instance == null)
                    return;

                SetEitr(__instance, PluginConfig.spellbook1.magicStatusEffectName, PluginConfig.spellbook1.eitr.Value, ref eitr);
                SetEitr(__instance, PluginConfig.spellbook2.magicStatusEffectName, PluginConfig.spellbook2.eitr.Value, ref eitr);
                SetEitr(__instance, PluginConfig.spellbook3.magicStatusEffectName, PluginConfig.spellbook3.eitr.Value, ref eitr);
                SetEitr(__instance, PluginConfig.lantern1.magicStatusEffectName, PluginConfig.lantern1.eitr.Value, ref eitr);
                SetEitr(__instance, PluginConfig.lantern2.magicStatusEffectName, PluginConfig.lantern2.eitr.Value, ref eitr);
                SetEitr(__instance, PluginConfig.lantern3.magicStatusEffectName, PluginConfig.lantern3.eitr.Value, ref eitr);
            }
            catch (Exception e) 
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            try
            {
                if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                    return;

                float value = 0f;

                if (HaveStatusEffect(__instance, PluginConfig.spellbook1.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.spellbook1);
                else if (HaveStatusEffect(__instance, PluginConfig.spellbook2.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.spellbook2);
                else if (HaveStatusEffect(__instance, PluginConfig.spellbook3.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.spellbook3);
                else if (HaveStatusEffect(__instance, PluginConfig.lantern1.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.lantern1);
                else if (HaveStatusEffect(__instance, PluginConfig.lantern2.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.lantern2);
                else if (HaveStatusEffect(__instance, PluginConfig.lantern3.magicStatusEffectName))
                    value = GetValueFromConfig(skillType, PluginConfig.lantern3);

                float newLevel = __result + value;
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
                __result = newLevel;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update skills in GetSkillLevel_Postfix: " + e);
            }
        }

        private static void SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool HaveStatusEffect(Skills __instance, string name)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));
        }

        private static float GetValueFromConfig(Skills.SkillType skillType, UtilitiesConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
