using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, Configs.ItemConfig config, bool renderIcon = false)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;

            if (renderIcon)
            {
                RenderManager.RenderRequest request = new RenderManager.RenderRequest(prefab);
                request.Rotation = RenderManager.IsometricRotation;
                // request.UseCache = true;
                itemConfig.Icon = RenderManager.Instance.Render(request);
            }

            RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

            if (requirements == null || requirements.Length == 0)
                Jotunn.Logger.LogError($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = requirements;

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
