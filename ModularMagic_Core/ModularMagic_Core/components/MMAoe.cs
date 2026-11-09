using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// The area hit of an attack of a staff (the burst of a cone, a nova, ...). It replaces the Aoe component on the prefab the attack spawns,
    /// see MMAoeHelper. That prefab is only spawned locally by the attack of the shooter (it has no ZNetView), so the other players never
    /// get a copy of it. The shooter hits everything in the area here, and the damage call sends the hit to the client that owns the target.
    /// The Aoe component can not do this: without a ZNetView it only hits the targets that the local client owns, and with one every
    /// client has a copy that deals damage on its own with values it does not know.
    /// The hit data comes from the attack, so it holds the damage of the staff scaled by the damage multiplier of the attack,
    /// with the skill factor applied once (the Aoe component applies it a second time).
    /// </summary>
    public class MMAoe : MonoBehaviour, IProjectile
    {
        // The area that is hit: a sphere around this object, or a box in the space of this object. The box points in the direction
        // of the attack, so it turns with the aim
        public bool sphere = false;
        public float radius = 1f;
        public Vector3 center = Vector3.zero;
        public Vector3 size = Vector3.one;

        // Whether props (trees, rocks, pieces) are hit too. Characters are always hit
        public bool hitProps = false;

        private static readonly Collider[] colliders = new Collider[128];
        private static readonly HashSet<GameObject> targets = new HashSet<GameObject>();
        private static readonly List<IMMAoeHitModifier> modifiers = new List<IMMAoeHitModifier>();
        private static int layerMaskCharacters = -1;
        private static int layerMaskProps = -1;

        public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
        {
            try
            {
                if (owner != null && hitData != null)
                {
                    // A modifier on the prefab can change the damage of the hit, the hit data of the attack itself is left as it is
                    HitData modified = hitData.Clone();
                    GetComponents(modifiers);

                    foreach (IMMAoeHitModifier modifier in modifiers)
                        modifier.Modify(modified);

                    Hit(owner, modified);
                }
            }
            finally
            {
                // The hit is a single moment, the visuals of the attack are separate effects of the attack
                Destroy(gameObject);
            }
        }

        public string GetTooltipString(int itemQuality)
        {
            return "";
        }

        private void Hit(Character owner, HitData hitData)
        {
            if (layerMaskCharacters == -1)
            {
                layerMaskCharacters = LayerMask.GetMask("character", "character_net", "character_ghost");
                layerMaskProps = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "hitbox", "character_noenv", "vehicle");
            }

            int layerMask = hitProps ? layerMaskCharacters | layerMaskProps : layerMaskCharacters;
            Vector3 origin = transform.position;
            int count = sphere
                ? Physics.OverlapSphereNonAlloc(origin, radius, colliders, layerMask)
                : Physics.OverlapBoxNonAlloc(origin + transform.rotation * center, size / 2f, colliders, transform.rotation, layerMask);

            targets.Clear();
            bool hitCharacter = false;

            for (int i = 0; i < count; i++)
            {
                Collider collider = colliders[i];
                GameObject target = Projectile.FindHitObject(collider);

                // A target has several colliders, it is hit once
                if (target == null || !targets.Add(target))
                    continue;

                IDestructible destructible = target.GetComponent<IDestructible>();

                if (destructible == null)
                    continue;

                Character character = destructible as Character;

                if (character != null && !CanHit(owner, character, hitData))
                    continue;

                HitData targetHit = hitData.Clone();
                targetHit.m_point = collider.bounds.center;
                targetHit.m_dir = (targetHit.m_point - origin).normalized;
                targetHit.m_hitCollider = collider;
                targetHit.m_ranged = true;

                destructible.Damage(targetHit);

                if (character != null)
                    hitCharacter = true;
            }

            // The skill is raised once per hit that reached a character, by the amount of the attack (m_raiseSkillAmount of the attack on the ItemDrop)
            if (hitCharacter && owner.IsPlayer() && hitData.m_skill != Skills.SkillType.None && hitData.m_skill != Skills.SkillType.All && hitData.m_skillRaiseAmount > 0f)
                owner.RaiseSkill(hitData.m_skill, hitData.m_skillRaiseAmount);
        }

        // Only enemies are hit: not the shooter, not the same kind of creature, not friends and not a target that is dodging
        private static bool CanHit(Character owner, Character character, HitData hitData)
        {
            if (character == owner || character.m_name == owner.m_name)
                return false;

            bool enemy = BaseAI.IsEnemy(owner, character)
                || (character.GetBaseAI() != null && character.GetBaseAI().IsAggravatable() && owner.IsPlayer());

            if (!enemy)
                return false;

            return !(hitData.m_dodgeable && character.IsDodgeInvincible());
        }
    }
}
