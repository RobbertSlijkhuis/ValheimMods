using Glimuleikar.Configs;
using HarmonyLib;
using System;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class DamagePatchesWBTI
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), "Damage")]
        public static bool WearNTearDamage_Prefix(WearNTear __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return true;

                Character attacker = hit.GetAttacker();

                if (attacker == null)
                    return true;

                return PluginConfig.configDamageStructuresEnable.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in WearNTearDamage_Prefix: " + e);
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static bool CharacterDamage_Prefix(Character __instance, ref HitData hit)
        {
            try
            {
                if (hit.m_hitType == HitData.HitType.Fall)
                    return PluginConfig.configDamageFallEnable.Value;

                Character attacker = hit.GetAttacker();

                if (attacker == null)
                    return true;

                if (hit.m_hitType == HitData.HitType.PlayerHit && !PluginConfig.configDamagePlayersEnable.Value)
                {
                    hit.m_damage = new HitData.DamageTypes();
                    return true;
                }

                if (hit.m_hitType == HitData.HitType.EnemyHit)
                {
                    return PluginConfig.configDamageMonstersEnable.Value;
                }

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in CharacterDamage_Prefix: " + e);
                return true;
            }
        }
    }
}
