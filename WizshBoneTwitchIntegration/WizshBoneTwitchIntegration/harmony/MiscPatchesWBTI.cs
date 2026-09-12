using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class MiscPatchesWBTI
    {
        private static readonly FieldInfo s_instancesField = AccessTools.Field(typeof(ZNetScene), "m_instances");
        private static float s_nextStaleInstanceScanTime;
        private const float StaleInstanceScanInterval = 1f;

        // Defensive + diagnostic. ZNetScene.RemoveObjects() NREs (UnityEngine.Component.get_gameObject(),
        // called on m_instances.Values) whenever m_instances still holds a ZNetView whose GameObject was
        // already destroyed through a path that skipped ZNetScene.Destroy()'s own m_instances.Remove()
        // step - once that happens it repeats every tick forever (~30/s), since the exception aborts
        // before vanilla ever reaches its own cleanup. Root cause not yet confirmed (traced through
        // several destroy paths without finding the exact one); sanitize any such stale entries before
        // vanilla iterates them, and log which prefab it was so a recurrence can be traced to its source
        // instead of just crashing on every subsequent tick. Throttled - this walks the entire tracked-
        // object dictionary, which isn't free to do unconditionally 30x/second on top of vanilla's own
        // per-tick reconciliation.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZNetScene), "RemoveObjects")]
        public static void RemoveObjects_Prefix(ZNetScene __instance)
        {
            try
            {
                if (Time.time < s_nextStaleInstanceScanTime || s_instancesField == null)
                    return;

                s_nextStaleInstanceScanTime = Time.time + StaleInstanceScanInterval;

                if (!(s_instancesField.GetValue(__instance) is Dictionary<ZDO, ZNetView> instances) || instances.Count == 0)
                    return;

                List<ZDO> stale = null;

                foreach (KeyValuePair<ZDO, ZNetView> entry in instances)
                {
                    if (entry.Value == null)
                        (stale ??= new List<ZDO>()).Add(entry.Key);
                }

                if (stale == null)
                    return;

                foreach (ZDO zdo in stale)
                {
                    GameObject prefab = __instance.GetPrefab(zdo.GetPrefab());
                    Jotunn.Logger.LogWarning($"[WBTI] Found a destroyed ZNetView still tracked by ZNetScene (prefab: {(prefab != null ? prefab.name : zdo.GetPrefab().ToString())}) - removing it to prevent a NullReferenceException in ZNetScene.RemoveObjects.");
                    instances.Remove(zdo);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("RemoveObjects_Prefix failed: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
        public static void GlobalKeyAdd_Postfix(ref ZoneSystem __instance, string keyStr, bool canSaveToServerOptionKeys = true)
        {
            try
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                string loweredKey = keyStr.ToLower();

                if (!auth || !auth.m_loggedIn || (!loweredKey.Contains("defeated_") && !loweredKey.Contains("killed")))
                    return;

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                if (customRewards.m_enabled)
                    customRewards.SetRewards();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update rewards on GlobalKeyAdd_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Emote), "DoEmote")]
        public static void DoEmote_Postfix(Emotes emote)
        {
            try
            {
                if (emote == Emotes.ComeHere)
                    CreatureHelper.SetFollowInRadius(true);
                else if (emote == Emotes.NoNoNo)
                    CreatureHelper.SetFollowInRadius(false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DoEmote_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "Command")]
        public static void TameableCommand_Postfix(Tameable __instance)
        {
            try
            {
                TwitchCreaturePersistentData persistentData = __instance.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null)
                    return;

                bool isNowFollowing = __instance.m_monsterAI.GetFollowTarget() != null;
                persistentData.SetFollowing(isNowFollowing);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in TameableCommand_Postfix: " + e);
            }
        }

        // Player.OnDeath() (protected, patched by string name) returns early for non-owners per
        // decompiled Character.OnDeath()'s IsOwner() guard, so this only ever runs anything for the
        // client that owns the dying Player; the __instance == Player.m_localPlayer check below is
        // still kept as a second, cheap guard.
        //
        // This is the trigger point for TwitchSafeZone.HandleLocalPlayerDeath(): Game._RequestRespawn()
        // destroys the player GameObject ~10s after death (possibly still standing inside a safe
        // zone's trigger at that point), and Unity never fires OnTriggerExit for a destroyed
        // collider - so without this, the local player's safe-zone bookkeeping
        // (s_localPlayerZoneCount/m_playerIsInSafeZone) would stay stuck if they died in one.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnDeath")]
        public static void OnDeath_Postfix(Player __instance)
        {
            try
            {
                if (__instance == Player.m_localPlayer)
                    TwitchSafeZone.HandleLocalPlayerDeath();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Player.OnDeath_Postfix: " + e);
            }
        }
    }
}
