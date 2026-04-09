using Jotunn.Managers;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class MistHelper
    {
        public static void SpawnMist(MistData mistData, CustomRewardEvent customRewardEvent)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab("MistArea");

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find mist prefab to spawn");
                throw new System.Exception("Could not find mist prefab to spawn");
            }

            GameObject mist = UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            Mister mister = mist.GetComponent<Mister>();
            TwitchPersistentDestruction persistentDestruction = mist.GetComponent<TwitchPersistentDestruction>();
            persistentDestruction.SetStarted(mistData.duration);

            mister.m_height = mistData.height;
            mister.m_radius = mistData.radius;

            if (mistData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, mistData.announceMessage), 3000);
        }
    }
}
