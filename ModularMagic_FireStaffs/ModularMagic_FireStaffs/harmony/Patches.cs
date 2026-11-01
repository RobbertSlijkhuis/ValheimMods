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
