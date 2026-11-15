#nullable enable

using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The values that are applied to the item data of an item (the prefab of a staff, or the staff in the hand of a player).
    /// A value that is null is left as it is.
    /// </summary>
    public class UpdateItemDataOptions
    {
        public string? name = null;
        public string? description = null;
        public int? maxQuality = null;

        // The item data of the staff prefab, its main attack is copied onto the equipped staff
        public ItemData? mainAttack = null;
        public StatusEffect? equipStatusEffect = null;
        // The item data of the wrapper prefab that holds the secondary attack, its attack is copied onto the equipped staff
        public ItemData? secondaryAttack = null;

        public float? damageBlunt = null;
        public float? damageChop = null;
        public float? damageFire = null;
        public float? damageFrost = null;
        public float? damageGeneral = null;
        public float? damageLightning = null;
        public float? damagePickaxe = null;
        public float? damagePierce = null;
        public float? damagePoison = null;
        public float? damageSlash = null;
        public float? damageSpirit = null;
        public float? damageBluntPerLevel = null;
        public float? damageChopPerLevel = null;
        public float? damageFirePerLevel = null;
        public float? damageFrostPerLevel = null;
        public float? damageGeneralPerLevel = null;
        public float? damageLightningPerLevel = null;
        public float? damagePickaxePerLevel = null;
        public float? damagePiercePerLevel = null;
        public float? damagePoisonPerLevel = null;
        public float? damageSlashPerLevel = null;
        public float? damageSpiritPerLevel = null;

        public float? attackEitr = null;
        public float? attackStamina = null;
        public GameObject? attackProjectile = null;
        public float? projectileVelocity = null;
        public float? projectileAccuracy = null;
        // The time between two bursts of the main attack (m_burstInterval)
        public float? burstInterval = null;
        public float? launchAngle = null;
        public float? damageMultiplier = null;
        public float? forceMultiplier = null;
        public float? secondaryAttackEitr = null;

        public float? weight = null;
        public float? maxDurability = null;
        public float? movementModifier = null;
        public float? blockPower = null;
        public float? timedBlockBonus = null;
        public float? deflectionForce = null;
        public float? attackForce = null;
        public float? backstabBonus = null;
        public float? eitrRegen = null;
    }
}
