using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnOptions
    {
        public SpawnCreatureData creatureData;
        public CustomRewardEvent customReward;
        public bool IsUserInputRequired;
        public Transform transform;

        public SpawnOptions(SpawnCreatureData creatureData, Transform transform, CustomRewardEvent customReward = null)
        {
            this.creatureData = creatureData;
            this.customReward = customReward;
            this.transform = transform;
        }
    }
}
