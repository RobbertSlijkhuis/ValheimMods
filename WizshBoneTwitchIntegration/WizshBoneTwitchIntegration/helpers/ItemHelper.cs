using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class ItemHelper
    {
        public static void SpawnItem(ItemData itemData, Transform transform, float force)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(itemData.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError($"Could not find prefab {itemData.prefabName} to spawn!");
                return;
            }

            GameObject spawned = ZNetViewHelper.Instantiate(prefab, transform.position, transform.rotation);
            ItemDrop itemDrop = spawned.GetComponent<ItemDrop>();
            Rigidbody rigidBody = spawned.GetComponent<Rigidbody>();

            int quality = Mathf.Clamp(itemData.quality, 1, ItemData.MaxQuality);
            if (quality > 1)
                itemDrop.SetQuality(quality);

            if (itemData.amount > 1)
                itemDrop.SetStack(itemData.amount);

            // Same as Inventory.AddItem (and the Refinement Forge): full durability for the item's
            // actual quality, which scales per level above 1 - including levels past its max quality.
            itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability();
            rigidBody.AddForce((transform.forward * force) + (transform.up * force), ForceMode.Acceleration);
        }
    }
}
