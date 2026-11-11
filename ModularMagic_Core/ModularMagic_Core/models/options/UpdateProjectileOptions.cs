#nullable enable

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The values that are applied to the Projectile component of a prefab. A value that is null is left as it is.
    /// </summary>
    public class UpdateProjectileOptions
    {
        public float? aoe = null;
        public float? damageBlunt = null;
        public float? damageChop = null;
        public float? damageFire = null;
        public float? damageFrost = null;
        public float? damageGeneral = null;
        public float? damageLightning = null;
        public float? damagePickaxe = null;
        public float? damagePierce = null;
        public float? damagePoison = null;
        public float? damageSpirit = null;
        public float? attackForce = null;
    }
}
