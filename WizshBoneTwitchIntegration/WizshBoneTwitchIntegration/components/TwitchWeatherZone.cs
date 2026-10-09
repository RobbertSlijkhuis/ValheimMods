using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    /// <summary>
    /// Replicates a weather redeem's EnvZone to every client: its weather (or weather list to cycle through),
    /// force flag, size and optionally the player it follows. EnvZone's own fields and the capsule collider
    /// aren't networked, so without this only the spawning client's copy would have the redeem's settings.
    /// A single weather is just a one-entry list.
    /// </summary>
    internal class TwitchWeatherZone : MonoBehaviour
    {
        private const float MinInterval = 0.1f;

        private ZNetView m_netView;
        private EnvZone m_envZone;
        private string[] m_weathers;
        private float m_interval;
        private double m_startTime;
        private bool m_force;
        private ZDOID m_followZdoid = ZDOID.None;
        private Transform m_followTarget;
        private int m_lastIndex = -1;
        private bool m_frostResist;
        private CapsuleCollider m_capsule;
        private float m_nextFrostCheck;

        private const float FrostCheckInterval = 0.5f;

        private static readonly int s_configuredHash = "WBTI_WeatherZone_Configured".GetStableHashCode();
        private static readonly int s_weathersHash   = "WBTI_WeatherZone_Weathers".GetStableHashCode();
        private static readonly int s_intervalHash   = "WBTI_WeatherZone_Interval".GetStableHashCode();
        private static readonly int s_startHash      = "WBTI_WeatherZone_StartMs".GetStableHashCode();
        private static readonly int s_forceHash      = "WBTI_WeatherZone_Force".GetStableHashCode();
        private static readonly int s_heightHash     = "WBTI_WeatherZone_Height".GetStableHashCode();
        private static readonly int s_radiusHash     = "WBTI_WeatherZone_Radius".GetStableHashCode();
        private static readonly int s_followUserHash = "WBTI_WeatherZone_FollowUserID".GetStableHashCode();
        private static readonly int s_followObjHash  = "WBTI_WeatherZone_FollowObjID".GetStableHashCode();
        private static readonly int s_frostHash      = "WBTI_WeatherZone_FrostResist".GetStableHashCode();

        // Only the client that triggers the redeem calls Initialize() directly. Every other client (and
        // this one after a zone reload) gets its own copy of this networked zone via ZNetView, so it
        // reads the same config back out of the ZDO here to configure itself identically.
        public void Awake()
        {
            enabled = false;

            try
            {
                m_netView = GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                ZDO zdo = m_netView.GetZDO();

                if (!zdo.GetBool(s_configuredHash, false))
                    return;

                ApplyConfig(
                    zdo.GetString(s_weathersHash, "").Split('|'),
                    zdo.GetFloat(s_intervalHash, MinInterval),
                    zdo.GetLong(s_startHash, 0L) / 1000.0,
                    zdo.GetBool(s_forceHash, false),
                    zdo.GetFloat(s_heightHash, 0f),
                    zdo.GetFloat(s_radiusHash, 0f),
                    new ZDOID(zdo.GetLong(s_followUserHash, 0L), (uint)zdo.GetInt(s_followObjHash, 0)),
                    zdo.GetBool(s_frostHash, false));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchWeatherZone.Awake failed: " + e);
            }
        }

        public void Initialize(string[] weathers, float interval, bool force, float height, float radius, ZDOID followTarget, bool frostResist)
        {
            try
            {
                // The shared world clock keeps a cycling zone on the same weather on every client.
                double startTime = ZNet.instance != null ? ZNet.instance.GetTimeSeconds() : 0.0;

                if (m_netView != null && m_netView.IsValid())
                {
                    ZDO zdo = m_netView.GetZDO();
                    zdo.Set(s_weathersHash, string.Join("|", weathers));
                    zdo.Set(s_intervalHash, interval);
                    zdo.Set(s_startHash, (long)(startTime * 1000.0));
                    zdo.Set(s_forceHash, force);
                    zdo.Set(s_heightHash, height);
                    zdo.Set(s_radiusHash, radius);
                    zdo.Set(s_followUserHash, followTarget.UserID);
                    zdo.Set(s_followObjHash, (int)followTarget.ID);
                    zdo.Set(s_frostHash, frostResist);
                    zdo.Set(s_configuredHash, true);
                }

                ApplyConfig(weathers, interval, startTime, force, height, radius, followTarget, frostResist);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchWeatherZone.Initialize failed: " + e);
            }
        }

        private void ApplyConfig(string[] weathers, float interval, double startTime, bool force, float height, float radius, ZDOID followTarget, bool frostResist)
        {
            m_envZone = GetComponent<EnvZone>();
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            if (m_envZone == null || capsuleCollider == null || weathers.Length == 0)
                return;

            m_capsule = capsuleCollider;
            m_frostResist = frostResist;
            m_weathers = weathers;
            m_interval = Mathf.Max(MinInterval, interval);
            m_startTime = startTime;
            m_force = force;
            m_followZdoid = followTarget;
            m_envZone.m_force = force;
            capsuleCollider.height = height;
            capsuleCollider.radius = radius;

            enabled = true;

            // Registered only after the capsule has its final size - the bounds outline is built
            // from the collider's dimensions right at registration.
            ColliderBoundsHelper.Register(gameObject, ColliderBoundsHelper.WeatherZoneColor);
        }

        // Picks the current weather from the shared clock. A single-weather zone just sets it once.
        public void Update()
        {
            if (m_frostResist)
                UpdateFrostWard();

            int index = 0;

            if (m_weathers.Length > 1 && ZNet.instance != null)
            {
                double elapsed = Math.Max(0.0, ZNet.instance.GetTimeSeconds() - m_startTime);
                index = (int)(elapsed / m_interval) % m_weathers.Length;
            }

            if (index == m_lastIndex)
                return;

            m_lastIndex = index;
            m_envZone.m_environment = m_weathers[index];
        }

        // Each client protects only its own local player, and only while they're inside the zone - the
        // weather itself is applied per client the same way (EnvZone only reacts to the local player).
        private void UpdateFrostWard()
        {
            if (Time.time < m_nextFrostCheck)
                return;

            m_nextFrostCheck = Time.time + FrostCheckInterval;

            Player player = Player.m_localPlayer;

            if (player == null || m_capsule == null)
                return;

            Vector3 position = player.transform.position;

            // ClosestPoint returns the point itself when it's inside the collider.
            if ((m_capsule.ClosestPoint(position) - position).sqrMagnitude < 0.0001f)
                FrostWardHelper.Apply(player);
        }

        // Keeps the zone centered on the player who redeemed it. Every client moves its own copy, since a
        // transform isn't synced; if the target isn't loaded on this client the zone just stays put.
        // If the target is destroyed (death, logout) the zone stays where it was for the rest of its life.
        public void LateUpdate()
        {
            if (m_followZdoid == ZDOID.None)
                return;

            if (m_followTarget == null)
            {
                GameObject target = ZNetScene.instance?.FindInstance(m_followZdoid);
                if (target == null)
                    return;

                m_followTarget = target.transform;
            }

            transform.position = m_followTarget.position;

            // The zone's ZDO otherwise never moves from its spawn point, and ZDOMan uses that stored
            // position to decide which peers even receive it. Mirrors ZSyncTransform.OwnerSync().
            if (m_netView != null && m_netView.IsValid() && m_netView.IsOwner())
                m_netView.GetZDO().SetPosition(transform.position);
        }

        // Only the spawning client has WeatherHelper's onEnd delegate, so a forced environment would
        // otherwise stay stuck on every other client once the zone is gone.
        public void OnDestroy()
        {
            ColliderBoundsHelper.Unregister(gameObject);

            if (m_frostResist)
                FrostWardHelper.Remove(Player.m_localPlayer);

            if (m_force && EnvMan.instance != null)
                EnvMan.instance.SetForceEnvironment("");
        }
    }
}
