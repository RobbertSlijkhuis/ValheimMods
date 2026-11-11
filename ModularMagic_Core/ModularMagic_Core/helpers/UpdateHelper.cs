using ModularMagic_Core.Components;
using ModularMagic_Core.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Applies values from the options to the parts of a prefab or an item: its item data, its projectile, its SpawnAbility, its area
    /// hit (MMAoe) or its summoned creature. A value that is null in the options is left as it is.
    /// </summary>
    public class UpdateHelper
    {
        /// <summary>
        /// Applies the options to the item data of the prefab, not to an item in a player's inventory.
        /// </summary>
        public static void UpdateItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                throw new Exception("ItemDrop is null");

            UpdateItemData(itemDrop.m_itemData, options);
        }

        /// <summary>
        /// Applies the options to the item data of an item (for example the staff in the hands of the player).
        /// </summary>
        public static void UpdateItemData(ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");

            // Copy the attack, otherwise the item shares the attack object with the prefab (or the cone item) and the changes below leak into other staffs
            if (options.mainAttack != null) { itemData.m_shared.m_attack = options.mainAttack.m_shared.m_attack.Clone(); }

            if (options.name != null) { itemData.m_shared.m_name = options.name; }
            if (options.description != null) { itemData.m_shared.m_description = options.description; }
            if (options.maxQuality != null) { itemData.m_shared.m_maxQuality = (int)options.maxQuality; }

            if (options.damageBlunt != null) { itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
            if (options.damageFire != null) { itemData.m_shared.m_damages.m_fire = (float)options.damageFire; }
            if (options.damageFrost != null) { itemData.m_shared.m_damages.m_frost = (float)options.damageFrost; }
            if (options.damageGeneral != null) { itemData.m_shared.m_damages.m_damage = (float)options.damageGeneral; }
            if (options.damageLightning != null) { itemData.m_shared.m_damages.m_lightning = (float)options.damageLightning; }
            if (options.damagePickaxe != null) { itemData.m_shared.m_damages.m_pickaxe = (float)options.damagePickaxe; }
            if (options.damagePierce != null) { itemData.m_shared.m_damages.m_pierce = (float)options.damagePierce; }
            if (options.damagePoison != null) { itemData.m_shared.m_damages.m_poison = (float)options.damagePoison; }
            if (options.damageSlash != null) { itemData.m_shared.m_damages.m_slash = (float)options.damageSlash; }
            if (options.damageSpirit != null) { itemData.m_shared.m_damages.m_spirit = (float)options.damageSpirit; }
            if (options.damageBluntPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_blunt = (float)options.damageBluntPerLevel; }
            if (options.damageChopPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_chop = (float)options.damageChopPerLevel; }
            if (options.damageGeneralPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_damage = (float)options.damageGeneralPerLevel; }
            if (options.damagePickaxePerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_pickaxe = (float)options.damagePickaxePerLevel; }
            if (options.damagePiercePerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_pierce = (float)options.damagePiercePerLevel; }
            if (options.damagePoisonPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_poison = (float)options.damagePoisonPerLevel; }
            if (options.damageSlashPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_slash = (float)options.damageSlashPerLevel; }
            if (options.damageSpiritPerLevel != null) { itemData.m_shared.m_damagesPerLevel.m_spirit = (float)options.damageSpiritPerLevel; }

            if (options.equipStatusEffect != null) { itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }

            if (options.attackEitr != null) { itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
            if (options.attackProjectile != null) { itemData.m_shared.m_attack.m_attackProjectile = options.attackProjectile; }
            if (options.projectileVelocity != null) { itemData.m_shared.m_attack.m_projectileVel = (float)options.projectileVelocity; }
            if (options.projectileAccuracy != null) { itemData.m_shared.m_attack.m_projectileAccuracy = (float)options.projectileAccuracy; }
            if (options.burstInterval != null) { itemData.m_shared.m_attack.m_burstInterval = (float)options.burstInterval; }
            if (options.launchAngle != null) { itemData.m_shared.m_attack.m_launchAngle = (float)options.launchAngle; }
            if (options.damageMultiplier != null) { itemData.m_shared.m_attack.m_damageMultiplier = (float)options.damageMultiplier; }
            if (options.forceMultiplier != null) { itemData.m_shared.m_attack.m_forceMultiplier = (float)options.forceMultiplier; }

            if (options.weight != null) { itemData.m_shared.m_weight = (float)options.weight; }
            if (options.maxDurability != null) { itemData.m_shared.m_maxDurability = (float)options.maxDurability; }
            if (options.movementModifier != null) { itemData.m_shared.m_movementModifier = (float)options.movementModifier; }
            if (options.blockPower != null) { itemData.m_shared.m_blockPower = (float)options.blockPower; }
            if (options.timedBlockBonus != null) { itemData.m_shared.m_timedBlockBonus = (float)options.timedBlockBonus; }
            if (options.deflectionForce != null) { itemData.m_shared.m_deflectionForce = (float)options.deflectionForce; }
            if (options.attackForce != null) { itemData.m_shared.m_attackForce = (float)options.attackForce; }
            if (options.backstabBonus != null) { itemData.m_shared.m_backstabBonus = (float)options.backstabBonus; }
            if (options.eitrRegen != null) { itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }

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
            if (options.damageBlunt != null) { projectile.m_damage.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { projectile.m_damage.m_chop = (float)options.damageChop; }
            if (options.damageFire != null) { projectile.m_damage.m_fire = (float)options.damageFire; }
            if (options.damageFrost != null) { projectile.m_damage.m_frost = (float)options.damageFrost; }
            if (options.damageGeneral != null) { projectile.m_damage.m_damage = (float)options.damageGeneral; }
            if (options.damageLightning != null) { projectile.m_damage.m_lightning = (float)options.damageLightning; }
            if (options.damagePickaxe != null) { projectile.m_damage.m_pickaxe = (float)options.damagePickaxe; }
            if (options.damagePierce != null) { projectile.m_damage.m_pierce = (float)options.damagePierce; }
            if (options.damagePoison != null) { projectile.m_damage.m_poison = (float)options.damagePoison; }
            if (options.damageSpirit != null) { projectile.m_damage.m_spirit = (float)options.damageSpirit; }
            if (options.attackForce != null) { projectile.m_attackForce = (float)options.attackForce; }
        }

        /// <summary>
        /// The prefab that has the SpawnAbility itself, for example the script prefab that a projectile spawns when it hits.
        /// </summary>
        public static void UpdateSpawnAbility(GameObject prefab, UpdateSpawnAbilityOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            SpawnAbility spawnAbility = prefab.GetComponent<SpawnAbility>();

            if (spawnAbility == null)
                throw new Exception("SpawnAbility is null");

            if (options.minToSpawn != null) { spawnAbility.m_minToSpawn = (int)options.minToSpawn; }
            if (options.maxToSpawn != null) { spawnAbility.m_maxToSpawn = (int)options.maxToSpawn; }
            if (options.maxSpawns != null) { spawnAbility.m_maxSpawned = (int)options.maxSpawns; }
            if (options.spawnRadius != null) { spawnAbility.m_spawnRadius = (float)options.spawnRadius; }
            if (options.projectileAccuracy != null) { spawnAbility.m_projectileAccuracy = (float)options.projectileAccuracy; }
            if (options.spawnDelay != null) { spawnAbility.m_spawnDelay = (float)options.spawnDelay; }
        }

        /// <summary>
        /// The prefab an attack spawns (the projectile of its main attack). For a wrapper prefab that holds a secondary attack this is
        /// the script prefab with the SpawnAbility of the attack.
        /// </summary>
        public static GameObject GetAttackProjectile(GameObject attackPrefab)
        {
            if (attackPrefab == null)
                throw new Exception("Prefab is null");

            ItemDrop itemDrop = attackPrefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                throw new Exception("ItemDrop is null");

            GameObject projectile = itemDrop.m_itemData.m_shared.m_attack.m_attackProjectile;

            if (projectile == null)
                throw new Exception("Attack prefab is null");

            return projectile;
        }

        /// <summary>
        /// The area hit of an attack (see MMAoe). Its damage and attack force come from the staff, so only the radius can be set.
        /// </summary>
        public static void UpdateAoe(GameObject prefab, UpdateAoeOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            MMAoe aoe = prefab.GetComponent<MMAoe>();

            if (aoe == null)
                throw new Exception("MMAoe is null");

            if (options.aoe != null) { aoe.radius = (float)options.aoe; }
        }

        public static void UpdateHumanoid(GameObject prefab, UpdateHumanoidOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Humanoid humanoid = prefab.GetComponent<Humanoid>();

            if (humanoid == null)
                throw new Exception("Humanoid is null");

            if (options.health != null) { humanoid.m_health = (float)options.health; }
        }

        /// <summary>
        /// The item data of the attack (the weapon) of a summoned creature.
        /// </summary>
        public static void UpdateHumanoidAttackItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Humanoid humanoid = prefab.GetComponent<Humanoid>();

            if (humanoid == null)
                throw new Exception("Humanoid is null");

            GameObject attackPrefab = humanoid.m_randomWeapon[0];

            if (attackPrefab == null)
                throw new Exception("Attack prefab is null");

            UpdateItemData(attackPrefab, options);
        }
    }
}
