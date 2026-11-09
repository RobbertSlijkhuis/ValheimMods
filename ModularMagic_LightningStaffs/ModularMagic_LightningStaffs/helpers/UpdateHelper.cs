using ModularMagic_LightningStaffs.Models;
using System;
using UnityEngine;
using MMAoe = ModularMagic_Core.Components.MMAoe;

namespace ModularMagic_LightningStaffs.Helpers
{
    internal class UpdateHelper
    {
        /// <summary>
        /// Applies the options to the item data of an item (for example the staff in the hands of the player), not to its prefab.
        /// </summary>
        public static void UpdateItemData(ItemDrop.ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");

            // Copy the attack, otherwise the item shares the attack object with the prefab and the changes below leak into other staffs
            if (options.mainAttack != null) { itemData.m_shared.m_attack = options.mainAttack.m_shared.m_attack.Clone(); }

            if (options.equipStatusEffect != null) { itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }

            if (options.attackEitr != null) { itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
            if (options.projectileVelocity != null) { itemData.m_shared.m_attack.m_projectileVel = (float)options.projectileVelocity; }
            if (options.projectileAccuracy != null) { itemData.m_shared.m_attack.m_projectileAccuracy = (float)options.projectileAccuracy; }
            if (options.launchAngle != null) { itemData.m_shared.m_attack.m_launchAngle = (float)options.launchAngle; }
            if (options.burstInterval != null) { itemData.m_shared.m_attack.m_burstInterval = (float)options.burstInterval; }
            if (options.damageMultiplier != null) { itemData.m_shared.m_attack.m_damageMultiplier = (float)options.damageMultiplier; }
            if (options.forceMultiplier != null) { itemData.m_shared.m_attack.m_forceMultiplier = (float)options.forceMultiplier; }

            // Assign the attack first, the eitr below is then set on the copy instead of being replaced by it
            if (options.secondaryAttack != null) { itemData.m_shared.m_secondaryAttack = options.secondaryAttack.m_shared.m_attack.Clone(); }
            if (options.secondaryAttackEitr != null) { itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
        }

        public static void UpdateProjectile(GameObject prefab, UpdateProjectileOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Projectile projectile = prefab.GetComponent<Projectile>();

            if (projectile == null)
                throw new Exception("Projectile is null");

            if (options.aoe != null) { projectile.m_aoe = (float)options.aoe; }
            if (options.damageLightning != null) { projectile.m_damage.m_lightning = (float)options.damageLightning; }
            if (options.damageChop != null) { projectile.m_damage.m_chop = (float)options.damageChop; }
            if (options.damagePickaxe != null) { projectile.m_damage.m_pickaxe = (float)options.damagePickaxe; }
            if (options.attackForce != null) { projectile.m_attackForce = (float)options.attackForce; }
        }

        // The prefab that has the SpawnAbility itself, for example the script prefab that a projectile spawns when it hits
        public static void UpdateSpawnAbility(GameObject prefab, UpdateSpawnAbilityOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            SpawnAbility spawnAbility = prefab.GetComponent<SpawnAbility>();

            if (spawnAbility == null)
                throw new Exception("SpawnAbility is null");

            if (options.minToSpawn != null) { spawnAbility.m_minToSpawn = (int)options.minToSpawn; }
            if (options.maxToSpawn != null) { spawnAbility.m_maxToSpawn = (int)options.maxToSpawn; }
            if (options.spawnRadius != null) { spawnAbility.m_spawnRadius = (float)options.spawnRadius; }
            if (options.projectileAccuracy != null) { spawnAbility.m_projectileAccuracy = (float)options.projectileAccuracy; }
            if (options.spawnDelay != null) { spawnAbility.m_spawnDelay = (float)options.spawnDelay; }
        }

        // The area hit of an attack (see MMAoe). Its damage and attack force come from the staff, so only the radius can be set
        public static void UpdateAoe(GameObject prefab, UpdateAoeOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            MMAoe aoe = prefab.GetComponent<MMAoe>();

            if (aoe == null)
                throw new Exception("MMAoe is null");

            if (options.aoe != null) { aoe.radius = (float)options.aoe; }
        }

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
                if (options.projectileVelocity != null) { itemDrop.m_itemData.m_shared.m_attack.m_projectileVel = (float)options.projectileVelocity; }
                if (options.projectileAccuracy != null) { itemDrop.m_itemData.m_shared.m_attack.m_projectileAccuracy = (float)options.projectileAccuracy; }
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
