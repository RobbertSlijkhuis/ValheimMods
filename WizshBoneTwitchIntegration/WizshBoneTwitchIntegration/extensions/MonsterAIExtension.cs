using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class MonsterAIExtension
    {
        public static bool SetFollowPlayer(this MonsterAI monsterAI, GameObject gameObject)
        {
            TwitchCreaturePersistentData persistentCreatureData = monsterAI.gameObject.GetComponent<TwitchCreaturePersistentData>();

            if (persistentCreatureData != null)
                persistentCreatureData.m_isFollowing = gameObject != null ? true : false;

            monsterAI.m_follow = gameObject;
            return true;
        }

        public static IEnumerator WakeUpAfterDelay(this MonsterAI monsterAI, float delay)
        {
            yield return new WaitForSeconds(delay);

            monsterAI.Wakeup();
        }
    }
}
