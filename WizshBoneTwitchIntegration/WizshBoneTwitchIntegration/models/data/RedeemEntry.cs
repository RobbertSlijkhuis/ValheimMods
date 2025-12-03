using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemEntry
    {
        public string backgroundColor;
        public int cooldown;
        public List<SpawnCreatureData> creatures;
        public string globalKey;
        public int points;
        public string title;
        public string type;
        public bool userInput;

        public RedeemEntry()
        {
            backgroundColor = "#a970ff";
            cooldown = 0;
            creatures = null;
            globalKey = null;
            points = 0;
            title = "";
            type = RedeemType.Undefined;
            userInput = false;
        }

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
