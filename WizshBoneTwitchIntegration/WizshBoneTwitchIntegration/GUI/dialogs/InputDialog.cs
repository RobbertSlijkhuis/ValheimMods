using Jotunn.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A reusable modal panel prompting for a single line of text - the generic version of
    /// GUI_OLD/dialogs/ProfileNameDialog.cs, decoupled from its baked-in
    /// <c>ProfileNameDialogMode</c> enum: callers pass title/description/confirm-button text
    /// directly instead, the same way <see cref="ConfirmDialog"/> already does. This makes it
    /// reusable by any future tab needing a single text-input prompt, not just profile names.
    ///
    /// Shown via <see cref="Show"/>; Cancel always dismisses it, Confirm dismisses it only if the
    /// caller's validation succeeds - otherwise the panel stays open and shows the error inline.
    /// </summary>
    internal class InputDialog
    {
        private GameObject m_panel;
        private Text       m_titleText;
        private Text       m_descriptionText;
        private InputField m_valueInput;
        private Text       m_feedbackText;
        private Button     m_confirmButton;
        private Button     m_cancelButton;
        private Text       m_confirmButtonText;
        private bool       m_blockingInput;

        private const float PanelWidth   = 460f;
        private const float PanelHeight  = 300f;
        private const float TitleY       = -40f;
        private const float DescriptionY = -85f;
        private const float InputY       = -150f;
        private const float FeedbackY    = -195f;
        private const float InputWidth   = 360f;
        private const float ConfirmBtnX  = -110f;
        private const float CancelBtnX   =  110f;
        private const float BtnY         =  40f;
        private const float BtnWidth     =  160f;
        private const float BtnHeight    =  50f;

        /// <summary>
        /// Creates the panel once and hides it. Call <see cref="Show"/> to display it.
        /// Must be called after <see cref="GUIManager.OnCustomGUIAvailable"/>.
        /// </summary>
        public void Init()
        {
            if (m_panel != null)
                return;

            m_panel = GUIManager.Instance.CreateWoodpanel(
                parent:    GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position:  new Vector2(0f, 0f),
                width:     PanelWidth,
                height:    PanelHeight,
                draggable: false
            );
            m_panel.transform.SetAsLastSibling();

            m_titleText = GUIManager.Instance.CreateText(
                text:                "",
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, TitleY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            18,
                color:               GUIManager.Instance.ValheimOrange,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_titleText.alignment = TextAnchor.MiddleCenter;

            m_descriptionText = GUIManager.Instance.CreateText(
                text:                "",
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, DescriptionY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            14,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              50f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_descriptionText.alignment = TextAnchor.MiddleCenter;

            m_valueInput = GuiFieldBuilder.CreateInputField(m_panel, new Vector2(0f, InputY), InputWidth);

            m_feedbackText = GUIManager.Instance.CreateText(
                text:                "",
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, FeedbackY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            GuiFieldBuilder.FieldFontSize,
                color:               GUIManager.Instance.ValheimYellow,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_feedbackText.alignment = TextAnchor.MiddleCenter;

            m_confirmButton     = CreateButton(ConfirmBtnX, GUIManager.Instance.ValheimOrange);
            m_confirmButtonText = m_confirmButton.GetComponentInChildren<Text>();

            m_cancelButton = CreateButton(CancelBtnX, GUIManager.Instance.ValheimBeige);
            m_cancelButton.GetComponentInChildren<Text>().text = "Cancel";

            m_panel.SetActive(false);
        }

        /// <summary>
        /// Displays the dialog prefilled with <paramref name="suggestedValue"/>. Cancel always
        /// closes the panel. Confirm invokes <paramref name="onConfirm"/> with the trimmed
        /// contents of the input field: a null return closes the panel, a non-null return is
        /// treated as a validation error and shown inline, leaving the panel open.
        /// </summary>
        public void Show(
            string title,
            string description,
            string suggestedValue,
            Func<string, string> onConfirm,
            string confirmText = "Confirm",
            string cancelText  = "Cancel",
            int maxLength = 0)
        {
            if (m_panel == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] InputDialog.Show called before Init().");
                return;
            }

            m_titleText.text         = title;
            m_descriptionText.text   = description;
            m_confirmButtonText.text = confirmText;
            m_cancelButton.GetComponentInChildren<Text>().text = cancelText;
            m_valueInput.characterLimit = maxLength; // 0 = unlimited, matches GuiFieldBuilder.CreateInputField's default
            m_valueInput.text        = suggestedValue;
            m_feedbackText.text      = "";

            m_confirmButton.onClick.RemoveAllListeners();
            m_confirmButton.onClick.AddListener(() =>
            {
                string value = m_valueInput.text.Trim();
                string error = onConfirm?.Invoke(value);

                if (error != null)
                {
                    m_feedbackText.text = error;
                    return;
                }

                Hide();
            });

            m_cancelButton.onClick.RemoveAllListeners();
            m_cancelButton.onClick.AddListener(Hide);

            m_panel.transform.SetAsLastSibling();
            m_panel.SetActive(true);

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }
        }

        public void Hide()
        {
            if (m_panel != null)
                m_panel.SetActive(false);

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }
        }

        public bool IsVisible => m_panel != null && m_panel.activeSelf;

        private Button CreateButton(float posX, Color textColor)
        {
            GameObject btnObj = GuiHelper.CreateButton(
                text:      "",
                parent:    m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position:  new Vector2(posX, BtnY),
                width:     BtnWidth,
                height:    BtnHeight
            );
            btnObj.SetActive(true);
            btnObj.GetComponentInChildren<Text>().color = textColor;
            return btnObj.GetComponent<Button>();
        }
    }
}
