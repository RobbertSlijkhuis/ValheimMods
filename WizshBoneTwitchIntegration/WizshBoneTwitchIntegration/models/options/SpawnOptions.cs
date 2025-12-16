using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnOptions
    {
        public SpawnCreatureData creatureData;
        public CustomRewardEvent customReward;
        public bool ignoreWard;
        public Transform transform;

        public SpawnOptions(SpawnCreatureData creatureData, Transform transform, CustomRewardEvent customReward = null, bool ignoreWard = false)
        {
            this.creatureData = creatureData;
            this.customReward = customReward;
            this.ignoreWard = ignoreWard;
            this.transform = transform;
        }
    }
}
