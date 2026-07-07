using HarmonyLib;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class TimeStopPatchesWBTI
    {
        // Freezes ANY Aoe (not just Twitch-spawned hazards - monster attacks, thorns, any vanilla
        // area effect) the instant it spawns inside an already-active Time Stop zone. Aoe.Awake()
        // (private - patched by string name) is the earliest point m_activationTimer etc. are
        // initialized, running synchronously during Instantiate() before any physics step could
        // process a trigger/collision on it. See TwitchPhysicsFreezeData.FreezeDamage for why a
        // later poll-only approach isn't fast enough on its own. Resolves to transform.root so
        // multi-Aoe hazards (e.g. Smite's rod + area) share one wrapper rather than getting a
        // separate one each.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Aoe), "Awake")]
        public static void Aoe_Awake_Postfix(Aoe __instance)
        {
            if (!TwitchTimeStopZone.IsPositionFrozen(__instance.transform.position))
                return;

            GameObject root = __instance.transform.root.gameObject;
            TwitchPhysicsFreezeData freezeData = root.GetComponent<TwitchPhysicsFreezeData>() ?? root.AddComponent<TwitchPhysicsFreezeData>();
            freezeData.FreezeDamage();

            Jotunn.Logger.LogWarning($"[WBTI] Aoe.Awake froze damage on {root.name} at {__instance.transform.position}");
        }

        // Block player input while frozen so the movement system can't fight the frozen rigidbody.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
        public static bool PlayerController_FixedUpdate_Prefix()
        {
            return !TimeStopHelper.IsPlayerFrozen;
        }

        // Player.Update() calls StopDoodadControl() directly off the "Use" key press (letting go
        // of a ship's wheel or a saddle) with no check for anything like our freeze - without this,
        // a frozen player could still voluntarily release the boat/tame they're attached to.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), nameof(Player.StopDoodadControl))]
        public static bool Player_StopDoodadControl_Prefix(Player __instance)
        {
            return !(TimeStopHelper.IsPlayerFrozen && __instance == Player.m_localPlayer);
        }

        // Character.UpdateRotation() (private - patched by string name) directly overwrites
        // transform.rotation to face the camera/look direction whenever AlwaysRotateCamera() is
        // true (mid-attack, drawing a bow, blocking, etc.), completely independent of
        // PlayerController and of the Rigidbody's FreezeAll constraints (a script assigning
        // transform.rotation bypasses physics constraints, which only stop torque-driven
        // rotation). Without this, a player frozen mid-attack could still turn to face wherever
        // they're looking.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "UpdateRotation")]
        public static bool Character_UpdateRotation_Prefix(Character __instance, ref float __result)
        {
            if (__instance != Player.m_localPlayer || !TimeStopHelper.IsPlayerFrozen)
                return true;

            __result = 0f;
            return false;
        }

        // Ship and Floating (boats, logs, driftwood, other buoyant props) drive their physics via
        // IMonoUpdater.CustomFixedUpdate, which Valheim's own update dispatcher calls directly -
        // disabling the component or freezing the Rigidbody alone doesn't stop them, since they
        // keep repositioning/re-tilting every frame regardless of "enabled".
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ship), nameof(Ship.CustomFixedUpdate))]
        public static bool Ship_CustomFixedUpdate_Prefix(Ship __instance)
        {
            return __instance.GetComponentInParent<TwitchPhysicsFreezeData>() == null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floating), nameof(Floating.CustomFixedUpdate))]
        public static bool Floating_CustomFixedUpdate_Prefix(Floating __instance)
        {
            return __instance.GetComponentInParent<TwitchPhysicsFreezeData>() == null;
        }

        // ZSyncTransform is the generic networked-Rigidbody sync component present on virtually
        // every synced physics object (ships, floating debris, etc.) - it manages the Rigidbody's
        // kinematic/velocity state itself based on network ownership every frame via IMonoUpdater.
        // Left unpatched, it fights our freeze (re-clearing isKinematic) and produces a slow drift.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZSyncTransform), nameof(ZSyncTransform.CustomFixedUpdate))]
        public static bool ZSyncTransform_CustomFixedUpdate_Prefix(ZSyncTransform __instance)
        {
            return __instance.GetComponentInParent<TwitchPhysicsFreezeData>() == null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZSyncTransform), nameof(ZSyncTransform.CustomLateUpdate))]
        public static bool ZSyncTransform_CustomLateUpdate_Prefix(ZSyncTransform __instance)
        {
            return __instance.GetComponentInParent<TwitchPhysicsFreezeData>() == null;
        }
    }
}
