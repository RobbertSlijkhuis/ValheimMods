using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Hides <see cref="m_scrollbarObj"/> whenever <see cref="m_content"/> doesn't overflow
    /// <see cref="m_viewport"/>, without ever touching either RectTransform's size.
    ///
    /// This exists instead of Unity's built-in <c>ScrollRect.ScrollbarVisibility.
    /// AutoHideAndExpandViewport</c> because that mode also *expands the viewport* to reclaim the
    /// scrollbar's space whenever it hides it - which breaks any UI built against a fixed "how
    /// wide is the content area" assumption (e.g. ProfilesTab.cs's column layout) the moment
    /// enough content exists to need the scrollbar again: whatever was positioned against the
    /// full, unreserved width suddenly finds itself partly hidden under the now-visible bar.
    /// <see cref="ScrollableList"/> instead keeps the viewport permanently reserving the
    /// scrollbar's width (stable for callers to build against) and uses this purely-visual toggle
    /// when <c>autoHideScrollbar</c> is requested.
    ///
    /// Must be attached to an always-active sibling of the scrollbar (e.g. the scroll root) -
    /// never to the scrollbar GameObject itself, since Unity stops calling LateUpdate the moment
    /// a component's own GameObject is deactivated, which would prevent it from ever reactivating.
    /// </summary>
    internal class ScrollbarAutoHide : MonoBehaviour
    {
        private RectTransform m_content;
        private RectTransform m_viewport;
        private GameObject m_scrollbarObj;

        public void Init(RectTransform content, RectTransform viewport, GameObject scrollbarObj)
        {
            m_content = content;
            m_viewport = viewport;
            m_scrollbarObj = scrollbarObj;
        }

        private void LateUpdate()
        {
            // Small epsilon so floating-point noise right at the boundary doesn't flicker the bar.
            bool needed = m_content.rect.height > m_viewport.rect.height + 0.5f;
            if (m_scrollbarObj.activeSelf != needed)
                m_scrollbarObj.SetActive(needed);
        }
    }
}
