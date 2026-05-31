using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData : CloneableData
    {
        public bool random = false;
        public List<CreatureData> list = new List<CreatureData>();

        public SpawnCreatureData() { }
    }
}
