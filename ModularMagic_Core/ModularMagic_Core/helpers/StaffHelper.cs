using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Models;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Creates the items of a staff mod.
    /// </summary>
    public static class StaffHelper
    {
        /// <summary>
        /// Registers a staff as an item with the recipe of its config, and sets the stats of its config on the prefab. A stat the staff has no
        /// config entry for is left as it is on the prefab.
        /// </summary>
        public static void CreateStaff(GameObject prefab, StaffConfig config)
        {
            Jotunn.Configs.ItemConfig itemConfig = new Jotunn.Configs.ItemConfig();
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

            UpdateHelper.UpdateItemData(prefab, new UpdateItemDataOptions()
            {
                maxQuality = config.maxQuality?.Value,
                weight = config.weight?.Value,
                maxDurability = config.maxDurability?.Value,
                movementModifier = config.movementSpeed?.Value,
                damageBlunt = config.damageBlunt?.Value,
                damageBluntPerLevel = config.damageBluntPerLevel?.Value,
                damageChop = config.damageChop?.Value,
                damageChopPerLevel = config.damageChopPerLevel?.Value,
                damageFire = config.damageFire?.Value,
                damageFrost = config.damageFrost?.Value,
                damageLightning = config.damageLightning?.Value,
                damagePickaxe = config.damagePickaxe?.Value,
                damagePickaxePerLevel = config.damagePickaxePerLevel?.Value,
                damagePierce = config.damagePierce?.Value,
                damagePiercePerLevel = config.damagePiercePerLevel?.Value,
                damagePoison = config.damagePoison?.Value,
                damagePoisonPerLevel = config.damagePoisonPerLevel?.Value,
                damageSlash = config.damageSlash?.Value,
                damageSlashPerLevel = config.damageSlashPerLevel?.Value,
                damageSpirit = config.damageSpirit?.Value,
                damageSpiritPerLevel = config.damageSpiritPerLevel?.Value,
                blockPower = config.blockArmor?.Value,
                deflectionForce = config.deflectionForce?.Value,
                attackForce = config.attackForce?.Value,
                backstabBonus = config.backStab?.Value,
                attackEitr = config.useEitr?.Value,
                attackStamina = config.attackStamina?.Value,
                projectileVelocity = config.projectileVelocity?.Value,
                projectileAccuracy = config.projectileAccuracy?.Value,
                burstInterval = config.projectileBurst?.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
