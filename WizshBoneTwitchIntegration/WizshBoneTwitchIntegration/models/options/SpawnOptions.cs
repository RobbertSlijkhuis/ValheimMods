using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnOptions
    {
        public SpawnCreatureData creatureData;
        public CustomRewardEvent customReward;
        public string prefabName;
        public Transform transform;

        public SpawnOptions(string prefabName, Transform transform, SpawnCreatureData creatureData, CustomRewardEvent customReward)
        {
            this.creatureData = creatureData;
            this.customReward = customReward;
            this.prefabName = prefabName;
            this.transform = transform;
        }
    }
}
