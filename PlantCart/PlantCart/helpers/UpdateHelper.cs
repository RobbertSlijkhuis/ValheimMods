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
            string oldTraderName = PlantCart.currentTrader;
            bool traderChanged = options.trader != null && options.trader != oldTraderName;
            string newTraderName = traderChanged ? options.trader : oldTraderName;

            // Permanent prefab templates - governs any trader instantiated from now on
            GameObject oldPrefab = ZNetScene.instance.m_prefabs.Find(item => item.name == oldTraderName);
            Trader oldPrefabTrader = oldPrefab.GetComponent<Trader>();
            RemoveTradeItem(oldPrefabTrader);

            Trader newPrefabTrader = oldPrefabTrader;
            if (traderChanged)
            {
                GameObject newPrefab = ZNetScene.instance.m_prefabs.Find(item => item.name == newTraderName);
                newPrefabTrader = newPrefab.GetComponent<Trader>();
            }
            AddTradeItem(newPrefabTrader);

            // Already-instantiated live NPC(s) in the currently loaded scene, if any
            Trader oldLiveTrader = FindLiveTrader(oldTraderName);
            RemoveTradeItem(oldLiveTrader);

            Trader newLiveTrader = traderChanged ? FindLiveTrader(newTraderName) : oldLiveTrader;
            AddTradeItem(newLiveTrader);

            if (traderChanged)
                PlantCart.currentTrader = newTraderName;
        }

        private static Trader FindLiveTrader(string prefabName)
        {
            if (prefabName == null)
                return null;

            int prefabHash = prefabName.GetStableHashCode();
            Trader[] liveTraders = UnityEngine.Object.FindObjectsByType<Trader>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (Trader trader in liveTraders)
            {
                ZDO zdo = trader.GetComponent<ZNetView>()?.GetZDO();
                if (zdo != null && zdo.GetPrefab() == prefabHash)
                    return trader;
            }
            return null;
        }

        private static Trader.TradeItem FindTradeItem(Trader trader)
        {
            return trader?.m_items.Find(item => item.m_prefab == PlantCart.Instance.prefabs.Plow.GetComponent<ItemDrop>());
        }

        private static void RemoveTradeItem(Trader trader)
        {
            Trader.TradeItem existing = FindTradeItem(trader);
            if (existing != null)
                trader.m_items.Remove(existing);
        }

        private static void AddTradeItem(Trader trader)
        {
            if (trader != null)
                trader.m_items.Add(CreateNewTradeItem());
        }

        public static Trader.TradeItem CreateNewTradeItem()
        {
            Trader.TradeItem item = new Trader.TradeItem();
            item.m_prefab = PlantCart.Instance.prefabs.Plow.GetComponent<ItemDrop>();
            item.m_price = PluginConfig.rustedPlow.cost.Value;
            item.m_requiredGlobalKey = PluginConfig.rustedPlow.requiredGlobalKey.Value;
            item.m_stack = PluginConfig.rustedPlow.stack.Value;
            item.m_tooltip = string.Empty; // StoreGui.FillList() reads m_tooltip.Length with no null-guard; "new TradeItem()" leaves it null (Unity-deserialized vanilla entries default to "")
            item.m_buyPlayerEffects = new EffectList(); // StoreGui.BuySelectedItem() calls m_buyPlayerEffects.Create() with no null-guard after a successful purchase; "new TradeItem()" leaves it null (Unity-deserialized vanilla entries default to a real EffectList)

            return item;
        }
    }
}
