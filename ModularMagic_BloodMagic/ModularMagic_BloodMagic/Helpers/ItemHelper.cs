using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_BloodMagic.Helpers
{
    internal class ItemHelper
    {
        public static void Create(GameObject prefab, WeaponConfig config)
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

            UpdateItemData(prefab, new UpdateItemDataOptions()
            {
                damageBlunt          = config.damageBlunt?.Value ?? 0f,
                damageChop           = config.damageChop?.Value ?? 0f,
                damageFire           = config.damageFire?.Value ?? 0f,
                damageFrost          = config.damageFrost?.Value ?? 0f,
                damageGeneral        = config.damageGeneral?.Value ?? 0f,
                damageLightning      = config.damageLightning?.Value ?? 0f,
                damagePickaxe        = config.damagePickaxe?.Value ?? 0f,
                damagePierce         = config.damagePierce?.Value ?? 0f,
                damagePoison         = config.damagePoison?.Value ?? 0f,
                damageSlash          = config.damageSlash?.Value ?? 0f,
                damageSpirit         = config.damageSpirit?.Value ?? 0f,

                damageBluntPerLevel     = config.damageBluntPerLevel?.Value ?? 0f,
                damageChopPerLevel      = config.damageChopPerLevel?.Value ?? 0f,
                damageFirePerLevel      = config.damageFirePerLevel?.Value ?? 0f,
                damageFrostPerLevel     = config.damageFrostPerLevel?.Value ?? 0f,
                damageGeneralPerLevel   = config.damageGeneralPerLevel?.Value ?? 0f,
                damageLightningPerLevel = config.damageLightningPerLevel?.Value ?? 0f,
                damagePickaxePerLevel   = config.damagePickaxePerLevel?.Value ?? 0f,
                damagePiercePerLevel    = config.damagePiercePerLevel?.Value ?? 0f,
                damagePoisonPerLevel    = config.damagePoisonPerLevel?.Value ?? 0f,
                damageSlashPerLevel     = config.damageSlashPerLevel?.Value ?? 0f,
                damageSpiritPerLevel    = config.damageSpiritPerLevel?.Value ?? 0f,

                attackForce      = config.attackForce.Value,
                blockPower       = config.blockArmor.Value,
                deflectionForce  = config.deflectionForce.Value,
                backstabBonus    = config.backStab.Value,

                maxDurability    = config.maxDurability.Value,
                movementModifier = config.movementSpeed.Value,
                weight           = config.weight.Value,

                attackEitr         = config.attackEitr?.Value ?? 0f,
                projectileAccuracy = config.projectileAccuracy?.Value ?? 0f,
                projectileBurst    = config.projectileBurst?.Value ?? 0f,
                projectileVelocity = config.projectileVelocity?.Value ?? 0f,
            });

            ItemManager.Instance.AddItem(new CustomItem(prefab, true, itemConfig));
        }

        public static void UpdateItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
                throw new ArgumentException($"Prefab '{prefab.name}' does not have an ItemDrop component.", nameof(prefab));

            UpdateItemData(itemDrop.m_itemData, options);
        }

        public static void UpdateItemData(ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new ArgumentNullException(nameof(itemData));

            if (options.mainAttack != null) { itemData.m_shared.m_attack = options.mainAttack.m_shared.m_attack; }

            if (options.name != null) { itemData.m_shared.m_name = options.name; }
            if (options.description != null) { itemData.m_shared.m_description = options.description; }

            if (options.damageBlunt != null) { itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
            if (options.damageFire != null) { itemData.m_shared.m_damages.m_fire = (float)options.damageFire; }
            if (options.damageFrost != null) { itemData.m_shared.m_damages.m_frost = (float)options.damageFrost; }
            if (options.damageGeneral != null) { itemData.m_shared.m_damages.m_damage = (float)options.damageGeneral; }
            if (options.damageLightning != null) { itemData.m_shared.m_damages.m_lightning = (float)options.damageLightning; }
            if (options.damagePickaxe != null) { itemData.m_shared.m_damages.m_pickaxe = (float)options.damagePickaxe; }
            if (options.damagePierce != null) { itemData.m_shared.m_damages.m_pierce = (float)options.damagePierce; }
            if (options.damagePoison != null) { itemData.m_shared.m_damages.m_poison = (float)options.damagePoison; }
            if (options.damagePoison != null) { itemData.m_shared.m_damages.m_poison = (float)options.damagePoison; }
            if (options.damageSlash != null) { itemData.m_shared.m_damages.m_slash = (float)options.damageSlash; }
            if (options.damageSpirit != null) { itemData.m_shared.m_damages.m_spirit = (float)options.damageSpirit; }

            if (options.damageBluntPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_blunt = (float)options.damageBluntPerLevel; }
            if (options.damageChopPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_chop = (float)options.damageChopPerLevel; }
            if (options.damageFirePerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_fire = (float)options.damageFirePerLevel; }
            if (options.damageFrostPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_frost = (float)options.damageFrostPerLevel; }
            if (options.damageGeneralPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_damage = (float)options.damageGeneralPerLevel; }
            if (options.damageLightningPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_lightning = (float)options.damageLightningPerLevel; }
            if (options.damagePickaxePerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_pickaxe = (float)options.damagePickaxePerLevel; }
            if (options.damagePiercePerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_pierce = (float)options.damagePiercePerLevel; }
            if (options.damagePoisonPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_poison = (float)options.damagePoisonPerLevel; }
            if (options.damageSlashPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_slash = (float)options.damageSlashPerLevel; }
            if (options.damageSpiritPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_spirit = (float)options.damageSpiritPerLevel; }

            if (options.equipStatusEffect == null || options.equipStatusEffect.name != "empty_MMBM") { itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }
            if (options.attackEitr != null)    { itemData.m_shared.m_attack.m_attackEitr    = (float)options.attackEitr; }
            if (options.attackStamina != null) { itemData.m_shared.m_attack.m_attackStamina = (float)options.attackStamina; }
            if (options.projectileVelocity != null) { itemData.m_shared.m_attack.m_projectileVel = (float)options.projectileVelocity; }
            if (options.projectileAccuracy != null) { itemData.m_shared.m_attack.m_projectileAccuracy = (float)options.projectileAccuracy; }
            if (options.projectileBurst != null) { itemData.m_shared.m_attack.m_burstInterval = (float)options.projectileBurst; }
            if (options.weight != null) { itemData.m_shared.m_weight = (float)options.weight; }
            if (options.maxDurability != null) { itemData.m_shared.m_maxDurability = (float)options.maxDurability; }
            if (options.movementModifier != null) { itemData.m_shared.m_movementModifier = (float)options.movementModifier; }
            if (options.blockPower != null) { itemData.m_shared.m_blockPower = (float)options.blockPower; }
            if (options.timedBlockBonus != null) { itemData.m_shared.m_timedBlockBonus = (float)options.timedBlockBonus; }
            if (options.deflectionForce != null) { itemData.m_shared.m_deflectionForce = (float)options.deflectionForce; }
            if (options.attackForce != null) { itemData.m_shared.m_attackForce = (float)options.attackForce; }
            if (options.backstabBonus != null) { itemData.m_shared.m_backstabBonus = (float)options.backstabBonus; }

            if (options.secondaryAttackEitr != null) { itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
            if (options.secondaryLaunchAngle != null) { itemData.m_shared.m_secondaryAttack.m_launchAngle = (float)options.secondaryLaunchAngle; }
            if (options.secondaryProjectileVelocity != null) { itemData.m_shared.m_secondaryAttack.m_projectileVel = (float)options.secondaryProjectileVelocity; }
            if (options.secondaryProjectileAccuracy != null) { itemData.m_shared.m_secondaryAttack.m_projectileAccuracy = (float)options.secondaryProjectileAccuracy; }
            if (options.secondaryAttack != null) { itemData.m_shared.m_secondaryAttack = options.secondaryAttack.m_shared.m_attack; }
        }
    }
}
