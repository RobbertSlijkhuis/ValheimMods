using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZoneControls : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        
        private ZNetView m_netView;
        private Transform projectorTransform;
        private Transform colliderTrans;
        private Transform parentTrans;
        private bool isProjectorOn;
        private float radius;
        private readonly int radiusHash = "SafeZoneRadius_WBTI".GetStableHashCode();

        public void Awake()
        {
            try
            {
                m_netView = transform.parent.gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                projectorTransform = transform.Find("projector");
                colliderTrans = transform.parent.Find("safezone");
                parentTrans = transform.parent;
                isProjectorOn = false;
                radius = m_netView.GetZDO().GetFloat(radiusHash, 30f);

                SetRadius(radius);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not finish TwitchSafeZoneControls Awake: " + e);
            }
        }

        public string GetHoverText()
        {
            string title = "Twitchy Ward (Active)\n";
            string inputString = Localization.instance.Localize("[<color=yellow>$KEY_Use</color>]");
            string inputAltString = Localization.instance.Localize("[<color=yellow>$KEY_AltPlace + $KEY_Use</color>]");
            string text = $"{inputString} Change radius\n{inputAltString} Show radius projector";
            return title + text;
        }

        public string GetHoverName()
        {
            return gameObject.name;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (hold)
                    return false;

                if (alt)
                {
                    if (projectorTransform == null)
                        throw new Exception("Could not find projector gameObject");

                    isProjectorOn = !isProjectorOn;
                    projectorTransform.gameObject.SetActive(isProjectorOn);
                    return true;
                }

                TextInput.instance.RequestText(this, "Enter radius", 3);
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
                return false;
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public string GetText()
        {
            return $"{m_netView.GetZDO().GetFloat(radiusHash)}";
        }

        public void SetText(string value)
        {
            if (!IsDigitsOnly(value))
                return;

            float floatValue = float.Parse(value);
            SetRadius(floatValue);
        }

        public void SetRadius(float value)
        {
            if (colliderTrans == null || projectorTransform == null || parentTrans == null)
                throw new Exception("Transform is null");

            value = Mathf.Clamp(value, 3f, 1000f);

            CircleProjector projectorComp = projectorTransform.gameObject.GetComponent<CircleProjector>();
            CapsuleCollider colliderComp = colliderTrans.gameObject.GetComponent<CapsuleCollider>();
            ParticleSystemForceField forceFieldComp = parentTrans.gameObject.GetComponent<ParticleSystemForceField>();
            projectorComp.m_radius = value;
            projectorComp.m_nrOfSegments = Mathf.RoundToInt(value * 4f);
            projectorComp.m_speed = 3.2f / value;
            colliderComp.radius = value;
            forceFieldComp.endRange = value;

            m_netView.GetZDO().Set(radiusHash, value);
        }

        private bool IsDigitsOnly(string value)
        {
            foreach (char character in value)
            {
                if (character < '0' || character > '9')
                    return false;
            }

            return true;
        }
    }
}
