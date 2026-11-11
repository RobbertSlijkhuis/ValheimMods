namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Helpers for the damage of a staff.
    /// </summary>
    public static class DamageHelper
    {
        /// <summary>
        /// The main physical damage: a staff has just one of blunt, pierce and slash (a damage type rune moves the main damage from one to
        /// another), so they are never added up and the one that is set is the main damage.
        /// </summary>
        public static float GetMainPhysical(HitData.DamageTypes damage)
        {
            return UnityEngine.Mathf.Max(damage.m_blunt, UnityEngine.Mathf.Max(damage.m_pierce, damage.m_slash));
        }
    }
}
