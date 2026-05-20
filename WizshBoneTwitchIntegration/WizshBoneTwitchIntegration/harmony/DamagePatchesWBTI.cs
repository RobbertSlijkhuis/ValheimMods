using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

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

                Character character = hit.GetAttacker();

                if (character == null)
                    return true;

                TwitchCreaturePersistentData persistentData = character.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null)
                    return true;

                return persistentData.m_allowDamageStructures;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in WearNTearDamage_Prefix: " + e);
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static void CharacterDamage_Prefix(Character __instance, ref HitData hit)
        {
            try
            {
                Character attacker = hit.GetAttacker();

                if (attacker == null)
                    return;

                TwitchCreatureClaim creatureClaim = attacker.gameObject.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim == null || !creatureClaim.m_isSpawn)
                    return;

                TwitchCreaturePersistentData persistentData = attacker.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null)
                    return;

                ValheimCreature creature = CreatureHelper.GetValheimCreature(attacker.gameObject.name);

                if (creature == null)
                    return;

                float playerTier = ProgressionHelper.GetPlayerTier();
                float damageScale = persistentData.m_damageScale != 0 ? persistentData.m_damageScale : PluginConfig.configCreaturesdamageScale.Value;
                float scale = CreatureHelper.CalculateScale(playerTier, creature.tier, damageScale);
                CreatureHelper.ScaleHitDamage(ref hit, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in CharacterDamage_Prefix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Aoe), "ShouldHit")]
        public static void ShouldHit_Postfix(Aoe __instance, ref bool __result, Collider collider)
        {
            try
            {
                TwitchAllowDamage allowDamage = __instance.gameObject.GetComponent<TwitchAllowDamage>();

                if (allowDamage == null)
                    return;

                GameObject gameObject = Projectile.FindHitObject(collider);

                if (gameObject == null)
                    return;
                
                Ship ship = gameObject.GetComponent<Ship>();
                WearNTear wearNTear = gameObject.GetComponent<WearNTear>();

                if (ship != null && !allowDamage.m_allowDamageShips)
                {
                    //Jotunn.Logger.LogWarning("Prevent AOE damage to ship!");
                    __result = false;
                }

                else if (wearNTear != null && !allowDamage.m_allowDamageStructures)
                {
                    //Jotunn.Logger.LogWarning("Prevent AOE damage to structures!");
                    __result = false;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnHit_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ImpactEffect), "OnCollisionEnter")]
        public static bool OnCollisionEnter_Prefix(ImpactEffect __instance, Collision info)
        {
            try
            {
                ContactPoint contactPoint = info.contacts[0];
                GameObject gameObject = Projectile.FindHitObject(contactPoint.otherCollider);
                Ship ship = null;
                WearNTear wearNTear = null;
                Piece piece = null;

                if (gameObject == null)
                    return true;

                TwitchAllowDamage allowDamage = __instance.gameObject.GetComponent<TwitchAllowDamage>();

                if (allowDamage != null)
                {
                    ship = gameObject.GetComponent<Ship>();
                    wearNTear = gameObject.GetComponent<WearNTear>();
                    piece = gameObject.GetComponent<Piece>();
                }
                else
                {
                    allowDamage = gameObject.gameObject.GetComponent<TwitchAllowDamage>();
                    ship = __instance.GetComponent<Ship>();
                    wearNTear = __instance.GetComponent<WearNTear>();
                    piece = __instance.GetComponent<Piece>();
                }

                if (allowDamage == null || piece == null || piece.GetCreator() == 0)
                    return true;

                if (ship != null && !allowDamage.m_allowDamageShips)
                {
                    // Jotunn.Logger.LogWarning("Prevent IMPACT damage to ship! " + gameObject.name);
                    return false;
                }

                else if (wearNTear != null && !allowDamage.m_allowDamageStructures)
                {
                    // Jotunn.Logger.LogWarning("Prevent IMPACT damage to structures! " + gameObject.name);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnCollisionEnter_Prefix: " + e);
                return true;
            }
        }
    }
}
