using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class TransformHelper
    {
        private const int MaxPositionTries = 5;
        private const float GroundRaycastUp = 2f;
        private const float GroundRaycastDistance = 8f;
        private const float CeilingRaycastDistance = 15f;
        private const float ObstructionCheckHeight = 1f;
        private const float ObstructionCheckRadius = 0.5f;
        private const float DefaultFlyingHeight = 10f;
        private const float FlyingCeilingMargin = 1.5f;
        private static readonly int GroundMask = LayerMask.GetMask("Default", "static_solid", "terrain", "Default_small");

        /// <summary>
        /// Instantiate a Quaternion with a Vector3 used for rotation euler angles
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Quaternion GenerateRotation(Vector3 vector)
        {
            Quaternion rotation = new Quaternion(0, 0, 0, 0);
            rotation.eulerAngles = vector;
            return rotation;
        }

        /// <summary>
        /// Update the location by spawn position type and offset
        /// </summary>
        /// <param name="type"></param>
        /// <param name="transform"></param>
        /// <param name="positionOffset"></param>
        /// <returns></returns>
        public static Vector3 UpdateSpawnLocation(string type, Transform transform, PositionOffsetData positionOffset = null, float positionRadius = 10f)
        {
            if (positionOffset == null)
                positionOffset = new PositionOffsetData();

            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            Vector3 newPos;
            Vector3 offset = positionOffset.ToVector();

            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                    newPos = new Vector3(offset.x, offset.y, 3f + offset.z);
                    newPos = GetRelativePosition(position, forward, right, up, newPos);
                    bool indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();
                    return TryPlaceOnGround(newPos, position.y, indoor, out Vector3 placedFront) ? placedFront : position;
                case nameof(SpawnPositionType.Random):
                    indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

                    for (int i = 0; i < MaxPositionTries; i++)
                    {
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        float distance = Random.Range(4f, positionRadius);
                        Vector2 circle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                        newPos = new Vector3(position.x + circle.x, position.y, position.z + circle.y);

                        if (TryPlaceOnGround(newPos, position.y, indoor, out Vector3 placed))
                            return placed;
                    }

                    return position;
                case nameof(SpawnPositionType.RandomBehind):
                    indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

                    for (int i = 0; i < MaxPositionTries; i++)
                    {
                        float back = Random.Range(-50f, -30f);
                        float side = Random.Range(-50f, 50f);
                        Vector3 pos = new Vector3(side + offset.x, offset.y, back + offset.z);
                        newPos = GetRelativePosition(position, forward, right, up, pos);

                        if (TryPlaceOnGround(newPos, position.y, indoor, out Vector3 placed))
                            return placed;
                    }

                    return position;
                case nameof(SpawnPositionType.RandomFlying):
                    indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

                    for (int i = 0; i < MaxPositionTries; i++)
                    {
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        float distance = Random.Range(4f, positionRadius);
                        Vector2 circle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                        newPos = new Vector3(position.x + circle.x, position.y, position.z + circle.y);

                        if (TryPlaceFlying(newPos, position.y, indoor, out Vector3 placed))
                            return placed;
                    }

                    return position;
                default:
                    newPos = new Vector3(offset.x, offset.y, offset.z);
                    newPos = GetRelativePosition(position, forward, right, up, newPos);
                    return IsObstructed(newPos) ? position : newPos;
            }
        }

        /// <summary>
        /// Update the position by origin postion and offset
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="forward"></param>
        /// <param name="right"></param>
        /// <param name="up"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 GetRelativePosition(Vector3 origin, Vector3 forward, Vector3 right, Vector3 up, Vector3 offset)
        {
            return origin + forward * offset.z + right * offset.x + up * offset.y;
        }

        /// <summary>
        /// Finds the ground height for a candidate XZ position and verifies the resulting
        /// spot isn't obstructed by a wall/rock. Indoors, ground is found via a local
        /// downward raycast from near the player's height (ZoneSystem.GetSolidHeight raycasts
        /// from above the world and hits dungeon ceilings instead of the floor). Outdoors,
        /// ZoneSystem.GetSolidHeight is used as before.
        /// </summary>
        private static bool TryPlaceOnGround(Vector3 candidate, float referenceHeight, bool indoor, out Vector3 result)
        {
            result = candidate;

            if (!TryGetGroundHeight(candidate, referenceHeight, indoor, out float height))
                return false;

            result.y = height;
            return !IsObstructed(result);
        }

        /// <summary>
        /// Finds solid ground height at a candidate XZ position, and (indoors) confirms there's
        /// a ceiling overhead. Returns false if no valid ground could be found, or - indoors -
        /// if nothing was found overhead within reach (e.g. an outdoor ledge near a dungeon
        /// entrance, not an actual room).
        /// </summary>
        public static bool TryGetGroundHeight(Vector3 candidate, float referenceHeight, bool indoor, out float height, int outdoorHeightMargin = 1000)
        {
            if (!TryGetFloorHeight(candidate, referenceHeight, indoor, out height, outdoorHeightMargin))
                return false;

            if (!indoor)
                return true;

            // Reject outdoor ledges near dungeon entrances: a valid indoor spot must have a ceiling overhead.
            return TryGetCeilingHeight(candidate, height + 0.1f, CeilingRaycastDistance, out _);
        }

        /// <summary>
        /// Finds solid floor height at a candidate XZ position only - no ceiling requirement.
        /// Indoors, floor is found via a local downward raycast from near <paramref name="referenceHeight"/>
        /// (ZoneSystem.GetSolidHeight raycasts from 1000 units above the world and hits dungeon
        /// ceilings/roofs instead of the floor). Outdoors, ZoneSystem.GetSolidHeight is used as before.
        /// </summary>
        private static bool TryGetFloorHeight(Vector3 candidate, float referenceHeight, bool indoor, out float height, int outdoorHeightMargin = 1000)
        {
            if (!indoor)
                return ZoneSystem.instance.GetSolidHeight(candidate, out height, outdoorHeightMargin);

            Vector3 rayOrigin = new Vector3(candidate.x, referenceHeight + GroundRaycastUp, candidate.z);

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GroundRaycastUp + GroundRaycastDistance, GroundMask))
            {
                height = referenceHeight;
                return false;
            }

            height = hit.point.y;
            return true;
        }

        /// <summary>
        /// Raycasts straight up from <paramref name="fromHeight"/> at the candidate's XZ and
        /// returns the height of whatever it hits (a dungeon ceiling, a roof, a tree canopy)
        /// within <paramref name="maxDistance"/>. Unlike <see cref="TryGetGroundHeight"/>'s
        /// ceiling check (which only confirms one exists), this keeps the actual hit height so
        /// a caller can place something just under it.
        /// </summary>
        private static bool TryGetCeilingHeight(Vector3 candidate, float fromHeight, float maxDistance, out float ceilingHeight)
        {
            if (!Physics.Raycast(new Vector3(candidate.x, fromHeight, candidate.z), Vector3.up, out RaycastHit hit, maxDistance, GroundMask))
            {
                ceilingHeight = 0f;
                return false;
            }

            ceilingHeight = hit.point.y;
            return true;
        }

        /// <summary>
        /// Checks whether a spot just above ground level overlaps a wall/rock, to avoid
        /// spawning creatures inside solid geometry.
        /// </summary>
        private static bool IsObstructed(Vector3 position)
        {
            return Physics.CheckSphere(position + Vector3.up * ObstructionCheckHeight, ObstructionCheckRadius, GroundMask);
        }

        /// <summary>
        /// Finds a safe elevated spot at a candidate XZ position for "flying" spawns: a floor
        /// must exist below (same floor detection <see cref="TryGetGroundHeight"/> uses, indoor
        /// and outdoor), and the desired elevation (<see cref="DefaultFlyingHeight"/> above the
        /// floor) is clamped under whatever ceiling/roof/canopy is found overhead, if anything is
        /// within reach - this covers dungeon ceilings and outdoor roofs/tree canopies with one
        /// check. Outdoors in the open it's usually a no-op (nothing overhead within range), so
        /// the default elevation is used as-is. Rejects the candidate if the clamp would land at
        /// or below the floor (room/gap too short to fly in).
        /// </summary>
        private static bool TryPlaceFlying(Vector3 candidate, float referenceHeight, bool indoor, out Vector3 result)
        {
            result = candidate;

            if (!TryGetFloorHeight(candidate, referenceHeight, indoor, out float floorHeight))
                return false;

            float elevation = floorHeight + DefaultFlyingHeight;

            if (TryGetCeilingHeight(candidate, floorHeight + 0.1f, CeilingRaycastDistance, out float ceilingHeight))
            {
                elevation = ceilingHeight - FlyingCeilingMargin;

                if (elevation <= floorHeight)
                    return false;
            }

            result.y = elevation;
            return !IsObstructed(result);
        }

        /// <summary>
        /// Horizontal-only rotation so an object at <paramref name="from"/> faces
        /// <paramref name="target"/>, ignoring height differences so it doesn't tilt up/down.
        /// Returns <paramref name="fallback"/> if the two points are basically coincident
        /// (degenerate <see cref="Quaternion.LookRotation(Vector3)"/> input). Not tied to any
        /// particular object type - usable for creatures, chests, or any other spawned prefab.
        /// </summary>
        public static Quaternion GetFacingRotation(Vector3 from, Vector3 target, Quaternion fallback)
        {
            Vector3 direction = target - from;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return fallback;

            return Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Convenience wrapper around <see cref="GetFacingRotation"/> for the common case of
        /// facing the local player specifically.
        /// </summary>
        public static Quaternion FacePlayer(Vector3 from, Quaternion fallback)
        {
            return GetFacingRotation(from, Player.m_localPlayer.transform.position, fallback);
        }

        /// <summary>
        /// A random yaw-only rotation, for spawns that don't need to face anything in
        /// particular (the fallback when a redeem's <c>facePlayer</c> is off).
        /// </summary>
        public static Quaternion RandomFacing()
        {
            return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
    }
}
