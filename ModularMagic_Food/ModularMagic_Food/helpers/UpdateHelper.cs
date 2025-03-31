using ModularMagic_Food.Models;
using System;
using UnityEngine;

namespace ModularMagic_Food.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemDropStats(GameObject prefab, UpdateItemDropStatsOptions options)
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
                if (options.movementModifier != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementModifier; }
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.health != null) { itemDrop.m_itemData.m_shared.m_food = (float)options.health; }
                if (options.healthRegen != null) { itemDrop.m_itemData.m_shared.m_foodRegen = (float)options.healthRegen; }
                if (options.stamina != null) { itemDrop.m_itemData.m_shared.m_foodStamina = (float)options.stamina; }
                if (options.eitr != null) { itemDrop.m_itemData.m_shared.m_foodEitr = (float)options.eitr; }
                if (options.burnTime != null) { itemDrop.m_itemData.m_shared.m_foodBurnTime = (float)options.burnTime; }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + error);
            }
        }
    }
}
