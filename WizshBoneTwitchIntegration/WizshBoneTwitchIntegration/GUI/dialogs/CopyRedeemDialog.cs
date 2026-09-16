using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A modal panel prompting for a new redeem title and a target profile, backing the
    /// per-redeem "Copy" action. Like <see cref="InputDialog"/>, the dialog stays open and shows
    /// an inline error when <c>onConfirm</c> reports a validation failure (e.g. a name collision),
    /// instead of always closing.
    ///
    /// Copy-adapted from GUI_OLD/dialogs/CopyRedeemDialog.cs (new namespace, uses this folder's
    /// own <see cref="SearchableDropdown"/>/<see cref="GuiFieldBuilder"/>) rather than referenced
    /// directly, per the new UI's isolation-from-GUI_OLD constraint.
    /// </summary>
    internal class CopyRedeemDialog
    {
        private GameObject m_panel;
        private Text       m_titleText;
        private Text       m_descriptionText;
        private InputField m_nameInput;
        private SearchableDropdown m_profileDropdown;
        private Text       m_feedbackText;
        private Button     m_confirmButton;
        private Button     m_cancelButton;
        private bool       m_blockingInput;

        private const float PanelWidth   = 460f;
        private const float PanelHeight  = 320f;
        private const float TitleY       = -40f;
        private const float DescriptionY = -75f;
        private const float DropdownY    = -125f;
        private const float InputY       = -175f;
        private const float FeedbackY    = -215f;
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

            m_titleText = GUIManager.Instance.CreateText(
                text:                "Copy Redeem",
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
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_descriptionText.alignment = TextAnchor.MiddleCenter;

            m_profileDropdown = new SearchableDropdown();
            m_profileDropdown.Build(m_panel, new Vector2(0f, DropdownY), FieldWidth, GuiFieldBuilder.FieldHeight, new List<string>(), null);

            m_nameInput = GuiFieldBuilder.CreateInputField(m_panel, new Vector2(0f, InputY), FieldWidth);

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

            m_confirmButton = CreateButton(ConfirmBtnX, GUIManager.Instance.ValheimOrange);
            m_confirmButton.GetComponentInChildren<Text>().text = "Copy";

            m_cancelButton = CreateButton(CancelBtnX, GUIManager.Instance.ValheimBeige);
            m_cancelButton.GetComponentInChildren<Text>().text = "Cancel";

            m_panel.SetActive(false);
        }

        /// <summary>
        /// Displays the dialog for copying <paramref name="redeemTitle"/>, with the profile
        /// dropdown populated from <paramref name="profiles"/> (defaulting to
        /// <paramref name="defaultProfile"/>) and the name field prefilled with
        /// <paramref name="suggestedName"/>. <paramref name="onConfirm"/> is invoked with the
        /// selected profile and the trimmed name; a non-null return value is treated as a
        /// validation error and shown inline without closing the dialog, a null return closes it.
        /// </summary>
        public void Show(string redeemTitle, List<string> profiles, string defaultProfile, string suggestedName, Func<string, string, string> onConfirm)
        {
            if (m_panel == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] CopyRedeemDialog.Show called before Init().");
                return;
            }

            m_descriptionText.text = $"Copy '{redeemTitle}' to profile:";
            m_feedbackText.text    = "";
            m_nameInput.text       = suggestedName;

            m_profileDropdown.SetOptions(profiles, defaultProfile);

            m_confirmButton.onClick.RemoveAllListeners();
            m_confirmButton.onClick.AddListener(() =>
            {
                string targetProfile = m_profileDropdown.Value;
                string newTitle      = m_nameInput.text.Trim();

                string error = onConfirm?.Invoke(targetProfile, newTitle);
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
            GameObject btnObj = GUIManager.Instance.CreateButton(
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
