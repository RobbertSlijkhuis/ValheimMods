using HarmonyLib;
using ModularMagic_Core.Components;
using System;
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

                string imbuementsString = item.m_customData.GetValueSafe(ModularMagic_Core.imbuementDataKey);

                if (imbuementsString == null)
                    return;

                RuneTable imbuementTable = __instance.transform.parent.gameObject.GetComponent<RuneTable>();

                if (imbuementTable == null)
                    return;

                imbuementTable.StaffAttach(imbuementsString, item);
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
                if (!hold || __instance == null)
                    return;

                RuneTable imbuementTable = __instance.transform.parent.gameObject.GetComponent<RuneTable>();

                if (imbuementTable == null)
                    return;

                imbuementTable.StaffRemove();

            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Interact_Postfix: " + e);
            }
        }
    }
}
