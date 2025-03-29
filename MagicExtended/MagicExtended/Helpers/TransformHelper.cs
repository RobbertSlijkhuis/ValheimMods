using UnityEngine;

namespace MagicExtended.Helpers
{
    internal class TransformHelper
    {
        public static Quaternion generateRotation(Vector3 vector)
        {
            Quaternion rotation = new Quaternion(0, 0, 0, 0);
            rotation.eulerAngles = vector;
            return rotation;
        }
    }
}
