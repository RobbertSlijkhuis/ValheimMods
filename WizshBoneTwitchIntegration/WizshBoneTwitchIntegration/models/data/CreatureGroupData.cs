using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class CreatureGroupData : CloneableData
    {
        public string group;
        public List<CreatureData> list = new List<CreatureData>();

        public CreatureGroupData() { }
    }
}
