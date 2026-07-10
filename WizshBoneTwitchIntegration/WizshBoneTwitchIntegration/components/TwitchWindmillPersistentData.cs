using System;
using System.Reflection;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchWindmillPersistentData : MonoBehaviour
    {
        // Captured once from the vanilla Windmill component by ConfigurePrefab(), called from
        // WizshBoneTwitchIntegration.SetupPieces() right before that vanilla component is removed
        // from the "Windmill_WBTI" clone. [SerializeField] is required here even though nothing
        // ever edits these in the Inspector - Unity's Instantiate() clones a MonoBehaviour's fields
        // through its serialization system, so a plain private field (no [SerializeField]) resets
        // to default/null on every actual spawn instead of inheriting the template's configured
        // value, even though ConfigurePrefab() only ever runs once on the template itself.
        [SerializeField] private Transform m_propeller;
        [SerializeField] private Transform m_grindstone;
        [SerializeField] private Transform m_bom;
        [SerializeField] private AudioSource[] m_sfxLoops;
        [SerializeField] private GameObject m_propellerAOE;
        [SerializeField] private Aoe m_propellerAoe;
        [SerializeField] private float m_prefabPropSpeed;
        [SerializeField] private float m_prefabGrindSpeed;
        [SerializeField] private float m_maxPitch;
        [SerializeField] private float m_maxVol;
        [SerializeField] private float m_audioChangeSpeed;

        private float m_rotationSpeed;
        private float m_seed;
        private float m_propAngle;
        private float m_grindAngle;
        private bool m_active;
        private float m_aoeResetTimer;
        private bool m_audioRamped;

        private static readonly int s_speedHash = "WBTI_Windmill_Speed".GetStableHashCode();
        private static readonly int s_seedHash  = "WBTI_Windmill_Seed".GetStableHashCode();

        private static readonly FieldInfo s_aoeHitListField = typeof(Aoe)
            .GetField("m_hitList", BindingFlags.NonPublic | BindingFlags.Instance);

        public void ConfigurePrefab(Windmill vanillaWindmill)
        {
            m_propeller        = vanillaWindmill.m_propeller;
            m_grindstone       = vanillaWindmill.m_grindstone;
            m_bom              = vanillaWindmill.m_bom;
            m_sfxLoops         = vanillaWindmill.m_sfxLoops;
            m_propellerAOE     = vanillaWindmill.m_propellerAOE;
            m_prefabPropSpeed  = vanillaWindmill.m_propellerRotationSpeed;
            m_prefabGrindSpeed = vanillaWindmill.m_grindstoneRotationSpeed;
            m_maxPitch         = vanillaWindmill.m_maxPitch;
            m_maxVol           = vanillaWindmill.m_maxVol;
            m_audioChangeSpeed = vanillaWindmill.m_audioChangeSpeed;
            m_propellerAoe     = m_propellerAOE != null ? m_propellerAOE.GetComponent<Aoe>() : null;
        }

        public void Awake()
        {
            try
            {
                ZNetView netView = gameObject.GetComponent<ZNetView>();

                if (netView == null || !netView.IsValid())
                    return;

                float speed = netView.GetZDO().GetFloat(s_speedHash, 0f);
                if (speed <= 0f)
                    return;

                StartBehavior(speed, netView.GetZDO().GetFloat(s_seedHash, 0f));
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
                ZNetView netView = gameObject.GetComponent<ZNetView>();

                if (netView == null || !netView.IsValid())
                    return;

                float clampedSpeed = Mathf.Clamp(speed, 0f, 10f);
                float seed = UnityEngine.Random.Range(0f, 1000f);

                netView.GetZDO().Set(s_speedHash, clampedSpeed);
                netView.GetZDO().Set(s_seedHash, seed);

                StartBehavior(clampedSpeed, seed);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchWindmillPersistentData.Initialize failed: " + e);
            }
        }

        private void StartBehavior(float speed, float seed)
        {
            m_rotationSpeed = speed;
            m_seed          = seed;
            m_active        = true;

            // Allow repeated blade hits: OnTriggerStay fires continuously; we clear the hit
            // list ourselves every second because Aoe only clears it in the OverlapSphere path.
            if (m_propellerAoe != null)
            {
                m_propellerAoe.m_triggerEnterOnly = false;
                if (s_aoeHitListField == null)
                    Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: Aoe m_hitList field not found — repeated AOE hits won't work.");
            }

            if (m_propeller == null)
                Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: m_propeller is null on this prefab.");
            if (m_grindstone == null)
                Jotunn.Logger.LogWarning("[WBTI] TwitchWindmillPersistentData: m_grindstone is null on this prefab.");

            // AOE damage always active, regardless of wind speed - set once here rather than every
            // frame in LateUpdate(), since nothing ever turns it back off afterward.
            if (m_propellerAOE != null)
                m_propellerAOE.SetActive(true);
        }

        private void LateUpdate()
        {
            if (!m_active)
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
            if (m_bom != null)
                m_bom.rotation = Quaternion.Euler(0f, headAngle, 0f);

            if (m_propeller != null)
            {
                m_propAngle += m_prefabPropSpeed * (m_rotationSpeed / 10f) * dt;
                m_propeller.localRotation = Quaternion.Euler(0f, 0f, m_propAngle);
            }

            if (m_grindstone != null)
            {
                m_grindAngle += m_prefabGrindSpeed * (m_rotationSpeed / 10f) * dt;
                m_grindstone.localRotation = Quaternion.Euler(0f, m_grindAngle, 0f);
            }

            // Ramp audio toward max volume/pitch - same MoveTowards smoothing vanilla
            // Windmill.UpdateAudio() used, just always targeting max instead of wind-scaled.
            // Stops touching AudioSource.volume/pitch once every source has reached target,
            // rather than writing the same already-reached value every frame forever.
            if (!m_audioRamped && m_sfxLoops != null)
            {
                bool allRamped = true;

                foreach (AudioSource source in m_sfxLoops)
                {
                    source.volume = Mathf.MoveTowards(source.volume, m_maxVol, m_audioChangeSpeed * dt);
                    source.pitch  = Mathf.MoveTowards(source.pitch, m_maxPitch, m_audioChangeSpeed * dt);

                    if (source.volume != m_maxVol || source.pitch != m_maxPitch)
                        allRamped = false;
                }

                m_audioRamped = allRamped;
            }
        }
    }
}
