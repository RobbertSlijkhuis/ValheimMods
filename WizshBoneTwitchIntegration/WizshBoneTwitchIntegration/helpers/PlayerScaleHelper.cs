using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class PlayerScaleHelper
    {
        private static float s_currentTargetScale = 1f;
        private static float s_currentLerpDuration = 0.3f;
        private static float s_currentSpeedMultiplier = 1f;
        private static PlayerSnapshot s_baseSnapshot;

        public static float LerpDuration => s_currentLerpDuration;
        public static float SpeedMultiplier => s_currentSpeedMultiplier;
        public static float CurrentScale => s_currentTargetScale;

        public static void Apply(string effectName, float duration, SE_PlayerScaleData scaleData)
        {
            bool isShrink = effectName == StatusEffectType.PlayerShrink;

            float scaleStep = isShrink ? -scaleData.scaleDelta : scaleData.scaleDelta;
            s_currentTargetScale = Mathf.Clamp(s_currentTargetScale + scaleStep, scaleData.scaleMin, scaleData.scaleMax);
            s_currentLerpDuration = scaleData.scaleDuration;

            float speedStep = isShrink ? -scaleData.speedMultiplierDelta : scaleData.speedMultiplierDelta;
            s_currentSpeedMultiplier = Mathf.Clamp(s_currentSpeedMultiplier + speedStep, scaleData.speedMultiplierMin, scaleData.speedMultiplierMax);

            Player player = Player.m_localPlayer;
            SEMan  seman  = player.GetSEMan();
            int    seHash = "PlayerScale".GetStableHashCode();

            if (!seman.HaveStatusEffect(seHash))
            {
                SE_PlayerScale se = WizshBoneTwitchIntegration.Instance.effects.PlayerScale;
                se.m_ttl = duration;
                seman.AddStatusEffect(se);
            }

            if (s_baseSnapshot == null)
                s_baseSnapshot = new PlayerSnapshot(player);

            ApplySpeedMultiplier(player);

            // Lerp to the new target — always start from current actual scale
            WizshBoneTwitchIntegration.Instance.StartCoroutine(
                LerpVisualScale(player, player.transform.localScale.x, s_currentTargetScale, s_currentLerpDuration));
        }

        public static IEnumerator LerpVisualScale(Player player, float from, float to, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                ApplyVisualScale(player, Mathf.Lerp(from, to, t));
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            ApplyVisualScale(player, to);
        }

        // Held/sheathed item visuals are parented under hand/back joints in the player's
        // hierarchy, so their rendered size already depends on whatever scale that hierarchy
        // applies. Rather than guessing, compute the local scale needed to make each item's
        // *world* scale come out to the target value.
        public static void ApplyVisualScale(Player player, float scale)
        {
            player.transform.localScale = Vector3.one * scale;

            VisEquipment vis = player.m_visEquipment;

            if (vis == null)
                return;

            SetWorldScale(vis.m_rightItemInstance, scale);
            SetWorldScale(vis.m_leftItemInstance, scale);
            SetWorldScale(vis.m_rightBackItemInstance, scale);
            SetWorldScale(vis.m_leftBackItemInstance, scale);
            SetWorldScale(vis.m_helmetItemInstance, scale);
            SetWorldScale(vis.m_hairItemInstance, scale);
            SetWorldScale(vis.m_beardItemInstance, scale);
        }

        public static void SetWorldScale(GameObject instance, float targetWorldScale)
        {
            if (instance == null)
                return;

            Transform transform = instance.transform;
            Vector3 parentLossy = transform.parent != null ? transform.parent.lossyScale : Vector3.one;

            transform.localScale = new Vector3(
                SafeDivide(targetWorldScale, parentLossy.x),
                SafeDivide(targetWorldScale, parentLossy.y),
                SafeDivide(targetWorldScale, parentLossy.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
        }

        private static void ApplySpeedMultiplier(Player player)
        {
            player.m_acceleration     = s_baseSnapshot.acceleration * s_currentSpeedMultiplier;
            player.m_speed            = s_baseSnapshot.speed * s_currentSpeedMultiplier;
            player.m_runSpeed         = s_baseSnapshot.runSpeed * s_currentSpeedMultiplier;
            player.m_walkSpeed        = s_baseSnapshot.walkSpeed * s_currentSpeedMultiplier;
            player.m_crouchSpeed      = s_baseSnapshot.crouchSpeed * s_currentSpeedMultiplier;
            player.m_swimAcceleration = s_baseSnapshot.swimAcceleration * s_currentSpeedMultiplier;
            player.m_swimSpeed        = s_baseSnapshot.swimSpeed * s_currentSpeedMultiplier;
            player.m_swimDepth        = s_baseSnapshot.swimDepth * s_currentSpeedMultiplier;
            player.m_jumpForce        = s_baseSnapshot.jumpForce * s_currentSpeedMultiplier;
            player.m_jumpForceForward = s_baseSnapshot.jumpForceForward * s_currentSpeedMultiplier;
        }

        public static void SnapToNormal()
        {
            s_currentTargetScale = 1f;
            s_currentSpeedMultiplier = 1f;

            if (Player.m_localPlayer != null)
            {
                ApplyVisualScale(Player.m_localPlayer, 1f);

                if (s_baseSnapshot != null)
                    s_baseSnapshot.Apply(Player.m_localPlayer);
            }

            s_baseSnapshot = null;
        }

        public static void ResetScale()
        {
            s_currentTargetScale = 1f;
            s_currentSpeedMultiplier = 1f;

            if (Player.m_localPlayer != null && s_baseSnapshot != null)
                s_baseSnapshot.Apply(Player.m_localPlayer);

            s_baseSnapshot = null;
        }
    }
}
