using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_FireStaffs.Models;
using UnityEngine;

namespace ModularMagic_FireStaffs.Helpers
{
    internal class ItemHelper
    {
        public static void CreateStaff(GameObject prefab, StaffConfig config)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, config.recipeUpgrade.Value, config.recipeMultiplier.Value);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = simpleRequirements;

            UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                maxQuality = config.maxQuality.Value,
                movementModifier = config.movementSpeed.Value,
                blockPower = config.blockArmor.Value,
                deflectionForce = config.deflectionForce.Value,
                attackForce = config.attackForce.Value,
                damageBlunt = config.damageBlunt.Value,
                damageFire = config.damageFire.Value,
                attackEitr = config.useEitr.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
