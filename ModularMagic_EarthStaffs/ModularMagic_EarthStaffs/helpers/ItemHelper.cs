using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
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
                Jotunn.Logger.LogError($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = simpleRequirements;

            UpdateHelper.UpdateItemData(prefab, new UpdateItemDataOptions()
            {
                damageBlunt = config.damageBlunt == null ? 0f : config.damageBlunt.Value,
                damageChop = config.damageChop == null ? 0f : config.damageChop.Value,
                damagePickaxe = config.damagePickaxe == null ? 0f : config.damagePickaxe.Value,
                damagePoison = config.damagePoison == null ? 0f : config.damagePoison.Value,
                damageSpirit = config.damageSpirit == null ? 0f : config.damageSpirit.Value,
                damageBluntPerLevel = config.damageBluntPerLevel == null ? 0f : config.damageBluntPerLevel.Value,
                damageChopPerLevel = config.damageChopPerLevel == null ? 0f : config.damageChopPerLevel.Value,
                damagePickaxePerLevel = config.damagePickaxePerLevel == null ? 0f : config.damagePickaxePerLevel.Value,
                damageSpiritPerLevel = config.damageSpiritPerLevel == null ? 0f : config.damageSpiritPerLevel.Value,
                attackEitr = config.useEitr.Value,
                projectileVelocity = config.projectileVelocity == null ? 0f : config.projectileVelocity.Value,
                projectileAccuracy = config.projectileAccuracy == null ? 0f : config.projectileAccuracy.Value,
                projectileBurst = config.projectileBurst == null ? 0f : config.projectileBurst == null ? 0f : config.projectileBurst.Value,
                weight = config.weight.Value,
                maxDurability = config.maxDurability.Value,
                maxQuality = config.maxQuality.Value,
                movementModifier = config.movementSpeed.Value,
                blockPower = config.blockArmor.Value,
                deflectionForce = config.deflectionForce.Value,
                attackForce = config.attackForce.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
