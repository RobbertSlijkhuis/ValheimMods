using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData
    {
        public string backgroundColor;
        public int cooldown;
        public int cost;
        public List<SpawnCreatureData> creatureData;
        public bool isUserInputRequired;
        public string globalKey;
        public string title;
        public string type;

        public RedeemData(string type, string title, int cost, string backgroundColor, int cooldown = 0, bool isUserInputRequired = false, string globalKey = null, List<SpawnCreatureData> creatureData = null)
        {
            this.backgroundColor = backgroundColor;
            this.cooldown = cooldown;
            this.cost = cost;
            this.creatureData = creatureData;
            this.isUserInputRequired = isUserInputRequired;
            this.globalKey = globalKey;
            this.title = title;
            this.type = type;
        }
    }
}
