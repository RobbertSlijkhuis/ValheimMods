using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData
    {
        public bool random = false;
        public List<CreatureData> list = new List<CreatureData>();

        public SpawnCreatureData() { }
    }
}
