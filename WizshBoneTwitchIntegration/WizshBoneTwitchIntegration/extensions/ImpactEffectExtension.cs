using System.Collections;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class ImpactEffectExtension
    {
        public static IEnumerator ResetShowerSettings(this ImpactEffect impactEffect)
        {
            yield return new WaitForSeconds(2.5f);

            impactEffect.m_damages.m_blunt = 50f;
            impactEffect.m_damages.m_chop = 30f;
        }
    }
}
