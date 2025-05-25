using AzuExtendedPlayerInventory;
using HarmonyLib;
using ModularMagic_Utilities.Helpers;
using System.Collections.Generic;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class CustomSlotPatches
    {
        [HarmonyPatch(typeof(VisEquipment), "OnEnable")]
        [HarmonyPostfix]
        private static void OnEnable_Postfix(VisEquipment __instance)
        {
            if (CustomSlot._magicSlots.ContainsKey(__instance) || !__instance.m_isPlayer)
                return;
            CustomSlot._magicSlots[__instance] = new CustomSlot(__instance);
        }

        [HarmonyPatch(typeof(VisEquipment), "OnDisable")]
        [HarmonyPostfix]
        private static void OnDisable_Postfix(VisEquipment __instance) => CustomSlot._magicSlots.Remove(__instance);

        [HarmonyPatch(typeof(VisEquipment), "UpdateEquipmentVisuals")]
        [HarmonyPostfix]
        private static void UpdateEquipmentVisuals_Postfix(VisEquipment __instance)
        {
            if (!__instance.m_isPlayer)
                return;
            CustomSlot._magicSlots[__instance].UpdateEquipmentVisuals();
        }

        [HarmonyPriority(800)]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "Awake")]
        private static void Awake_Prefix(Player __instance)
        {
            if (!API.IsLoaded())
                return;
            CustomSlot._magicSlots.Add(__instance.GetComponent<VisEquipment>(), new CustomSlot(__instance.GetComponent<VisEquipment>()));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "IsItemEquiped")]
        private static void IsItemEquipped_Postfix(
        Humanoid __instance,
        ItemDrop.ItemData item,
        ref bool __result)
        {
            CustomSlot magicSlot;
            if (!API.IsLoaded() || !(__instance is Player player) || !CustomSlot._magicSlots.TryGetValue(player.m_visEquipment, out magicSlot))
                return;
            __result = true;
        }

        [HarmonyPatch(typeof(Humanoid), "SetupVisEquipment")]
        [HarmonyPrefix]
        private static void SetupVisEquipment_Prefix(Humanoid __instance)
        {
            CustomSlot magicSlot;
            if (!API.IsLoaded() || !(__instance is Player player) || !CustomSlot._magicSlots.TryGetValue(player.m_visEquipment, out magicSlot))
                return;
            magicSlot.SetTomeItem(magicSlot._equippedTomeItem == null ? "" : magicSlot._equippedTomeItem.m_dropPrefab.name);
        }

        [HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
        [HarmonyPrefix]
        private static void UnEquipAllItems_Prefix(Humanoid __instance)
        {
            if (!API.IsLoaded() || !(__instance is Player player))
                return;
            player.UnequipItem(CustomSlot._magicSlots[player.m_visEquipment]._equippedTomeItem, false);
        }

        private static bool EquipItem(
        Humanoid humanoid,
        ItemDrop.ItemData item,
        bool triggerEquipmentEffects)
        {
            if (!API.IsLoaded() || !(humanoid is Player player))
                return false;
            if (CustomSlot.IsTomeItem(item))
            {
                player.UnequipItem(CustomSlot._magicSlots[player.m_visEquipment]._equippedTomeItem, triggerEquipmentEffects);
                CustomSlot._magicSlots[player.m_visEquipment]._equippedTomeItem = item;
                return true;
            }

            return true;
        }

        private static void UnEquipItem(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (!API.IsLoaded() || !(humanoid is Player player))
                return;
            if (CustomSlot._magicSlots[player.m_visEquipment]._equippedTomeItem == item)
                CustomSlot._magicSlots[player.m_visEquipment]._equippedTomeItem = null;
        }

        private static void AddStatusEffects(Humanoid humanoid, HashSet<StatusEffect> statusEffects)
        {
            CustomSlot magicSlot;
            if (!API.IsLoaded() || !(humanoid is Player player) || !CustomSlot._magicSlots.TryGetValue(player.m_visEquipment, out magicSlot))
                return;
            StatusEffect equipStatusEffect1 = magicSlot._equippedTomeItem?.m_shared.m_equipStatusEffect;
            if (equipStatusEffect1 != null)
                statusEffects.Add(equipStatusEffect1);
        }
    }
}
