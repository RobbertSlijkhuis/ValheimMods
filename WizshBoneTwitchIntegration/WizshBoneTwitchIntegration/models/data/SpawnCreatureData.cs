using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData
    {
        public bool allowDrops;
        public int amount;
        public bool isHallucination;
        public int level;
        public string prefabName;
        public string position;
        public string talkMessage;
        public bool talks;

        public SpawnCreatureData()
        {
            allowDrops = false;
            amount = 1;
            level = 1;
            prefabName = "";
            position = SpawnPositionType.OnPlayer;
            talks = false;
        }
        public SpawnCreatureData(string prefabName, int level = 1, int amount = 1, string position = nameof(SpawnPositionType.OnPlayer), bool allowDrops = false, bool talks = false)
        {
            this.allowDrops = allowDrops;
            this.amount = amount;
            this.level = level;
            this.prefabName = prefabName;
            this.position = position;
            this.talks = talks;
        }
    }
}
