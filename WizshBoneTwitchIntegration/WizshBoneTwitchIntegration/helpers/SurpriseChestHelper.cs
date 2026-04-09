using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SurpriseChestHelper
    {
        public static void SpawnSupriseChest(GameObject prefab, SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            Transform transform = Player.m_localPlayer.transform;
            GameObject chest = UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);
            chest.transform.localPosition = TransformHelper.UpdateSpawnLocation(SpawnPositionType.InFrontOfPlayerHigh, chest.transform, new PositionOffsetData());
            chest.transform.localRotation = TransformHelper.UpdateSpawnRotation(SpawnPositionType.InFrontOfPlayerHigh, chest.transform);

            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }
    }
}
