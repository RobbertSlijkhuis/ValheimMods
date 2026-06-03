using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Factory for creating a vertical-only scroll view with an always-visible scrollbar.
    /// Returns the <c>Content</c> <see cref="GameObject"/> that children should be parented into.
    /// </summary>
    internal static class ScrollableView
    {
        public const float ScrollbarWidth = 16f;

        private const float ScrollSensitivity = 80f;

        private static readonly Color TrackColor  = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private static readonly Color HandleColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        /// <summary>
        /// Creates a scroll view that stretches to fill its parent, inset by the given offsets.
        /// </summary>
        /// <param name="parent">The parent <see cref="GameObject"/>.</param>
        /// <param name="name">Name prefix used for the generated GameObjects.</param>
        /// <param name="offsetMin">
        ///   Bottom-left inset from the parent edges (e.g. <c>new Vector2(50f, 80f)</c>
        ///   = 50 px from left/right, 80 px from bottom).
        /// </param>
        /// <param name="offsetMax">
        ///   Top-right inset as negative values (e.g. <c>new Vector2(-50f, -130f)</c>
        ///   = 50 px from right, 130 px from top).
        /// </param>
        /// <param name="backgroundColor">Optional background color. Defaults to fully transparent.</param>
        /// <returns>The <c>Content</c> container. Parent your scroll items here.</returns>
        public static GameObject CreateStretched(
            GameObject parent,
            string     name,
            Vector2    offsetMin,
            Vector2    offsetMax,
            Color?     backgroundColor    = null,
            bool       autoHideScrollbar  = false)
        {
            GameObject scrollRoot = CreateScrollRoot(parent, name, Vector2.zero, Vector2.one);
            scrollRoot.GetComponent<Image>().color = backgroundColor ?? new Color(0f, 0f, 0f, 0f);

            RectTransform rt = scrollRoot.GetComponent<RectTransform>();
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            return BuildInterior(scrollRoot, autoHideScrollbar);
        }

        /// <summary>
        /// Creates a scroll view with an explicit size anchored at a specific position.
        /// </summary>
        /// <param name="parent">The parent <see cref="GameObject"/>.</param>
        /// <param name="name">Name prefix used for the generated GameObjects.</param>
        /// <param name="anchoredPosition">Position relative to the chosen anchor point.</param>
        /// <param name="width">Total width including the scrollbar.</param>
        /// <param name="height">Visible height of the scroll view.</param>
        /// <param name="anchorMin">Anchor min. Defaults to top-center <c>(0.5, 1)</c>.</param>
        /// <param name="anchorMax">Anchor max. Defaults to top-center <c>(0.5, 1)</c>.</param>
        /// <param name="pivot">Pivot point. Defaults to top-center <c>(0.5, 1)</c>.</param>
        /// <param name="backgroundColor">Optional background color. Defaults to a dark semi-transparent panel.</param>
        /// <returns>The <c>Content</c> container. Parent your scroll items here.</returns>
        public static GameObject CreateFixed(
            GameObject parent,
            string     name,
            Vector2    anchoredPosition,
            float      width,
            float      height,
            Vector2?   anchorMin          = null,
            Vector2?   anchorMax          = null,
            Vector2?   pivot              = null,
            Color?     backgroundColor    = null,
            bool       autoHideScrollbar  = false)
        {
            Vector2 aMin = anchorMin ?? new Vector2(0.5f, 1f);
            Vector2 aMax = anchorMax ?? new Vector2(0.5f, 1f);
            Vector2 piv  = pivot    ?? new Vector2(0.5f, 1f);

            GameObject scrollRoot = CreateScrollRoot(parent, name, aMin, aMax);
            scrollRoot.GetComponent<Image>().color = backgroundColor ?? new Color(0.1f, 0.1f, 0.1f, 0.5f);

            RectTransform rt = scrollRoot.GetComponent<RectTransform>();
            rt.pivot            = piv;
            rt.sizeDelta        = new Vector2(width, height);
            rt.anchoredPosition = anchoredPosition;

            return BuildInterior(scrollRoot, autoHideScrollbar);
        }

        /// <summary>
        /// Sets the height of a content <see cref="GameObject"/> previously returned by
        /// <see cref="CreateFixed"/> or <see cref="CreateStretched"/>, preserving its width.
        /// </summary>
        /// <param name="content">The content container returned by the Create methods.</param>
        /// <param name="height">The new total height in pixels.</param>
        public static void SetContentHeight(GameObject content, float height)
        {
            RectTransform rt = content.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        // ── private helpers ──────────────────────────────────────────────────

        private static GameObject CreateScrollRoot(
            GameObject parent,
            string     name,
            Vector2    anchorMin,
            Vector2    anchorMax)
        {
            GameObject scrollRoot = new GameObject(name + "ScrollView");
            scrollRoot.transform.SetParent(parent.transform, false);

            RectTransform rt = scrollRoot.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;

            // Image is required for raycasting (scroll wheel input)
            scrollRoot.AddComponent<Image>().color = Color.clear;

            ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal        = false;
            scrollRect.vertical          = true;
            scrollRect.scrollSensitivity = ScrollSensitivity;
            scrollRect.movementType      = ScrollRect.MovementType.Clamped;

            return scrollRoot;
        }

        private static GameObject BuildInterior(GameObject scrollRoot, bool autoHideScrollbar)
        {
            ScrollRect scrollRect = scrollRoot.GetComponent<ScrollRect>();

            // Viewport — leave room on the right for the always-visible scrollbar
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);

            RectTransform viewportRt = viewport.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = new Vector2(-ScrollbarWidth, 0f);

            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRt;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin        = new Vector2(0f, 1f);
            contentRt.anchorMax        = new Vector2(1f, 1f);
            contentRt.pivot            = new Vector2(0.5f, 1f);
            contentRt.sizeDelta        = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;

            scrollRect.content = contentRt;

            // Scrollbar track
            GameObject scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollRoot.transform, false);

            RectTransform scrollbarRt = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRt.anchorMin        = new Vector2(1f, 0f);
            scrollbarRt.anchorMax        = new Vector2(1f, 1f);
            scrollbarRt.pivot            = new Vector2(1f, 0.5f);
            scrollbarRt.sizeDelta        = new Vector2(ScrollbarWidth, 0f);
            scrollbarRt.anchoredPosition = Vector2.zero;

            scrollbarObj.AddComponent<Image>().color = TrackColor;

            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(scrollbarObj.transform, false);

            RectTransform handleRt = handleObj.AddComponent<RectTransform>();
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.sizeDelta = Vector2.zero;

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = HandleColor;

            scrollbar.handleRect    = handleRt;
            scrollbar.targetGraphic = handleImage;

            scrollRect.verticalScrollbar           = scrollbar;
            scrollRect.verticalScrollbarVisibility = autoHideScrollbar ? ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport : ScrollRect.ScrollbarVisibility.Permanent;

            return content;
        }
    }
}