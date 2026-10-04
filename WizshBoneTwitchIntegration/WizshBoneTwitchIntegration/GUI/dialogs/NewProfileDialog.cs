using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A modal panel prompting for a new profile's name and what it starts from: an empty profile
    /// (the default) or a copy of an existing profile's redeems and settings. Like
    /// <see cref="InputDialog"/>, the dialog stays open and shows an inline error when
    /// <c>onConfirm</c> reports a validation failure (e.g. a name collision), instead of always
    /// closing.
    /// </summary>
    internal class NewProfileDialog
    {
        private GameObject m_panel;
        private Text       m_titleText;
        private Text       m_descriptionText;
        private InputField m_nameInput;
        private Text       m_sourceLabelText;
        private SearchableDropdown m_sourceDropdown;
        private Text       m_feedbackText;
        private Button     m_confirmButton;
        private Button     m_cancelButton;
        private bool       m_blockingInput;

        /// <summary>The dropdown value meaning "no source profile" - never a valid profile name.</summary>
        private const string EmptyOptionValue = "";

        private const float PanelWidth   = 460f;
        private const float PanelHeight  = 340f;
        private const float TitleY       = -40f;
        private const float DescriptionY = -75f;
        private const float InputY       = -125f;
        private const float SourceLabelY = -165f;
        private const float DropdownY    = -195f;
        private const float FeedbackY    = -240f;
        private const float FieldWidth   = 360f;
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

            m_titleText = CreateLabel(TitleY, 18, GUIManager.Instance.ValheimOrange);
            m_descriptionText = CreateLabel(DescriptionY, 14, GUIManager.Instance.ValheimBeige);

            m_nameInput = GuiFieldBuilder.CreateInputField(m_panel, new Vector2(0f, InputY), FieldWidth);

            m_sourceLabelText = CreateLabel(SourceLabelY, 14, GUIManager.Instance.ValheimBeige);
            m_sourceLabelText.text = "Start from:";

            m_sourceDropdown = new SearchableDropdown();
            m_sourceDropdown.Build(m_panel, new Vector2(0f, DropdownY), FieldWidth, GuiFieldBuilder.FieldHeight, new List<DropdownOption>(), EmptyOptionValue);

            m_feedbackText = CreateLabel(FeedbackY, GuiFieldBuilder.FieldFontSize, GUIManager.Instance.ValheimYellow);

            m_confirmButton = CreateButton(ConfirmBtnX, GUIManager.Instance.ValheimOrange);
            m_cancelButton  = CreateButton(CancelBtnX, GUIManager.Instance.ValheimBeige);
            m_cancelButton.GetComponentInChildren<Text>().text = "Cancel";

            m_panel.SetActive(false);
        }

        /// <summary>
        /// Displays the dialog with an empty name field and the dropdown set to "Empty profile",
        /// followed by <paramref name="profiles"/>. <paramref name="onConfirm"/> is invoked with the
        /// trimmed name and the selected source profile (null for an empty profile); a non-null
        /// return value is treated as a validation error and shown inline without closing the
        /// dialog, a null return closes it.
        /// </summary>
        public void Show(
            string title,
            string description,
            List<string> profiles,
            Func<string, string, string> onConfirm,
            string confirmText = "Create",
            int maxLength = 0)
        {
            if (m_panel == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] NewProfileDialog.Show called before Init().");
                return;
            }

            m_titleText.text         = title;
            m_descriptionText.text   = description;
            m_nameInput.characterLimit = maxLength; // 0 = unlimited, matches GuiFieldBuilder.CreateInputField's default
            m_nameInput.text         = "";
            m_feedbackText.text      = "";
            m_confirmButton.GetComponentInChildren<Text>().text = confirmText;

            List<DropdownOption> options = new List<DropdownOption> { new DropdownOption(EmptyOptionValue, "Empty (no redeems)") };
            options.AddRange(profiles.Select(p => new DropdownOption(p, $"Copy of '{p}'")));
            m_sourceDropdown.SetOptions(options, EmptyOptionValue);

            m_confirmButton.onClick.RemoveAllListeners();
            m_confirmButton.onClick.AddListener(() =>
            {
                string name   = m_nameInput.text.Trim();
                string source = m_sourceDropdown.Value;

                string error = onConfirm?.Invoke(name, string.IsNullOrEmpty(source) ? null : source);
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

        private Text CreateLabel(float posY, int fontSize, Color color)
        {
            Text text = GUIManager.Instance.CreateText(
                text:                "",
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, posY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            fontSize,
                color:               color,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

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
