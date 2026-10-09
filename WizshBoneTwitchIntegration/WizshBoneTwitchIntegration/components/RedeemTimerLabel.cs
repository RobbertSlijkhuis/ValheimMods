using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    // Purely client-local debug label - shows the time left on a TwitchPersistentDestruction
    // countdown at the center of the object (weather zone, redeem boat, ...), 1m above it. It's a
    // UI element on RedeemTimerHelper's screen-space overlay canvas, repositioned every frame with
    // Camera.WorldToScreenPoint (the way vanilla's enemy health bars work) rather than a world-space
    // object: overlay canvases draw after post-processing, so depth of field, bloom and the like
    // never touch it, and it stays a constant on-screen size at any distance. Hidden while behind the
    // camera, off-screen, beyond MaxDistance, or while the large map is open. Added/removed by
    // RedeemTimerHelper as the "Show safezone bounds" toggle changes. No ZNetView/ZDO of its own, so
    // it's never networked - each client shows its own countdown from the replicated start time.
    internal class RedeemTimerLabel : MonoBehaviour
    {
        private const float HeightAboveCenter = 1f;
        private const float MaxDistance = 250f;
        private const float OffscreenMargin = 50f;

        private const int FontSize = 20;
        private const float PaddingX = 8f;
        private const float PaddingY = 3f;

        private static readonly Color TextColor = new Color(1f, 0.9f, 0.4f, 1f);
        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.8f);

        private TwitchPersistentDestruction m_destruction;
        private Canvas m_canvas;
        private GameObject m_label;
        private RectTransform m_rect;
        private Text m_text;
        private Vector3 m_localCenter;
        private bool m_centerComputed;
        private int m_shownSeconds = -1;
        private bool m_shownPaused;

        public void Initialize(TwitchPersistentDestruction destruction)
        {
            try
            {
                m_destruction = destruction;
                m_canvas = RedeemTimerHelper.GetCanvas();

                // The root carries the background; the text sits on a child stretched over it. Sized to
                // the text in UpdateText(), so it starts hidden until LateUpdate() positions it.
                m_label = new GameObject("WBTI_RedeemTimerLabel", typeof(RectTransform), typeof(Image));
                m_label.transform.SetParent(m_canvas.transform, false);

                m_rect = (RectTransform)m_label.transform;
                m_rect.anchorMin = Vector2.zero;
                m_rect.anchorMax = Vector2.zero;
                m_rect.pivot = new Vector2(0.5f, 0.5f);

                Image background = m_label.GetComponent<Image>();
                background.color = BackgroundColor;
                background.raycastTarget = false;

                GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(m_label.transform, false);

                RectTransform textRect = (RectTransform)textObject.transform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                m_text = textObject.GetComponent<Text>();
                m_text.font = GUIManager.Instance.AveriaSerifBold;
                m_text.fontSize = FontSize;
                m_text.alignment = TextAnchor.MiddleCenter;
                m_text.horizontalOverflow = HorizontalWrapMode.Overflow;
                m_text.verticalOverflow = VerticalWrapMode.Overflow;
                m_text.color = TextColor;
                m_text.raycastTarget = false;

                m_label.SetActive(false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("RedeemTimerLabel.Initialize failed: " + e);
            }
        }

        // Center of the object's physical body (a ship's hull, a zone's collider), so the text sits
        // mid-object rather than on the pivot, which for a boat is down at the waterline. Solid
        // colliders win over triggers - a boat's oversized trigger volumes (e.g. the onboard
        // safezone) would otherwise drag the center off the hull; an object with only triggers
        // (weather/time stop zones) uses those instead.
        private Vector3 ComputeWorldCenter()
        {
            Bounds? solid = null;
            Bounds? trigger = null;

            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled)
                    continue;

                if (collider.isTrigger)
                    trigger = Encapsulated(trigger, collider.bounds);
                else
                    solid = Encapsulated(solid, collider.bounds);
            }

            return (solid ?? trigger)?.center ?? transform.position;
        }

        private static Bounds Encapsulated(Bounds? existing, Bounds addition)
        {
            if (existing == null)
                return addition;

            Bounds bounds = existing.Value;
            bounds.Encapsulate(addition);
            return bounds;
        }

        public void LateUpdate()
        {
            if (m_label == null || m_destruction == null)
                return;

            // Deferred to the first frame the label is actually drawn rather than done in
            // Initialize(): that can run from Awake, before the object's colliders are sized/enabled.
            if (!m_centerComputed)
            {
                m_localCenter = transform.InverseTransformPoint(ComputeWorldCenter());
                m_centerComputed = true;
            }

            Camera camera = Camera.main;
            bool show = camera != null && m_destruction.RemainingSeconds >= 0f && !Minimap.IsOpen();
            Vector3 screenPoint = Vector3.zero;

            if (show)
            {
                Vector3 worldPosition = transform.TransformPoint(m_localCenter) + Vector3.up * HeightAboveCenter;
                screenPoint = camera.WorldToScreenPoint(worldPosition);

                show = screenPoint.z > 0f
                    && Vector3.Distance(camera.transform.position, worldPosition) <= MaxDistance
                    && screenPoint.x > -OffscreenMargin && screenPoint.x < Screen.width + OffscreenMargin
                    && screenPoint.y > -OffscreenMargin && screenPoint.y < Screen.height + OffscreenMargin;
            }

            if (m_label.activeSelf != show)
                m_label.SetActive(show);

            if (!show)
                return;

            UpdateText();

            // Screen pixels to canvas units - the canvas scales with screen height (see
            // RedeemTimerHelper.GetCanvas), so this keeps the label a consistent size at any resolution.
            m_rect.anchoredPosition = new Vector2(screenPoint.x, screenPoint.y) / m_canvas.scaleFactor;
        }

        // Only touches the text when the displayed second (or paused state) actually changes.
        private void UpdateText()
        {
            int seconds = Mathf.CeilToInt(m_destruction.RemainingSeconds);
            bool paused = m_destruction.IsPaused;

            if (seconds == m_shownSeconds && paused == m_shownPaused)
                return;

            m_shownSeconds = seconds;
            m_shownPaused = paused;

            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string formatted = time.TotalHours >= 1
                ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
                : $"{time.Minutes}:{time.Seconds:00}";

            m_text.text = paused ? formatted + " (paused)" : formatted;
            m_rect.sizeDelta = new Vector2(m_text.preferredWidth + PaddingX * 2f, m_text.preferredHeight + PaddingY * 2f);
        }

        public void OnDestroy()
        {
            if (m_label != null)
                Destroy(m_label);
        }
    }
}
