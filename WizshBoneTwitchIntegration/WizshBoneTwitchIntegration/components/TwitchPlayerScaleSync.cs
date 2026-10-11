using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    // Replicates the grow/shrink player resize (PlayerScaleHelper) to the other clients. Valheim
    // doesn't sync transform.localScale, so without this a resized player only looks resized to
    // themselves.
    //
    // Same replicate-through-the-ZDO pattern as the other networked components (no RPC): the owning
    // player publishes the target scale and lerp duration via Publish() whenever they change, and
    // every other client's copy of that player lerps its own visuals to the replicated target here.
    // Attached to every Player (see EntityScalePatchesWBTI.PlayerAwake_Postfix).
    internal class TwitchPlayerScaleSync : MonoBehaviour
    {
        public static readonly int TargetScaleHash = "WBTI_PlayerScale".GetStableHashCode();
        public static readonly int DurationHash = "WBTI_PlayerScaleDur".GetStableHashCode();

        private Player m_player;
        private ZNetView m_netView;

        // Seconds between ZDO reads of the replicated target (see Update).
        private const float PollInterval = 0.1f;
        private float m_pollTimer;

        private bool m_initialized;
        private float m_target = 1f;
        private float m_from = 1f;
        private float m_duration;
        private float m_elapsed;

        // Called by the owning (local) player whenever the target scale changes.
        public static void Publish(Player player, float target, float duration)
        {
            ZNetView netView = player != null ? player.GetComponent<ZNetView>() : null;

            if (netView == null || !netView.IsValid() || !netView.IsOwner())
                return;

            ZDO zdo = netView.GetZDO();

            zdo.Set(DurationHash, duration);
            zdo.Set(TargetScaleHash, target);
        }

        public void Awake()
        {
            try
            {
                m_player = GetComponent<Player>();
                m_netView = GetComponent<ZNetView>();

                // A fresh player starts at normal size; make sure no stale value is replicated.
                if (m_netView != null && m_netView.IsValid() && m_netView.IsOwner())
                    m_netView.GetZDO().Set(TargetScaleHash, 1f);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPlayerScaleSync.Awake failed: " + e);
            }
        }

        // Reads the replicated target and starts a lerp when it changed.
        private void PollTarget(ZDO zdo)
        {
            float target = zdo.GetFloat(TargetScaleHash, 1f);

            if (!m_initialized)
            {
                // Joined/came into range of an already-resized player: show the final size immediately.
                m_initialized = true;
                m_target = target;
                m_duration = 0f;
                m_elapsed = 0f;
            }
            else if (!Mathf.Approximately(target, m_target))
            {
                m_from = m_player.transform.localScale.x;
                m_target = target;
                m_duration = Mathf.Max(0f, zdo.GetFloat(DurationHash, 0f));
                m_elapsed = 0f;
            }
        }

        public void Update()
        {
            try
            {
                // The local player is driven by PlayerScaleHelper's own lerp. Compare against
                // m_localPlayer rather than IsOwner(): it doesn't depend on who owns the ZDO.
                if (m_player == null || m_netView == null || !m_netView.IsValid() || ReferenceEquals(m_player, Player.m_localPlayer))
                    return;

                // The target only changes on a rare redeem, so the ZDO is polled a few times a second
                // instead of every frame; the lerp below still steps every frame.
                m_pollTimer -= Time.deltaTime;

                if (!m_initialized || m_pollTimer <= 0f)
                {
                    m_pollTimer = PollInterval;
                    PollTarget(m_netView.GetZDO());
                }

                if (m_elapsed < m_duration)
                {
                    m_elapsed += Time.deltaTime;
                    PlayerScaleHelper.ApplyVisualScale(m_player, Mathf.Lerp(m_from, m_target, Mathf.Clamp01(m_elapsed / m_duration)));
                }
                else if (!Mathf.Approximately(m_player.transform.localScale.x, m_target))
                {
                    PlayerScaleHelper.ApplyVisualScale(m_player, m_target);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPlayerScaleSync.Update failed: " + e);
            }
        }
    }
}
