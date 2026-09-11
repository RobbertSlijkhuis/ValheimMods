using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SplashMeads.Configs;
using SplashMeads.Models;
using System;
using UnityEngine;

namespace SplashMeads.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, SplashMeadConfig config)
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.Enabled = config.enable.Value;
                itemConfig.Name = config.name.Value;
                itemConfig.Description = config.description.Value;
                itemConfig.CraftingStation = config.craftingStation.Value;
                itemConfig.MinStationLevel = config.minStationLevel.Value;
                itemConfig.Amount = config.recipeAmount.Value;
                RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                if (requirements == null || requirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    itemConfig.Requirements = requirements;

                
                UpdateHelper.UpdateItemData(prefab, new UpdateItemDataOptions()
                {
                    weight = config.weight.Value,
                    maxStackSize = config.maxStackSize.Value,
                });
                UpdateHelper.UpdateDuration(prefab, config.duration.Value);
                UpdateHelper.UpdateRadius(prefab, config.radius.Value);

                ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create item: " + e);
            }
        }
    }
}
