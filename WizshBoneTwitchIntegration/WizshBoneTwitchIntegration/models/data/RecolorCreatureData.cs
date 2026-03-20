using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RecolorCreatureData
    {
        public bool emissive = false;
        public float emissiveMultiplier = 2f;
        public Material material;
        public bool isLight = false;
        public bool isParticle = false;
        public bool disable = false;
        public string transformPath;
        public Material zebraMaterial;

        public RecolorCreatureData() { }

        public RecolorCreatureData(string transformPath)
        {
            this.transformPath = transformPath;
        }
    }
}
