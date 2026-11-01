namespace ModularMagic_FireStaffs.Types
{
    /// <summary>
    /// What the wrapper attack spawns and carries the damage: an AOE (nova) or a projectile (meteors).
    /// This decides how the config values are applied.
    /// </summary>
    internal class SecondaryAttackType
    {
        public static string AOE => "AOE";
        public static string Projectile => "Projectile";
    }
}
