using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SurpriseChestData : CloneableData
    {
        public int amount = 1;
        public string announceMessage;
        public bool facePlayer = true;
        public float force = 200f;
        public bool interact = true;
        public List<SurpriseChestSpawnData> items = new List<SurpriseChestSpawnData>();
        public int mimicChance = 0;
        public string position = SpawnPositionType.Random;
        public PositionOffsetData positionOffset = new PositionOffsetData();
        public bool random = true;
        public string redeemTitle;
        public float spawnDelay = 0.7f;
        public string type = ChestType.Iron;
        public int yeetChance = 0;

        public SurpriseChestData() { }
    }
}
