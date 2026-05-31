using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnAbilitySpawnData : CloneableData
    {
        public List<CreatureData> creatureData;
        public ItemData itemData;

        public SpawnAbilitySpawnData() { }
    }
}
