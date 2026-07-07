using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Darkens a row's background <see cref="Image"/> and reveals a set of otherwise-hidden
    /// <see cref="GameObject"/>s (e.g. action buttons) while the pointer hovers over the row.
    /// </summary>
    internal class RowHoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color NormalColor = new Color(0f, 0f, 0f, 0f);
        private static readonly Color HoverColor  = new Color(0f, 0f, 0f, 0.5f);

        private Image m_background;
        private List<GameObject> m_revealOnHover;

        public void Init(Image background, List<GameObject> revealOnHover)
        {
            m_background    = background;
            m_revealOnHover = revealOnHover;

            m_background.color = NormalColor;
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
            m_background.color = NormalColor;
            foreach (GameObject go in m_revealOnHover)
                go.SetActive(false);
        }
    }
}
