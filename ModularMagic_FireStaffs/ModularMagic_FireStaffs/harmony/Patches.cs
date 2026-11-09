using HarmonyLib;
using ModularMagic_FireStaffs.Helpers;
using System;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_FireStaffs.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            FireImbuementHelper.GameIsReady = true;
        }

        // The Attack Speed rune speeds up the animation of the normal attack, see AttackSpeedHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public static void StartAttack_Postfix(Humanoid __instance, bool secondaryAttack, bool __result)
        {
            if (__result)
                AttackSpeedHelper.OnAttackStarted(__instance, secondaryAttack);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UpdateAttack")]
        public static void UpdateAttack_Postfix(Humanoid __instance)
        {
            AttackSpeedHelper.OnAttackUpdated(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !FireImbuementHelper.GameIsReady)
                    return;

                if (!CoreImbuementHelper.HasImbuements(item) || !FireImbuementHelper.IsFireStaff(item))
                    return;

                FireImbuementHelper.ApplyImbuements(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }
    }
}
