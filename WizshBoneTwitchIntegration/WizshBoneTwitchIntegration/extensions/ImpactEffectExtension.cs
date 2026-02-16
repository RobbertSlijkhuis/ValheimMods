using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class ImpactEffectExtension
    {
        public static IEnumerator ResetShowerSettings(this ImpactEffect impactEffect, DamageData damageData)
        {
            yield return new WaitForSeconds(2.5f);

            impactEffect.m_damages = DamageHelper.ConvertToDamageTypes(damageData);
        }
    }
}
