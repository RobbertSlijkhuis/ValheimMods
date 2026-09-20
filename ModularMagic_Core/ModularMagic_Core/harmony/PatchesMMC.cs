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
        /// <summary>
        /// UseItem only queues the item, the owner of the stand attaches it later. Stamp the player that placed the
        /// staff on the queued item, so it ends up in the data of the stand and only that player can edit.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "UseItem")]
        public static void UseItem_Postfix(ref ItemStand __instance, Humanoid user, ItemData item, ItemData ___m_queuedItem)
        {
            try
            {
                // It is queued only if the stand accepted it
                if (__instance == null || __instance.m_currentItemName != "" || item == null || ___m_queuedItem != item)
                    return;

                if (Player.m_localPlayer == null || RuneTable.FromStand(__instance) == null || !ImbuementHelper.HasImbuements(item))
                    return;

                ImbuementHelper.SetEditor(item, Player.m_localPlayer.GetPlayerID());
                // A staff starts without unsaved changes, the runes of an old draft were dropped when it left the stand
                ImbuementHelper.SetDraft(item, null);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in UseItem_Postfix: " + e);
            }
        }

        /// <summary>
        /// Runs on the owner of the stand right before the staff drops from the stand (also when the table is destroyed).
        /// The runes that were used for unsaved changes drop from their slots, so they are not lost with the staff.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemStand), "DropItem")]
        public static void DropItem_Prefix(ItemStand __instance)
        {
            try
            {
                RuneTable runeTable = RuneTable.FromStand(__instance);

                if (runeTable != null)
                    runeTable.DropDraftRunes();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DropItem_Prefix: " + e);
            }
        }

        /// <summary>
        /// The game has one icon per item, a rune has an icon for every level. Used by the inventory, hotbar, drag icon and upgrade panel.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetIcon))]
        public static void GetIcon_Postfix(ItemData __instance, ref Sprite __result)
        {
            if (RuneIconHelper.TryGetIcon(__instance, out Sprite icon))
                __result = icon;
        }

        /// <summary>
        /// Upgrading a rune to the next quality needs a station level of one higher per quality, but the rune table has no
        /// station upgrades, so runes only need the level of their recipe. The costs are what limits the upgrades.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Recipe), nameof(Recipe.GetRequiredStationLevel))]
        public static void GetRequiredStationLevel_Postfix(Recipe __instance, ref int __result)
        {
            if (ImbuementHelper.IsRune(__instance.m_item))
                __result = Mathf.Max(1, __instance.m_minStationLevel);
        }

        /// <summary>
        /// Runs on every client when the stand shows or removes its item (and every few seconds when it refreshes),
        /// so every client builds the rune table from what is really on the stand.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStand), "SetVisualItem")]
        public static void SetVisualItem_Postfix(ItemStand __instance)
        {
            try
            {
                RuneTable runeTable = RuneTable.FromStand(__instance);

                if (runeTable != null)
                    runeTable.SyncFromStand();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetVisualItem_Postfix: " + e);
            }
        }
    }
}
