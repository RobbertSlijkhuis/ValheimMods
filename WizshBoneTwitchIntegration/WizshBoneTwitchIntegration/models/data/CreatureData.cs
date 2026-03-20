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
        public bool commandable = false;
        public bool friendly = false;
        public string globalKeyAdd = "";
        public string globalKeyRemove = "";
        public bool isHallucination = false;
        public int level = 1;
        public float maxHealth = 0;
        public bool mistVision = true;
        public string name;
        public string prefabName;
        public string position = SpawnPositionType.OnPlayer;
        public PositionOffsetData positionOffset = new PositionOffsetData();
        public bool rename = true;
        public bool talkInteract = false;
        public int talkInterval = 0;
        public string talkMessage;
        public bool talks = false;
        public float size = 1f;

        public CreatureData() { }

        public CreatureData(string prefabName, int level = 1, int amount = 1, string position = nameof(SpawnPositionType.OnPlayer), bool allowDrops = false, bool friendly = false, bool commandable = false)
        {
            this.allowDrops = allowDrops;
            this.commandable = commandable;
            this.amount = amount;
            this.friendly = friendly;
            this.level = level;
            this.prefabName = prefabName;
            this.position = position;
        }
    }
}
