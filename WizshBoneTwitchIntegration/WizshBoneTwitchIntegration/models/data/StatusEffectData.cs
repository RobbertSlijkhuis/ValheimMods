using System.Collections.Generic;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Models
{
    internal class StatusEffectData
    {
        public float duration = -1f;
        public List<string> blockedBy = new List<string>();
        public string name;
        public int nameHash = -1;
        public bool persistsThroughDeath = false;
        public bool renew = false;

        public delegate void onEndDelegate();
        public delegate void onStartDelegate(StatusEffectData statusEffect);

        public onEndDelegate onEnd;
        public onStartDelegate onStart;

        public StatusEffectData() { }

        public void Init() 
        {
            nameHash = name.GetStableHashCode();
            onStart = OnStart;
        }

        public void OnStart(StatusEffectData statusEffect)
        {
            StatusEffect original = ObjectDB.instance.GetStatusEffect(statusEffect.nameHash);
            StatusEffect clone = original.Clone();

            if (duration != -1f)
                clone.m_ttl = duration;

            if (renew)
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect(clone);

            Player.m_localPlayer.GetSEMan().AddStatusEffect(clone);
        }
    }
}
