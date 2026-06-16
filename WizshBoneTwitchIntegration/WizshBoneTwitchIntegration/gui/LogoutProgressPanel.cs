using Jotunn.Managers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class LogoutProgressPanel
    {
        private GameObject    m_panel;
        private Text          m_messageText;
        private Text          m_dotsText;
        private MonoBehaviour m_runner;
        private Coroutine     m_dotCoroutine;

        private const float PanelWidth   = 340f;
        private const float PanelHeight  = 90f;
        private const float MessageY     = 12f;
        private const float DotsY        = -22f;

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

            m_messageText = GUIManager.Instance.CreateText(
                text:                 "",
                parent:               m_panel.transform,
                anchorMin:            new Vector2(0.5f, 0.5f),
                anchorMax:            new Vector2(0.5f, 0.5f),
                position:             new Vector2(0f, MessageY),
                font:                 GUIManager.Instance.AveriaSerifBold,
                fontSize:             14,
                color:                GUIManager.Instance.ValheimBeige,
                outline:              true,
                outlineColor:         Color.black,
                width:                PanelWidth - 30f,
                height:               50f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_messageText.alignment        = TextAnchor.MiddleCenter;
            m_messageText.horizontalOverflow = HorizontalWrapMode.Wrap;

            m_dotsText = GUIManager.Instance.CreateText(
                text:                 "",
                parent:               m_panel.transform,
                anchorMin:            new Vector2(0.5f, 0.5f),
                anchorMax:            new Vector2(0.5f, 0.5f),
                position:             new Vector2(0f, DotsY),
                font:                 GUIManager.Instance.AveriaSerifBold,
                fontSize:             14,
                color:                GUIManager.Instance.ValheimBeige,
                outline:              true,
                outlineColor:         Color.black,
                width:                PanelWidth - 30f,
                height:               20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_dotsText.alignment = TextAnchor.MiddleCenter;

            m_panel.SetActive(false);
        }

        public void Show(MonoBehaviour runner, string message)
        {
            if (m_panel == null)
                return;

            m_runner              = runner;
            m_messageText.text    = message;
            m_dotsText.text       = "";
            m_panel.transform.SetAsLastSibling();
            m_panel.SetActive(true);

            StopDotCoroutine();
            m_dotCoroutine = runner.StartCoroutine(AnimateDots());
        }

        public void UpdateMessage(string message)
        {
            if (m_panel == null || !m_panel.activeSelf)
                return;

            StopDotCoroutine();
            m_messageText.text = message;
            m_dotsText.text    = "";
        }

        public void Hide()
        {
            StopDotCoroutine();

            if (m_panel != null)
                m_panel.SetActive(false);
        }

        private void StopDotCoroutine()
        {
            if (m_runner != null && m_dotCoroutine != null)
            {
                m_runner.StopCoroutine(m_dotCoroutine);
                m_dotCoroutine = null;
            }
        }

        private IEnumerator AnimateDots()
        {
            int step = 0;
            while (true)
            {
                step = (step % 3) + 1;
                m_dotsText.text = new string('.', step);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
    }
}
