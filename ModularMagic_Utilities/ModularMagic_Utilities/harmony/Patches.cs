using HarmonyLib;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;

namespace MagicExtended.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            if (__instance == null)
                return;

            SetEitr(__instance, ConfigUtilities.spellbook1.magicStatusEffectName, ConfigUtilities.spellbook1.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.spellbook2.magicStatusEffectName, ConfigUtilities.spellbook2.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.spellbook3.magicStatusEffectName, ConfigUtilities.spellbook3.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern1.magicStatusEffectName, ConfigUtilities.lantern1.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern2.magicStatusEffectName, ConfigUtilities.lantern2.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern3.magicStatusEffectName, ConfigUtilities.lantern3.eitr.Value, ref eitr);
        }

        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        [HarmonyPostfix]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                return;

            float value = 0f;
            float currentLevel = __instance.GetSkill(skillType).m_level;

            if (HaveStatusEffect(__instance, ConfigUtilities.spellbook1.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook1);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.spellbook2.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook2);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.spellbook3.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook3);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern1.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.lantern1);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern2.magicStatusEffectName))
            {
               value = GetValueFromConfig(skillType, ConfigUtilities.lantern2);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern3.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.lantern3);
            }

            if (value == 0f)
            {
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref currentLevel);
                __result = currentLevel;
                return;
            }

            float newLevel = currentLevel + value;
            __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
            __result = newLevel;
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

        private static bool HaveStatusEffect(Skills __instance, string statusEffect)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(statusEffect));
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
