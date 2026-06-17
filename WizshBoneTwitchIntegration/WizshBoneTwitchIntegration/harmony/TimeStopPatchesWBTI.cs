using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class TimeStopPatchesWBTI
    {
        private static bool s_playerFrozen = false;
        private static Rigidbody s_playerRb;
        private static Animator s_playerAnimator;
        private static RigidbodyConstraints s_originalConstraints;

        // Register RPCs as soon as ZNetScene is ready so all clients have the handler before
        // any time stop RPC can arrive.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        public static void ZNetScene_Awake_Postfix()
        {
            try
            {
                TimeStopHelper.RegisterRPCs();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopPatchesWBTI: Failed to register RPCs: " + e);
            }
        }

        // Block player input while frozen so the movement system can't fight the frozen rigidbody.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
        public static bool PlayerController_FixedUpdate_Prefix()
        {
            return !s_playerFrozen;
        }

        public static void FreezePlayer(float duration)
        {
            try
            {
                if (Player.m_localPlayer == null)
                    return;

                s_playerRb       = Player.m_localPlayer.GetComponent<Rigidbody>();
                s_playerAnimator = Player.m_localPlayer.GetComponentInChildren<Animator>();

                if (s_playerRb != null)
                {
                    s_originalConstraints       = s_playerRb.constraints;
                    s_playerRb.velocity         = Vector3.zero;
                    s_playerRb.angularVelocity  = Vector3.zero;
                    s_playerRb.constraints      = RigidbodyConstraints.FreezeAll;
                }

                if (s_playerAnimator != null)
                    s_playerAnimator.speed = 0f;

                s_playerFrozen = true;

                WizshBoneTwitchIntegration.Instance.StartCoroutine(UnfreezePlayerAfter(duration));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopPatchesWBTI.FreezePlayer failed: " + e);
            }
        }

        private static IEnumerator UnfreezePlayerAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            UnfreezePlayer();
        }

        private static void UnfreezePlayer()
        {
            try
            {
                s_playerFrozen = false;

                if (s_playerRb != null)
                    s_playerRb.constraints = s_originalConstraints;

                if (s_playerAnimator != null)
                    s_playerAnimator.speed = 1f;

                s_playerRb       = null;
                s_playerAnimator = null;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopPatchesWBTI.UnfreezePlayer failed: " + e);
            }
        }
    }
}
