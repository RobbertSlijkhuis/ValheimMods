using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        private static readonly AccessTools.FieldRef<Character, bool> s_groundContactRef =
            AccessTools.FieldRefAccess<Character, bool>("m_groundContact");

        private static readonly FieldInfo s_firesField = AccessTools.Field(typeof(Fire), "s_fires");

        private static readonly AccessTools.FieldRef<Character, float> s_maxAirAltitudeRef =
            AccessTools.FieldRefAccess<Character, float>("m_maxAirAltitude");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "UpdateGroundContact")]
        public static void UpdateGroundContact_Prefix(Character __instance)
        {
            try
            {
                if (!__instance.IsPlayer() || !ReferenceEquals(__instance, Player.m_localPlayer))
                    return;

                if (!s_groundContactRef(__instance))
                    return;

                if (PlayerScaleHelper.CurrentScale <= 1f)
                    return;

                float speedMultiplier = PlayerScaleHelper.SpeedMultiplier;

                // Fall damage normally starts at 4m. Shift the whole fall-damage curve (both the
                // no-damage floor and the lethal ceiling) proportionally to the speed multiplier.
                float shift = 4f * speedMultiplier - 4f;
                s_maxAirAltitudeRef(__instance) -= shift;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in UpdateGroundContact_Prefix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), "Damage")]
        public static bool WearNTearDamage_Prefix(WearNTear __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return true;

                if (IndestructibleHelper.ShouldProtect(__instance.gameObject))
                    return false;

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

        // WearNTear.UpdateWear() (structural support loss, unroofed rain-rot, Ashlands decay) calls
        // ApplyDamage directly, bypassing Damage/RPC_Damage entirely - patch it too so indestructible
        // objects can't collapse from lack of support even though combat damage is already blocked above.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
        public static bool WearNTearApplyDamage_Prefix(WearNTear __instance)
        {
            try
            {
                if (__instance == null)
                    return true;

                return !IndestructibleHelper.ShouldProtect(__instance.gameObject);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in WearNTearApplyDamage_Prefix: " + e);
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Destructible), "Damage")]
        public static bool DestructibleDamage_Prefix(Destructible __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return true;

                return !IndestructibleHelper.ShouldProtect(__instance.gameObject);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DestructibleDamage_Prefix: " + e);
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
                if (!__result)
                    return;

                GameObject gameObject = Projectile.FindHitObject(collider);

                if (gameObject == null)
                    return;

                Character character = gameObject.GetComponent<Character>();

                if (character != null && DamageHelper.IsProtectedBossHit(__instance.gameObject, character))
                {
                    __result = false;
                    return;
                }

                TwitchPersistentDamage persistentDamage = __instance.gameObject.GetComponentInParent<TwitchPersistentDamage>();

                if (persistentDamage == null)
                    return;

                Ship ship = gameObject.GetComponent<Ship>();
                WearNTear wearNTear = gameObject.GetComponent<WearNTear>();

                if (ship != null && !persistentDamage.m_damageShips)
                {
                    //Jotunn.Logger.LogWarning("Prevent AOE damage to ship!");
                    __result = false;
                }

                else if (wearNTear != null && !persistentDamage.m_damageStructures)
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

                Character character = gameObject.GetComponent<Character>();

                if (character != null && DamageHelper.IsProtectedBossHit(__instance.gameObject, character))
                    return false;

                TwitchPersistentDamage persistentDamage = __instance.gameObject.GetComponentInParent<TwitchPersistentDamage>();

                if (persistentDamage != null)
                {
                    ship = gameObject.GetComponent<Ship>();
                    wearNTear = gameObject.GetComponent<WearNTear>();
                    piece = gameObject.GetComponent<Piece>();
                }
                else
                {
                    persistentDamage = gameObject.GetComponentInParent<TwitchPersistentDamage>();
                    ship = __instance.GetComponent<Ship>();
                    wearNTear = __instance.GetComponent<WearNTear>();
                    piece = __instance.GetComponent<Piece>();
                }

                if (persistentDamage == null || piece == null || piece.GetCreator() == 0)
                    return true;

                if (ship != null && !persistentDamage.m_damageShips)
                {
                    // Jotunn.Logger.LogWarning("Prevent IMPACT damage to ship! " + gameObject.name);
                    return false;
                }

                else if (wearNTear != null && !persistentDamage.m_damageStructures)
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Projectile), "IsValidTarget")]
        public static void IsValidTarget_Postfix(Projectile __instance, IDestructible destr, ref bool __result)
        {
            try
            {
                if (!__result)
                    return;

                if (destr is Character character && DamageHelper.IsProtectedBossHit(__instance.gameObject, character))
                    __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in IsValidTarget_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Fire), "DoDamage")]
        public static bool FireDoDamage_Prefix(Fire __instance, IDestructible toHit)
        {
            try
            {
                return !(toHit is Character character && DamageHelper.IsProtectedBossHit(__instance.gameObject, character));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in FireDoDamage_Prefix: " + e);
                return true;
            }
        }

        // Hop 1: when a WBTI projectile hits, propagate TwitchPersistentDamage to any Fire
        // objects spawned as hit effects (Fire A). Those fires are in Fire.s_fires by the time
        // this Postfix runs because m_hitEffects.Create() is synchronous.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Projectile), "OnHit")]
        public static void ProjectileOnHit_Postfix(Projectile __instance)
        {
            try
            {
                TwitchPersistentDamage persistentDamage = __instance.gameObject.GetComponentInParent<TwitchPersistentDamage>();

                if (persistentDamage == null)
                    return;

                Vector3 pos = __instance.transform.position;
                persistentDamage.RegisterHitPosition(pos);

                List<Fire> fires = (List<Fire>)s_firesField.GetValue(null);

                if (fires == null)
                    return;

                foreach (Fire fire in fires)
                {
                    if (Vector3.Distance(fire.transform.position, pos) <= 2f)
                        persistentDamage.PropagateToFire(fire);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("ProjectileOnHit_Postfix failed: " + e);
            }
        }

        // Hop 2: Fire A (which now has TwitchPersistentDamage) spawns a lingering terrain fire
        // (Fire B) synchronously via HitTerrain. Redo the same downward raycast to find the spawn
        // point and propagate to the new fire found in Fire.s_fires near that point.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fire), "HitTerrain")]
        public static void FireHitTerrain_Postfix(Fire __instance)
        {
            try
            {
                TwitchPersistentDamage persistentDamage = __instance.gameObject.GetComponentInParent<TwitchPersistentDamage>();

                if (persistentDamage == null)
                    return;

                if (!Physics.Raycast(__instance.transform.position, Vector3.down, out RaycastHit hitInfo, __instance.m_terrainMaxDist, LayerMask.GetMask("terrain")))
                    return;

                List<Fire> fires = (List<Fire>)s_firesField.GetValue(null);

                foreach (Fire fire in fires)
                {
                    if (fire == __instance || Vector3.Distance(fire.transform.position, hitInfo.point) > 0.5f)
                        continue;

                    persistentDamage.PropagateToFire(fire);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("FireHitTerrain_Postfix failed: " + e);
            }
        }
    }
}
