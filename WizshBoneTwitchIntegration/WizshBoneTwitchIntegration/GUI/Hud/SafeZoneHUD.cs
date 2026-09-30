using Jotunn.Managers;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Copy-adapted from GUI_OLD/panels/SafeZoneHUDPanel.cs (new namespace, moved out of GUI_OLD
    /// per the new UI's isolation constraint - see GUI/dialogs/ConfirmDialog.cs's identical
    /// rationale). A plain class rather than a MonoBehaviour - unlike the GUI_OLD original (which
    /// had to be its own component so harmony/LoginPatchesWBTI.cs could AddComponent it directly),
    /// it's now owned by <see cref="WizshBoneGUI"/> the same way that class already owns
    /// <see cref="WizshBoneHUD"/>/<see cref="ConfirmDialog"/> - inited on
    /// <c>GUIManager.OnCustomGUIAvailable</c>, no Unity lifecycle callbacks needed.
    ///
    /// Renders a persistent safe zone label slightly above center screen using GUIManager,
    /// mirroring the WizshBoneHUD pattern.
    /// </summary>
    internal class SafeZoneHUD
    {
        private GameObject m_text;
        private bool m_wantVisible;

        private const string MessageText = "Streamer is in a safe zone!";

        public void Init()
        {
            if (m_text != null)
                return;

            int fontSize = MessageHud.instance != null && MessageHud.instance.m_messageCenterText != null
                ? (int)MessageHud.instance.m_messageCenterText.fontSize
                : 24;

            m_text = GUIManager.Instance.CreateText(
                text: MessageText,
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -300f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 500f,
                height: 50f,
                addContentSizeFitter: false
            );

            m_text.SetActive(false);
        }

        public void Show()
        {
            m_wantVisible = true;
            SyncVisibility();
        }

        public void Hide()
        {
            m_wantVisible = false;
            SyncVisibility();
        }

        /// <summary>
        /// Applies the wanted visible state, additionally hidden while vanilla's Ctrl+F3 "hide UI"
        /// toggle is on (this label lives on Jotunn's own canvas, which vanilla's Hud never touches).
        /// Called every frame by <see cref="WizshBoneGUI"/> so toggling Ctrl+F3 takes effect
        /// immediately even while the player stays in the safe zone.
        /// </summary>
        public void SyncVisibility()
        {
            if (m_text != null)
                m_text.SetActive(m_wantVisible && !Hud.IsUserHidden());
        }
    }
}
