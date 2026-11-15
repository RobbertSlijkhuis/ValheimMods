using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ModularMagic_BloodMagic.Components
{
    /// <summary>
    /// Attached to every apparition creature prefab (the vanilla Charred prefabs, so a wild Charred has it too and does nothing with it).
    /// The ZDO of an apparition holds the prefab name of the scythe that made it and the player who summoned it, both written when it is
    /// spawned (see ApparitionHelper). An apparition does not die from losing its health: at 0 health it is a husk, which does nothing,
    /// makes no sound and can only be hit by its summoner (which kills it, see ApparitionHelper.TryKillHusk), until its summoner
    /// reanimates it. A husk can not be pushed and other creatures walk around it. Every client derives the look and the behaviour of
    /// the apparition from its health, so nothing is sent.
    /// The pose a husk stopped in is saved in the ZDO by its owner, so it is the same on every client and after the husk is loaded again.
    /// </summary>
    internal class ApparitionController : MonoBehaviour
    {
        private static readonly int WeaponKey       = "ApparitionData_MMBM".GetStableHashCode();
        private static readonly int SummonerKey     = "MMBM_Summoner".GetStableHashCode();
        private static readonly int ReanimationsKey = "MMBM_Reanimations".GetStableHashCode();
        private static readonly int HuskAnimKey     = "MMBM_HuskAnim".GetStableHashCode();
        private static readonly int HuskAnimTimeKey = "MMBM_HuskAnimTime".GetStableHashCode();
        private static readonly int ScytheQualityKey = "MMBM_ScytheQuality".GetStableHashCode();
        private static readonly int ReanimateNumberKey = "MMBM_ReanimateNumber".GetStableHashCode();
        private static readonly int ReanimateStartKey = "MMBM_ReanimateStart".GetStableHashCode();
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        // The seconds in which an apparition that becomes a husk slows to a stop, and its particles and glow fade
        private const float HuskSlowdownTime = 1f;

        // The seconds between the start of the reanimation effect and the reanimation itself
        private const float ReanimateDelay = 1.7f;

        // The glow an apparition gets when it is reanimated (after ReanimateDelay), it goes to the colour of the apparition in
        // ReanimateGlowTime seconds. As bright as the colours of the apparitions (see ApparitionHelper.GetOptions)
        private static readonly Color ReanimateGlowColor = new Color(4f, 0f, 0f);
        private const float ReanimateGlowTime = 3f;

        // True while this client shows the red glow of a reanimation
        private bool m_reanimateGlowShown;

        // The reanimation whose effect this client has shown, so it is shown once
        private int m_shownReanimateNumber;

        // A particle system of the apparition that was playing when it became a husk, with the emission it had
        private struct HuskParticles
        {
            public ParticleSystem system;
            public float rateOverTime;
            public float rateOverDistance;
        }

        private ZNetView?  m_netView;
        private Character? m_character;
        private BaseAI?    m_baseAI;
        private Animator?  m_animator;
        private FootStep?  m_footStep;
        private int        m_awakeFrame;
        private string     m_weaponName = "";
        private long       m_summoner;
        private Color      m_emissionColor;

        // The materials this apparition got in ApplyVisuals, their emission is turned off while it is a husk
        private readonly List<Material> m_materials = new List<Material>();

        // The particles that fade and stop while the apparition is a husk, they start again when it is reanimated
        private readonly List<HuskParticles> m_huskParticles = new List<HuskParticles>();

        // The husk state that was last applied on this client, null when nothing was applied yet
        private bool? m_appliedHusk;

        // The seconds that are left of the slowdown of a husk on this client, 0 when it stands still
        private float m_slowdownLeft;

        // The culling mode the Animator had before the apparition became a husk
        private AnimatorCullingMode m_cullingMode;

        // Whether the Animator sent its animation events before the apparition became a husk
        private bool m_fireEvents;

        // A husk can not be pushed: its Rigidbody is frozen once it stands on the ground, with the constraints it had before
        private Rigidbody? m_body;
        private bool m_bodyFrozen;
        private RigidbodyConstraints m_constraints;

        // Cuts a hole in the navmesh of the game while the apparition is a husk, so other creatures walk around it
        private NavMeshObstacle? m_obstacle;

        // The sounds on the apparition that were muted because it is a husk, they are unmuted when it is reanimated
        private readonly List<AudioSource> m_mutedSources = new List<AudioSource>();
        private readonly List<AudioSource> m_audioSources = new List<AudioSource>();

        // The saved pose of the husk (see SavePose) that this client shows, the hash is 0 when it shows none
        private int   m_shownPoseHash;
        private float m_shownPoseTime;

        // The key of the reanimation of this apparition in the ZDO of its summoner, 0 when it is not made yet
        private int m_reanimateKey;

        // The scale of the prefab, before ApplyVisuals gives the apparition the size of its scythe
        private float m_nativeScale = 1f;

        /// <summary>The player ID of the player who summoned the apparition, 0 for a creature that is not an apparition.</summary>
        public long Summoner => m_summoner;

        public Character? Character => m_character;

        /// <summary>True while the apparition is banished: it has no health left and waits for its summoner.</summary>
        public bool IsHusk => m_character != null && m_character.GetHealth() <= 0f;

        /// <summary>
        /// The size of the apparition compared to its prefab. The attack numbers of the game are made for the size of the prefab, see
        /// AttackScaleHelper. 1 when nothing has to change.
        /// </summary>
        public float AttackScale
        {
            get
            {
                float scale = transform.localScale.x;
                return scale > 0f && m_nativeScale > 0f ? scale / m_nativeScale : 1f;
            }
        }

        /// <summary>
        /// The quality of the scythe that summoned the apparition, which sets the maximum of apparitions for its reanimation. Written once
        /// when it is summoned. An apparition from before it was stored has quality 1.
        /// </summary>
        public int ScytheQuality
        {
            get
            {
                ZDO? zdo = GetZDO();
                return zdo != null ? Mathf.Max(1, zdo.GetInt(ScytheQualityKey, 1)) : 1;
            }
        }

        /// <summary>The number of times the apparition was reanimated, written by its owner together with the health.</summary>
        public int ReanimationsDone
        {
            get
            {
                ZDO? zdo = GetZDO();
                return zdo != null ? zdo.GetInt(ReanimationsKey, 0) : 0;
            }
        }

        /// <summary>
        /// The key in the ZDO of the summoner that holds the reanimation the summoner has paid for, see ApparitionHelper.TryRequestReanimation.
        /// </summary>
        public int ReanimateKey
        {
            get
            {
                if (m_reanimateKey == 0)
                {
                    ZDO? zdo = GetZDO();

                    if (zdo != null)
                        m_reanimateKey = ("MMBM_Reanimate_" + zdo.m_uid).GetStableHashCode();
                }

                return m_reanimateKey;
            }
        }

        /// <summary>
        /// True when the ZDO is the one of an apparition: it has the scythe that made it and its summoner. It reads the ZDO, so it also
        /// works before the Awake of the controller has run.
        /// </summary>
        public static bool HasApparitionData(ZDO? zdo)
        {
            return zdo != null && zdo.GetLong(SummonerKey, 0L) != 0L && zdo.GetString(WeaponKey, "") != "";
        }

        private void Awake()
        {
            // Read before ApplyVisuals changes the size. SetData runs after Awake, so this is always the scale of the prefab
            m_nativeScale = transform.localScale.x;

            m_netView    = GetComponent<ZNetView>();
            m_character  = GetComponent<Character>();
            m_baseAI     = GetComponent<BaseAI>();
            m_animator   = GetComponentInChildren<Animator>();
            m_footStep   = GetComponent<FootStep>();
            m_body       = GetComponent<Rigidbody>();
            m_awakeFrame = Time.frameCount;

            ZDO? zdo = GetZDO();

            if (zdo == null)
            {
                Jotunn.Logger.LogError("ApparitionController: could not find ZNetView in Awake.");
                return;
            }

            m_weaponName = zdo.GetString(WeaponKey, "");
            m_summoner   = zdo.GetLong(SummonerKey, 0L);

            if (string.IsNullOrEmpty(m_weaponName))
                return;

            ApparitionOptions? options = GetOptions();
            if (options == null)
                return;

            ApplyVisuals(options);

            // An apparition from before the summoner was stored has none: it is not registered, so it can die like it did before
            if (m_summoner != 0L)
                ApparitionHelper.Register(this);
        }

        private void OnDestroy()
        {
            ApparitionHelper.Unregister(this);

            foreach (Material material in m_materials)
            {
                if (material != null)
                    Destroy(material);
            }
        }

        /// <summary>
        /// Called by ApparitionHelper right after the game has spawned the apparition, on the client that spawned it (the owner of the ZDO).
        /// Writes the scythe, its quality and the summoner to the ZDO and applies all options.
        /// </summary>
        public void SetData(string weaponName, int scytheQuality, long summoner)
        {
            m_weaponName = weaponName;
            m_summoner   = summoner;
            m_netView    = m_netView != null ? m_netView : GetComponent<ZNetView>();
            m_character  = m_character != null ? m_character : GetComponent<Character>();
            m_baseAI     = m_baseAI != null ? m_baseAI : GetComponent<BaseAI>();
            m_animator   = m_animator != null ? m_animator : GetComponentInChildren<Animator>();
            m_footStep   = m_footStep != null ? m_footStep : GetComponent<FootStep>();
            m_body       = m_body != null ? m_body : GetComponent<Rigidbody>();

            ZDO? zdo = GetZDO();

            if (zdo == null || m_character == null)
            {
                Jotunn.Logger.LogError("ApparitionController: could not find ZNetView in SetData.");
                return;
            }

            zdo.Set(WeaponKey, weaponName);
            zdo.Set(ScytheQualityKey, Mathf.Max(1, scytheQuality));
            zdo.Set(SummonerKey, summoner);

            ApparitionOptions? options = GetOptions();
            if (options == null)
                return;

            ApplyVisuals(options);

            // The health is only set here. The game does not reset the max health of an apparition (see ApparitionPatches), so it is kept
            // when the apparition is loaded again
            m_character.SetMaxHealth(options.health);
            m_character.SetHealth(options.health);

            SetupFollower();
            ApparitionHelper.Register(this);
        }

        private void Update()
        {
            // A wild Charred has this component too
            if (m_summoner == 0L || m_character == null || m_netView == null || !m_netView.IsValid())
                return;

            // A husk has its animation switched off, so an apparition that is loaded as a husk first gets a frame to take its pose
            if (Time.frameCount <= m_awakeFrame)
                return;

            bool husk  = IsHusk;
            bool owner = m_netView.IsOwner();

            if (m_appliedHusk != husk)
            {
                // An apparition that is loaded as a husk was not seen becoming one: it gets no effect and no slowdown
                bool seenChange = m_appliedHusk != null;

                // The effect is networked, so one client spawns it
                if (seenChange && owner)
                    ApparitionHelper.SpawnStateEffect(transform.position);

                if (seenChange && husk)
                    ApparitionHelper.PlayBanishSound(transform.position, owner);

                if (husk)
                    EnterHusk(seenChange);
                else if (m_appliedHusk == true)
                    LeaveHusk();

                m_appliedHusk = husk;

                // The game checks this on the client that owns the enemy, so it is set on every client
                m_character.m_aiSkipTarget = husk;
            }

            if (!husk)
            {
                // After LeaveHusk, which sets the normal glow
                UpdateReanimateGlow();
                return;
            }

            UpdateHuskPose();
            MuteSounds();
            FreezeBodyOnGround();
            ShowReanimateEffect();

            if (!owner)
                return;

            // The AI of a husk is off, so nothing else stops the walk or the attack it was in
            if (m_baseAI != null)
                m_baseAI.StopMoving();

            StopAttack();
            SavePose();
            ApplyPaidReanimation();
        }

        /// <summary>
        /// The owner of a husk reanimates it when its summoner has paid for a reanimation that was not done yet. The payment is in the ZDO of
        /// the summoner and stays there, so it is also found by a client that becomes the owner later. The count and the health are written
        /// together: when they are lost (an older state of a previous owner arrives), the payment is seen again and it is done again.
        /// The reanimation starts with an effect and is done ReanimateDelay seconds later. The start is written to the ZDO of the husk, so a
        /// client that becomes the owner during the wait goes on from it and does not start again.
        /// </summary>
        private void ApplyPaidReanimation()
        {
            Player summoner = Player.GetPlayer(m_summoner);
            ZDO? zdo = GetZDO();

            if (summoner == null || zdo == null || m_character == null || ZNet.instance == null)
                return;

            int paid = ApparitionHelper.GetPaidReanimation(summoner, this);

            if (paid <= ReanimationsDone)
                return;

            long now = ZNet.instance.GetTime().Ticks;

            if (zdo.GetInt(ReanimateNumberKey, 0) != paid)
            {
                zdo.Set(ReanimateNumberKey, paid);
                zdo.Set(ReanimateStartKey, now);

                // A networked effect is spawned once, by the owner. Otherwise every client shows it, see ShowReanimateEffect
                if (ApparitionHelper.IsReanimateEffectNetworked)
                    ApparitionHelper.SpawnReanimateEffect(transform.position);

                Jotunn.Logger.LogWarning($"[Apparition] '{name}' starts to return (reanimation {paid})");
                return;
            }

            if (GetSecondsSince(zdo.GetLong(ReanimateStartKey, now), now) < ReanimateDelay)
                return;

            // The pose of the husk is cleared in the same write, so the next time it is banished a new pose is saved
            zdo.Set(ReanimationsKey, paid);
            zdo.Set(HuskAnimKey, 0);
            zdo.Set(HuskAnimTimeKey, 0f);
            m_character.SetHealth(m_character.GetMaxHealth());

            Jotunn.Logger.LogWarning($"[Apparition] '{name}' is reanimated (reanimation {paid})");
        }

        /// <summary>
        /// Every client shows the effect of a reanimation that its owner has started, once, when the effect is not networked. A client
        /// that loads the husk when the reanimation started longer ago than the effect lasts does not show it.
        /// </summary>
        private void ShowReanimateEffect()
        {
            ZDO? zdo = GetZDO();

            if (zdo == null || ZNet.instance == null)
                return;

            int number = zdo.GetInt(ReanimateNumberKey, 0);

            if (number == 0 || number == m_shownReanimateNumber)
                return;

            m_shownReanimateNumber = number;

            // Done already, or spawned once by the owner because the effect is networked
            if (number <= ReanimationsDone || ApparitionHelper.IsReanimateEffectNetworked)
                return;

            long now = ZNet.instance.GetTime().Ticks;

            if (GetSecondsSince(zdo.GetLong(ReanimateStartKey, now), now) < ReanimateDelay)
                ApparitionHelper.SpawnReanimateEffect(transform.position);
        }

        private static double GetSecondsSince(long ticks, long now)
        {
            return (now - ticks) / (double)System.TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// The owner kills the apparition with the normal death of the game (its death effects, then it is removed for every client), see
        /// ApparitionHelper.TryKillHusk. The game would wait for the death animation of a creature that has one, but the Animator of a
        /// husk is off, so the death is done here right away.
        /// </summary>
        public void Kill()
        {
            if (m_character == null || m_character.IsDead())
                return;

            // An apparition drops nothing. The drops are only cleared on the client that summoned it (SetupFollower), and the ragdoll
            // takes the drops of the creature even when its drops are off
            CharacterDrop? characterDrop = GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();

            m_character.m_isDead = true;
            m_character.OnDeath();
        }

        /// <summary>
        /// Makes the apparition a husk on this client. A husk does nothing, has no glow and no particles, and stands in the pose it
        /// stopped in. The AI and the Animator are switched off like the time stop of WizshBoneTwitchIntegration does: they are state
        /// of this client and not synced, so every client does it for itself (also the client that becomes the owner later).
        /// When the client saw the apparition become a husk, it slows to a stop and its particles and glow fade in HuskSlowdownTime.
        /// </summary>
        private void EnterHusk(bool slowDown)
        {
            if (m_baseAI != null)
            {
                m_baseAI.enabled = false;

                // The idle sound is repeated with an Invoke, which Unity keeps doing for a disabled component
                m_baseAI.CancelInvoke("DoIdleSound");
            }

            // The footsteps follow the animation, which still runs while the husk slows down
            if (m_footStep != null)
                m_footStep.enabled = false;

            EnableObstacle();

            if (m_animator != null)
            {
                // The animation events (sounds, hits) would still be sent while the husk slows down
                m_fireEvents = m_animator.fireEvents;
                m_animator.fireEvents = false;

                // The Animator of a creature does not move the bones while the creature is off screen. A husk is posed by hand
                // (UpdateHuskPose), which would then not show, so its Animator always animates
                m_cullingMode = m_animator.cullingMode;
                m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                // The game sets the speed of an Animator back to 1 every physics step, so a husk can not be slowed down or stopped
                // with the speed. A disabled Animator is not touched by the game, and is stepped by hand while the husk slows down
                m_animator.enabled = false;
            }

            m_huskParticles.Clear();

            foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>())
            {
                if (!particles.isPlaying)
                    continue;

                ParticleSystem.EmissionModule emission = particles.emission;

                m_huskParticles.Add(new HuskParticles()
                {
                    system = particles,
                    rateOverTime = emission.rateOverTimeMultiplier,
                    rateOverDistance = emission.rateOverDistanceMultiplier,
                });
            }

            m_shownPoseHash = 0;
            m_shownPoseTime = 0f;
            m_slowdownLeft = slowDown ? HuskSlowdownTime : 0f;

            if (slowDown)
                return;

            // Loaded as a husk: it is dark and still right away. The Animator is stepped once so the bones have a pose, the saved pose
            // follows in UpdateHuskPose
            ApplyHuskFade(0f);
            StopHuskParticles();

            if (m_animator != null)
                m_animator.Update(0f);
        }

        /// <summary>
        /// Makes a husk an active apparition again on this client, with its AI, animation, glow and particles back.
        /// </summary>
        private void LeaveHusk()
        {
            if (m_baseAI != null)
            {
                m_baseAI.enabled = true;
                m_baseAI.InvokeRepeating("DoIdleSound", m_baseAI.m_idleSoundInterval, m_baseAI.m_idleSoundInterval);
            }

            if (m_footStep != null)
                m_footStep.enabled = true;

            if (m_animator != null)
            {
                m_animator.cullingMode = m_cullingMode;
                m_animator.fireEvents = m_fireEvents;
                m_animator.enabled = true;
            }

            UnmuteSounds();
            UnfreezeBody();

            if (m_obstacle != null)
                m_obstacle.enabled = false;

            ApplyHuskFade(1f);

            foreach (HuskParticles particles in m_huskParticles)
            {
                if (particles.system != null)
                    particles.system.Play(false);
            }

            m_huskParticles.Clear();
            m_slowdownLeft = 0f;
            m_shownPoseHash = 0;
            m_shownPoseTime = 0f;
        }

        /// <summary>
        /// The animation of a husk on this client, every frame. While it slows down, the Animator is stepped with a part of the frame
        /// time that goes to 0, and the particles and the glow fade with it. After that the husk shows the pose its owner has saved, so
        /// every client ends in the same pose, also a client that loads the husk later.
        /// </summary>
        private void UpdateHuskPose()
        {
            if (m_slowdownLeft > 0f)
            {
                m_slowdownLeft = Mathf.Max(0f, m_slowdownLeft - Time.deltaTime);

                // 1 at the start of the slowdown, 0 at the end
                float strength = m_slowdownLeft / HuskSlowdownTime;

                if (m_animator != null)
                    m_animator.Update(Time.deltaTime * strength);

                ApplyHuskFade(strength);

                if (m_slowdownLeft <= 0f)
                    StopHuskParticles();

                return;
            }

            ZDO? zdo = GetZDO();

            if (zdo == null || m_animator == null)
                return;

            int hash = zdo.GetInt(HuskAnimKey, 0);
            float time = zdo.GetFloat(HuskAnimTimeKey, 0f);

            if (hash == 0 || (hash == m_shownPoseHash && time == m_shownPoseTime))
                return;

            // The state is set and the Animator is stepped without time, which puts the bones in that pose
            m_animator.Play(hash, 0, time);
            m_animator.Update(0f);

            m_shownPoseHash = hash;
            m_shownPoseTime = time;
        }

        /// <summary>
        /// The owner of a husk saves the pose it stopped in, once, when its slowdown is over: the animation state of the base layer and
        /// the time in it. Like the time stop of WizshBoneTwitchIntegration, the other layers are not saved.
        /// </summary>
        private void SavePose()
        {
            ZDO? zdo = GetZDO();

            if (m_slowdownLeft > 0f || zdo == null || m_animator == null || zdo.GetInt(HuskAnimKey, 0) != 0)
                return;

            AnimatorStateInfo state = m_animator.GetCurrentAnimatorStateInfo(0);

            if (state.fullPathHash == 0)
                return;

            zdo.Set(HuskAnimKey, state.fullPathHash);
            zdo.Set(HuskAnimTimeKey, state.normalizedTime);

            // The owner is in this pose already
            m_shownPoseHash = state.fullPathHash;
            m_shownPoseTime = state.normalizedTime;
        }

        /// <summary>
        /// The owner of a husk interrupts the attack it was in, the way the game does it when a creature starts another attack. The
        /// animation of a husk still runs while it slows down, and without this the hit of that attack could still land.
        /// </summary>
        private void StopAttack()
        {
            Humanoid? humanoid = m_character as Humanoid;

            if (humanoid == null || humanoid.m_currentAttack == null)
                return;

            humanoid.m_currentAttack.Stop();
            humanoid.m_previousAttack = humanoid.m_currentAttack;
            humanoid.m_currentAttack = null;
        }

        /// <summary>
        /// The glow and the particles of a husk: 1 is the apparition as it was, 0 is no glow and no new particles.
        /// </summary>
        private void ApplyHuskFade(float strength)
        {
            SetEmission(Color.Lerp(Color.black, m_emissionColor, strength));

            foreach (HuskParticles particles in m_huskParticles)
            {
                if (particles.system == null)
                    continue;

                ParticleSystem.EmissionModule emission = particles.system.emission;
                emission.rateOverTimeMultiplier = particles.rateOverTime * strength;
                emission.rateOverDistanceMultiplier = particles.rateOverDistance * strength;
            }
        }

        // The glow of the apparition (its chest and eyes): the emission of the materials it got in ApplyVisuals
        private void SetEmission(Color color)
        {
            foreach (Material material in m_materials)
            {
                if (material != null)
                    material.SetColor(EmissionColorId, color);
            }
        }

        /// <summary>
        /// When an apparition is reanimated (ReanimateDelay after the start, see ApplyPaidReanimation), its glow starts red and goes to the
        /// colour of the apparition in ReanimateGlowTime seconds. Every client works it out from the start in the ZDO, so they all show
        /// the same, also a client that loads the apparition halfway. Called every frame while it is not a husk.
        /// </summary>
        private void UpdateReanimateGlow()
        {
            ZDO? zdo = GetZDO();

            if (zdo == null || ZNet.instance == null)
                return;

            int number = zdo.GetInt(ReanimateNumberKey, 0);
            float progress = 1f;

            // The last reanimation that was started is done: it is the one that is shown
            if (number != 0 && number == ReanimationsDone)
            {
                long now = ZNet.instance.GetTime().Ticks;
                double sinceReanimation = GetSecondsSince(zdo.GetLong(ReanimateStartKey, now), now) - ReanimateDelay;

                progress = Mathf.Clamp01((float)(sinceReanimation / ReanimateGlowTime));
            }

            if (progress < 1f)
            {
                SetEmission(Color.Lerp(ReanimateGlowColor, m_emissionColor, progress));
                m_reanimateGlowShown = true;
            }
            else if (m_reanimateGlowShown)
            {
                // Done: back to the normal glow, once
                SetEmission(m_emissionColor);
                m_reanimateGlowShown = false;
            }
        }

        // The particles that are out already live on, no new ones come
        private void StopHuskParticles()
        {
            foreach (HuskParticles particles in m_huskParticles)
            {
                if (particles.system != null)
                    particles.system.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        /// <summary>
        /// Freezes the Rigidbody of the husk once it stands on the ground, so it can not be pushed by hits, players or creatures that walk
        /// into it. It waits for the ground, so a husk that is in the air when it is banished lands first. Done on every client, like
        /// the rest of the husk, so a client that becomes the owner later has it frozen too. Like the time stop of WizshBoneTwitchIntegration,
        /// the constraints are used and not isKinematic, which the game sets itself for a creature of another client.
        /// </summary>
        private void FreezeBodyOnGround()
        {
            if (m_bodyFrozen || m_body == null || m_character == null || !m_character.IsOnGround())
                return;

            m_constraints = m_body.constraints;

            if (!m_body.isKinematic)
            {
                m_body.linearVelocity  = Vector3.zero;
                m_body.angularVelocity = Vector3.zero;
            }

            m_body.constraints = RigidbodyConstraints.FreezeAll;
            m_bodyFrozen = true;
        }

        private void UnfreezeBody()
        {
            if (!m_bodyFrozen)
                return;

            if (m_body != null)
                m_body.constraints = m_constraints;

            m_bodyFrozen = false;
        }

        /// <summary>
        /// The pathfinding of the game uses the navmesh of Unity, which is built from the ground, rocks and buildings but not from creatures,
        /// so a creature walks straight into a husk. A carving NavMeshObstacle cuts the husk out of the navmesh, so the paths go around it.
        /// It only cuts while the husk stands still. Every client runs the AI of the creatures it owns, so every client has it.
        /// </summary>
        private void EnableObstacle()
        {
            if (m_obstacle == null)
            {
                CapsuleCollider? capsule = GetComponent<CapsuleCollider>();

                m_obstacle = gameObject.AddComponent<NavMeshObstacle>();
                m_obstacle.shape = NavMeshObstacleShape.Capsule;
                m_obstacle.carving = true;
                m_obstacle.carveOnlyStationary = true;

                // On the same transform as the collider, so it has the size of the apparition too
                if (capsule != null)
                {
                    m_obstacle.center = capsule.center;
                    m_obstacle.radius = capsule.radius;
                    m_obstacle.height = capsule.height;
                }
            }

            m_obstacle.enabled = true;
        }

        /// <summary>
        /// Mutes every sound on the husk, every frame, so a sound that is added later (like the loop of a status effect) is muted too.
        /// The game sets the volume of its sounds every frame, but never the mute.
        /// </summary>
        private void MuteSounds()
        {
            GetComponentsInChildren(true, m_audioSources);

            foreach (AudioSource source in m_audioSources)
            {
                if (source.mute)
                    continue;

                source.mute = true;
                m_mutedSources.Add(source);
            }

            m_audioSources.Clear();
        }

        // Only the sounds that were muted for the husk are unmuted
        private void UnmuteSounds()
        {
            foreach (AudioSource source in m_mutedSources)
            {
                if (source != null)
                    source.mute = false;
            }

            m_mutedSources.Clear();
        }

        private void SetupFollower()
        {
            Tameable? tameable = GetComponent<Tameable>();
            if (tameable == null)
            {
                Jotunn.Logger.LogError("ApparitionController: Tameable not found in SetupFollower.");
                return;
            }

            tameable.m_monsterAI.MakeTame();

            if (Player.m_localPlayer != null)
                tameable.Command(Player.m_localPlayer, message: false);
            else
                Jotunn.Logger.LogWarning("ApparitionController: SetupFollower called but Player.m_localPlayer is null.");

            CharacterDrop? characterDrop = GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();
        }

        public ApparitionOptions? GetOptions()
        {
            ApparitionOptions? options = ApparitionHelper.GetOptions(m_weaponName);

            if (options == null)
                Jotunn.Logger.LogWarning($"ApparitionController: no options found for weapon '{m_weaponName}'.");

            return options;
        }

        private ZDO? GetZDO()
        {
            return m_netView != null ? m_netView.GetZDO() : null;
        }

        private void ApplyVisuals(ApparitionOptions options)
        {
            transform.localScale = options.size;
            m_emissionColor = options.emissionColor;

            Material charredMat = ModularMagic_BloodMagic.Instance.materials.Charred;

            foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Material instancedMat = new Material(charredMat);
                instancedMat.EnableKeyword("_EMISSION");
                instancedMat.SetColor(EmissionColorId, options.emissionColor);
                renderer.materials = new Material[] { instancedMat };
                m_materials.Add(instancedMat);
            }

            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = ps.main;
                main.startColor = options.emissionColor;
            }
        }
    }
}
