using System.Collections.Generic;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Models
{
    internal class StatusEffectData
    {
        public float duration = -1f;
        public float durationRemaining = -1f;
        public List<string> blockedBy = new List<string>();
        public string name;
        public int nameHash = -1;
        public bool persistsThroughDeath = false;
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
            //Jotunn.Logger.LogWarning("restartFromDeath: " + restartFromDeath);
            //Jotunn.Logger.LogWarning("Duration: " + statusEffect.duration);
            //Jotunn.Logger.LogWarning("Remaining: " + statusEffect.durationRemaining);
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
