using System;
using System.Reflection;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPhysicsFreezeData : MonoBehaviour
    {
        private Rigidbody m_rigidbody;
        private Projectile m_projectile;
        private Windmill m_windmill;
        private ParticleSystem[] m_particleSystems;
        private Aoe[] m_aoes;
        private bool[] m_aoeInitRun;
        private bool[] m_aoeHitCharacters;
        private TimedDestruction[] m_timedDestructions;
        private bool m_frozeRigidbody;
        private bool m_damageFrozen;

        // Set once the particle/rigidbody/etc. half of the freeze has run - lets
        // TwitchTimeStopZone tell a damage-only-frozen object (see FreezeDamage) apart from one
        // that's already fully handled.
        public bool VisualFrozen { get; private set; }

        private static readonly FieldInfo s_aoeInitRunField = typeof(Aoe)
            .GetField("m_initRun", BindingFlags.NonPublic | BindingFlags.Instance);

        // Blocks damage immediately, independent of the particle/visual freeze below. Must be
        // called synchronously, in the same frame the hazard spawns, to close the window before
        // Unity's next physics step could otherwise deliver an instant trigger hit - a zone's own
        // poll (which only runs every ~0.1s) is too slow to react in time.
        //
        // Disabling the Aoe component does NOT reliably stop this on its own - Unity still
        // delivers OnTriggerEnter/OnTriggerStay to a disabled MonoBehaviour in practice (confirmed
        // in-game: damage still landed with the component fully disabled), even though it does
        // stop CustomFixedUpdate (via Aoe.OnDisable removing it from Aoe.Instances, which
        // MonoUpdaters.FixedUpdate iterates) and Update(). So the disable below still matters for
        // pausing m_ttl/the delayed Initiate() path, but the actual damage block has to happen at
        // the hit-decision itself: Aoe.ShouldHit() unconditionally returns false for any Character
        // when m_hitCharacters is false, regardless of which path (trigger or timed) reached it.
        public void FreezeDamage()
        {
            try
            {
                if (m_damageFrozen)
                    return;

                m_damageFrozen = true;

                m_aoes = GetComponentsInChildren<Aoe>(true);
                m_timedDestructions = GetComponentsInChildren<TimedDestruction>(true);

                if (m_aoes.Length > 0)
                {
                    m_aoeInitRun = new bool[m_aoes.Length];
                    m_aoeHitCharacters = new bool[m_aoes.Length];

                    for (int i = 0; i < m_aoes.Length; i++)
                    {
                        // Snapshot m_initRun first - Aoe.OnEnable() unconditionally resets it to
                        // true, which would re-fire Initiate() (a second hit) on unfreeze if this
                        // was a single-shot Aoe whose one hit had already resolved before we froze
                        // it.
                        m_aoeInitRun[i] = s_aoeInitRunField != null && (bool)s_aoeInitRunField.GetValue(m_aoes[i]);
                        m_aoeHitCharacters[i] = m_aoes[i].m_hitCharacters;

                        m_aoes[i].m_hitCharacters = false;
                        m_aoes[i].enabled = false;
                    }
                }

                // TimedDestruction schedules its destroy call via InvokeRepeating, which keeps
                // firing on schedule regardless of the component's enabled state - it has to be
                // explicitly cancelled to stop the rod from being destroyed while frozen.
                foreach (TimedDestruction timedDestruction in m_timedDestructions)
                    timedDestruction.CancelInvoke();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPhysicsFreezeData.FreezeDamage failed: " + e);
            }
        }

        public void Initialize()
        {
            try
            {
                FreezeDamage();

                m_rigidbody = GetComponent<Rigidbody>();
                m_projectile = GetComponent<Projectile>();
                m_windmill = GetComponent<Windmill>();
                m_particleSystems = GetComponentsInChildren<ParticleSystem>(true);

                // Projectiles (arrows, spears, thrown weapons) move by manually integrating
                // velocity into the transform every frame rather than via the physics engine,
                // so freezing their Rigidbody (if any) wouldn't stop them - the component itself
                // has to be disabled to halt its Update/FixedUpdate.
                if (m_projectile != null)
                    m_projectile.enabled = false;

                // The vanilla Windmill component has its own Update() driving wind-based rotation
                // and audio, independent of TwitchWindmillPersistentData's LateUpdate override -
                // disabling it stops that residual rotation from showing through while frozen.
                if (m_windmill != null)
                    m_windmill.enabled = false;

                // Freezes the effect's own particle simulation (e.g. the Smite lightning rod) in
                // place, rather than letting it keep animating while everything else is frozen.
                foreach (ParticleSystem particles in m_particleSystems)
                    particles.Pause(false);

                if (m_rigidbody != null && !m_rigidbody.isKinematic)
                {
                    m_rigidbody.velocity = Vector3.zero;
                    m_rigidbody.angularVelocity = Vector3.zero;
                    m_rigidbody.isKinematic = true;
                    m_frozeRigidbody = true;
                }

                VisualFrozen = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPhysicsFreezeData.Initialize failed: " + e);
            }
        }

        public void Unfreeze()
        {
            try
            {
                if (m_projectile != null)
                    m_projectile.enabled = true;

                if (m_windmill != null)
                    m_windmill.enabled = true;

                if (m_particleSystems != null)
                {
                    foreach (ParticleSystem particles in m_particleSystems)
                    {
                        if (particles != null)
                            particles.Play(false);
                    }
                }

                if (m_aoes != null)
                {
                    for (int i = 0; i < m_aoes.Length; i++)
                    {
                        if (m_aoes[i] == null)
                            continue;

                        m_aoes[i].enabled = true;
                        m_aoes[i].m_hitCharacters = m_aoeHitCharacters[i];
                        s_aoeInitRunField?.SetValue(m_aoes[i], m_aoeInitRun[i]);
                    }
                }

                if (m_timedDestructions != null)
                {
                    foreach (TimedDestruction timedDestruction in m_timedDestructions)
                    {
                        if (timedDestruction != null)
                            timedDestruction.Trigger();
                    }
                }

                if (m_frozeRigidbody && m_rigidbody != null)
                    m_rigidbody.isKinematic = false;

                Destroy(this);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPhysicsFreezeData.Unfreeze failed: " + e);
            }
        }
    }
}
