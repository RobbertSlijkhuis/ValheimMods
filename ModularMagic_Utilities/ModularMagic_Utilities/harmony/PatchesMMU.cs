using HarmonyLib;
using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
//using System;
//using ModularMagic_Utilities.Helpers;
//using System;
//using static ItemDrop;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class PatchesMMU
    {
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
