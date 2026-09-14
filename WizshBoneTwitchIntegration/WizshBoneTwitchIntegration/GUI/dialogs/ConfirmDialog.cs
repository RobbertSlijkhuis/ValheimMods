using Jotunn.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A reusable modal confirmation panel built on top of a Valheim wood panel.
    /// Shown via <see cref="Show"/> and dismissed automatically when either button is pressed.
    ///
    /// Copy-adapted from GUI_OLD/dialogs/ConfirmDialog.cs (new namespace, uses this folder's own
    /// <see cref="InputBlockGate"/>) rather than referenced directly, per the new UI's isolation
    /// constraint.
    /// </summary>
    internal class ConfirmDialog
    {
        private GameObject m_panel;
        private Text       m_titleText;
        private Text       m_descriptionText;
        private Button     m_confirmButton;
        private Button     m_cancelButton;
        private Text       m_confirmButtonText;
        private Text       m_cancelButtonText;
        private bool       m_blockingInput;

        private const float PanelWidth   = 420f;
        private const float PanelHeight  = 200f;
        private const float TitleY       = -40f;
        private const float DescriptionY = -90f;
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

            m_confirmButton     = CreateButton(ConfirmBtnX, GUIManager.Instance.ValheimOrange);
            m_confirmButtonText = m_confirmButton.GetComponentInChildren<Text>();

            m_cancelButton     = CreateButton(CancelBtnX, GUIManager.Instance.ValheimBeige);
            m_cancelButtonText = m_cancelButton.GetComponentInChildren<Text>();

            m_panel.SetActive(false);
        }

        /// <summary>
        /// Displays the dialog with the given content. Both callbacks automatically close the panel.
        /// </summary>
        /// <param name="title">Bold orange title line.</param>
        /// <param name="description">Body text describing the action.</param>
        /// <param name="onConfirm">Invoked when the user clicks the confirm button.</param>
        /// <param name="onCancel">Invoked when the user clicks the cancel button. May be <c>null</c>.</param>
        /// <param name="confirmText">Label for the confirm button. Defaults to "Confirm".</param>
        /// <param name="cancelText">Label for the cancel button. Defaults to "Cancel".</param>
        public void Show(
            string title,
            string description,
            Action onConfirm,
            Action onCancel      = null,
            string confirmText   = "Confirm",
            string cancelText    = "Cancel")
        {
            if (m_panel == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] ConfirmDialog.Show called before Init().");
                return;
            }

            m_titleText.text       = title;
            m_descriptionText.text = description;
            m_confirmButtonText.text = confirmText;
            m_cancelButtonText.text  = cancelText;

            m_confirmButton.onClick.RemoveAllListeners();
            m_confirmButton.onClick.AddListener(() =>
            {
                Hide();
                onConfirm?.Invoke();
            });

            m_cancelButton.onClick.RemoveAllListeners();
            m_cancelButton.onClick.AddListener(() =>
            {
                Hide();
                onCancel?.Invoke();
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
