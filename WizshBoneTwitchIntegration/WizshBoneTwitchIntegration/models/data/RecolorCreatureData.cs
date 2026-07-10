using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RecolorCreatureData
    {
        public bool isSurtling = false;
        public bool disable = false;
        public bool enable = false;
        public bool emissive = false;
        public float emissiveMultiplier = 2f;
        public bool isLight = false;
        public bool isParticle = false;
        public bool isGear = false;
        public Material material;
        public int materialIndex = 0;
        public float particleAlpha = 0.2f;
        public string transformPath;
        public Material zebraMaterial;

        public RecolorCreatureData() { }

        public RecolorCreatureData(string transformPath)
        {
            this.transformPath = transformPath;
        }
    }
}
