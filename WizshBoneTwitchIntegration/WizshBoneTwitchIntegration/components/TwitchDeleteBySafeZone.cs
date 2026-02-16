using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchDeleteBySafeZone : MonoBehaviour
    {
        public Rigidbody m_rigidBody;
        public CapsuleCollider m_capsuleCollider;

        public void Awake()
        {
            gameObject.layer = 10;
            gameObject.tag = "Untagged";

            m_rigidBody = gameObject.GetComponent<Rigidbody>();
            m_capsuleCollider = gameObject.GetComponent<CapsuleCollider>();

            if (m_rigidBody == null)
                m_rigidBody = gameObject.AddComponent<Rigidbody>();

            if (m_capsuleCollider == null)
                m_capsuleCollider = gameObject.AddComponent<CapsuleCollider>();

            m_rigidBody.sleepThreshold = 0;
            m_rigidBody.WakeUp();
        }

        public void SetKinematic(bool value)
        {
            m_rigidBody.isKinematic = value;
        }

        public void SetCenter(Vector3 center)
        {
            m_capsuleCollider.center = center;
        }
    }
}
