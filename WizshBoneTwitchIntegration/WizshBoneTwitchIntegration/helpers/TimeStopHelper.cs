using System;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class TimeStopHelper
    {
        private static bool s_playerFrozen = false;
        private static Rigidbody s_playerRb;
        private static Animator s_playerAnimator;
        private static RigidbodyConstraints s_originalConstraints;

        public static bool IsPlayerFrozen => s_playerFrozen;

        public static void Apply(TimeStopData data, CustomRewardEvent rewardEvent)
        {
            if (data == null || Player.m_localPlayer == null)
                return;

            GameObject prefab = WizshBoneTwitchIntegration.Instance.prefabs.TimeStopZone;

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find TimeStopZone prefab to spawn");
                return;
            }

            // If the redeemer is steering a boat or riding a tamed creature, anchor the zone to
            // that vehicle instead of the player so it follows along rather than being left behind.
            Ship controlledShip = Player.m_localPlayer.GetControlledShip();
            Character riddenCharacter = Player.m_localPlayer.IsRiding()
                ? (Player.m_localPlayer.GetDoodadController() as Sadle)?.GetCharacter()
                : null;
            GameObject attachTarget = controlledShip != null ? controlledShip.gameObject : riddenCharacter?.gameObject;

            Vector3 spawnPosition = attachTarget != null ? attachTarget.transform.position : Player.m_localPlayer.transform.position;
            ZDOID attachZdoid = attachTarget?.GetComponent<ZNetView>()?.GetZDO().m_uid ?? ZDOID.None;

            GameObject zone = ZNetViewHelper.Instantiate(prefab, spawnPosition, Quaternion.identity);

            // Unlike mist/weather, a time stop zone can freeze the player - it must never be allowed
            // to run indefinitely, so a duration <= 0 falls back to the default instead of being
            // treated as "forever".
            float duration = data.duration > 0f ? data.duration : 10f;

            TwitchPersistentDestruction persistentDestruction = zone.GetComponent<TwitchPersistentDestruction>();
            persistentDestruction.SetStarted((int)duration);

            TwitchTimeStopZone zoneScript = zone.GetComponent<TwitchTimeStopZone>();
            zoneScript.Initialize(data.radius, data.freezeEnemies, data.freezePlayer, data.freezeProjectiles, duration, attachZdoid);

            if (!string.IsNullOrEmpty(data.announceMessage))
                Player.m_localPlayer.Message(
                    MessageHud.MessageType.Center,
                    MessageHelper.ParseVariables("{{user}}", rewardEvent.RedeemerName, data.announceMessage));
        }

        public static void FreezePlayer()
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

                // Disabling the Animator (rather than just zeroing speed) is required: with speed = 0
                // the state machine still evaluates zero-duration transitions on Update(), snapping
                // the player to their Idle state pose instead of holding the current frame.
                if (s_playerAnimator != null)
                    s_playerAnimator.enabled = false;

                s_playerFrozen = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopHelper.FreezePlayer failed: " + e);
            }
        }

        public static void UnfreezePlayer()
        {
            try
            {
                s_playerFrozen = false;

                if (s_playerRb != null)
                    s_playerRb.constraints = s_originalConstraints;

                if (s_playerAnimator != null)
                    s_playerAnimator.enabled = true;

                s_playerRb       = null;
                s_playerAnimator = null;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopHelper.UnfreezePlayer failed: " + e);
            }
        }
    }
}
