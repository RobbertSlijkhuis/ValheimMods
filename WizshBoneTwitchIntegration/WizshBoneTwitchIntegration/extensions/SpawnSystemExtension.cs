using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class SpawnSystemExtension
    {
        public static int GetNrOfTwitchInstances(this SpawnSystem spawnSystem, float maxRange = 100f)
        {
            int num = 0;

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                if (maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                {
                    continue;
                }

                num++;
            }

            return num;
        }
    }
}
