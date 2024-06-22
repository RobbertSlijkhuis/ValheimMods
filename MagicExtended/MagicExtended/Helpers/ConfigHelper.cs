using MagicExtended.Models;
using System;
using UnityEngine;

namespace MagicExtended.Helpers
{
    internal static class ConfigHelper
    {
        public static void PatchStats(GameObject prefab, PatchStatsOptions options)
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
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = options.maxQuality; }
                if (options.movementModifier > -9999f) { itemDrop.m_itemData.m_shared.m_movementModifier = options.movementModifier; }

                if (options.blockPower > -1f) { itemDrop.m_itemData.m_shared.m_blockPower = options.blockPower; }
                if (options.deflectionForce > -1f) { itemDrop.m_itemData.m_shared.m_deflectionForce = options.deflectionForce; }
                if (options.attackForce > -1f) { itemDrop.m_itemData.m_shared.m_attackForce = options.attackForce; }
                if (options.backstabBonus > -1f) { itemDrop.m_itemData.m_shared.m_backstabBonus = options.backstabBonus; }

                if (options.attackEitr > -1f) { itemDrop.m_itemData.m_shared.m_attack.m_attackEitr = options.attackEitr; }
                if (options.secondaryAttackEitr > -1f) { itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackEitr = options.secondaryAttackEitr; }
                if (options.damageBlunt > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_blunt = options.damageBlunt; }
                if (options.damageChop > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_chop = options.damageChop; }
                if (options.damageFire > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_fire = options.damageFire; }
                if (options.damageFrost > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_frost = options.damageFrost; }
                if (options.damageLightning > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_lightning = options.damageLightning; }
                if (options.damagePickaxe > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = options.damagePickaxe; }
                if (options.damagePierce > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_pierce = options.damagePierce; }
                if (options.damageSlash > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_slash = options.damageSlash; }
                if (options.damageSpirit > -1f) { itemDrop.m_itemData.m_shared.m_damages.m_spirit = options.damageSpirit; }

                if (options.eitrRegen > -1f) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = options.eitrRegen; }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not patch stats: " + error);
            }
        }
    }
}
