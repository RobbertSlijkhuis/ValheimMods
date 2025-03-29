using UnityEngine;

namespace MagicExtended.Models
{
    internal class TransformOptions
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public TransformOptions()
        {
            this.position = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector3(0f, 0f, 0f);
            this.scale = new Vector3(0f, 0f, 0f);
        }
    }
}
