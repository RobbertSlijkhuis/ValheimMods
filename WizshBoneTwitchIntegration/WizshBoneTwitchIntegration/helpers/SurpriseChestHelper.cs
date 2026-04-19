using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SurpriseChestHelper
    {
        public static void SpawnSupriseChest(GameObject prefab, SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            Transform transform = Player.m_localPlayer.transform;
            Vector3 spawnPosition = TransformHelper.UpdateSpawnLocation(chestData.position, transform.transform, chestData.positionOffset);
            Quaternion spawnRotation = TransformHelper.UpdateSpawnRotation(chestData.position, transform.transform.rotation);
            GameObject chest = UnityEngine.Object.Instantiate(prefab, spawnPosition, spawnRotation);
            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }
    }
}
