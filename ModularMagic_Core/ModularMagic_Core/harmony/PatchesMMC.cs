using HarmonyLib;
using ModularMagic_Core.Components;
using ModularMagic_Core.Helpers;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Harmony
{
    [HarmonyPatch]
    public class PatchesMMC
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "UseItem")]
        public static void UseItem_Postfix(ref ItemStand __instance, Humanoid user, ItemData item)
        {
            try
            {
                if (__instance == null || __instance.m_currentItemName != "" || item == null)
                    return;

                string imbuementsStringEarth = item.m_customData.GetValueSafe(ModularMagic_Core.imbuementMMESDataKey);

                if (imbuementsStringEarth == null)
                    return;

                ImbuementTable comp = __instance.transform.parent.gameObject.GetComponent<ImbuementTable>();
                comp.StaffAttach(imbuementsStringEarth, item);

                //var skills = Player.m_localPlayer.GetSkills();
                //var skillList = skills.GetSkillList();

                //foreach (Skills.Skill skill in skillList)
                //{
                //    Jotunn.Logger.LogWarning(skill.m_info.m_skill);
                //    Jotunn.Logger.LogWarning(skill.m_level);
                //    Jotunn.Logger.LogWarning(skill.m_accumulator);
                //}
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Somethign went wrong in UseItem_Postfix: " + e);
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

                ImbuementTable comp = __instance.transform.parent.gameObject.GetComponent<ImbuementTable>();

                if (comp == null)
                    return;

                comp.StaffRemove();

            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not disable weather in Interact_Postfix: " + e);
            }
        }
    }
}
