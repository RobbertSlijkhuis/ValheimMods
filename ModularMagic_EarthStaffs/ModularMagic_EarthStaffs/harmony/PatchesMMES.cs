using HarmonyLib;
using ModularMagic_EarthStaffs.Helpers;
using System;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_EarthStaffs.Harmony
{
    [HarmonyPatch]
    public class PatchesMMES
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            try
            {
                EarthImbuementHelper.GameIsReady = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GameStart_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ref Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !EarthImbuementHelper.GameIsReady)
                    return;

                if (!CoreImbuementHelper.HasImbuements(item) || !EarthImbuementHelper.IsEarthStaff(item))
                    return;

                EarthImbuementHelper.ApplyImbuements(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }
    }
}
