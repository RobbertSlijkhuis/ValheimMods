using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class CreatureData
    {
        public string announceMessage;
        public bool aggravatable = false;
        public bool allowDrops = false;
        public bool allowDamageStructures = true;
        public int amount = 1;
        public string bossEvent = "";
        public bool commandable = false;
        public float damageScale = 0;
        public bool friendly = false;
        public string globalKeyAdd = "";
        public string globalKeyRemove = "";
        public string group;
        public float healthScale = 0;
        public int index;
        public bool isHallucination = false;
        public int level = 1;
        public float maxHealth = 0;
        public int maxSpawned = 0;
        public bool mistVision = true;
        public string name;
        public string prefabName;
        public string position = SpawnPositionType.Random;
        public PositionOffsetData positionOffset = new PositionOffsetData();
        public float positionRadius = 10f;
        public bool random = false;
        public bool rename = true;
        public bool talkInteract = false;
        public int talkInterval = 0;
        public string talkMessage;
        public bool talks = false;
        public float size = 1f;

        public CreatureData() { }
    }
}
