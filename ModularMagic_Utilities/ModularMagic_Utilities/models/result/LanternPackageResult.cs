using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class LanternPackageResult
    {
        public bool applyLanternChanges;
        public Material lanternOn;
        public Material lanternOff;
        public string flareColor;
        public string lightColor;
        public float? lightRange;
        public float? lightIntensity;
        public string materialColor;
        public long playerId;
        public int type;
        public bool value;

        public LanternPackageResult(Material lanternOn, Material lanternOff, long playerId, int type, bool value, bool applyLanternChanges, string flareColor, string lightColor, float? lightRange, float? lightIntensity, string materialColor)
        {
            this.applyLanternChanges = applyLanternChanges;
            this.lanternOn = lanternOn;
            this.lanternOff = lanternOff;
            this.flareColor = flareColor;
            this.lightColor = lightColor;
            this.lightRange = lightRange;
            this.lightIntensity = lightIntensity;
            this.materialColor = materialColor;
            this.playerId = playerId;
            this.type = type;
            this.value = value;
        }
    }
}
