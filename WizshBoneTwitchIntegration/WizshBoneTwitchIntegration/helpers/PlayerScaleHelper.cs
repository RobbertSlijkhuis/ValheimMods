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
        private static float s_baseAutoCloseDistance = -1f;
        private static InventoryGui s_autoCloseGui;

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
            ApplySwimDepth(player);
            ApplyReach(player);

            // Lerp to the new target — always start from current actual scale
            WizshBoneTwitchIntegration.Instance.StartCoroutine(
                LerpVisualScale(player, player.transform.localScale.x, s_currentTargetScale, s_currentLerpDuration));
        }

        public static IEnumerator LerpVisualScale(Player player, float from, float to, float duration)
        {
            float timeElapsed = 0f;

            // Other clients lerp their copy of this player to the same target (see TwitchPlayerScaleSync).
            TwitchPlayerScaleSync.Publish(player, to, duration);

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

        // The player's actual current size (mid-lerp included). Works for any player, local or
        // remote, since remote players are resized by TwitchPlayerScaleSync.
        public static bool TryGetVisualScale(VisEquipment vis, out float scale)
        {
            Player player = vis != null ? vis.GetComponentInParent<Player>() : null;

            scale = player != null ? player.transform.localScale.x : 1f;

            return player != null && scale != 1f;
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
            player.m_jumpForce        = s_baseSnapshot.jumpForce * s_currentSpeedMultiplier;
            player.m_jumpForceForward = s_baseSnapshot.jumpForceForward * s_currentSpeedMultiplier;
        }

        // How deep the water must be before the player swims (and how far under the surface they
        // float) is a body-size distance, not a speed. Tying it to the speed multiplier made a sped-up
        // player only start swimming in deeper and deeper water (x3 speed = ~5.6 m), so they waded
        // instead and never got their swim speed.
        private static void ApplySwimDepth(Player player)
        {
            player.m_swimDepth = s_baseSnapshot.swimDepth * s_currentTargetScale;
        }

        // Character.ApplyLiquidResistance slows wading by speed^2 (speed -= speed * speed * depth *
        // 0.05), which grows faster than the speed itself and eats a boosted speed (x3 walk speed can
        // end up slower than vanilla in water). The Harmony patch runs it on the unboosted speed and
        // scales the result back up, so water costs the same fraction of speed at any multiplier.
        // Only the local player's movement is simulated here; everyone else gets 1.
        public static float GetWadeSpeedFactor(Character character)
        {
            if (!ReferenceEquals(character, Player.m_localPlayer) || s_currentSpeedMultiplier <= 0f)
                return 1f;

            return s_currentSpeedMultiplier;
        }

        // Interact/place/pickup/auto-close distances are flat world units measured from the eye, which
        // rises with a grown player, so ground-level things (chests, items, boats) fall out of reach.
        // Scale them up with the player. Shrinking deliberately doesn't reduce reach (like jump fall
        // damage), hence the clamp at 1.
        private static void ApplyReach(Player player)
        {
            float reach = Mathf.Max(1f, s_currentTargetScale);

            player.m_maxInteractDistance = s_baseSnapshot.maxInteractDistance * reach;
            player.m_maxPlaceDistance     = s_baseSnapshot.maxPlaceDistance * reach;
            player.m_autoPickupRange      = s_baseSnapshot.autoPickupRange * reach;

            // An open container closes once the player is further than this away; it must grow with the
            // interact distance or a chest opened from the extended reach would close immediately.
            InventoryGui gui = InventoryGui.instance;

            if (gui == null)
                return;

            // The base belongs to one InventoryGui instance. A recreated instance (e.g. after a
            // world change) starts from its own default, so re-read instead of reusing a stale base.
            if (!ReferenceEquals(gui, s_autoCloseGui))
            {
                s_autoCloseGui = gui;
                s_baseAutoCloseDistance = gui.m_autoCloseDistance;
            }

            gui.m_autoCloseDistance = s_baseAutoCloseDistance * reach;
        }

        private static void RestoreAutoCloseDistance()
        {
            // Unity's null check is false for a destroyed instance, so a gone InventoryGui is skipped.
            if (s_autoCloseGui != null && s_baseAutoCloseDistance >= 0f)
                s_autoCloseGui.m_autoCloseDistance = s_baseAutoCloseDistance;

            s_autoCloseGui = null;
            s_baseAutoCloseDistance = -1f;
        }

        public static void SnapToNormal()
        {
            s_currentTargetScale = 1f;
            s_currentSpeedMultiplier = 1f;

            if (Player.m_localPlayer != null)
            {
                ApplyVisualScale(Player.m_localPlayer, 1f);
                TwitchPlayerScaleSync.Publish(Player.m_localPlayer, 1f, 0f);

                if (s_baseSnapshot != null)
                    s_baseSnapshot.Apply(Player.m_localPlayer);
            }

            RestoreAutoCloseDistance();
            s_baseSnapshot = null;
        }

        public static void ResetScale()
        {
            s_currentTargetScale = 1f;
            s_currentSpeedMultiplier = 1f;

            if (Player.m_localPlayer != null)
            {
                // Tell the other clients the target is back to normal even if the caller's own lerp
                // never publishes it - otherwise they keep showing the old size (the lerp coroutine
                // publishes the same values, so this is a no-op repeat in the normal path).
                TwitchPlayerScaleSync.Publish(Player.m_localPlayer, 1f, s_currentLerpDuration);

                if (s_baseSnapshot != null)
                    s_baseSnapshot.Apply(Player.m_localPlayer);
            }

            RestoreAutoCloseDistance();

            s_baseSnapshot = null;
        }
    }
}
