using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnOptions
    {
        public CreatureData creatureData;
        public CustomRewardEvent customReward;
        public bool ignoreWard;
        public Transform transform;

        public SpawnOptions(CreatureData creatureData, Transform transform, CustomRewardEvent customReward = null, bool ignoreWard = false)
        {
            this.creatureData = creatureData;
            this.customReward = customReward;
            this.ignoreWard = ignoreWard;
            this.transform = transform;
        }
    }
}
