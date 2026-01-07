using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData
    {
        public bool aggravatable = false;
        public bool allowDrops = false;
        public int amount = 1;
        public bool commandable = false;
        public bool friendly = false;
        public bool isHallucination = false;
        public int level = 1;
        public bool mistVision = true;
        public string prefabName;
        public string position = SpawnPositionType.OnPlayer;
        public bool rename = true;
        public bool talkInteract = false;
        public int talkInterval = 0;
        public string talkMessage;
        public bool talks = false;

        public SpawnCreatureData() { }

        public SpawnCreatureData(string prefabName, int level = 1, int amount = 1, string position = nameof(SpawnPositionType.OnPlayer), bool allowDrops = false, bool friendly = false, bool commandable = false)
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
