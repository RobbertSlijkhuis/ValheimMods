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
                
                itemConfig.Enabled = config.enable.Value;
                itemConfig.Name = config.name.Value;
                itemConfig.Description = config.description.Value;
                itemConfig.CraftingStation = config.craftingStation.Value;
                itemConfig.MinStationLevel = config.minStationLevel.Value;
                RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                if (requirements == null || requirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    itemConfig.Requirements = requirements;

                ItemDrop simpleDrop = prefab.GetComponent<ItemDrop>();
                MagicStatusEffect equipStatusEffect = ScriptableObject.CreateInstance<MagicStatusEffect>();
                equipStatusEffect.name = config.magicStatusEffectName;
                equipStatusEffect.m_name = simpleDrop.m_itemData.m_shared.m_name;
                equipStatusEffect.m_icon = simpleDrop.m_itemData.GetIcon();
                equipStatusEffect.SetAll(config.eitr.Value, config.elementalMagic.Value, config.bloodMagic.Value);

                UpdateHelper.UpdateItemDropStats(prefab, new UpdateItemDropOptions()
                {
                    equipStatusEffect = equipStatusEffect,
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
