using System;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A reusable modal panel prompting for a viewer's name + colors - the editable
    /// <c>ViewerEntry</c> fields (<see cref="WizshBoneTwitchIntegration.Models.ViewerEntry"/>;
    /// <c>effects</c> isn't editable here, matching RedesignUI.dc.html's viewer-edit modal). Same
    /// panel lifecycle as <see cref="InputDialog"/> (<see cref="Init"/> once, <see cref="Show"/>,
    /// <see cref="Hide"/>, <see cref="IsVisible"/>, blocks input via <see cref="InputBlockGate"/>),
    /// with two side-by-side color swatches (<see cref="GuiFieldBuilder.CreateColorField"/>): the
    /// main color and the emission (glow) color.
    ///
    /// The emission color is "linked" to the main color while the two are equal: the emission swatch
    /// then follows the main swatch as it is picked, and a linked emission color is handed back as ""
    /// (<c>ViewerEntry.emissionColor</c>'s "same as color"), so a later change to the main color keeps
    /// the glow in step.
    /// </summary>
    internal class ViewerEditDialog
    {
        private GameObject m_panel;
        private Text       m_titleText;
        private Text       m_descriptionText;
        private InputField m_nameInput;
        private GameObject m_colorSwatch;
        private GameObject m_emissionSwatch;
        private Text       m_feedbackText;
        private Button     m_confirmButton;
        private Button     m_cancelButton;
        private Text       m_confirmButtonText;
        private bool       m_blockingInput;

        private string m_currentColor = "#ffffff";
        private string m_currentEmissionColor = "#ffffff";
        private bool   m_emissionLinked = true;

        private const float PanelWidth   = 460f;
        private const float PanelHeight  = 380f;
        private const float TitleY       = -40f;
        private const float DescriptionY = -80f;
        private const float NameLabelY   = -120f;
        private const float NameInputY   = -150f;
        private const float ColorLabelY  = -195f;
        private const float ColorSwatchY = -225f;
        private const float FeedbackY    = -270f;
        private const float FieldWidth   = 360f;
        private const float ColorGap     = 10f;
        private const float ColorWidth   = (FieldWidth - ColorGap) / 2f;
        private const float ColorX       = -(ColorWidth + ColorGap) / 2f;
        private const float EmissionX    =  (ColorWidth + ColorGap) / 2f;
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

            m_titleText       = CreateCenteredLabel(TitleY, 18, GUIManager.Instance.ValheimOrange, 30f);
            m_descriptionText = CreateCenteredLabel(DescriptionY, 14, GUIManager.Instance.ValheimBeige, 50f);

            CreateFieldLabel("Name", NameLabelY);
            m_nameInput = GuiFieldBuilder.CreateInputField(m_panel, new Vector2(0f, NameInputY), FieldWidth);

            CreateFieldLabel("Color", ColorLabelY, ColorX, ColorWidth);
            CreateFieldLabel("Emission color", ColorLabelY, EmissionX, ColorWidth);
            // The swatches are (re)built fresh via RebuildColorSwatch/RebuildEmissionSwatch, since
            // GuiFieldBuilder.CreateColorField bakes its initial color in at creation time and has
            // no public "set color" API afterwards.

            m_feedbackText = CreateCenteredLabel(FeedbackY, GuiFieldBuilder.FieldFontSize, GUIManager.Instance.ValheimYellow, 30f);

            m_confirmButton     = CreateButton(ConfirmBtnX, GUIManager.Instance.ValheimOrange);
            m_confirmButtonText = m_confirmButton.GetComponentInChildren<Text>();

            m_cancelButton = CreateButton(CancelBtnX, GUIManager.Instance.ValheimBeige);
            m_cancelButton.GetComponentInChildren<Text>().text = "Cancel";

            m_panel.SetActive(false);
        }

        /// <summary>
        /// Displays the dialog prefilled with <paramref name="suggestedName"/>/
        /// <paramref name="suggestedColor"/>/<paramref name="suggestedEmissionColor"/> (empty =
        /// same as the main color). Cancel always closes the panel. Confirm invokes
        /// <paramref name="onConfirm"/> with the trimmed name, the color hex and the emission color
        /// hex ("" when it matches the color): a null return closes the panel, a non-null return is
        /// treated as a validation error and shown inline, leaving the panel open.
        /// </summary>
        public void Show(
            string title,
            string description,
            string suggestedName,
            string suggestedColor,
            string suggestedEmissionColor,
            Func<string, string, string, string> onConfirm,
            string confirmText = "Confirm",
            string cancelText  = "Cancel")
        {
            if (m_panel == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] ViewerEditDialog.Show called before Init().");
                return;
            }

            m_titleText.text         = title;
            m_descriptionText.text   = description;
            m_confirmButtonText.text = confirmText;
            m_cancelButton.GetComponentInChildren<Text>().text = cancelText;
            m_nameInput.text         = suggestedName;
            m_feedbackText.text      = "";

            string color = string.IsNullOrEmpty(suggestedColor) ? "#ffffff" : suggestedColor;
            string emissionColor = string.IsNullOrEmpty(suggestedEmissionColor) ? color : suggestedEmissionColor;

            m_emissionLinked = IsSameColor(color, emissionColor);
            RebuildColorSwatch(color);
            RebuildEmissionSwatch(emissionColor);

            m_confirmButton.onClick.RemoveAllListeners();
            m_confirmButton.onClick.AddListener(() =>
            {
                GuiHelper.CloseOpenColorPicker();

                string name  = m_nameInput.text.Trim();
                string emission = m_emissionLinked || IsSameColor(m_currentEmissionColor, m_currentColor) ? "" : m_currentEmissionColor;
                string error = onConfirm?.Invoke(name, m_currentColor, emission);

                if (error != null)
                {
                    m_feedbackText.text = error;
                    return;
                }

                Hide();
            });

            m_cancelButton.onClick.RemoveAllListeners();
            m_cancelButton.onClick.AddListener(() =>
            {
                GuiHelper.CloseOpenColorPicker();
                Hide();
            });

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

        private void RebuildColorSwatch(string hexValue)
        {
            m_currentColor = hexValue;

            if (m_colorSwatch != null)
                GameObject.Destroy(m_colorSwatch);

            m_colorSwatch = GuiFieldBuilder.CreateColorField(
                m_panel, new Vector2(ColorX, ColorSwatchY), ColorWidth, m_currentColor,
                "Pick Viewer Color", OnColorPicked, applyLive: true);
        }

        private void RebuildEmissionSwatch(string hexValue)
        {
            m_currentEmissionColor = hexValue;

            if (m_emissionSwatch != null)
                GameObject.Destroy(m_emissionSwatch);

            m_emissionSwatch = GuiFieldBuilder.CreateColorField(
                m_panel, new Vector2(EmissionX, ColorSwatchY), ColorWidth, m_currentEmissionColor,
                "Pick Viewer Emission Color", OnEmissionColorPicked, applyLive: true);
        }

        private void OnColorPicked(string hex)
        {
            m_currentColor = hex;

            if (m_emissionLinked)
                RebuildEmissionSwatch(hex);
        }

        private void OnEmissionColorPicked(string hex)
        {
            m_currentEmissionColor = hex;
            m_emissionLinked = IsSameColor(hex, m_currentColor);
        }

        private static bool IsSameColor(string hexA, string hexB)
        {
            return string.Equals(hexA, hexB, StringComparison.OrdinalIgnoreCase);
        }

        private Text CreateCenteredLabel(float y, int fontSize, Color color, float height)
        {
            Text label = GUIManager.Instance.CreateText(
                text:                "",
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, y),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            fontSize,
                color:               color,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              height,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            return label;
        }

        private void CreateFieldLabel(string text, float y, float x = 0f, float width = FieldWidth)
        {
            Text label = GUIManager.Instance.CreateText(
                text:                text,
                parent:              m_panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(x, y),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            GuiFieldBuilder.FieldFontSize,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               width,
                height:              20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
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
