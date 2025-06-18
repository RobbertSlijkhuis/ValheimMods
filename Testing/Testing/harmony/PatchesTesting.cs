using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Numerics;
using Testing.Configs;
using UnityEngine;

namespace Testing.Harmony
{
    [HarmonyPatch]
    public class PatchesTesting
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void Postfix(ref Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return;

                if (hit.m_statusEffectHash != Testing.barlyWineSplashHash && hit.m_statusEffectHash != Testing.frostResistSplashHash && hit.m_statusEffectHash != Testing.poisonResistSplashHash)
                    return;

                if (__instance.GetSEMan().HaveStatusEffect(Testing.barlyWineSplashHash))
                {
                    __instance.GetSEMan().RemoveStatusEffect(Testing.barlyWineSplashHash);
                    __instance.GetSEMan().AddStatusEffect(Testing.barlyWineSplashHash);
                }

                if (__instance.GetSEMan().HaveStatusEffect(Testing.frostResistSplashHash))
                {
                    __instance.GetSEMan().RemoveStatusEffect(Testing.frostResistSplashHash);
                    __instance.GetSEMan().AddStatusEffect(Testing.frostResistSplashHash);
                }

                if (__instance.GetSEMan().HaveStatusEffect(Testing.poisonResistSplashHash))
                {
                    __instance.GetSEMan().RemoveStatusEffect(Testing.poisonResistSplashHash);
                    __instance.GetSEMan().AddStatusEffect(Testing.poisonResistSplashHash);
                }

            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }
    }
}
