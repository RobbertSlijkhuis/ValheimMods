using Jotunn.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Modal panel showing the embedded news (title plus heading/body sections in a scroll view),
    /// shown via <see cref="Show"/>. Unlike <see cref="ConfirmDialog"/> it sits on a full-screen dim
    /// blocker, because it can open unprompted over the shell and nothing under it (sidebar, tabs,
    /// the shell's own Close button) should be clickable until it's dismissed. The single Close
    /// button hides it and then invokes the caller's callback (which records the news as seen).
    /// </summary>
    internal class NewsDialog
    {
        private GameObject m_root;
        private Text       m_titleText;
        private GameObject m_content;
        private bool       m_blockingInput;

        private NewsData   m_populatedFor;
        private Action     m_onClosed;

        private const float PanelWidth    = 720f;
        private const float PanelHeight   = 520f;
        private const float TitleY        = -40f;
        private const float ScrollY       = -70f;
        private const float ScrollWidth   = 660f;
        private const float ScrollHeight  = 360f;
        private const float BtnY          = 40f;
        private const float BtnWidth      = 160f;
        private const float BtnHeight     = 50f;

        private const float TextPadding    = 12f;
        private const float HeadingSize    = 16f;
        private const float BodySize       = 14f;
        private const float SectionGap     = 18f;
        private const float HeadingBodyGap = 6f;

        private static readonly float TextWidth = ScrollWidth - ScrollableList.ScrollbarWidth - 2f * TextPadding;

        public bool IsVisible => m_root != null && m_root.activeSelf;

        /// <summary>
        /// Creates the dialog once and hides it. Must be called after
        /// <see cref="GUIManager.OnCustomGUIAvailable"/>.
        /// </summary>
        public void Init()
        {
            if (m_root != null)
                return;

            m_root = GuiHelper.CreateRegion(
                GUIManager.CustomGUIFront, "NewsDialog",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                offsetMin: Vector2.zero, offsetMax: Vector2.zero);
            GuiHelper.AddBackground(m_root, new Color(0f, 0f, 0f, 0.55f)); // raycast target: blocks clicks underneath

            GameObject panel = GUIManager.Instance.CreateWoodpanel(
                parent:    m_root.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position:  new Vector2(0f, 0f),
                width:     PanelWidth,
                height:    PanelHeight,
                draggable: false
            );

            m_titleText = GUIManager.Instance.CreateText(
                text:                "",
                parent:              panel.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, TitleY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            20,
                color:               GUIManager.Instance.ValheimOrange,
                outline:             true,
                outlineColor:        Color.black,
                width:               PanelWidth - 40f,
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_titleText.alignment = TextAnchor.MiddleCenter;

            m_content = ScrollableList.CreateFixed(panel, "News", new Vector2(0f, ScrollY), ScrollWidth, ScrollHeight);

            GameObject closeBtn = GuiHelper.CreateButton(
                text:      "Close",
                parent:    panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position:  new Vector2(0f, BtnY),
                width:     BtnWidth,
                height:    BtnHeight
            );
            closeBtn.SetActive(true);
            closeBtn.GetComponentInChildren<Text>().color = GUIManager.Instance.ValheimOrange;
            closeBtn.GetComponent<Button>().onClick.AddListener(Close);

            m_root.SetActive(false);
        }

        /// <summary>
        /// Displays <paramref name="news"/>. <paramref name="onClosed"/> runs after the player
        /// presses Close (not when the dialog is merely shown) - may be <c>null</c>.
        /// </summary>
        public void Show(NewsData news, Action onClosed)
        {
            if (m_root == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] NewsDialog.Show called before Init().");
                return;
            }

            if (news == null)
                return;

            m_onClosed = onClosed;

            if (!ReferenceEquals(m_populatedFor, news))
                Populate(news);

            m_root.transform.SetAsLastSibling();
            m_root.SetActive(true);

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }
        }

        private void Close()
        {
            Hide();

            Action onClosed = m_onClosed;
            m_onClosed = null;
            onClosed?.Invoke();
        }

        private void Hide()
        {
            if (m_root != null)
                m_root.SetActive(false);

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }
        }

        private void Populate(NewsData news)
        {
            m_populatedFor = news;
            m_titleText.text = news.title ?? "";

            GuiHelper.ClearContainer(m_content);

            float y = -TextPadding;

            if (news.sections != null)
            {
                foreach (NewsSection section in news.sections)
                {
                    if (!string.IsNullOrEmpty(section.heading))
                        y -= AddText(section.heading, (int)HeadingSize, GUIManager.Instance.ValheimOrange, y) + HeadingBodyGap;

                    if (!string.IsNullOrEmpty(section.body))
                        y -= AddText(section.body.TrimEnd(), (int)BodySize, GUIManager.Instance.ValheimBeige, y);

                    y -= SectionGap;
                }
            }

            ScrollableList.SetContentHeight(m_content, -y + TextPadding - SectionGap);
        }

        /// <summary>
        /// Adds a top-pivoted, wrapping <see cref="Text"/> at <paramref name="topY"/> inside the
        /// scroll content, sized to its own wrapped height. Returns that height.
        /// </summary>
        private float AddText(string text, int fontSize, Color color, float topY)
        {
            Text label = GUIManager.Instance.CreateText(
                text:                text,
                parent:              m_content.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, topY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            fontSize,
                color:               color,
                outline:             true,
                outlineColor:        Color.black,
                width:               TextWidth,
                height:              20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.UpperLeft;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            float height = label.preferredHeight;
            label.rectTransform.sizeDelta = new Vector2(TextWidth, height);
            GuiHelper.PivotToTop(label.rectTransform, topY);

            return height;
        }
    }
}
