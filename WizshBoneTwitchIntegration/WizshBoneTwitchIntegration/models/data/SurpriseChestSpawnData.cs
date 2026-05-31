using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{ 
    internal class SurpriseChestSpawnData : CloneableData
    {
        public List<CreatureData> creatureData;
        public ItemData itemData;

        public SurpriseChestSpawnData() { }
    }
}
