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

            Vector3 spawnPosition = TransformHelper.UpdateSpawnLocation(chestData.position, transform, chestData.positionOffset);
            Quaternion spawnRotation = TransformHelper.UpdateSpawnRotation(chestData.position, transform.rotation);

            // "InFrontOfPlayer" only offsets from the player's own height, so on stairs/slopes
            // (common in dungeons) the chest can end up floating or clipped into geometry.
            // Snap it onto the actual floor beneath that spot, without changing the X/Z placement
            // or the existing fallback-to-on-player behavior when the spot is obstructed.
            bool indoor = Player.m_localPlayer.InInterior();

            if (TransformHelper.TryGetGroundHeight(spawnPosition, transform.position.y, indoor, out float height))
                spawnPosition.y = height + chestData.positionOffset.y;

            GameObject chest = UnityEngine.Object.Instantiate(prefab, spawnPosition, spawnRotation);
            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }
    }
}
