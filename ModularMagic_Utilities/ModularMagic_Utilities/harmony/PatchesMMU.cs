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
                Jotunn.Logger.LogError("Could not add component in AwakePlayerController_Postfix: " + e);
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

                Transform weatherStoneTrans = __instance.gameObject.transform.Find("weatherstone");

                if (weatherStoneTrans == null)
                    return;

                WeatherStone weatherStone = weatherStoneTrans.gameObject.GetComponent<WeatherStone>();
                EnvZone envZone = weatherStoneTrans.gameObject?.GetComponent<EnvZone>();

                if (weatherStone == null || weatherStone.isInitialised || envZone == null)
                    return;

                weatherStone.isInitialised = true;
                string newEnv = "";

                if (__instance.m_currentItemName == PluginConfig.spellbook1.name.Value)
                    newEnv = "Clear";

                if (__instance.m_currentItemName == PluginConfig.spellbook2.name.Value)
                    newEnv = "ThunderStorm";

                if (__instance.m_currentItemName == PluginConfig.lantern2.name.Value)
                    newEnv = "SnowStorm";

                if (envZone.m_environment == newEnv)
                    return;

                envZone.m_environment = newEnv;
                _ActivateMarbleItemStandEffects(__instance, true);

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

                Transform weatherStoneTrans = __instance.gameObject.transform.Find("weatherstone");

                if (weatherStoneTrans == null)
                    return;

                EnvZone envZone = weatherStoneTrans.gameObject?.GetComponent<EnvZone>();

                if (envZone == null)
                    return;

                List<string> nameList = new List<string>()
                {
                    PluginConfig.spellbook1.name.Value,
                    PluginConfig.spellbook2.name.Value,
                    PluginConfig.spellbook3.name.Value,
                    PluginConfig.lantern1.name.Value,
                    PluginConfig.lantern2.name.Value,
                    PluginConfig.lantern3.name.Value,
                };

                if (!nameList.Contains(item.m_shared.m_name))
                    return;

                string newEnv = "";

                if (item.m_shared.m_name == PluginConfig.spellbook1.name.Value)
                    newEnv = "Clear";

                if (item.m_shared.m_name == PluginConfig.spellbook2.name.Value)
                    newEnv = "ThunderStorm";

                if (item.m_shared.m_name == PluginConfig.lantern2.name.Value)
                    newEnv = "SnowStorm";

                if (newEnv == "")
                    return;

                envZone.m_environment = newEnv;
                _ActivateMarbleItemStandEffects(__instance, true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set item on itemstand: " + e);
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

                Transform weatherStoneTrans = __instance.gameObject.transform.Find("weatherstone");

                if (weatherStoneTrans == null)
                    return;

                EnvZone envZone = weatherStoneTrans.gameObject?.GetComponent<EnvZone>();

                if (envZone == null)
                    return;

                if (envZone.m_environment == "")
                    return;

                envZone.m_environment = "";
                _ActivateMarbleItemStandEffects(__instance, false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not take item from itemstand: " + e);
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

                _SetEitr(__instance, PluginConfig.spellbook1.magicStatusEffectName, PluginConfig.spellbook1.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.spellbook2.magicStatusEffectName, PluginConfig.spellbook2.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.spellbook3.magicStatusEffectName, PluginConfig.spellbook3.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern1.magicStatusEffectName, PluginConfig.lantern1.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern2.magicStatusEffectName, PluginConfig.lantern2.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern3.magicStatusEffectName, PluginConfig.lantern3.eitr.Value, ref eitr);
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

                if (_HaveStatusEffect(__instance, PluginConfig.spellbook1.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook1);
                else if (_HaveStatusEffect(__instance, PluginConfig.spellbook2.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook2);
                else if (_HaveStatusEffect(__instance, PluginConfig.spellbook3.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook3);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern1.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern1);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern2.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern2);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern3.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern3);

                float newLevel = __result + value;
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
                __result = newLevel;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update skills in GetSkillLevel_Postfix: " + e);
            }
        }

        private static void _ActivateMarbleItemStandEffects(ItemStand itemStand, bool value)
        {
            Jotunn.Logger.LogWarning("_ActivateMarbleItemStandEffects() " + value);
            try
            {
                Color emissionColor = new Color(0f, 0.5676858f, 1.294612f, 1f);
                Color emissionOffColor = new Color(0f, 0f, 0f, 1f);
                Color emissionRuneOffColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                Transform rimTrans = itemStand.gameObject.transform.Find("New/emission");
                Transform shieldTrans = itemStand.gameObject.transform.Find("New/forcefield");
                Transform runesTrans = itemStand.gameObject.transform.Find("New/runes emission");
                Transform weatherStonrTrans = itemStand.gameObject.transform.Find("weatherstone");
                WeatherStone weatherStone = weatherStonrTrans.GetComponent<WeatherStone>();

                if (rimTrans != null)
                {
                    MeshRenderer meshRendererComp = rimTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];

                    if (mat != null)
                    {
                        // mat.SetColor("_EmissionColor", value ? emissionColor : emissionOffColor);
                        weatherStone.StartLerpColor(mat, value ? emissionOffColor : emissionColor, value ? emissionColor : emissionOffColor, 3f);
                    }

                    Transform particleTrans = rimTrans.gameObject.transform.Find("particles");

                    if (particleTrans != null)
                    {
                        particleTrans.gameObject.SetActive(value);
                    }

                    Transform lightsTrans = rimTrans.gameObject.transform.Find("light");

                    if (lightsTrans == null)
                        return;

                    lightsTrans.gameObject.SetActive(value);
                }

                if (runesTrans != null)
                {
                    MeshRenderer meshRendererComp = runesTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];

                    if (mat == null)
                        return;

                    //mat.SetColor("_EmissionColor", value ? emissionColor : emissionRuneOffColor);
                    weatherStone.StartLerpColor(mat, value ? emissionRuneOffColor : emissionColor, value ? emissionColor : emissionRuneOffColor, 1.5f);

                }

                if (shieldTrans != null)
                {
                    shieldTrans.gameObject.SetActive(value);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not enable/disable Marble Item Stand effects: " + e);
            }
        }

        private static void _SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool _HaveStatusEffect(Skills __instance, string name)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));
        }

        private static float _GetValueFromConfig(Skills.SkillType skillType, UtilitiesConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
