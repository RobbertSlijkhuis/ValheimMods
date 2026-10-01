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

            if (itemData.quality > 1)
                itemDrop.SetQuality(itemData.quality);

            if (itemData.stackSize > 1)
                itemDrop.SetStack(itemData.stackSize);

            itemDrop.m_itemData.m_durability = itemDrop.m_itemData.m_shared.m_maxDurability + itemDrop.m_itemData.m_shared.m_durabilityPerLevel * itemData.quality;
            rigidBody.AddForce((transform.forward * force) + (transform.up * force), ForceMode.Acceleration);
        }
    }
}
