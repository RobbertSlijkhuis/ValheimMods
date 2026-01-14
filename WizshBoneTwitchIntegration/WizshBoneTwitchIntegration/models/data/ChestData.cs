using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class ChestData
    {
        public int amount = 1;
        public string announceMessage;
        public float force = 200f;
        public bool interact = true;
        public List<string> items = new List<string>();
        public int mimicChance = 0;
        public bool random = true;
        public string type = ChestType.Iron;
        public int yeetChance = 0;

        public ChestData() { }
    }
}
