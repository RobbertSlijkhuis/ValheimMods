using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Darkens a row's background <see cref="Image"/> and reveals a set of otherwise-hidden
    /// <see cref="GameObject"/>s (e.g. action buttons) while the pointer hovers over the row.
    ///
    /// Copy-adapted from GUI_OLD/components/RowHoverReveal.cs (new namespace, per the new UI's
    /// isolation-from-GUI_OLD constraint) - unchanged otherwise, it was already fully generic.
    /// </summary>
    internal class RowHoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color HoverColor = new Color(0f, 0f, 0f, 0.5f);

        private Image m_background;
        private List<GameObject> m_revealOnHover;
        private Color m_normalColor;

        /// <summary>
        /// <paramref name="normalColor"/> is the row's own idle background (e.g. a zebra-striped
        /// list's alternating row color, or <see cref="Color.clear"/> for a plain row) - restored
        /// on <see cref="OnPointerExit"/> instead of a single hardcoded transparent constant, so
        /// zebra striping survives hover.
        /// </summary>
        public void Init(Image background, List<GameObject> revealOnHover, Color normalColor)
        {
            m_background    = background;
            m_revealOnHover = revealOnHover;
            m_normalColor   = normalColor;

            m_background.color = m_normalColor;
            foreach (GameObject go in m_revealOnHover)
                go.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_background.color = HoverColor;
            foreach (GameObject go in m_revealOnHover)
                go.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_background.color = m_normalColor;
            foreach (GameObject go in m_revealOnHover)
                go.SetActive(false);
        }
    }
}
