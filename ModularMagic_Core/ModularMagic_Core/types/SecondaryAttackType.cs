namespace ModularMagic_Core.Types
{
    /// <summary>
    /// What a secondary attack spawns, which decides how the values of its config are applied.
    /// </summary>
    public static class SecondaryAttackType
    {
        public static string Projectile => "Projectile";
        public static string AOE => "AOE";
        public static string Script => "Script";
        public static string Humanoid => "Humanoid";
    }
}
