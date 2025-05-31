using ModularMagic_Armors.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace ModularMagic_Armors.Helpers
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
                if (options.armor != null) { itemDrop.m_itemData.m_shared.m_armor = (float)options.armor; }
                if (options.armorPerLevel != null) { itemDrop.m_itemData.m_shared.m_armorPerLevel = (float)options.armorPerLevel; }
                if (options.armorSetOptions != null) {
                    itemDrop.m_itemData.m_shared.m_setName = options.armorSetOptions.name;
                    itemDrop.m_itemData.m_shared.m_setSize = options.armorSetOptions.size;
                    itemDrop.m_itemData.m_shared.m_setStatusEffect = options.armorSetOptions.statusEffect;
                }
                if (options.equipStatusEffect == null || options.equipStatusEffect.name != "empty_MMA") { itemDrop.m_itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.maxDurability != null) { itemDrop.m_itemData.m_shared.m_maxDurability = (float)options.maxDurability; }
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = (int)options.maxQuality; }
                if (options.movementSpeed != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementSpeed; }
                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static UpdateItemDropStatsOptions TakeSnapShot(GameObject prefab)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                ArmorSetOptions armorSetOptions = new ArmorSetOptions()
                {
                    name = itemDrop.m_itemData.m_shared.m_setName,
                    size = itemDrop.m_itemData.m_shared.m_setSize,
                    statusEffect = itemDrop.m_itemData.m_shared.m_setStatusEffect,
                };

                return new UpdateItemDropStatsOptions()
                {
                    name = itemDrop.m_itemData.m_shared.m_name,
                    description = itemDrop.m_itemData.m_shared.m_description,
                    armor = itemDrop.m_itemData.m_shared.m_armor,
                    armorPerLevel = itemDrop.m_itemData.m_shared.m_armorPerLevel,
                    armorSetOptions = armorSetOptions,
                    equipStatusEffect = itemDrop.m_itemData.m_shared.m_equipStatusEffect,
                    weight = itemDrop.m_itemData.m_shared.m_weight,
                    maxDurability = itemDrop.m_itemData.m_shared.m_maxDurability,
                    maxQuality = itemDrop.m_itemData.m_shared.m_maxQuality,
                    movementSpeed = itemDrop.m_itemData.m_shared.m_movementModifier,
                    eitrRegen = itemDrop.m_itemData.m_shared.m_eitrRegenModifier,
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create snapshot of stats: " + e);
                return null;
            }
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                    throw new Exception("Local player is null");

                Inventory inv = Player.m_localPlayer.GetInventory();

                if (inv == null)
                    throw new Exception("Inventory is null");

                if (!inv.ContainsItemByName(name))
                    return;

                List<ItemDrop.ItemData> list = inv.GetAllItems();
                List<ItemDrop.ItemData> items = list.FindAll(item => item.m_shared.m_name == name);

                foreach (ItemDrop.ItemData item in items)
                {
                    if (item == null || item.m_shared == null)
                    {
                        Jotunn.Logger.LogError("Could not find " + name + " in inventory list to update Eitr regen");
                        continue;
                    }

                    Jotunn.Logger.LogWarning("Update Eitr regen to: " + value);
                    item.m_shared.m_eitrRegenModifier = value;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr regen on player: " + e);
            }
        }
    }
}
