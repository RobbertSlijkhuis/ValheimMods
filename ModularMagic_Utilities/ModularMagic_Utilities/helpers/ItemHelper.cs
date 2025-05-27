using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, UtilitiesConfig config, bool isLantern = false)
        {
            try
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

                UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropStatsOptions()
                {
                    description = config.description.Value,
                    weight = config.weight.Value,
                    eitrRegen = config.eitrRegen.Value,
                    demister = isLantern ? config.demister.Value : null,
                });

                ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create item: " + e);
            }
        }
    }
}
