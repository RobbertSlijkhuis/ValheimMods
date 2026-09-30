using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{ 
    internal class SurpriseChestSpawnData : CloneableData
    {
        public List<CreatureData> creatureData;
        public ItemData itemData;

        // Relative chance of this entry being picked when the chest picks its loot at random
        // (SurpriseChestData.random). An entry with weight 3 comes up three times as often as one
        // with weight 1; 0 = never picked. Missing in older profiles = 1, i.e. the old equal odds.
        public float weight = 1f;

        public SurpriseChestSpawnData() { }
    }
}
