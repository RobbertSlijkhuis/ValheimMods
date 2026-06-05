using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class StatusEffectData : CloneableData
    {
        public bool random = false;
        public List<StatusEffectEntry> list = new List<StatusEffectEntry>();

        public StatusEffectData() { }
    }

    internal class StatusEffectEntry : CloneableData
    {
        [EditorLabel("Duration")]
        [EditorTooltip("The duration of the status effect in seconds. Set to 0 for infinite duration.")]
        public float duration = -1f;

        [EditorLabel("Name")]
        [EditorTooltip("The name of the status effect.")]
        [StatusEffectNameDropdown]
        public string name;

        [EditorLabel("Persists Through Death")]
        [EditorTooltip("Whether the status effect should persist through death. If true, the status effect will not be removed when the player dies.")]
        public bool persistsThroughDeath = false;

        [EditorLabel("Renew")]
        [EditorTooltip("Whether to renew the status effect if it is already active. If true, the duration will be reset to the original duration when the status effect is applied again.")]
        public bool renew = false;

        public StatusEffectEntry() { }
    }
}
