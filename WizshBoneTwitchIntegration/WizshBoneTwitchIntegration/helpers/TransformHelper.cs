using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class TransformHelper
    {
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
            float height = 0f;

            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                    newPos = new Vector3(offset.x, offset.y, 3f + offset.z);
                    return GetRelativePosition(position, forward, right, up, newPos);
                case nameof(SpawnPositionType.Flying):
                    newPos = new Vector3(offset.x, 7f + offset.y, 10f + offset.z);
                    return GetRelativePosition(position, forward, right, up, newPos);
                case nameof(SpawnPositionType.Random):
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float distance = Random.Range(4f, positionRadius);
                    Vector2 circle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    newPos = new Vector3(position.x + circle.x, position.y, position.z + circle.y);

                    if (ZoneSystem.instance.GetSolidHeight(newPos, out height))
                        newPos.y = height;

                    return newPos;
                case nameof(SpawnPositionType.RandomBehind):
                    float back = Random.Range(-50f, -30f);
                    float side = Random.Range(-50f, 50f);
                    Vector3 pos = new Vector3(side + offset.x, offset.y, back + offset.z);
                    newPos = GetRelativePosition(position, forward, right, up, pos);

                    if (ZoneSystem.instance.GetSolidHeight(newPos, out height))
                        newPos.y = height;

                    return newPos;
                default:
                    newPos = new Vector3(offset.x, offset.y, offset.z);
                    return GetRelativePosition(position, forward, right, up, newPos);
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
