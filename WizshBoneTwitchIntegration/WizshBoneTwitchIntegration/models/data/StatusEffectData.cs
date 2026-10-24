using System.Collections.Generic;

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
        public string name;

        public float duration = -1f;

        public bool persistsThroughDeath = false;

        public bool renew = false;

        public SE_PlayerScaleData playerScale = new SE_PlayerScaleData();

        public StatusEffectEntry() { }
    }

    internal class SE_PlayerScaleData: CloneableData
    {
        public float scaleDelta = 0.3f;

        public float scaleMin = 0.4f;

        public float scaleMax = 4f;

        public float scaleDuration = 0.5f;

        public float speedMultiplierDelta = 0.125f;

        public float speedMultiplierMin = 0.75f;

        public float speedMultiplierMax = 2.25f;

        public SE_PlayerScaleData() { }
    }
}
