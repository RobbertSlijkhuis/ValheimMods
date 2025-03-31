using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using UnityEngine;

namespace ModularMagic_Utilities.helpers
{
    internal class ItemHelper
    {
        public static void create(GameObject prefab, UtilitiesConfig config, bool hasDemister = false)
        {
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = config.name.Value;
            itemConfig.Enabled = config.enable.Value;
            itemConfig.CraftingStation = config.craftingStation.Value;
            itemConfig.MinStationLevel = config.minStationLevel.Value;
            RequirementConfig[] simpleRequirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

            if (simpleRequirements == null || simpleRequirements.Length == 0)
                Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
            else
                itemConfig.Requirements = simpleRequirements;

            ItemDrop simpleDrop = prefab.GetComponent<ItemDrop>();
            MagicStatusEffect simpleStatusEffect = ScriptableObject.CreateInstance<MagicStatusEffect>();
            simpleStatusEffect.name = config.magicStatusEffectName;
            simpleStatusEffect.m_name = simpleDrop.m_itemData.m_shared.m_name;
            simpleStatusEffect.m_icon = simpleDrop.m_itemData.GetIcon();
            simpleStatusEffect.SetAll(config.eitr.Value, config.elementalMagic.Value, config.bloodMagic.Value);

            simpleDrop.m_itemData.m_shared.m_equipStatusEffect = simpleStatusEffect;

            ConfigHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
            {
                description = config.description.Value,
                eitrRegen = config.eitrRegen.Value,
                demister = hasDemister ? config.demister.Value : null,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }
    }
}
