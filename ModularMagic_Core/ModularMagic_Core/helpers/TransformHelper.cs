using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class TransformHelper
    {
        public static Quaternion GenerateRotation(Vector3 vector)
        {
            Quaternion rotation = new Quaternion(0, 0, 0, 0);
            rotation.eulerAngles = vector;
            return rotation;
        }
    }
}
