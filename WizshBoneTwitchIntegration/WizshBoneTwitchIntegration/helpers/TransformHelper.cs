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
        public static Vector3 UpdateSpawnLocation(string type, Transform transform, PositionOffsetData positionOffset)
        {
            Vector3 position;
            Vector3 offset = positionOffset.ToVector();

            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                    position = (transform.forward * (3f + offset.z)) + (transform.up * offset.y) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.InFrontOfPlayerHigh):
                    position = (transform.forward * (3f + offset.z)) + (transform.up * (3f + offset.y)) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.Flying):
                    position = (transform.forward * (10f + offset.z)) + (transform.up * (7f + offset.y)) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.RandomBehind):
                    position = (transform.forward * Random.Range(-30, -50f)) + (transform.right * Random.Range(-50, 50f)) + positionOffset.ToVector() + transform.position;

                    if (ZoneSystem.instance.FindFloor(position, out var height))
                        position.y = height;

                    return (transform.forward * offset.z) + (transform.up * offset.y) + (transform.right * offset.x) + position;
                default:
                    return (transform.forward * offset.z) + (transform.up * offset.y) + (transform.right * offset.x) + transform.position;
            }
        }

        /// <summary>
        /// Update the rotation by spawn location type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Quaternion UpdateSpawnRotation(string type, Transform transform)
        {
            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                case nameof(SpawnPositionType.InFrontOfPlayerHigh):
                case nameof(SpawnPositionType.Flying):
                    transform.Rotate(Vector3.up, 180f);
                    return transform.rotation;
                default:
                    return transform.rotation;
            }
        }
    }
}
