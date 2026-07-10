using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class CreatureGroupData : CloneableData
    {
        [EditorLabel("Group Name")]
        [EditorTooltip("Unique identifier referenced by SpawnCreature redeem entries via their 'group' field.")]
        public string group;

        [EditorLabel("Creatures")]
        [EditorTooltip("Creatures belonging to this group.")]
        public List<CreatureData> list = new List<CreatureData>();

        public CreatureGroupData() { }
    }
}
