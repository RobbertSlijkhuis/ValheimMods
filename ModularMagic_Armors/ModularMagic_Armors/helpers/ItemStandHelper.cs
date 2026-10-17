using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class ItemStandHelper
    {
        /**
         * Lets the items be put on the vanilla item stands.
         * A stand with an empty supported items list already accepts every item that has an 'attach' child, so only a stand with a list gets the items added.
         * Vanilla prefab edits persist for the whole session, so an item that is already in the list is skipped.
         */
        public static void AddSupportedItems(IEnumerable<string> standNames, IEnumerable<GameObject> itemPrefabs)
        {
            foreach (string standName in standNames)
            {
                GameObject standPrefab = PrefabManager.Instance.GetPrefab(standName);
                ItemStand stand = standPrefab != null ? standPrefab.GetComponent<ItemStand>() : null;

                if (stand == null)
                {
                    Jotunn.Logger.LogWarning($"[ItemStandHelper] Item stand '{standName}' not found, items not added");
                    continue;
                }

                if (stand.m_supportedItems.Count == 0)
                    continue;

                foreach (GameObject itemPrefab in itemPrefabs)
                {
                    ItemDrop itemDrop = itemPrefab != null ? itemPrefab.GetComponent<ItemDrop>() : null;

                    if (itemDrop == null || stand.m_supportedItems.Contains(itemDrop))
                        continue;

                    stand.m_supportedItems.Add(itemDrop);
                }
            }
        }
    }
}
