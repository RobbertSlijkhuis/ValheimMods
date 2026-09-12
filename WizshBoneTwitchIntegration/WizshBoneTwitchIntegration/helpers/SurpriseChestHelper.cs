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

            // "Random" is used here instead of a fixed offset like "InFrontOfPlayer": both now
            // get the same floor+ceiling+obstruction validation, but "Random" also gets several
            // tries at different nearby spots before falling back to on-player, so one fixed
            // point landing past a thin dungeon wall/railing doesn't end the search early.
            Vector3 spawnPosition = TransformHelper.UpdateSpawnLocation(chestData.position, transform, chestData.positionOffset);

            Quaternion spawnRotation = chestData.facePlayer
                ? TransformHelper.FacePlayer(spawnPosition, transform.rotation)
                : TransformHelper.RandomFacing();

            GameObject chest = ZNetViewHelper.Instantiate(prefab, spawnPosition, spawnRotation);
            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }
    }
}
