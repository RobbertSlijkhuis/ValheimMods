#nullable enable

using UnityEngine;

namespace MagicExtended.Models
{
    internal class AddPrefabToArmatureOptions
    {
        public bool active;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public AddPrefabToArmatureOptions()
        {
            this.active = false;
            this.position = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector3(0f, 0f, 0f);
            this.scale = new Vector3(0f, 0f, 0f);
        }
    }
}
