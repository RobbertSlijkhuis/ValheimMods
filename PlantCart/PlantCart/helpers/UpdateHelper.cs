using PlantCart.Configs;
using PlantCart.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace PlantCart.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            ItemData itemData = prefab.GetComponent<ItemDrop>().m_itemData;
            UpdateItemData(itemData, options);
        }

        public static void UpdateItemData(ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");

            if (options.name != null) { itemData.m_shared.m_name = options.name; }
            if (options.description != null) { itemData.m_shared.m_description = options.description; }
            if (options.teleportable != null) { itemData.m_shared.m_teleportable = (bool)options.teleportable; }
        }

        public static void UpdateTrader(UpdateTraderOptions options)
        {
            GameObject currentPrefab = ZNetScene.instance.m_prefabs.Find(item => item.name == PlantCart.currentTrader);
            Trader currentTrader = currentPrefab.GetComponent<Trader>();
            Trader.TradeItem currentItem = currentTrader.m_items.Find(item => item.m_prefab == PlantCart.Instance.prefabs.Plow.GetComponent<ItemDrop>());

            if (currentItem != null)
                currentTrader.m_items.Remove(currentItem);

            if (options.trader != null && options.trader != PlantCart.currentTrader)
            {
                GameObject newprefab = ZNetScene.instance.m_prefabs.Find(item => item.name == options.trader);
                Trader newTrader = newprefab.GetComponent<Trader>();
                newTrader.m_items.Add(CreateNewTradeItem());
                PlantCart.currentTrader = options.trader;
            }
            else
                currentTrader.m_items.Add(CreateNewTradeItem());
        }

        public static Trader.TradeItem CreateNewTradeItem()
        {
            Trader.TradeItem item = new Trader.TradeItem();
            item.m_prefab = PlantCart.Instance.prefabs.Plow.GetComponent<ItemDrop>();
            item.m_price = PluginConfig.rustedPlow.cost.Value;
            item.m_requiredGlobalKey = PluginConfig.rustedPlow.requiredGlobalKey.Value;
            item.m_stack = PluginConfig.rustedPlow.stack.Value;

            return item;
        }
    }
}
