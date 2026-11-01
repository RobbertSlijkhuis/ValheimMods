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
        /// so every client attaches or removes the rune table to match the staff that is really on the stand.
        /// The runes follow the data of the stand in RuneTable.Update.
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

        /// <summary>
        /// The game uses the eitr cost of the main attack of the weapon for every attack, also the secondary one.
        /// The secondary attacks that staff mods registered use their own cost, so the eitr saving of the runes works.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackEitr), new[] { typeof(Character), typeof(ItemData) })]
        public static void GetAttackEitr_Postfix(Attack __instance, Character character, ItemData weapon, ref float __result)
        {
            try
            {
                if (SecondaryAttackHelper.TryGetEitr(__instance, character, weapon, out float eitr))
                    __result = eitr;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetAttackEitr_Postfix: " + e);
            }
        }

        /// <summary>
        /// The registered secondary attacks with a cooldown can not start while the staff is recharging.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), "Start")]
        public static bool AttackStart_Prefix(Attack __instance, Humanoid character)
        {
            try
            {
                return SecondaryAttackHelper.CheckCooldown(character, __instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AttackStart_Prefix: " + e);
                return true;
            }
        }

        /// <summary>
        /// The cooldown starts when the attack really started. The game can still refuse it after the prefix
        /// (not enough eitr or stamina, a dungeon, ...), and then the staff must not start recharging.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Attack), "Start")]
        public static void AttackStart_Postfix(Attack __instance, Humanoid character, bool __result)
        {
            try
            {
                if (__result)
                    SecondaryAttackHelper.StartCooldown(character, __instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AttackStart_Postfix: " + e);
            }
        }
    }
}
