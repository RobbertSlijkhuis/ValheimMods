using UnityEngine;

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
    }
}
