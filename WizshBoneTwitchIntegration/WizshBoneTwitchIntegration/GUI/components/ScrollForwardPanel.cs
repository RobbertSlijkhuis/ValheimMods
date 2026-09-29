using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Sits on a panel nested inside a <see cref="ScrollRect"/> and decides per mouse-wheel event
    /// whether the panel handles the scroll itself or lets the outer scroll view have it. The
    /// panel's <c>tryScroll</c> callback receives the ScrollRect-equivalent pixel delta and returns
    /// true if it consumed the event (the outer view then stays put); otherwise the event is
    /// forwarded to the outer ScrollRect exactly as if this panel weren't there. Not a nested
    /// generic type on purpose - Unity can't AddComponent a MonoBehaviour declared inside a
    /// generic class (see <see cref="EntryListEditor{TEntry}"/>).
    /// </summary>
    internal class ScrollForwardPanel : MonoBehaviour, IScrollHandler
    {
        private ScrollRect m_outerScroll;
        private Func<float, bool> m_tryScroll;

        public void Init(ScrollRect outerScroll, Func<float, bool> tryScroll)
        {
            m_outerScroll = outerScroll;
            m_tryScroll = tryScroll;
        }

        public void OnScroll(PointerEventData eventData)
        {
            // Same conversion ScrollRect.OnScroll applies: wheel-down is negative in the event but
            // scrolls content up (positive anchoredPosition.y for a top-pivoted content).
            float delta = -eventData.scrollDelta.y * ScrollableList.ScrollSensitivity;
            if (m_tryScroll != null && m_tryScroll(delta))
            {
                eventData.Use();
                return;
            }

            if (m_outerScroll != null)
                m_outerScroll.OnScroll(eventData);
        }
    }
}
