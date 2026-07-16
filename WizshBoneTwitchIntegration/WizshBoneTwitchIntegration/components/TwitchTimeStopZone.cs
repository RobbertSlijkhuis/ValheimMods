using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchTimeStopZone : MonoBehaviour
    {
        private bool m_freezeEnemies;
        private bool m_freezePlayer;
        private bool m_freezeProjectiles;
        private bool m_localPlayerRegistered;
        private float m_radius;
        private float m_physicsPollTimer;
        private ZNetView m_netView;
        private ZDOID m_attachTargetZdoid = ZDOID.None;
        private Transform m_attachTarget;
        private readonly HashSet<ZDOID> m_registeredCreatures = new HashSet<ZDOID>();
        private readonly HashSet<GameObject> m_registeredPhysicsObjects = new HashSet<GameObject>();
        private readonly List<GameObject> m_departedPhysicsObjects = new List<GameObject>();
        private readonly Dictionary<GameObject, float> m_pendingAoeFreezeTimers = new Dictionary<GameObject, float>();

        // Gives a hazard's particle effect time to actually play before it gets paused - without
        // this, the zone's poll can catch (and pause) it on the very first tick after it appears,
        // before anything is visible, which looks wrong. Applies both to effects already present
        // on the prefab from spawn (Smite's rod) and ones TwitchPhysicsFreezeData.FreezeDamage
        // creates and reparents under the root itself (Detonate's explosion, via m_initiateEffect).
        // Tuned per hazard since a slow-developing effect (a rod descending) and a near-instant
        // one (an explosion burst) don't need the same amount of time to "show" before pausing.
        private static readonly Dictionary<string, float> s_aoeGraceDelayByRootName = new Dictionary<string, float>
        {
            { "lightningAOE(Clone)", 0.3f },       // Smite - lightning rod striking the ground
            { "BlobLava_explosion(Clone)", 0.2f }, // Detonate - vanilla Blob death explosion burst
        };

        // Unity only raises OnTriggerEnter/Exit for a collider pair if at least one side has a
        // Rigidbody. Projectiles (arrows, spears) move by manually integrating a velocity field
        // rather than via physics and carry no Rigidbody at all, so they'd never raise a trigger
        // event no matter how the zone's collider is set up. Fast-moving hazards (log rain,
        // meteors) can also tunnel through a thin trigger volume within a single physics step.
        // Polling OverlapSphere sidesteps both problems.
        private const float PhysicsPollInterval = 0.1f;

        // Tracks how many active zones currently have the local player frozen, so overlapping
        // zones don't unfreeze the player while still standing in another one.
        private static int s_localPlayerFreezeCount = 0;

        // Lets code outside any particular zone instance (e.g. a redeem spawning a new hazard)
        // check synchronously whether a position is already covered by an active zone, rather
        // than waiting for a zone's own poll to discover it later - see IsPositionFrozen.
        private static readonly List<TwitchTimeStopZone> s_activeZones = new List<TwitchTimeStopZone>();

        private static readonly int s_configuredHash        = "WBTI_TimeStopZone_Configured".GetStableHashCode();
        private static readonly int s_radiusHash             = "WBTI_TimeStopZone_Radius".GetStableHashCode();
        private static readonly int s_freezeEnemiesHash      = "WBTI_TimeStopZone_FreezeEnemies".GetStableHashCode();
        private static readonly int s_freezePlayerHash       = "WBTI_TimeStopZone_FreezePlayer".GetStableHashCode();
        private static readonly int s_freezeProjectilesHash  = "WBTI_TimeStopZone_FreezeProjectiles".GetStableHashCode();
        private static readonly int s_durationHash           = "WBTI_TimeStopZone_Duration".GetStableHashCode();
        private static readonly int s_attachUserIdHash       = "WBTI_TimeStopZone_AttachUserID".GetStableHashCode();
        private static readonly int s_attachObjIdHash        = "WBTI_TimeStopZone_AttachObjID".GetStableHashCode();

        // Only the client that triggers the redeem calls Initialize() directly. Every other client
        // gets its own replicated copy of this networked zone via ZNetView, so it must read the same
        // config back out of the ZDO here to configure itself identically.
        public void Awake()
        {
            try
            {
                m_netView = GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                ZDO zdo = m_netView.GetZDO();

                if (!zdo.GetBool(s_configuredHash, false))
                    return;

                ZDOID attachZdoid = new ZDOID(zdo.GetLong(s_attachUserIdHash, 0L), (uint)zdo.GetInt(s_attachObjIdHash, 0));

                ApplyConfig(
                    zdo.GetFloat(s_radiusHash, 0f),
                    zdo.GetBool(s_freezeEnemiesHash, false),
                    zdo.GetBool(s_freezePlayerHash, false),
                    zdo.GetBool(s_freezeProjectilesHash, false),
                    zdo.GetFloat(s_durationHash, 0f),
                    attachZdoid);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.Awake failed: " + e);
            }
        }

        public void Initialize(float radius, bool freezeEnemies, bool freezePlayer, bool freezeProjectiles, float duration, ZDOID attachTarget)
        {
            try
            {
                if (m_netView != null && m_netView.IsValid())
                {
                    ZDO zdo = m_netView.GetZDO();
                    zdo.Set(s_radiusHash, radius);
                    zdo.Set(s_freezeEnemiesHash, freezeEnemies);
                    zdo.Set(s_freezePlayerHash, freezePlayer);
                    zdo.Set(s_freezeProjectilesHash, freezeProjectiles);
                    zdo.Set(s_durationHash, duration);
                    zdo.Set(s_attachUserIdHash, attachTarget.UserID);
                    zdo.Set(s_attachObjIdHash, (int)attachTarget.ID);
                    zdo.Set(s_configuredHash, true);
                }

                ApplyConfig(radius, freezeEnemies, freezePlayer, freezeProjectiles, duration, attachTarget);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.Initialize failed: " + e);
            }
        }

        private void ApplyConfig(float radius, bool freezeEnemies, bool freezePlayer, bool freezeProjectiles, float duration, ZDOID attachTarget)
        {
            m_freezeEnemies      = freezeEnemies;
            m_freezePlayer       = freezePlayer;
            m_freezeProjectiles  = freezeProjectiles;
            m_radius             = radius;
            m_attachTargetZdoid  = attachTarget;

            SphereCollider sphereCollider = GetComponentInChildren<SphereCollider>();
            if (sphereCollider != null)
                sphereCollider.radius = radius;

            ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
            if (particles != null)
                particles.gameObject.SetActive(false);

            ShieldDomeHelper.ShowDome(this, transform.position, radius, ShieldColors.TimeStop);

            if (!s_activeZones.Contains(this))
                s_activeZones.Add(this);

            if (m_freezeProjectiles || m_freezeEnemies || m_freezePlayer)
                RefreshPhysicsObjectFreezes();
        }

        // Lets a hazard freeze its own damage the instant it spawns (see TwitchPhysicsFreezeData.
        // FreezeDamage and its call site in SpawnAbilityExtension.cs) instead of waiting for this
        // zone's own poll to notice it - Aoe's trigger-based hits can fire within the same physics
        // step the hazard is created, faster than any poll interval could react.
        public static bool IsPositionFrozen(Vector3 position)
        {
            foreach (TwitchTimeStopZone zone in s_activeZones)
            {
                if (zone.m_freezeProjectiles && Vector3.Distance(zone.transform.position, position) <= zone.m_radius)
                    return true;
            }

            return false;
        }

        public void FixedUpdate()
        {
            if (!m_freezeProjectiles && !m_freezeEnemies && !m_freezePlayer)
                return;

            m_physicsPollTimer -= Time.fixedDeltaTime;
            if (m_physicsPollTimer > 0f)
                return;

            m_physicsPollTimer = PhysicsPollInterval;
            RefreshPhysicsObjectFreezes();
        }

        // Keeps the zone (and its dome) centered on the boat/tame it was cast on top of, so it
        // follows along instead of being left behind. Only position is copied - not rotation -
        // so the spherical zone doesn't tilt with a rocking boat. If the target is ever destroyed
        // mid-duration, this just stops updating and the zone freezes in place for the rest of
        // its lifetime.
        public void LateUpdate()
        {
            if (m_attachTargetZdoid == ZDOID.None)
                return;

            if (m_attachTarget == null)
            {
                GameObject target = ZNetScene.instance?.FindInstance(m_attachTargetZdoid);
                if (target == null)
                    return;

                m_attachTarget = target.transform;
            }

            transform.position = m_attachTarget.position;
            ShieldDomeHelper.ShowDome(this, transform.position, m_radius, ShieldColors.TimeStop);

            // The zone's own ZDO otherwise never moves from its spawn point (ZNetView only sets a
            // ZDO's position once, at creation) - and ZDOMan uses that stored position to decide
            // which sector the zone belongs to, and therefore which peers it even gets sent to.
            // Without this, only the owner (who always keeps their own ZDOs loaded) and any peer
            // who happened to be near the original cast spot would ever see this zone once the
            // boat/tame sails off - everyone else would never receive it at all. Mirrors how
            // ZSyncTransform.OwnerSync() keeps a moving networked object's ZDO position current.
            if (m_netView != null && m_netView.IsValid() && m_netView.IsOwner())
                m_netView.GetZDO().SetPosition(transform.position);
        }

        // Polls for physics props (logs, debris, arrows, spears, etc.) and characters currently
        // inside the zone and freezes/unfreezes them as they enter/leave, since we can't rely on
        // OnTriggerEnter/Exit alone for these (see the PhysicsPollInterval comment above, and the
        // character-catching block below).
        private void RefreshPhysicsObjectFreezes()
        {
            try
            {
                HashSet<GameObject> stillPresent = new HashSet<GameObject>();

                // Windmills, doors, and Rigidbody-driven hazards (log rain/meteor debris once it
                // has landed, etc.) are all found via physical overlap, since they all carry a
                // Collider.
                foreach (Collider collider in Physics.OverlapSphere(transform.position, m_radius))
                {
                    Character character = collider.GetComponentInParent<Character>();

                    if (character != null)
                    {
                        // OnTriggerEnter only fires for a collider pair's first transition into
                        // overlap - a character already standing inside the zone's radius the
                        // moment it (or the character) first loads on this client (a newly-joined
                        // player, or someone just walking into render range of an already-active
                        // zone with a creature already inside it) never raises that event at all,
                        // since there's no "not touching -> touching" transition to detect.
                        // HandleCharacterEnter already dedupes (m_registeredCreatures /
                        // m_localPlayerRegistered), so calling it every poll tick is safe.
                        if (m_freezeEnemies || m_freezePlayer)
                            HandleCharacterEnter(character);

                        continue;
                    }

                    if (!m_freezeProjectiles)
                        continue;

                    GameObject target = ResolvePhysicsTarget(collider);
                    if (target == null)
                        continue;

                    // Physics.OverlapSphere only guarantees the collider that matched touches the
                    // sphere - not that the resolved target's own root position does. For a large
                    // structure like a windmill (whose propeller/blades extend well past its base),
                    // that collider can reach into the zone from well beyond m_radius. Re-check
                    // against the root position explicitly, same as the Projectile/Aoe scans below,
                    // so "in range" means the same thing across every detection path.
                    if (Vector3.Distance(target.transform.position, transform.position) > m_radius)
                        continue;

                    RegisterPhysicsObject(target, stillPresent);
                }

                if (!m_freezeProjectiles)
                    return;

                // Projectile (arrows, spears, thrown weapons, meteors while in flight) uses
                // raycasts for hit detection (see m_rayRadius) rather than a Collider, so it can
                // be entirely invisible to OverlapSphere. Scan live instances directly by distance
                // instead of going through a collider hierarchy.
                foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
                {
                    if (Vector3.Distance(projectile.transform.position, transform.position) > m_radius)
                        continue;

                    RegisterPhysicsObject(projectile.gameObject, stillPresent);
                }

                // Any Aoe (Twitch-spawned hazard or vanilla attack - monster claws, thorns, etc.)
                // carries no physics Collider of its own on the root, so - like Projectile above -
                // it's invisible to the OverlapSphere pass. Damage is already blocked synchronously
                // the instant an Aoe spawns inside an already-active zone (see the Aoe.Awake()
                // postfix in harmony/TimeStopPatchesWBTI.cs, and TwitchPhysicsFreezeData.
                // FreezeDamage) - this poll's job is to (eventually) pause the particle effect too,
                // and to catch the case where the zone appears only after the Aoe already spawned.
                foreach (Aoe aoe in FindObjectsByType<Aoe>(FindObjectsSortMode.None))
                {
                    GameObject aoeRoot = aoe.transform.root.gameObject;

                    if (Vector3.Distance(aoeRoot.transform.position, transform.position) > m_radius)
                        continue;

                    // Only hazards in s_aoeGraceDelayByRootName need a moment to visually develop
                    // before being frozen - other Aoe hazards have no such animation and can be
                    // frozen the instant they're seen instead.
                    if (s_aoeGraceDelayByRootName.TryGetValue(aoeRoot.name, out float graceDelay) && !m_registeredPhysicsObjects.Contains(aoeRoot))
                    {
                        if (!m_pendingAoeFreezeTimers.TryGetValue(aoeRoot, out float firstSeen))
                        {
                            m_pendingAoeFreezeTimers[aoeRoot] = Time.time;
                            continue;
                        }

                        if (Time.time - firstSeen < graceDelay)
                            continue;

                        m_pendingAoeFreezeTimers.Remove(aoeRoot);
                    }

                    RegisterPhysicsObject(aoeRoot, stillPresent);
                }

                m_departedPhysicsObjects.Clear();
                foreach (GameObject registered in m_registeredPhysicsObjects)
                {
                    if (registered == null || !stillPresent.Contains(registered))
                        m_departedPhysicsObjects.Add(registered);
                }

                foreach (GameObject departed in m_departedPhysicsObjects)
                {
                    m_registeredPhysicsObjects.Remove(departed);

                    // "?." bypasses Unity's overloaded == and doesn't detect a destroyed-but-not-
                    // yet-GC'd GameObject, so it would still try (and throw) on one - use a proper
                    // null check instead. Nothing to unfreeze if the object is already gone anyway.
                    if (departed == null)
                        continue;

                    departed.GetComponent<TwitchPhysicsFreezeData>()?.Unfreeze();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.RefreshPhysicsObjectFreezes failed: " + e);
            }
        }

        private void RegisterPhysicsObject(GameObject target, HashSet<GameObject> stillPresent)
        {
            stillPresent.Add(target);

            TwitchPhysicsFreezeData freezeData = target.GetComponent<TwitchPhysicsFreezeData>();

            // Already fully frozen (by us on an earlier poll, or by another overlapping zone) -
            // nothing left to do.
            if (freezeData != null && freezeData.VisualFrozen)
                return;

            if (!m_registeredPhysicsObjects.Add(target))
                return;

            // A component may already exist here without being fully frozen yet - a hazard like
            // the Smite lightning strike gets its damage blocked synchronously at spawn (see
            // TwitchPhysicsFreezeData.FreezeDamage), before this zone's poll ever runs. Finish
            // freezing it (particles/rigidbody/etc.) rather than creating a second wrapper.
            if (freezeData == null)
                freezeData = target.AddComponent<TwitchPhysicsFreezeData>();

            freezeData.Initialize();
        }

        // Windmill/door pieces are checked first since they're placed structures with no moving
        // Rigidbody of their own - their marker has to land on the exact piece root that their
        // own LateUpdate/coroutine checks for.
        private GameObject ResolvePhysicsTarget(Collider collider)
        {
            TwitchWindmillPersistentData windmill = collider.GetComponentInParent<TwitchWindmillPersistentData>();
            if (windmill != null)
                return windmill.gameObject;

            TwitchDoorPersistentData door = collider.GetComponentInParent<TwitchDoorPersistentData>();
            if (door != null)
                return door.gameObject;

            Rigidbody rigidbody = collider.GetComponentInParent<Rigidbody>();
            if (rigidbody == null)
                return null;

            // The boat/tame the zone is anchored to (if any) is exempt from freezing - it's
            // always sitting at distance 0 from the zone center and would otherwise get frozen
            // like any other prop.
            if (m_attachTarget != null && rigidbody.gameObject == m_attachTarget.gameObject)
                return null;

            // Already frozen by us - keep recognizing it as present regardless of the kinematic
            // flag, since that flag is our own doing. Without this, freezing a rigidbody makes it
            // stop matching "!isKinematic" below, so the very next poll would see it as departed,
            // unfreeze it (undoing our own freeze), then immediately re-detect and re-freeze it -
            // a self-inflicted thrash that let gravity nudge it down a little every cycle.
            if (rigidbody.GetComponent<TwitchPhysicsFreezeData>() != null)
                return rigidbody.gameObject;

            // Otherwise, only a currently-moving (non-kinematic) rigidbody is a new freeze candidate.
            if (!rigidbody.isKinematic)
                return rigidbody.gameObject;

            return null;
        }

        public void OnTriggerEnter(Collider collider)
        {
            try
            {
                Character character = collider.GetComponentInParent<Character>();
                if (character != null)
                    HandleCharacterEnter(character);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.OnTriggerEnter failed: " + e);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            try
            {
                Character character = collider.GetComponentInParent<Character>();
                if (character != null)
                    HandleCharacterExit(character);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.OnTriggerExit failed: " + e);
            }
        }

        // True only on the one client that actually owns/controls this specific character - the
        // local player. Every other Player instance seen here (including other real players) is
        // just a networked puppet on this client, same as any creature.
        private bool IsLocalPlayer(Character character)
        {
            return character.IsPlayer()
                && Player.m_localPlayer != null
                && (character as Player)?.GetPlayerID() == Player.m_localPlayer.GetPlayerID();
        }

        private void HandleCharacterEnter(Character character)
        {
            if (IsLocalPlayer(character))
            {
                // Local-only input/physics freeze (see TimeStopHelper.FreezePlayer) - only
                // meaningful on the client that actually owns and simulates this player.
                if (!m_freezePlayer || m_localPlayerRegistered)
                    return;

                m_localPlayerRegistered = true;
                s_localPlayerFreezeCount++;

                if (s_localPlayerFreezeCount == 1)
                    TimeStopHelper.FreezePlayer();

                return;
            }

            // Covers both enemies and other (non-local) players. TwitchTimeStopZone itself is
            // networked, so every client independently detects this same character entering the
            // zone and independently freezes its own local copy via TwitchFreezeData - that's what
            // makes Animator.enabled (per-client Unity state, never synced by the ZDO position
            // update) actually take effect for everyone watching, not just whichever client
            // triggered the redeem. Without this, a frozen remote player stops moving (position IS
            // synced) but every other client keeps playing their idle/look animations forever.
            bool shouldFreeze = character.IsPlayer() ? m_freezePlayer : m_freezeEnemies;
            if (!shouldFreeze)
                return;

            // The tame the zone is anchored to (if any) is exempt from freezing - it's
            // always sitting at distance 0 from the zone center and would otherwise get
            // frozen like any other creature.
            if (m_attachTarget != null && character.gameObject == m_attachTarget.gameObject)
                return;

            if (character.GetComponent<TwitchFreezeData>() != null)
                return;

            ZNetView netView = character.GetComponent<ZNetView>();
            if (netView == null || !netView.IsValid())
                return;

            ZDOID zdoid = netView.GetZDO().m_uid;
            if (!m_registeredCreatures.Add(zdoid))
                return;

            TwitchFreezeData freeze = character.gameObject.AddComponent<TwitchFreezeData>();
            freeze.Initialize();
        }

        private void HandleCharacterExit(Character character)
        {
            if (IsLocalPlayer(character))
            {
                UnregisterLocalPlayer();
                return;
            }

            ZNetView netView = character.GetComponent<ZNetView>();
            if (netView == null || !netView.IsValid())
                return;

            ZDOID zdoid = netView.GetZDO().m_uid;
            if (!m_registeredCreatures.Remove(zdoid))
                return;

            character.GetComponent<TwitchFreezeData>()?.Unfreeze();
        }

        public void OnDestroy()
        {
            try
            {
                if (m_registeredCreatures.Count > 0)
                {
                    foreach (ZDOID zdoid in m_registeredCreatures)
                    {
                        GameObject go = ZNetScene.instance?.FindInstance(zdoid);
                        go?.GetComponent<TwitchFreezeData>()?.Unfreeze();
                    }

                    m_registeredCreatures.Clear();
                }

                if (m_registeredPhysicsObjects.Count > 0)
                {
                    foreach (GameObject physicsObject in m_registeredPhysicsObjects)
                    {
                        if (physicsObject != null)
                            physicsObject.GetComponent<TwitchPhysicsFreezeData>()?.Unfreeze();
                    }

                    m_registeredPhysicsObjects.Clear();
                }

                UnregisterLocalPlayer();

                s_activeZones.Remove(this);

                ShieldDomeHelper.BreakDome(this);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.OnDestroy failed: " + e);
            }
        }

        private void UnregisterLocalPlayer()
        {
            if (!m_localPlayerRegistered)
                return;

            m_localPlayerRegistered = false;

            if (s_localPlayerFreezeCount > 0)
                s_localPlayerFreezeCount--;

            if (s_localPlayerFreezeCount == 0)
                TimeStopHelper.UnfreezePlayer();
        }
    }
}
