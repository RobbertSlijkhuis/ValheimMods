using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Food.Configs;
using ModularMagic_Food.Models;
using UnityEngine;

namespace ModularMagic_Food.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, FoodConfig config, GameObject pickablePrefab = null)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.Weight = config.weight.Value;

            if (config.recipe != null)
            {
                itemConfig.CraftingStation = config.craftingStation.Value;
                itemConfig.MinStationLevel = config.minStationLevel.Value;
                RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                if (simpleRequirements == null || simpleRequirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    itemConfig.Requirements = simpleRequirements;

            }

            UpdateHelper.UpdateItemDrop(prefab, new UpdateItemDropOptions()
            {
                maxStackSize = config.maxStackSize.Value,
                health = config.health.Value,
                healthRegen = config.healthRegen.Value,
                stamina = config.stamina.Value,
                eitr = config.eitr.Value,
                burnTime = config.burnTime.Value,
            });

            if (pickablePrefab != null)
            {
                UpdateHelper.UpdatePickable(pickablePrefab, new UpdatePickableOptions()
                {
                    amount = config.amount.Value,
                    respawnTimeMinutes = config.respawnTimeMinutes.Value,
                });
            }

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
