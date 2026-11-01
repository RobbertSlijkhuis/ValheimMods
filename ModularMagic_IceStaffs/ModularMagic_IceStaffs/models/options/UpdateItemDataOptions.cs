#nullable enable
using static ItemDrop;

namespace ModularMagic_IceStaffs.Models
{
    /// <summary>
    /// The values that are applied to the item data of an equipped staff. A value that is null is left as it is.
    /// </summary>
    internal class UpdateItemDataOptions
    {
        // The item data of the staff prefab, its main attack is copied onto the equipped staff
        public ItemData? mainAttack = null;
        public StatusEffect? equipStatusEffect = null;
        // The item data of the wrapper prefab that holds the secondary attack, its attack is copied onto the equipped staff
        public ItemData? secondaryAttack = null;

        public float? damageBlunt = null;
        public float? damageSlash = null;
        public float? damagePierce = null;
        public float? damageBluntPerLevel = null;
        public float? damageSlashPerLevel = null;
        public float? damagePiercePerLevel = null;

        public float? attackEitr = null;
        public float? projectileVelocity = null;
        public float? projectileAccuracy = null;
        public float? projectileBurst = null;
        public float? launchAngle = null;
        public float? secondaryAttackEitr = null;
    }
}
