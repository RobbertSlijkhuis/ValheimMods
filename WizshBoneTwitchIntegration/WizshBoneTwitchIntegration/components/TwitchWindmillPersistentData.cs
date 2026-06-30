using System;
using System.Reflection;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchWindmillPersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private Windmill m_windmill;
        private Smelter m_smelter;
        private float m_rotationSpeed;
        private float m_seed;
        private float m_propAngle;
        private float m_grindAngle;
        private float m_prefabPropSpeed;
        private float m_prefabGrindSpeed;
        private bool m_active;
        private Aoe m_propellerAoe;
        private float m_aoeResetTimer;

        private static readonly int s_speedHash = "WBTI_Windmill_Speed".GetStableHashCode();
        private static readonly int s_seedHash  = "WBTI_Windmill_Seed".GetStableHashCode();

        private static readonly FieldInfo s_coverField = typeof(Windmill)
            .GetField("m_cover", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo s_aoeHitListField = typeof(Aoe)
            .GetField("m_hitList", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Awake()
        {
            try
            {
                m_netView  = gameObject.GetComponent<ZNetView>();
                m_windmill = gameObject.GetComponent<Windmill>();

                if (m_netView == null || !m_netView.IsValid() || m_windmill == null)
                    return;

                float speed = m_netView.GetZDO().GetFloat(s_speedHash, 0f);
                if (speed <= 0f)
                    return;

                StartBehavior(speed, m_netView.GetZDO().GetFloat(s_seedHash, 0f));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchWindmillPersistentData.Awake failed: " + e);
            }
        }

        public void Initialize(float speed)
        {
            try
            {
                m_netView  = gameObject.GetComponent<ZNetView>();
                m_windmill = gameObject.GetComponent<Windmill>();

                if (m_netView == null || !m_netView.IsValid() || m_windmill == null)
                    return;

                float clampedSpeed = Mathf.Clamp(speed, 0f, 10f);
                float seed = UnityEngine.Random.Range(0f, 1000f);

                m_netView.GetZDO().Set(s_speedHash, clampedSpeed);
                m_netView.GetZDO().Set(s_seedHash, seed);

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.WindmillOverride, true);

                StartBehavior(clampedSpeed, seed);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchWindmillPersistentData.Initialize failed: " + e);
            }
        }

        private void StartBehavior(float speed, float seed)
        {
            m_rotationSpeed    = speed;
            m_seed             = seed;
            m_active           = true;
            m_smelter          = gameObject.GetComponent<Smelter>();
            m_prefabPropSpeed  = m_windmill.m_propellerRotationSpeed;
            m_prefabGrindSpeed = m_windmill.m_grindstoneRotationSpeed;

            // Zero cover and stop the periodic cover recalculation so GetPowerOutput()
            // returns actual wind intensity — needed for Windmill.UpdateAudio() to play at full volume.
            m_windmill.CancelInvoke("CheckCover");
            s_coverField?.SetValue(m_windmill, 0f);

            // Collapse audio ramp thresholds so any wind gives full volume/pitch.
            m_windmill.m_maxVolVel   = 0.01f;
            m_windmill.m_maxPitchVel = 0.01f;

            // Disable the Smelter so it never processes barley into flour.
            if (m_smelter != null)
                m_smelter.enabled = false;

            // Allow repeated blade hits: OnTriggerStay fires continuously; we clear the hit
            // list ourselves every second because Aoe only clears it in the OverlapSphere path.
            m_propellerAoe = m_windmill.m_propellerAOE?.GetComponent<Aoe>();
            if (m_propellerAoe != null)
            {
                m_propellerAoe.m_triggerEnterOnly = false;
                if (s_aoeHitListField == null)
                    Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: Aoe m_hitList field not found — repeated AOE hits won't work.");
            }

            if (m_windmill.m_propeller == null)
                Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: m_propeller is null on this prefab.");
            if (m_windmill.m_grindstone == null)
                Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: m_grindstone is null on this prefab.");
        }

        private void LateUpdate()
        {
            if (!m_active || m_windmill == null)
                return;

            float t  = (float)(ZNet.instance.GetTimeSeconds() % 10000.0);
            float dt = Time.deltaTime;

            // Clear the Aoe hit list every second so OnTriggerStay can re-damage the player.
            m_aoeResetTimer += dt;
            if (m_aoeResetTimer >= 1f && m_propellerAoe != null)
            {
                m_aoeResetTimer = 0f;
                (s_aoeHitListField?.GetValue(m_propellerAoe) as System.Collections.IList)?.Clear();
            }

            // Chaotic head rotation: base continuous spin + two sine noise terms.
            // Derived from synced server time + per-windmill seed → same on all clients.
            float spinRate  = m_rotationSpeed * 4.5f;
            float headAngle = t * spinRate
                + Mathf.Sin(t * 0.53f + m_seed) * 80f
                + Mathf.Sin(t * 1.17f + m_seed * 1.7f) * 40f;
            m_windmill.m_bom.rotation = Quaternion.Euler(0f, headAngle, 0f);

            // Drive propeller ourselves — bypasses wind/cover zeroing powerOutput.
            if (m_windmill.m_propeller != null)
            {
                m_propAngle += m_prefabPropSpeed * (m_rotationSpeed / 10f) * dt;
                m_windmill.m_propeller.localRotation = Quaternion.Euler(0f, 0f, m_propAngle);
            }

            // Drive grindstone — always spinning to match the unconditional grinding sound.
            if (m_windmill.m_grindstone != null)
            {
                m_grindAngle += m_prefabGrindSpeed * (m_rotationSpeed / 10f) * dt;
                m_windmill.m_grindstone.localRotation = Quaternion.Euler(0f, m_grindAngle, 0f);
            }

            // AOE damage always active, regardless of wind speed.
            m_windmill.m_propellerAOE.SetActive(true);
        }
    }
}
