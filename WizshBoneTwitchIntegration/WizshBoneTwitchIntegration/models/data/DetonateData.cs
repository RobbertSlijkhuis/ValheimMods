using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DetonateData : CloneableData
    {
        public string announceMessage;
        public DamageData damageData;
        public bool onlySpawned = false;
        public float radius = 20f;
        public string type = DetonateType.Fish;
        public List<string> values = new List<string>();

        public DetonateData() { }
    }
}
