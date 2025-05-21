using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class LanternPackageResult
    {
        public bool applyLanternChanges;
        public Material lanternOn;
        public Material lanternOff;
        public long playerId;
        public int type;
        public bool value;

        public LanternPackageResult(Material lanternOn, Material lanternOff, long playerId, int type, bool value, bool applyLanternChanges)
        {
            this.applyLanternChanges = applyLanternChanges;
            this.lanternOn = lanternOn;
            this.lanternOff = lanternOff;
            this.playerId = playerId;
            this.type = type;
            this.value = value;
        }
    }
}
