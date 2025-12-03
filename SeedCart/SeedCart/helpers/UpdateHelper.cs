using Jotunn.Managers;
using SeedCart.Configs;
using SeedCart.Models;
using SeedCart.Types;
using System;
using UnityEngine;
using static ItemDrop;

namespace SeedCart.Helpers
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
        }

        public static void UpdateTrader(UpdateTraderOptions options)
        {
            if (options.trader != null && options.trader != SeedCart.currentTrader)
            {
                GameObject prefabOld = PrefabManager.Instance.GetPrefab(SeedCart.currentTrader);
                Trader compOld = prefabOld.GetComponent<Trader>();
                Trader.TradeItem itemOld = compOld.m_items.Find(item => item.m_prefab == SeedCart.Instance.prefabs.Plow.GetComponent<ItemDrop>());
                Jotunn.Logger.LogWarning("itemOld: " + itemOld?.m_prefab?.m_itemData?.m_shared?.m_name);

                if (itemOld == null)
                {
                    Jotunn.Logger.LogError("Could not remove rusty plow from: " + SeedCart.currentTrader);
                    return;
                }

                compOld.m_items.Remove(itemOld);

                GameObject prefab = PrefabManager.Instance.GetPrefab(options.trader);
                Trader comp = prefab.GetComponent<Trader>();
                comp.m_items.Add(CreateNewTradeItem());
                SeedCart.currentTrader = options.trader;
            }
            else
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(SeedCart.currentTrader);
                Trader comp = prefab.GetComponent<Trader>();
                Trader.TradeItem itemOld = comp.m_items.Find(item => item.m_prefab == SeedCart.Instance.prefabs.Plow.GetComponent<ItemDrop>());
                Jotunn.Logger.LogWarning("itemOld: " + itemOld?.m_prefab?.m_itemData?.m_shared?.m_name);

                if (itemOld == null)
                {
                    Jotunn.Logger.LogError("Could not remove rusty plow from: " + SeedCart.currentTrader);
                    return;
                }

                comp.m_items.Remove(itemOld);
                comp.m_items.Add(CreateNewTradeItem());
            }
        }

        public static Trader.TradeItem CreateNewTradeItem()
        {
            Trader.TradeItem item = new Trader.TradeItem();
            item.m_prefab = SeedCart.Instance.prefabs.Plow.GetComponent<ItemDrop>();
            item.m_price = PluginConfig.rustedPlow.cost.Value;
            item.m_requiredGlobalKey = PluginConfig.rustedPlow.requiredGlobalKey.Value;
            item.m_stack = PluginConfig.rustedPlow.stack.Value;

            return item;
        }
    }
}
