using System.Collections.Generic;
using WizshBoneTwitchIntegration.GuiOld;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnCreatureData : CloneableData
    {
        [EditorLabel("Random")]
        [EditorTooltip("If enabled, a random creature from the list will be spawned.")]
        public bool random = false;

        [EditorLabel("Creatures")]
        [EditorTooltip("List of creatures to spawn.")]
        public List<CreatureData> list = new List<CreatureData>();

        public SpawnCreatureData() { }
    }
}
