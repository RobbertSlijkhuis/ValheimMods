using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData
    {
        public string backgroundColor;
        public int cost;
        public bool IsUserInputRequired;
        public List<SpawnCreatureData> creatureData;
        public string title;
        public string type;

        public RedeemData(string type, string title, int cost, string backgroundColor, bool IsUserInputRequired = false, List<SpawnCreatureData> creatureData = null)
        {
            this.backgroundColor = backgroundColor;
            this.cost = cost;
            this.IsUserInputRequired = IsUserInputRequired;
            this.creatureData = creatureData;
            this.title = title;
            this.type = type;
        }
    }
}
