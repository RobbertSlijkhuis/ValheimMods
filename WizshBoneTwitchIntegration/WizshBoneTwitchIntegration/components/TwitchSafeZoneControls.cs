using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZoneControls : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        // Shared across every client via ZDO (see the no-RPC pattern in CLAUDE.md) - any player
        // can be the one to cycle it via alt+use, not just the owner, so the shield dome and
        // projector stay in sync for everyone looking at the same ward.
        private enum VisualMode
        {
            Shield,
            Projector,
            None
        }

        private ZNetView m_netView;
        private Transform projectorTransform;
        private Transform colliderTrans;
        private Transform parentTrans;
        private VisualMode visualMode;

        // Caches the last VisualMode this instance actually applied, so Update() can detect a
        // change made by any client (including this one) and react exactly once - mirrors
        // TwitchSurpriseChest's m_openHandledLocally pattern.
        private VisualMode m_appliedVisualMode;
        private float radius;
        private readonly int radiusHash = "SafeZoneRadius_WBTI".GetStableHashCode();
        private readonly int visualModeHash = "SafeZoneVisualMode_WBTI".GetStableHashCode();

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
                radius = m_netView.GetZDO().GetFloat(radiusHash, 30f);
                visualMode = (VisualMode)m_netView.GetZDO().GetInt(visualModeHash, (int)VisualMode.Shield);
                m_appliedVisualMode = visualMode;

                SetRadius(radius);
                ApplyVisualMode();

                // The mode only ever changes from a player interacting (see Interact()), so a
                // twice-a-second check is plenty responsive without paying a per-frame Update() cost.
                InvokeRepeating(nameof(CheckVisualMode), 0.5f, 0.5f);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not finish TwitchSafeZoneControls Awake: " + e);
            }
        }

        private void CheckVisualMode()
        {
            try
            {
                if (m_netView == null || !m_netView.IsValid())
                    return;

                VisualMode zdoVisualMode = (VisualMode)m_netView.GetZDO().GetInt(visualModeHash, (int)VisualMode.Shield);

                if (zdoVisualMode == m_appliedVisualMode)
                    return;

                visualMode = zdoVisualMode;
                m_appliedVisualMode = zdoVisualMode;
                ApplyVisualMode();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZoneControls.CheckVisualMode failed: " + e);
            }
        }

        public string GetHoverText()
        {
            string title = "Twitchy Ward (Active)\n";
            string inputString = Localization.instance.Localize("[<color=yellow>$KEY_Use</color>]");
            string inputAltString = Localization.instance.Localize("[<color=yellow>$KEY_AltPlace + $KEY_Use</color>]");
            string text = $"{inputString} Change radius\n{inputAltString} Cycle shield/projector/hidden";
            return title + text;
        }

        public string GetHoverName()
        {
            return gameObject.name;
        }

        public float GetHoverOffset()
        {
            return 0f;
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

                    VisualMode nextMode = visualMode switch
                    {
                        VisualMode.Shield => VisualMode.Projector,
                        VisualMode.Projector => VisualMode.None,
                        _ => VisualMode.Shield,
                    };

                    // Any player can interact with this ward, not just its owner - claim
                    // ownership first so the write actually propagates (see CLAUDE.md/
                    // TwitchSurpriseChest.TryTriggerOpen). Update() then applies the change
                    // uniformly on every client, including this one.
                    if (!m_netView.IsOwner())
                        m_netView.ClaimOwnership();

                    m_netView.GetZDO().Set(visualModeHash, (int)nextMode);
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

            colliderTrans.gameObject.GetComponent<TwitchSafeZoneDebugVisual>()?.Refresh();

            if (visualMode == VisualMode.Shield)
                ShieldDomeHelper.ShowDome(this, parentTrans.position, value, ShieldColors.Ward);

            m_netView.GetZDO().Set(radiusHash, value);
        }

        // Applies the current visualMode to both the projector and the shield dome, showing at
        // most one of them at a time (or neither, for VisualMode.None).
        private void ApplyVisualMode()
        {
            projectorTransform.gameObject.SetActive(visualMode == VisualMode.Projector);

            if (visualMode == VisualMode.Shield)
                ShieldDomeHelper.ShowDome(this, parentTrans.position, m_netView.GetZDO().GetFloat(radiusHash, radius), ShieldColors.Ward);
            else
                ShieldDomeHelper.HideDome(this);
        }

        public void OnDestroy()
        {
            try
            {
                ShieldDomeHelper.HideDome(this);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZoneControls.OnDestroy failed: " + e);
            }
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
