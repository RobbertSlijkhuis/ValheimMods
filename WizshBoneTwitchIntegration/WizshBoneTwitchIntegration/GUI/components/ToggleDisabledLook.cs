using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Fades a toggle's checkmark while the toggle is disabled (e.g. under a CanvasGroup with
    /// `interactable` off). Jötunn's toggle style only tints the toggle's box (its target graphic)
    /// with the disabled color block; the checkmark is a separate Image and would stay full orange
    /// on a faded box. This applies the same tint (<see cref="DisabledTint"/> = Jötunn's toggle
    /// disabledColor) to the checkmark's own color, so a disabled checked box reads as one faded
    /// control like the disabled inputs and buttons.
    ///
    /// Lives on the toggle's own GameObject: Unity sends OnCanvasGroupChanged there whenever a
    /// CanvasGroup above it changes, which is the same signal the Selectable itself relies on.
    /// </summary>
    internal class ToggleDisabledLook : MonoBehaviour
    {
        // Jötunn's ValheimToggleColorBlock.disabledColor.
        private static readonly Color DisabledTint = new Color(0.784f, 0.784f, 0.784f, 0.502f);

        private Toggle m_toggle;
        private Graphic m_checkmark;
        private Color m_baseColor;

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
            m_checkmark = m_toggle != null ? m_toggle.graphic : null;

            if (m_checkmark != null)
                m_baseColor = m_checkmark.color;
        }

        private void OnEnable() => Apply();

        // Start runs after every OnEnable/CanvasGroup message of the frame, so the Toggle already
        // knows whether a group above it disables it.
        private void Start() => Apply();

        private void OnCanvasGroupChanged() => Apply();

        private void Apply()
        {
            if (m_checkmark == null)
                return;

            m_checkmark.color = m_toggle.IsInteractable()
                ? m_baseColor
                : new Color(m_baseColor.r * DisabledTint.r, m_baseColor.g * DisabledTint.g, m_baseColor.b * DisabledTint.b, m_baseColor.a * DisabledTint.a);
        }
    }
}
