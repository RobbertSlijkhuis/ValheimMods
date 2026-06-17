using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchTimeStopZone : MonoBehaviour
    {
        private bool m_freezeEnemies;
        private bool m_freezePlayer;
        private bool m_localPlayerRegistered;
        private ParticleSystem m_particles;
        private ZNetView m_netView;
        private readonly HashSet<ZDOID> m_registeredCreatures = new HashSet<ZDOID>();
        private readonly HashSet<Rigidbody> m_registeredRigidbodies = new HashSet<Rigidbody>();

        // Tracks how many active zones currently have the local player frozen, so overlapping
        // zones don't unfreeze the player while still standing in another one.
        private static int s_localPlayerFreezeCount = 0;

        private static readonly int s_configuredHash     = "WBTI_TimeStopZone_Configured".GetStableHashCode();
        private static readonly int s_radiusHash          = "WBTI_TimeStopZone_Radius".GetStableHashCode();
        private static readonly int s_freezeEnemiesHash   = "WBTI_TimeStopZone_FreezeEnemies".GetStableHashCode();
        private static readonly int s_freezePlayerHash    = "WBTI_TimeStopZone_FreezePlayer".GetStableHashCode();
        private static readonly int s_durationHash        = "WBTI_TimeStopZone_Duration".GetStableHashCode();

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

                ApplyConfig(
                    zdo.GetFloat(s_radiusHash, 0f),
                    zdo.GetBool(s_freezeEnemiesHash, false),
                    zdo.GetBool(s_freezePlayerHash, false),
                    zdo.GetFloat(s_durationHash, 0f));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.Awake failed: " + e);
            }
        }

        public void Initialize(float radius, bool freezeEnemies, bool freezePlayer, float duration)
        {
            try
            {
                if (m_netView != null && m_netView.IsValid())
                {
                    ZDO zdo = m_netView.GetZDO();
                    zdo.Set(s_radiusHash, radius);
                    zdo.Set(s_freezeEnemiesHash, freezeEnemies);
                    zdo.Set(s_freezePlayerHash, freezePlayer);
                    zdo.Set(s_durationHash, duration);
                    zdo.Set(s_configuredHash, true);
                }

                ApplyConfig(radius, freezeEnemies, freezePlayer, duration);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.Initialize failed: " + e);
            }
        }

        private void ApplyConfig(float radius, bool freezeEnemies, bool freezePlayer, float duration)
        {
            m_freezeEnemies = freezeEnemies;
            m_freezePlayer  = freezePlayer;

            SphereCollider sphereCollider = GetComponentInChildren<SphereCollider>();
            if (sphereCollider != null)
                sphereCollider.radius = radius;

            m_particles = GetComponentInChildren<ParticleSystem>();
            if (m_particles != null)
            {
                ParticleSystem.ShapeModule shape = m_particles.shape;

                // The Shape module has its own Scale (independent of the GameObject's Transform)
                // that multiplies radius - divide it out so "radius" here always means world meters
                // regardless of whatever scale the shape happens to be authored with.
                float shapeScale = Mathf.Max(shape.scale.x, 0.0001f);
                shape.radius = radius / shapeScale;

                // Stop emitting early enough that the longest-lived particles can fully die out
                // before TwitchPersistentDestruction destroys the zone, instead of popping out instantly.
                float lifetime = m_particles.main.startLifetime.constantMax;
                float stopDelay = Mathf.Max(duration - lifetime, 0f);
                StartCoroutine(StopEmittingAfterDelay(stopDelay));
            }
        }

        private IEnumerator StopEmittingAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (m_particles != null)
                m_particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void OnTriggerEnter(Collider collider)
        {
            try
            {
                Character character = collider.GetComponentInParent<Character>();
                if (character != null)
                {
                    HandleCharacterEnter(character);
                    return;
                }

                if (!m_freezeEnemies)
                    return;

                // Non-character physics hazards (e.g. log rain/meteors - see SpawnAbilityExtension.Spawn2's
                // "monsterAI == null && impactEffect != null" branch) have no Character component, so
                // freeze them by halting their Rigidbody instead.
                Rigidbody rigidbody = collider.GetComponentInParent<Rigidbody>();
                if (rigidbody == null || rigidbody.isKinematic)
                    return;

                if (rigidbody.GetComponentInChildren<ImpactEffect>(true) == null)
                    return;

                if (!m_registeredRigidbodies.Add(rigidbody))
                    return;

                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = true;
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
                {
                    HandleCharacterExit(character);
                    return;
                }

                Rigidbody rigidbody = collider.GetComponentInParent<Rigidbody>();
                if (rigidbody == null || !m_registeredRigidbodies.Remove(rigidbody))
                    return;

                rigidbody.isKinematic = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTimeStopZone.OnTriggerExit failed: " + e);
            }
        }

        private void HandleCharacterEnter(Character character)
        {
            if (character.IsPlayer())
            {
                if (!m_freezePlayer || m_localPlayerRegistered)
                    return;

                Player player = character as Player;
                if (player == null || Player.m_localPlayer == null || player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    return;

                m_localPlayerRegistered = true;
                s_localPlayerFreezeCount++;

                if (s_localPlayerFreezeCount == 1)
                    TimeStopHelper.FreezePlayer();
            }
            else
            {
                if (!m_freezeEnemies)
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
        }

        private void HandleCharacterExit(Character character)
        {
            if (character.IsPlayer())
            {
                Player player = character as Player;
                if (player == null || Player.m_localPlayer == null || player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    return;

                UnregisterLocalPlayer();
            }
            else
            {
                ZNetView netView = character.GetComponent<ZNetView>();
                if (netView == null || !netView.IsValid())
                    return;

                ZDOID zdoid = netView.GetZDO().m_uid;
                if (!m_registeredCreatures.Remove(zdoid))
                    return;

                character.GetComponent<TwitchFreezeData>()?.Unfreeze();
            }
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

                if (m_registeredRigidbodies.Count > 0)
                {
                    foreach (Rigidbody rigidbody in m_registeredRigidbodies)
                    {
                        if (rigidbody != null)
                            rigidbody.isKinematic = false;
                    }

                    m_registeredRigidbodies.Clear();
                }

                UnregisterLocalPlayer();
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
