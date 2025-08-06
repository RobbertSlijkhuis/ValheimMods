using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneMaterials
    {
        public Material material;
        public Material emissive;

        public RuneMaterials(Material material, Material emissive)
        {
            this.material = material;
            this.emissive = emissive;
        }
    }
}
