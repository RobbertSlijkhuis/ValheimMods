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
        private bool m_frozeRigidbody;
        private bool m_damageFrozen;

        // Set once the particle/rigidbody/etc. half of the freeze has run - lets
        // TwitchTimeStopZone tell a damage-only-frozen object (see FreezeDamage) apart from one
        // that's already fully handled.
        public bool VisualFrozen { get; private set; }

        private static readonly FieldInfo s_aoeInitRunField = typeof(Aoe)
            .GetField("m_initRun", BindingFlags.NonPublic | BindingFlags.Instance);

        // Blocks damage immediately, independent of the particle/visual freeze below. Called from
        // an Aoe.Awake() Harmony postfix (see harmony/TimeStopPatchesWBTI.cs) the instant an Aoe
        // spawns inside an already-active zone, and again later by TwitchTimeStopZone's poll for
        // anything that spawned before a zone existed - both call sites are safe to call
        // repeatedly. Must be reachable synchronously, in the same frame an Aoe spawns, to close
        // the window before Unity's next physics step could otherwise deliver an instant trigger
        // hit - a zone's own poll (which only runs every ~0.1s) alone is too slow to react in time.
        //
        // Disabling the Aoe is the actual damage block (Aoe.OnDisable removes it from Aoe.
        // Instances, which MonoUpdaters.FixedUpdate iterates via CustomFixedUpdate - the only path
        // that ever calls Initiate()/CheckHits()/OnHit() for a non-trigger Aoe like both Smite's
        // and Detonate's). m_hitCharacters is set false too as a second layer of defense, but
        // testing showed it is not sufficient on its own with the Aoe left enabled - a disabled
        // Aoe never reaches OnHit at all, which is the property actually being relied on here.
        //
        // Some hazards' entire visual effect is created by Aoe.Initiate() itself (m_initiateEffect
        // - a public EffectList), not an independent always-playing ParticleSystem already present
        // on the prefab. Confirmed for Detonate's explosion: fx_blobLava_explosion (what
        // m_initiateEffect spawns) carries only ParticleSystems, no Aoe/ImpactEffect/damage
        // component of its own, so creating it directly here is safe regardless of freeze state.
        // Disabling the Aoe above means the real Initiate() never runs until unfreeze, so nothing
        // would be visible at all until then - trigger that EffectList ourselves instead, entirely
        // bypassing Initiate()'s own CheckHits()/OnHit(). Reparenting what it spawns under this
        // root lets TwitchTimeStopZone's poll find and (after a grace period, for hazards in its
        // s_aoeGraceDelayRootNames) pause those particle systems the same way it already does for
        // Smite's rod.
        public void FreezeDamage()
        {
            try
            {
                if (m_damageFrozen)
                    return;

                m_damageFrozen = true;

                // Claim ownership before anything else, so this client becomes authoritative for
                // the object going forward - the same built-in mechanic ZNetViewHelper.Destroy()
                // already uses for the same reason. Aoe.CustomFixedUpdate() (which decides whether
                // to actually fire Initiate()/deal damage) only ever runs on the owning client, so
                // if a different client owns this ZDO and hasn't yet seen the zone locally (e.g.
                // their own copy of the zone's ZDO hasn't replicated to them yet), their un-frozen
                // Aoe would deal damage normally regardless of what every other client displays.
                // This also protects Rigidbody-based props handled in Initialize() below (which
                // calls this method first) from desyncing/snapping on unfreeze if their owner
                // isn't standing in the same zone and so never applies this freeze locally either.
                ZNetView netView = GetComponent<ZNetView>();
                if (netView != null && netView.IsValid() && !netView.IsOwner())
                    netView.ClaimOwnership();

                m_aoes = GetComponentsInChildren<Aoe>(true);
                m_aoeInitRun = new bool[m_aoes.Length];
                m_aoeHitCharacters = new bool[m_aoes.Length];

                for (int i = 0; i < m_aoes.Length; i++)
                {
                    m_aoeHitCharacters[i] = m_aoes[i].m_hitCharacters;
                    m_aoes[i].m_hitCharacters = false;

                    // Snapshot m_initRun first - Aoe.OnEnable() unconditionally resets it to true,
                    // which would re-fire Initiate() (a second hit) on unfreeze if this was a
                    // single-shot Aoe whose one hit had already resolved before we froze it.
                    m_aoeInitRun[i] = s_aoeInitRunField != null && (bool)s_aoeInitRunField.GetValue(m_aoes[i]);
                    m_aoes[i].enabled = false;

                    foreach (GameObject fx in m_aoes[i].m_initiateEffect.Create(m_aoes[i].transform.position, m_aoes[i].transform.rotation))
                        fx.transform.SetParent(transform, worldPositionStays: true);

                    // The real Initiate() still needs to fire normally once unfrozen, to deal the
                    // postponed damage (same as Smite) - but it would also re-run m_initiateEffect.
                    // Create(), spawning a second copy of the same effect we just triggered above.
                    // Permanently clearing the list on this instance prevents that, without
                    // touching the shared prefab template (Instantiate() deep-copies this array
                    // per clone, so other/future explosions are unaffected).
                    m_aoes[i].m_initiateEffect.m_effectPrefabs = Array.Empty<EffectList.EffectData>();
                }

                // TimedDestruction schedules its destroy call via InvokeRepeating, which keeps
                // firing on schedule regardless of the component's enabled state - it has to be
                // explicitly cancelled to stop the object from being destroyed while frozen.
                // Re-scanned on every call (not just the first) rather than cached, because some
                // hazards (e.g. the Smite rod) get a TimedDestruction attached a few lines after
                // spawning - the very first FreezeDamage() call (from the Aoe.Awake() postfix) can
                // run before that component even exists. TwitchTimeStopZone's poll calls this again
                // shortly after, by which point any such component has been added and gets caught.
                foreach (TimedDestruction timedDestruction in GetComponentsInChildren<TimedDestruction>(true))
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

                // Freezes the effect's own particle simulation (e.g. the Smite lightning rod, or a
                // frozen creature's own status-effect/weapon VFX) in place, rather than letting it
                // keep animating while everything else is frozen. Always runs, even for a creature
                // root also managed by TwitchFreezeData below - particles are independent of the
                // Rigidbody/Animator conflict that guards against.
                m_particleSystems = GetComponentsInChildren<ParticleSystem>(true);

                foreach (ParticleSystem particles in m_particleSystems)
                    particles.Pause(false);

                // A creature's own melee-attack Aoe is often parented under the creature itself,
                // so this can resolve to the same root TwitchFreezeData is already managing for a
                // frozen enemy. Its Rigidbody/Animator are already handled there (via
                // RigidbodyConstraints, not isKinematic) - touching the Rigidbody again here would
                // just have the two freeze mechanisms fight each other on unfreeze.
                if (GetComponent<TwitchFreezeData>() != null)
                {
                    VisualFrozen = true;
                    return;
                }

                m_rigidbody = GetComponent<Rigidbody>();
                m_projectile = GetComponent<Projectile>();
                m_windmill = GetComponent<Windmill>();

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

                foreach (TimedDestruction timedDestruction in GetComponentsInChildren<TimedDestruction>(true))
                    timedDestruction.Trigger();

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
