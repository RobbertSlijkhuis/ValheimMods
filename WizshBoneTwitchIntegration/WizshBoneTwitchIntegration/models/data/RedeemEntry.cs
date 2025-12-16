using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemEntry
    {
        public string backgroundColor = "#a970ff";
        public int cooldown = 0;
        public List<SpawnCreatureData> creatures;
        public string globalKey;
        public bool ignoreWard = false;
        public int points = 0;
        public string title = "";
        public string type = RedeemType.Undefined;
        public bool userInput = false;

        public RedeemEntry() {}

        public RedeemEntry(string type, string title, int points, string backgroundColor, int cooldown = 0, bool userInput = false, string globalKey = null, List<SpawnCreatureData> creatures = null)
        {
            this.backgroundColor = backgroundColor;
            this.cooldown = cooldown;
            this.creatures = creatures;
            this.globalKey = globalKey;
            this.points = points;
            this.title = title;
            this.type = type;
            this.userInput = userInput;
        }
    }
}
