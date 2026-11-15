using HarmonyLib;
using System;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The game does not change an item when it is upgraded at a crafting station: InventoryGui.DoCrafting unequips and removes the
    /// item and adds a new one of the next quality, without its custom data. Core moves the runes onto the new item, and a mod can move
    /// its own data with OnUpgraded.
    /// </summary>
    public static class ItemUpgradeHelper
    {
        /// <summary>
        /// Called after an item was upgraded, with the removed item and the new one. The runes are already on the new item. Also called
        /// when an upgrader item fails and adds the item back a quality lower.
        /// </summary>
        public static event Action<ItemData, ItemData> OnUpgraded;

        // The item that DoCrafting is upgrading, only set while it runs
        private static ItemData upgrading;

        /// <summary>Called before InventoryGui.DoCrafting, with the item it upgrades (null when it crafts a new item).</summary>
        internal static void Begin(ItemData upgradeItem)
        {
            upgrading = upgradeItem;
        }

        /// <summary>Called after InventoryGui.DoCrafting, also when it threw.</summary>
        internal static void End()
        {
            upgrading = null;
        }

        /// <summary>
        /// Called for every item that Inventory.AddItem adds by name. During an upgrade the first added item of the same kind is the
        /// upgraded item. An item of another kind is left alone, like the materials an upgrader item returns when it breaks the item.
        /// </summary>
        internal static void OnItemAdded(ItemData added)
        {
            if (upgrading == null || added == null || added.m_shared.m_name != upgrading.m_shared.m_name)
                return;

            ItemData from = upgrading;
            upgrading = null;

            // Only the saved runes. The draft and the editor only exist while an item is on the rune table, not at a crafting station
            string runes = from.m_customData.GetValueSafe(ImbuementHelper.DataKey);
            if (!string.IsNullOrEmpty(runes))
                added.m_customData[ImbuementHelper.DataKey] = runes;

            if (OnUpgraded == null)
                return;

            foreach (Action<ItemData, ItemData> handler in OnUpgraded.GetInvocationList())
            {
                try
                {
                    handler(from, added);
                }
                catch (Exception e)
                {
                    Jotunn.Logger.LogError("Something went wrong in an OnUpgraded handler: " + e);
                }
            }
        }
    }
}
