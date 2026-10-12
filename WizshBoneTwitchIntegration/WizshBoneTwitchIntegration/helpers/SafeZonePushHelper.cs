using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Pushes a ward-target creature out of a safe zone using vanilla's own knockback
    // (Character.ApplyPushback) instead of raw Rigidbody forces.
    //
    // Why not AddForce: Character.UpdateWalking re-sets the body's velocity toward the creature's own
    // desired velocity every fixed step, and caps that correction at 20 m/s per step, so any push
    // below ~20 m/s per step is erased on the next step (the old fixed 2000 Acceleration only
    // worked by brute-forcing past that cap). ApplyPushback sets Character.m_pushForce, which
    // AddPushbackForce folds into the desired velocity itself (forced to >= 20 m/s along the push
    // direction) and which also blocks the creature's own movement (IsKnockedBack) while it lasts.
    // m_pushForce decays at 100/s and is applied as force / mass * 2.5, so the push magnitude only
    // controls how long the 20 m/s kick lasts (~0.2 m per unit of magnitude), and anything <= 2
    // decays away before it is ever used.
    //
    // Multiplayer: movement and m_pushForce only run on the creature's owner (Character.CustomFixedUpdate),
    // so the push is only applied there - a non-owner's call would do nothing.
    internal static class SafeZonePushHelper
    {
        // Push magnitude (see above): ~0.2 m of kick per unit; 4 would be exactly one 20 m/s step (~0.4 m).
        private const float MinPush = 10f;
        // Extra push magnitude per metre the creature has penetrated into the zone.
        private const float PushPerMeterDepth = 6f;
        private const float MaxPush = 25f;
        // Below this the flattened separation direction is too vertical to use (e.g. standing on
        // top of a ship's zone box), so the horizontal radial direction is used instead.
        private const float MinHorizontalDir = 0.2f;

        private static bool s_loggedFallback;

        public static void PushOut(Character creature, Collider zoneCollider)
        {
            if (creature == null || !creature.IsOwner())
                return;

            GetPushOutDirection(creature, zoneCollider, out Vector3 dir, out float depth);

            float magnitude = Mathf.Clamp(MinPush + depth * PushPerMeterDepth, MinPush, MaxPush);

            // Undo ApplyPushback's own "/ mass * 2.5" and equipment scaling so `magnitude` is
            // what actually ends up in m_pushForce.
            float equipment = Mathf.Max(Mathf.Clamp01(1f + creature.GetEquipmentMovementModifier()), 0.01f);
            float force = magnitude * creature.GetMass() / 2.5f / equipment;

            creature.ApplyPushback(dir, force);
        }

        // Direction (horizontal, normalized) that gets the creature out of the zone the shortest way,
        // and how far it has to move along it. Physics.ComputePenetration gives both for the box/
        // capsule/sphere zones; when it can't (non-convex mesh, or a mostly vertical separation) the
        // direction falls back to radially away from the zone's center, with depth 0.
        private static void GetPushOutDirection(Character creature, Collider zoneCollider, out Vector3 dir, out float depth)
        {
            CapsuleCollider creatureCollider = creature.GetCollider();

            // The creature is passed first so the returned direction is the one that moves *it* out.
            if (creatureCollider != null
                && !(zoneCollider is MeshCollider mesh && !mesh.convex)
                && Physics.ComputePenetration(
                    creatureCollider, creatureCollider.transform.position, creatureCollider.transform.rotation,
                    zoneCollider, zoneCollider.transform.position, zoneCollider.transform.rotation,
                    out Vector3 separation, out float distance))
            {
                Vector3 flat = new Vector3(separation.x, 0f, separation.z);

                if (flat.magnitude >= MinHorizontalDir)
                {
                    dir = flat.normalized;
                    // Only the horizontal share of the way out counts as depth.
                    depth = distance * flat.magnitude;
                    return;
                }
            }

            if (!s_loggedFallback)
            {
                s_loggedFallback = true;
                Jotunn.Logger.LogWarning($"[WBTI] SafeZonePushHelper: ComputePenetration unusable for '{creature.name}' vs zone collider {zoneCollider.GetType().Name} (trigger={zoneCollider.isTrigger}), using radial fallback (logged once)");
            }

            Vector3 radial = creature.transform.position - zoneCollider.bounds.center;
            radial.y = 0f;

            dir = radial.sqrMagnitude > 0.0001f ? radial.normalized : creature.transform.forward;
            depth = 0f;
        }
    }
}
