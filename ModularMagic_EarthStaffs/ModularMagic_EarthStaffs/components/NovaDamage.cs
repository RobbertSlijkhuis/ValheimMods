using ModularMagic_Core.Components;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    /// <summary>
    /// On the prefab of the nova attack. The main damage of the nova is always pierce, whatever the damage type rune of the staff is.
    /// The rest of the damage of the staff (spirit, chop, pickaxe) is left as it is.
    /// </summary>
    internal class NovaDamage : MonoBehaviour, IMMAoeHitModifier
    {
        public void Modify(HitData hitData)
        {
            HitData.DamageTypes damage = hitData.m_damage;

            // Only one of the three is set: a staff can have just one damage type rune, which moves the main damage from blunt to pierce or slash.
            // They are never added up, the one that is set is the main damage
            float main = Mathf.Max(damage.m_blunt, Mathf.Max(damage.m_pierce, damage.m_slash));

            damage.m_blunt = 0f;
            damage.m_slash = 0f;
            damage.m_pierce = main;

            hitData.m_damage = damage;
        }
    }
}
