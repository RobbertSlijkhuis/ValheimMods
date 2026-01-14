using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class PositionOffsetData
    {
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;

        public PositionOffsetData() { }

        public Vector3 ToVector()
        {
            return new Vector3(x, y, z);
        }
    }
}
