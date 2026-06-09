using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class StatusEffectData : CloneableData
    {
        [EditorLabel("Random")]
        [EditorTooltip("Whether a random status effect is picked from the list (if there is only 1 this has no effect)")]
        public bool random = false;

        [EditorLabel("Status Effects")]
        [EditorTooltip("The list of status effects to apply or to pick randomly from.")]
        public List<StatusEffectEntry> list = new List<StatusEffectEntry>();

        public StatusEffectData() { }
    }

    internal class StatusEffectEntry : CloneableData
    {
        [EditorLabel("Name")]
        [EditorTooltip("The name of the status effect.")]
        [StatusEffectNameDropdown]
        public string name;

        [EditorLabel("Duration")]
        [EditorTooltip("The duration of the status effect in seconds. Set to -1 for default duration, 0 for infinite duration.")]
        public float duration = -1f;

        [EditorLabel("Persists through death")]
        [EditorTooltip("Whether the status effect should persist through death. If true, the status effect will not be removed when the player dies.")]
        public bool persistsThroughDeath = false;

        [EditorLabel("Renew")]
        [EditorTooltip("Whether to renew the status effect if it is already active. If true, the duration will be reset to the original duration when the status effect is applied again.")]
        public bool renew = false;

        public StatusEffectEntry() { }
    }
}
