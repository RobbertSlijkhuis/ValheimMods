using System;
using System.Collections;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchFreezeData : MonoBehaviour
    {
        private ZNetView m_netView;
        private MonsterAI m_monsterAI;
        private Animator m_animator;
        private Rigidbody m_rigidbody;
        private RigidbodyConstraints m_originalConstraints;
        private ParticleSystem[] m_particleSystems;

        private static readonly int s_startedHash   = "WBTI_Freeze_Started".GetStableHashCode();
        private static readonly int s_animHashHash  = "WBTI_Freeze_AnimHash".GetStableHashCode();
        private static readonly int s_animTimeHash  = "WBTI_Freeze_AnimTime".GetStableHashCode();

        public void Awake()
        {
            try
            {
                m_netView   = gameObject.GetComponent<ZNetView>();
                m_monsterAI = gameObject.GetComponent<MonsterAI>();
                m_animator  = gameObject.GetComponentInChildren<Animator>();
                m_rigidbody = gameObject.GetComponent<Rigidbody>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string started = m_netView.GetZDO().GetString(s_startedHash, "");
                if (started == "")
                    return;

                StartCoroutine(RestoreAnimAndFreeze());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.Awake failed: " + e);
            }
        }

        public void Initialize()
        {
            try
            {
                m_netView   = gameObject.GetComponent<ZNetView>();
                m_monsterAI = gameObject.GetComponent<MonsterAI>();
                m_animator  = gameObject.GetComponentInChildren<Animator>();
                m_rigidbody = gameObject.GetComponent<Rigidbody>();

                // Claim ownership before freezing, so this client (the one that actually detected
                // the freeze) becomes authoritative for the creature's ZDO going forward - the same
                // built-in mechanic ZNetViewHelper.Destroy() already uses. Without this, a creature
                // whose real owner isn't standing in the same zone (so their client never applies
                // this freeze locally) would keep having its position/state pushed over the network
                // by that owner the whole time, fighting the local freeze rather than just
                // desyncing once on unfreeze the way a Rigidbody prop would. Players are exempt -
                // a live player's own client must stay the ZDO owner (it's what drives their
                // movement input); on a player this component only mirrors the visual freeze
                // (Animator/particles) onto other clients' copies, so ownership never needs to move.
                bool isPlayer = gameObject.GetComponent<Character>()?.IsPlayer() ?? false;
                if (!isPlayer && m_netView != null && m_netView.IsValid() && !m_netView.IsOwner())
                    m_netView.ClaimOwnership();

                ZDO zdo = m_netView.GetZDO();

                if (m_animator != null)
                {
                    AnimatorStateInfo info = m_animator.GetCurrentAnimatorStateInfo(0);
                    zdo.Set(s_animHashHash, info.fullPathHash);
                    zdo.Set(s_animTimeHash, info.normalizedTime);
                }

                zdo.Set(s_startedHash, DateTime.Now.ToString());

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.Freeze, true);

                ApplyFreeze();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.Initialize failed: " + e);
            }
        }

        // Wait one frame so ZSyncAnimation finishes its initial restore, then re-apply our saved
        // animation snapshot and freeze on top of it.
        private IEnumerator RestoreAnimAndFreeze()
        {
            yield return null;

            try
            {
                if (m_animator != null)
                {
                    int   animHash = m_netView.GetZDO().GetInt(s_animHashHash, 0);
                    float animTime = m_netView.GetZDO().GetFloat(s_animTimeHash, 0f);

                    if (animHash != 0)
                        m_animator.Play(animHash, 0, animTime);
                }

                ApplyFreeze();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.RestoreAnimAndFreeze failed: " + e);
            }
        }

        private void ApplyFreeze()
        {
            if (m_monsterAI != null)
                m_monsterAI.enabled = false;

            if (m_rigidbody != null)
            {
                m_originalConstraints       = m_rigidbody.constraints;
                m_rigidbody.velocity        = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
                m_rigidbody.constraints     = RigidbodyConstraints.FreezeAll;
            }

            // Disabling the Animator (rather than just zeroing speed) is required: with speed = 0
            // the state machine still evaluates zero-duration transitions on Update(), snapping
            // creatures/player to their Idle state pose instead of holding the current frame.
            if (m_animator != null)
                m_animator.enabled = false;

            // Pauses any ongoing particle effects on the creature itself (status effects, weapon
            // trails, auras) so a frozen enemy looks fully frozen, not just AI/physics-wise.
            m_particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particles in m_particleSystems)
                particles.Pause(false);
        }

        public void Unfreeze()
        {
            try
            {
                if (m_monsterAI != null)
                    m_monsterAI.enabled = true;

                if (m_rigidbody != null)
                    m_rigidbody.constraints = m_originalConstraints;

                if (m_animator != null)
                    m_animator.enabled = true;

                if (m_particleSystems != null)
                {
                    foreach (ParticleSystem particles in m_particleSystems)
                    {
                        if (particles != null)
                            particles.Play(false);
                    }
                }

                ClearZDO();
                Destroy(this);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.Unfreeze failed: " + e);
            }
        }

        private void ClearZDO()
        {
            try
            {
                if (m_netView == null || !m_netView.IsValid())
                    return;

                ZDO zdo = m_netView.GetZDO();
                zdo.Set(s_startedHash,  "");
                zdo.Set(s_animHashHash, 0);
                zdo.Set(s_animTimeHash, 0f);

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.Freeze, false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.ClearZDO failed: " + e);
            }
        }
    }
}
