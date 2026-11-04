namespace ModularMagic_LightningStaffs.Types
{
    /// <summary>
    /// What the wrapper attack spawns and carries the damage: an AOE (nova) or a projectile (lightning bolts).
    /// This decides how the config values are applied.
    /// </summary>
    internal class SecondaryAttackType
    {
        public static string AOE => "AOE";
        public static string Projectile => "Projectile";
    }
}
