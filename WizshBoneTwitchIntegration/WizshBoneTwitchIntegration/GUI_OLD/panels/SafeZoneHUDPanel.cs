using Jotunn.Managers;
using UnityEngine;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Renders a persistent safe zone label slightly above center screen
    /// using GUIManager, mirroring the WizshBoneHUD pattern.
    /// Attached as a component to Game.instance via GameAwake_Postfix.
    /// </summary>
    internal class SafeZoneHUDPanel : MonoBehaviour
    {
        private GameObject m_text;

        private const string MessageText = "Streamer is in a safe zone!";

        public void Awake()
        {
            GUIManager.OnCustomGUIAvailable += OnGUIAvailable;
        }

        private void OnGUIAvailable()
        {
            GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;

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
            if (m_text != null)
                m_text.SetActive(true);
        }

        public void Hide()
        {
            if (m_text != null)
                m_text.SetActive(false);
        }

        private void OnDestroy()
        {
            GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;

            if (m_text != null)
                Destroy(m_text);
        }
    }
}