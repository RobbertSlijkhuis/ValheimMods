using HarmonyLib;
using ModularMagic_Armors.Helpers;
using System;
using UnityEngine;

namespace ModularMagic_Armors.Harmony
{
    [HarmonyPatch]
    public class PatchesMMA
    {
        // Runs every frame for every VisEquipment, so the accessory is checked first
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "GetHairItem")]
        public static void GetHairItem_Postfix(VisEquipment __instance, ItemDrop.ItemData.AccessoryType accessory, ref int __result)
        {
            if (accessory != ItemDrop.ItemData.AccessoryType.Beard || __result == 0)
                return;

            if (BeardHelper.ShouldHideBeard(__instance))
                __result = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FootStep), "DoEffect")]
        public static void DoEffect_Postfix(FootStep __instance, Vector3 point)
        {
            FootstepHelper.AddDarkWizardFootstep(__instance, point, __instance.transform.position - __instance.m_lastPosition);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                eitr += StatsHelper.GetEquippedEitr(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }
    }
}
