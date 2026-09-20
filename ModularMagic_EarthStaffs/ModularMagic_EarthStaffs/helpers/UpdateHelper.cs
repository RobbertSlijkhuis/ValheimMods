using ModularMagic_EarthStaffs.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class UpdateHelper
    {

        public static void UpdateItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");


            ItemData itemData = prefab.GetComponent<ItemDrop>().m_itemData;
            UpdateItemData(itemData, options);
            UpdateItemDataInHand(itemData, options);
        }

        public static void UpdateItemData(ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");
            
            // Copy the attack, otherwise the item shares the attack object with the prefab (or the cone item) and the changes below leak into other staffs
            if (options.mainAttack != null) { itemData.m_shared.m_attack = options.mainAttack.m_shared.m_attack.Clone(); }

            if (options.name != null) { itemData.m_shared.m_name = options.name; }
            if (options.description != null) { itemData.m_shared.m_description = options.description; }
            if (options.damageBlunt != null) { itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
            if (options.damageGeneral != null) { itemData.m_shared.m_damages.m_damage = (float)options.damageGeneral; }
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
            if (options.equipStatusEffect == null || options.equipStatusEffect.name != "empty_MMES") { itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }
            if (options.attackEitr != null) { itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
            if (options.attackProjectile != null) { itemData.m_shared.m_attack.m_attackProjectile = options.attackProjectile; }
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

            // Assign the attack first, the values below are then set on the copy instead of being replaced by it
            if (options.secondaryAttack != null) { itemData.m_shared.m_secondaryAttack = options.secondaryAttack.m_shared.m_attack.Clone(); }

            if (options.secondaryAttackEitr != null) { itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
            if (options.secondaryLaunchAngle != null) { itemData.m_shared.m_secondaryAttack.m_launchAngle = (float)options.secondaryLaunchAngle; }
            if (options.secondaryProjectileVelocity != null) { itemData.m_shared.m_secondaryAttack.m_projectileVel = (float)options.secondaryProjectileVelocity; }
            if (options.secondaryProjectileAccuracy != null) { itemData.m_shared.m_secondaryAttack.m_projectileAccuracy = (float)options.secondaryProjectileAccuracy; }
        }

        public static void UpdateItemDataInHand(ItemData itemData, UpdateItemDataOptions options)
        {
            if (Player.m_localPlayer == null)
                return;

            ItemData weaponItemData = Player.m_localPlayer.GetCurrentWeapon();

            if (weaponItemData.m_dropPrefab.name != ModularMagic_EarthStaffs.prefabs.StaffEarth1.name && 
                weaponItemData.m_dropPrefab.name != ModularMagic_EarthStaffs.prefabs.StaffEarth2.name &&
                weaponItemData.m_dropPrefab.name != ModularMagic_EarthStaffs.prefabs.StaffEarth3.name
            )
                return;

            UpdateItemData(weaponItemData, options);
        }

        public static void UpdateProjectile(GameObject prefab, UpdateProjectileOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Projectile projectile = prefab.GetComponent<Projectile>();
            UpdateProjectile(projectile, options);
        }

        public static void UpdateProjectile(Projectile projectile, UpdateProjectileOptions options)
        {
            if (projectile == null)
                throw new Exception("Projectile is null");

            if (options.aoe != null) { projectile.m_aoe = (float)options.aoe; }
            if (options.damageBlunt != null) { projectile.m_damage.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { projectile.m_damage.m_chop = (float)options.damageChop; }
            if (options.damagePickaxe != null) { projectile.m_damage.m_pickaxe = (float)options.damagePickaxe; }
            if (options.damagePoison != null) { projectile.m_damage.m_poison = (float)options.damagePoison; }
            if (options.damageSpirit != null) { projectile.m_damage.m_spirit = (float)options.damageSpirit; }
            if (options.attackForce != null) { projectile.m_attackForce = (float)options.attackForce; }
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

        public static void UpdateSpawnAbility(GameObject prefab, UpdateSpawnAbilityOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                throw new Exception("ItemDrop is null");

            GameObject attackPrefab = itemDrop.m_itemData.m_shared.m_attack.m_attackProjectile;

            if (attackPrefab == null)
                throw new Exception("Attack prefab is null");

            SpawnAbility spawnAbility = attackPrefab.GetComponent<SpawnAbility>();

            if (spawnAbility == null)
                throw new Exception("SpawnAbility is null");

            if (options.minToSpawn != null) { spawnAbility.m_minToSpawn = (int)options.minToSpawn; }
            if (options.maxToSpawn != null) { spawnAbility.m_maxToSpawn = (int)options.maxToSpawn; }
            if (options.maxSpawns != null) { spawnAbility.m_maxSpawned = (int)options.maxSpawns; }
            if (options.spawnRadius != null) { spawnAbility.m_spawnRadius = (int)options.spawnRadius; }

        }

        public static void UpdateAoe(GameObject prefab, UpdateAoeOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Aoe aoe = prefab.GetComponent<Aoe>();

            if (aoe == null)
                throw new Exception("Aoe is null");

            if (options.aoe != null) { aoe.m_radius = (float)options.aoe; }
            if (options.damageBlunt != null) { aoe.m_damage.m_blunt = (float)options.damageBlunt; }
            if (options.damageChop != null) { aoe.m_damage.m_chop = (float)options.damageChop; }
            if (options.damagePickaxe != null) { aoe.m_damage.m_pickaxe = (float)options.damagePickaxe; }
            if (options.damagePierce != null) { aoe.m_damage.m_pierce = (float)options.damagePierce; }
            if (options.damagePoison != null) { aoe.m_damage.m_poison = (float)options.damagePoison; }
            if (options.damageSpirit != null) { aoe.m_damage.m_spirit = (float)options.damageSpirit; }
            if (options.attackForce != null) { aoe.m_attackForce = (float)options.attackForce; }
        }
    }
}
