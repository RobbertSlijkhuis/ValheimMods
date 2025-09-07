using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Core.Configs;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ItemHelper
    {
        public static void CreateMaterial(GameObject prefab, MaterialConfig config)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;
            RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

            if (requirements == null || requirements.Length == 0)
                Jotunn.Logger.LogError($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = requirements;

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
