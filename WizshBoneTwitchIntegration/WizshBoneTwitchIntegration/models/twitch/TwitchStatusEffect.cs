using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchStatusEffect
    {
        public float duration;
        public string name;
        public int nameHash;
        public List<string> blockedByStatusEffects = new List<string>();

        public delegate void onEndDelegate();
        public delegate void onStartDelegate(TwitchStatusEffect statusEffect);

        public onEndDelegate onEnd;
        public onStartDelegate onStart;

        public TwitchStatusEffect(string name, float duration)
        {
            this.duration = duration;
            this.name = name;
            this.nameHash = name.GetStableHashCode();
        }
    }
}
