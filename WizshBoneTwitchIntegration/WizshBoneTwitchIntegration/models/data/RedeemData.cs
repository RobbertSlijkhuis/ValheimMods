using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData
    {
        public string backgroundColor;
        public int cost;
        public List<SpawnCreatureData> spawnCreatureData;
        public string title;
        public string type;

        public RedeemData(string type, string title, int cost, string backgroundColor, List<SpawnCreatureData> spawnCreatureData = null)
        {
            this.backgroundColor = backgroundColor;
            this.cost = cost;
            this.spawnCreatureData = spawnCreatureData;
            this.title = title;
            this.type = type;
        }
    }
}
