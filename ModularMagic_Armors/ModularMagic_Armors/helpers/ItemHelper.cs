using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.StatusEffects;
using ModularMagic_Armors.Types;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, ArmorConfig config)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.Description = config.description.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;
            RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, config.recipeUpgrade.Value, config.recipeMultiplier.Value);

            if (requirements == null || requirements.Length == 0)
                Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = requirements;

            MagicStatusEffect equipStatusEffect = ScriptableObject.CreateInstance<MagicStatusEffect>();
            equipStatusEffect.name = config.magicStatusEffectName;
            equipStatusEffect.m_name = config.name.Value;
            equipStatusEffect.SetAll(config.eitr.Value, config.elementalMagic.Value, config.bloodMagic.Value);

            UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                armor = config.armor.Value,
                armorPerLevel = config.armorPerLevel.Value,
                equipStatusEffect = equipStatusEffect,
                weight = config.weight.Value,
                maxDurability = config.maxDurability.Value,
                maxQuality = config.maxQuality.Value,
                movementSpeed = config.movementSpeed.Value,
                eitrRegen = config.eitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }

        public static void Adjust(GameObject prefab, ArmorConfig config, ArmorSetOptions armorSetOptions = null)
        {
            MagicStatusEffect equipStatusEffect = ScriptableObject.CreateInstance<MagicStatusEffect>();
            equipStatusEffect.name = config.magicStatusEffectName;
            equipStatusEffect.m_name = config.name.Value;
            equipStatusEffect.SetAll(config.eitr.Value, config.elementalMagic.Value, config.bloodMagic.Value);

            UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                name = config.name.Value,
                description = config.description.Value,
                armor = config.armor.Value,
                armorPerLevel = config.armorPerLevel.Value,
                armorSetOptions = armorSetOptions,
                equipStatusEffect = equipStatusEffect,
                weight = config.weight.Value,
                maxDurability = config.maxDurability.Value,
                maxQuality = config.maxQuality.Value,
                movementSpeed = config.movementSpeed.Value,
                eitrRegen = config.eitrRegen.Value,
            });

            RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
            {
                prefab = prefab,
                name = $"Recipe_{prefab.name}",
                enable = config.enable.Value,
                craftingStation = config.craftingStation.Value,
                requiredStationLevel = config.minStationLevel.Value,
                updateType = RecipeUpdateType.ALL,
                requirements = config.recipe.Value,
                upgradeRequirements = config.recipeUpgrade.Value,
                upgradeMultiplier = config.recipeMultiplier.Value,
            });
        }

        public static void ResetToVanilla(GameObject prefab, UpdateItemDropStatsOptions options, RecipeSnapShot recipe)
        {
            UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                name = options.name,
                description = options.description,
                armor = options.armor,
                armorPerLevel = options.armorPerLevel,
                armorSetOptions = options.armorSetOptions,
                equipStatusEffect = options.equipStatusEffect,
                weight = options.weight,
                maxDurability = options.maxDurability,
                maxQuality = options.maxQuality,
                movementSpeed = options.movementSpeed,
                eitrRegen = options.eitrRegen,
            });

            RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
            {
                prefab = prefab,
                name = $"Recipe_{prefab.name}",
                updateType = RecipeUpdateType.FROM_RECIPE,
                fromRecipe = recipe,
            });
        }
    }
}
