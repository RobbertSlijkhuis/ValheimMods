using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData
    {
        public bool allowDrops;
        public int count;
        public int level;
        public string prefabName;
        public string position;
        public bool talks;

        public SpawnCreatureData(string prefabName, int level = 1, int count = 1, bool allowDrops = false, bool talks = false, string position = nameof(SpawnPositionType.OnPlayer))
        {
            this.allowDrops = allowDrops;
            this.count = count;
            this.level = level;
            this.prefabName = prefabName;
            this.position = position;
            this.talks = talks;
        }
    }
}
