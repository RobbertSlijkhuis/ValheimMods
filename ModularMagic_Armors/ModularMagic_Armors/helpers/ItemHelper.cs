using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.StatusEffects;
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
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, config.recipeUpgrade.Value, config.recipeMultiplier.Value);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = simpleRequirements;

            ItemDrop simpleDrop = prefab.GetComponent<ItemDrop>();
            MagicStatusEffect simpleStatusEffect = ScriptableObject.CreateInstance<MagicStatusEffect>();
            simpleStatusEffect.name = config.magicStatusEffectName;
            simpleStatusEffect.m_name = config.name.Value;
            simpleStatusEffect.SetAll(config.eitr.Value, config.elementalMagic.Value, config.bloodMagic.Value);
            simpleDrop.m_itemData.m_shared.m_equipStatusEffect = simpleStatusEffect;

            UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                description = config.description.Value,
                armor = config.armor.Value,
                armorPerLevel = config.armorPerLevel.Value,
                weight = config.weight.Value,
                maxDurability = config.maxDurability.Value,
                maxQuality = config.maxQuality.Value,
                movementSpeed = config.movementSpeed.Value,
                eitrRegen = config.eitrRegen.Value,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
