using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class MonsterAIExtension
    {
        public static IEnumerator WakeUpAfterDelay(this MonsterAI monsterAI, float delay)
        {
            yield return new WaitForSeconds(delay);

            monsterAI.Wakeup();
        }
    }
}
