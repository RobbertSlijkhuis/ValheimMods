using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Components
{
    // Attached only to redeem-spawned Ships (see SpawnAbilityExtension.cs / PersistentComponentFlags.ShipIdlePhysics)
    // - never to player-built boats, which have no such component so ShipPatchesWBTI's Harmony prefix
    // is a no-op for them. Throttles Ship.CustomFixedUpdate to every Nth physics tick while nobody is
    // actively steering, since vanilla runs its full buoyancy simulation (5-point water sampling, up to
    // 6 AddForceAtPosition impulses, unconditional Rigidbody.WakeUp()) every tick regardless of
    // occupancy - expensive with many boats piled up at once (e.g. Boatpocalypse).
    //
    // Purely local, per-client bookkeeping - no ZDO state. Every client decides its own cadence
    // independently; the only cross-client-sensitive input (ShipControlls.HaveValidUser()) is already
    // ZDO-backed by vanilla, so every client reaches the same "controlled?" answer regardless.
    internal class TwitchShipIdlePhysicsData : MonoBehaviour
    {
        // How far above the water Ship's own buoyancy force block considers "no need to apply force"
        // (Ship.m_disableLevel) plus a margin - past this, fully skip rather than throttle, since
        // nothing useful happens yet (still falling, or resting on land/another hull in the pile) and
        // letting an occasional large-dt tick through here would corrupt Ship's own
        // m_lastDepth/m_lastUpdateWaterForceTime water-impact-damage bookkeeping with a false reading.
        private const float FarAboveWaterMargin = 3f;

        // The distance check is itself cheap (Floating.GetWaterLevel has a cached fast path) but
        // there's no reason to re-sample every tick while nothing's changing quickly.
        private const float DistanceCheckInterval = 0.25f;

        private Ship m_ship;
        private WaterVolume m_waterVolumeCache;
        private float m_distanceCheckTimer;
        private bool m_nearWater;

        private float m_accumulatedDeltaTime;
        private int m_skippedTicks;

        public void Awake()
        {
            m_ship = GetComponent<Ship>();
        }

        // Called from ShipPatchesWBTI.Ship_CustomFixedUpdate_Prefix. Returns true to let the original
        // Ship.CustomFixedUpdate run this tick, false to skip it entirely. On a throttled "catch-up"
        // tick, fixedDeltaTime is rewritten to the accumulated elapsed time across the skipped ticks so
        // the force integration still accounts for the real elapsed time instead of under-correcting.
        public bool ShouldRunTick(ref float fixedDeltaTime)
        {
            try
            {
                if (m_ship == null)
                    return true;

                ShipControlls controlls = m_ship.m_shipControlls;

                // HaveValidUser() only becomes true via an explicit, successful helm interaction
                // (RequestControl/RequestRespons -> ZDOVars.s_user) - unlike HasPlayerOnboard(), it
                // doesn't churn from players getting shoved between hulls during the landing pile-up,
                // so it's safe to snap on/off immediately without waiting for a scheduled tick.
                if (controlls != null && controlls.HaveValidUser())
                {
                    m_accumulatedDeltaTime = 0f;
                    m_skippedTicks = 0;
                    return true;
                }

                m_distanceCheckTimer -= fixedDeltaTime;
                if (m_distanceCheckTimer <= 0f)
                {
                    m_distanceCheckTimer = DistanceCheckInterval;

                    float waterLevel = Floating.GetWaterLevel(m_ship.transform.position, ref m_waterVolumeCache);
                    float heightAboveWater = m_ship.transform.position.y - waterLevel - m_ship.m_waterLevelOffset;
                    m_nearWater = heightAboveWater <= m_ship.m_disableLevel + FarAboveWaterMargin;
                }

                if (!m_nearWater)
                    return false;

                int throttle = Mathf.Max(1, PluginConfig.configShipIdlePhysicsThrottle.Value);

                m_accumulatedDeltaTime += fixedDeltaTime;

                if (++m_skippedTicks < throttle)
                    return false;

                fixedDeltaTime = m_accumulatedDeltaTime;
                m_accumulatedDeltaTime = 0f;
                m_skippedTicks = 0;
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchShipIdlePhysicsData.ShouldRunTick failed: " + e);
                return true;
            }
        }
    }
}
