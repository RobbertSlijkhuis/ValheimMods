using UnityEngine;
using UnityEngine.EventSystems;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Clears the EventSystem's selection after a click on this control. A clicked Selectable stays
    /// "selected" until something else is, and Jötunn's toggle style draws the selected state as
    /// bright as hover - so a clicked toggle kept looking hovered after the pointer left. Reacts to
    /// real pointer clicks only (not programmatic changes, e.g. a Reset button flipping the toggle),
    /// so it never steals focus from an input field the user is typing in.
    /// </summary>
    internal class DeselectOnClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
