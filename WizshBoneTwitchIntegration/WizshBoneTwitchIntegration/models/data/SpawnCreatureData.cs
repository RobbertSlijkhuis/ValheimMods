using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData
    {
        public bool allowDrops;
        public int amount;
        public bool isFriendly;
        public bool isHallucination;
        public int level;
        public string prefabName;
        public string position;
        public bool rename;
        public int talkInterval;
        public string talkMessage;
        public bool talks;

        public SpawnCreatureData()
        {
            allowDrops = false;
            amount = 1;
            isFriendly = false;
            isHallucination = false;
            level = 1;
            prefabName = "";
            position = SpawnPositionType.OnPlayer;
            rename = true;
            talkInterval = 0;
            talks = false;
        }
        public SpawnCreatureData(string prefabName, int level = 1, int amount = 1, string position = nameof(SpawnPositionType.OnPlayer), bool allowDrops = false, bool isFriendly = false)
        {
            this.allowDrops = allowDrops;
            this.amount = amount;
            this.isFriendly = isFriendly;
            this.level = level;
            this.prefabName = prefabName;
            this.position = position;
        }
    }
}
