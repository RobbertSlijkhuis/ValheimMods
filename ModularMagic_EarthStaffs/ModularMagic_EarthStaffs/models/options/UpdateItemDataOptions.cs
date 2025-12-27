#nullable enable
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Models
{
    internal class UpdateItemDataOptions
    {
        public string? name = null;
        public string? description = null;
        public float? damageBlunt = null;
        public float? damageChop = null;
        public float? damagePickaxe = null;
        public float? damagePierce = null;
        public float? damagePoison = null;
        public float? damageSpirit = null;
        public float? damageSlash = null;
        public float? damageBluntPerLevel = null;
        public float? damageChopPerLevel = null;
        public float? damagePickaxePerLevel = null;
        public float? damagePiercePerLevel = null;
        public float? damagePoisonPerLevel = null;
        public float? damageSlashPerLevel = null;
        public float? damageSpiritPerLevel = null;
        public float? attackEitr = null;
        public float? secondaryAttackEitr = null;
        public StatusEffect? equipStatusEffect = ScriptableObject.CreateInstance<StatusEffect>();
        public float? projectileVelocity = null;
        public float? projectileAccuracy = null;
        public float? projectileBurst = null;
        public float? secondaryLaunchAngle = null;
        public float? secondaryProjectileVelocity = null;
        public float? secondaryProjectileAccuracy = null;
        public float? weight = null;
        public float? maxDurability = null;
        public float? movementModifier = null;
        public float? blockPower = null;
        public float? timedBlockBonus = null;
        public float? deflectionForce = null;
        public float? attackForce = null;
        public float? backstabBonus = null;
        public ItemData? secondaryAttack = null;
        public Skills.SkillType? specialSkill = null;

        public UpdateItemDataOptions()
        {
            equipStatusEffect.name = "empty_MMES";
        }
    }
}
