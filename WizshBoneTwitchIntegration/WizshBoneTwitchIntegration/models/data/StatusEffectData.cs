using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Models
{
    internal class StatusEffectData : CloneableData
    {
        [EditorLabel("Duration")]
        [EditorTooltip("The duration of the status effect in seconds. Set to 0 for infinite duration.")]
        public float duration = -1f;

        [EditorHidden]
        public float durationRemaining = -1f;

        [EditorHidden]
        public List<string> blockedBy = new List<string>();

        [EditorLabel("Name")]
        [EditorTooltip("The name of the status effect.")]
        [StatusEffectNameDropdown]
        public string name;

        [EditorHidden]
        public int nameHash = -1;

        [EditorLabel("Persists Through Death")]
        [EditorTooltip("Whether the status effect should persist through death. If true, the status effect will not be removed when the player dies.")]
        public bool persistsThroughDeath = false;

        [EditorLabel("Renew")]
        [EditorTooltip("Whether to renew the status effect if it is already active. If true, the duration will be reset to the original duration when the status effect is applied again.")]
        public bool renew = false;

        public delegate void onEndDelegate();
        public delegate void onStartDelegate(StatusEffectData statusEffect, bool restartFromDeath);

        public onEndDelegate onEnd;
        public onStartDelegate onStart;

        public StatusEffectData() { }

        public void Init()
        {
            string orignalName = StatusEffectHelper.GetByMeadID(name);
            durationRemaining = duration;

            if (orignalName != "")
            {
                name = orignalName;
            }

            nameHash = name.GetStableHashCode();
            onStart = OnStart;
        }

        public void OnStart(StatusEffectData statusEffect, bool restartFromDeath)
        {
            if (statusEffect.renew)
            {
                StatusEffect currentStatusEffect = Player.m_localPlayer.GetSEMan().GetStatusEffect(statusEffect.nameHash);

                if (currentStatusEffect != null)
                {
                    currentStatusEffect.m_ttl = statusEffect.duration;
                    currentStatusEffect.ResetTime();
                    return;
                }
            }

            StatusEffect original = ObjectDB.instance.GetStatusEffect(statusEffect.nameHash);

            if (original == null)
            {
                Jotunn.Logger.LogWarning($"Could not find the original for status effect {statusEffect.name}");
                return;
            }

            StatusEffect clone = original.Clone();

            if (durationRemaining != -1f)
                clone.m_ttl = restartFromDeath ? durationRemaining : duration;

            Player.m_localPlayer.GetSEMan().AddStatusEffect(clone);
        }
    }
}
