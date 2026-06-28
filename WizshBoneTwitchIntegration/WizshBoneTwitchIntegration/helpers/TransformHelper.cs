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
                    return IsObstructed(newPos) ? position : newPos;
                case nameof(SpawnPositionType.Flying):
                    newPos = new Vector3(offset.x, 7f + offset.y, 10f + offset.z);
                    newPos = GetRelativePosition(position, forward, right, up, newPos);
                    return IsObstructed(newPos) ? position : newPos;
                case nameof(SpawnPositionType.Random):
                    bool indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

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

            if (indoor)
            {
                Vector3 rayOrigin = new Vector3(candidate.x, referenceHeight + GroundRaycastUp, candidate.z);

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GroundRaycastUp + GroundRaycastDistance, GroundMask))
                    return false;

                result.y = hit.point.y;

                // Reject outdoor ledges near dungeon entrances: a valid indoor spot must have a ceiling overhead.
                if (!Physics.Raycast(new Vector3(result.x, result.y + 0.1f, result.z), Vector3.up, CeilingRaycastDistance, GroundMask))
                    return false;
            }
            else
            {
                if (!ZoneSystem.instance.GetSolidHeight(candidate, out float height))
                    return false;

                result.y = height;
            }

            return !IsObstructed(result);
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
        /// Update the rotation by spawn location type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Quaternion UpdateSpawnRotation(string type, Quaternion rotation)
        {
            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                case nameof(SpawnPositionType.InFrontOfPlayerHigh):
                case nameof(SpawnPositionType.Flying):
                    return rotation * Quaternion.Euler(0f, 180f, 0f);

                default:
                    return rotation;
            }
        }
    }
}
