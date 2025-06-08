using Jotunn.Managers;
using ModularMagic_Food.Models;
using System;
using UnityEngine;

namespace ModularMagic_Food.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemDrop(GameObject prefab, UpdateItemDropOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                if (options.name != null) { itemDrop.m_itemData.m_shared.m_name = options.name; }
                if (options.description != null) { itemDrop.m_itemData.m_shared.m_description = options.description; }
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.maxStackSize != null) { itemDrop.m_itemData.m_shared.m_maxStackSize = (int)options.maxStackSize; }
                if (options.health != null) { itemDrop.m_itemData.m_shared.m_food = (float)options.health; }
                if (options.healthRegen != null) { itemDrop.m_itemData.m_shared.m_foodRegen = (float)options.healthRegen; }
                if (options.stamina != null) { itemDrop.m_itemData.m_shared.m_foodStamina = (float)options.stamina; }
                if (options.eitr != null) { itemDrop.m_itemData.m_shared.m_foodEitr = (float)options.eitr; }
                if (options.burnTime != null) { itemDrop.m_itemData.m_shared.m_foodBurnTime = (float)options.burnTime; }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static void UpdatePickable(GameObject prefab, UpdatePickableOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Pickable pickable = prefab.GetComponent<Pickable>();

                if (pickable == null)
                    throw new Exception("Pickable is null");

                if (options.amount != null) { pickable.m_amount = (int)options.amount; }
                if (options.respawnTimeMinutes != null) { pickable.m_respawnTimeMinutes = (float)options.respawnTimeMinutes; }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update pickable: " + e);
            }
        }
    }
}
