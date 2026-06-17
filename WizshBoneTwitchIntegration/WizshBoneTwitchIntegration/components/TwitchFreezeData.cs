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

        private static readonly int s_startedHash   = "WBTI_Freeze_Started".GetStableHashCode();
        private static readonly int s_durationHash  = "WBTI_Freeze_Duration".GetStableHashCode();
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

                float duration   = m_netView.GetZDO().GetFloat(s_durationHash, 0f);
                float timePassed = (float)DateTime.Now.Subtract(DateTime.Parse(started)).TotalSeconds;
                float remaining  = duration - timePassed;

                if (remaining <= 0f)
                {
                    ClearZDO();
                    Destroy(this);
                    return;
                }

                StartCoroutine(RestoreAnimAndFreeze(remaining));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.Awake failed: " + e);
            }
        }

        public void Initialize(float duration)
        {
            try
            {
                m_netView   = gameObject.GetComponent<ZNetView>();
                m_monsterAI = gameObject.GetComponent<MonsterAI>();
                m_animator  = gameObject.GetComponentInChildren<Animator>();
                m_rigidbody = gameObject.GetComponent<Rigidbody>();

                ZDO zdo = m_netView.GetZDO();

                if (m_animator != null)
                {
                    AnimatorStateInfo info = m_animator.GetCurrentAnimatorStateInfo(0);
                    zdo.Set(s_animHashHash, info.fullPathHash);
                    zdo.Set(s_animTimeHash, info.normalizedTime);
                }

                zdo.Set(s_startedHash,  DateTime.Now.ToString());
                zdo.Set(s_durationHash, duration);

                TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
                baseData?.SetFlag(PersistentComponentFlags.Freeze, true);

                ApplyFreeze();
                StartCoroutine(UnfreezeAfter(duration));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchFreezeData.Initialize failed: " + e);
            }
        }

        // Wait one frame so ZSyncAnimation finishes its initial restore, then re-apply our saved
        // animation snapshot and freeze on top of it.
        private IEnumerator RestoreAnimAndFreeze(float remaining)
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
                yield break;
            }

            yield return new WaitForSeconds(remaining);
            Unfreeze();
        }

        private IEnumerator UnfreezeAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            Unfreeze();
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
        }

        private void Unfreeze()
        {
            try
            {
                if (m_monsterAI != null)
                    m_monsterAI.enabled = true;

                if (m_rigidbody != null)
                    m_rigidbody.constraints = m_originalConstraints;

                if (m_animator != null)
                    m_animator.enabled = true;

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
                zdo.Set(s_durationHash, 0f);
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
