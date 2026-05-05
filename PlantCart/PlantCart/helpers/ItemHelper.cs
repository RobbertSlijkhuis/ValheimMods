using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using PlantCart.Configs;
using UnityEngine;

namespace PlantCart.Helpers
{
    internal class ItemHelper
    {
        public static void CreateMaterial(GameObject prefab, MaterialConfig config)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            //itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
