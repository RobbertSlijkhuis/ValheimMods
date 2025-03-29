#nullable enable

using MagicExtended.Models;
using System;
using UnityEngine;

namespace MagicExtended.Helpers
{
    internal class ConfigHelper
    {
        public static void UpdateItemDropStats(GameObject prefab, UpdateItemDropStatsOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                if (options.name != null) { itemDrop.m_itemData.m_shared.m_name = options.name; }
                if (options.description != null) { itemDrop.m_itemData.m_shared.m_description = options.description; }
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = (int)options.maxQuality; }
                if (options.movementModifier != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementModifier; }

                if (options.blockPower != null) { itemDrop.m_itemData.m_shared.m_blockPower = (float)options.blockPower; }
                if (options.deflectionForce != null) { itemDrop.m_itemData.m_shared.m_deflectionForce = (float)options.deflectionForce; }
                if (options.attackForce != null) { itemDrop.m_itemData.m_shared.m_attackForce = (float)options.attackForce; }
                if (options.backstabBonus != null) { itemDrop.m_itemData.m_shared.m_backstabBonus = (float)options.backstabBonus; }

                if (options.attackEitr != null) { itemDrop.m_itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
                if (options.secondaryAttackEitr != null) { itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
                if (options.damageBlunt != null) { itemDrop.m_itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
                if (options.damageChop != null) { itemDrop.m_itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
                if (options.damageFire != null) { itemDrop.m_itemData.m_shared.m_damages.m_fire = (float)options.damageFire; }
                if (options.damageFrost != null) { itemDrop.m_itemData.m_shared.m_damages.m_frost = (float)options.damageFrost; }
                if (options.damageLightning != null) { itemDrop.m_itemData.m_shared.m_damages.m_lightning = (float)options.damageLightning; }
                if (options.damagePickaxe != null) { itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = (float)options.damagePickaxe; }
                if (options.damagePierce != null) { itemDrop.m_itemData.m_shared.m_damages.m_pierce = (float)options.damagePierce; }
                if (options.damageSlash != null) { itemDrop.m_itemData.m_shared.m_damages.m_slash = (float)options.damageSlash; }
                if (options.damageSpirit != null) { itemDrop.m_itemData.m_shared.m_damages.m_spirit = (float)options.damageSpirit; }

                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + error);
            }
        }
    }
}
