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
        [OnValueChanged("OnNameChanged")]
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

        [EditorVisibleWhen("name", "PlayerShrink", "PlayerGrow")]
        public SE_PlayerScaleData playerScale = new SE_PlayerScaleData();

        public StatusEffectEntry() { }

        public void OnNameChanged()
        {
            float ttl = FieldUIBuilder.LookupStatusEffectTTL(name);
            duration = ttl;
        }
    }

    internal class SE_PlayerScaleData: CloneableData
    {
        [EditorLabel("Scale Delta")]
        [EditorTooltip("The amount to increase or decrease the player's scale by.")]
        public float scaleDelta = 0.3f;

        [EditorLabel("Scale Min")]
        [EditorTooltip("The minimum scale the player can be reduced to.")]
        public float scaleMin = 0.4f;

        [EditorLabel("Scale Max")]
        [EditorTooltip("The maximum scale the player can be increased to.")]
        public float scaleMax = 4f;

        [EditorLabel("Scale Duration")]
        [EditorTooltip("The duration of the scale animation.")]
        public float scaleDuration = 0.5f;

        [EditorLabel("Speed/Jump Multiplier Delta")]
        [EditorTooltip("The amount to increase or decrease the player's speed/jump multiplier by.")]
        public float speedMultiplierDelta = 0.125f;

        [EditorLabel("Speed/Jump Multiplier Min")]
        [EditorTooltip("The minimum speed/jump multiplier the player can be reduced to.")]
        public float speedMultiplierMin = 0.75f;

        [EditorLabel("Speed/Jump Multiplier Max")]
        [EditorTooltip("The maximum speed/jump multiplier the player can be increased to.")]
        public float speedMultiplierMax = 2.25f;

        public SE_PlayerScaleData() { }
    }
}
