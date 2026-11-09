using ModularMagic_Core.Components;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    /// <summary>
    /// On the prefab of the cone attack. The cone deals poison and spirit damage, each as much as the main damage of the staff
    /// (blunt, or pierce or slash when a rune changed it), already scaled by the damage multiplier of the cone.
    /// The other damage of the staff (spirit, chop, pickaxe) is not part of the cone.
    /// </summary>
    internal class ConeDamage : MonoBehaviour, IMMAoeHitModifier
    {
        public void Modify(HitData hitData)
        {
            HitData.DamageTypes damage = hitData.m_damage;
            // Only one of the three is set: a staff can have just one damage type rune, which moves the main damage from blunt to pierce or slash.
            // They are never added up, the one that is set is the main damage
            float main = Mathf.Max(damage.m_blunt, Mathf.Max(damage.m_pierce, damage.m_slash));

            hitData.m_damage = new HitData.DamageTypes
            {
                m_poison = main,
                m_spirit = main,
            };
        }
    }
}
