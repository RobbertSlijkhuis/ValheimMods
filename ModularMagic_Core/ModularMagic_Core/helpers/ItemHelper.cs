using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ItemHelper
    {
        // The icon is optional, without one the item uses the icon of the prefab
        public static void Create(GameObject prefab, Configs.ItemConfig config, Sprite icon = null)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;

            if (icon != null)
                itemConfig.Icon = icon;

            // The upgrade costs only exist for items that can be upgraded
            RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, config.recipeUpgrade?.Value, config.recipeMultiplier?.Value);

            if (requirements == null || requirements.Length == 0)
                Jotunn.Logger.LogError($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = requirements;

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
