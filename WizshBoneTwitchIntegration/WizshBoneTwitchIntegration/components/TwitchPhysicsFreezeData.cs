using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPhysicsFreezeData : MonoBehaviour
    {
        private Rigidbody m_rigidbody;
        private Projectile m_projectile;
        private Windmill m_windmill;
        private bool m_frozeRigidbody;

        public void Initialize()
        {
            try
            {
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
