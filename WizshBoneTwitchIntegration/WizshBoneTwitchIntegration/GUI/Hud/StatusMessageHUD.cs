using System;
using System.Collections.Generic;
using System.Text;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A plain class rather than a MonoBehaviour - it's owned by <see cref="WizshBoneGUI"/> the
    /// same way that class already owns <see cref="WizshBoneHUD"/>/<see cref="ConfirmDialog"/> - inited on
    /// <c>GUIManager.OnCustomGUIAvailable</c>, no Unity lifecycle callbacks needed.
    ///
    /// Renders persistent status messages slightly above center screen using GUIManager, mirroring the
    /// WizshBoneHUD pattern. Anything can <see cref="Show"/> a message under its own id and <see cref="Hide"/> it
    /// again; the messages that are showing stack top to bottom, highest priority first (earlier first among equals),
    /// each in its own color. The known messages and their texts live in <see cref="StatusMessages"/>.
    /// </summary>
    internal class StatusMessageHUD
    {
        private class Message
        {
            public string Id;
            public string Text;
            public Color Color;
            public int Priority;
            public Func<bool> VisibleWhen;
        }

        // Kept sorted: highest priority first, and a message that was shown earlier before later ones of the same priority.
        private readonly List<Message> m_messages = new List<Message>();
        private readonly StringBuilder m_builder = new StringBuilder();
        private GameObject m_text;
        private Text m_label;
        private RectTransform m_rect;

        // SyncVisibility runs every frame; the text is only rebuilt when the list or what is visible has changed.
        private bool m_dirty;
        private int m_lastVisible = -1;

        private const float LineHeight = 50f;
        private const float LabelWidth = 900f;
        private const float FirstLineY = -300f;

        public void Init()
        {
            if (m_text != null)
                return;

            int fontSize = MessageHud.instance != null && MessageHud.instance.m_messageCenterText != null
                ? (int)MessageHud.instance.m_messageCenterText.fontSize
                : 24;

            m_text = GUIManager.Instance.CreateText(
                text: "",
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, FirstLineY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: LineHeight,
                addContentSizeFitter: false
            );

            m_label = m_text.GetComponent<Text>();
            m_rect = m_text.GetComponent<RectTransform>();

            // The first line stays where the single safe zone line always was (centred on FirstLineY); more lines
            // grow downwards, so the box hangs from its top edge.
            m_label.alignment = TextAnchor.UpperCenter;
            m_label.raycastTarget = false; // purely informational - never swallow clicks in the middle of the screen
            m_label.verticalOverflow = VerticalWrapMode.Overflow;
            m_rect.pivot = new Vector2(0.5f, 1f);
            m_rect.anchoredPosition = new Vector2(0f, FirstLineY + LineHeight / 2f);

            m_text.SetActive(false);
        }

        /// <summary>
        /// Shows the message <paramref name="id"/>, or updates it if it is already showing.
        /// <paramref name="visibleWhen"/> (optional) is checked every frame and hides the message while it returns false.
        /// </summary>
        public void Show(string id, string text, Color color, int priority = 0, Func<bool> visibleWhen = null)
        {
            Message message = m_messages.Find(m => m.Id == id);

            // An update with the same priority keeps its place in the list.
            bool inPlace = message != null && message.Priority == priority;

            if (message == null)
                message = new Message { Id = id };
            else if (!inPlace)
                m_messages.Remove(message);

            message.Priority = priority;
            message.Text = text;
            message.Color = color;
            message.VisibleWhen = visibleWhen;

            if (!inPlace)
            {
                // After every message of the same or a higher priority.
                int index = m_messages.FindIndex(m => m.Priority < priority);
                m_messages.Insert(index < 0 ? m_messages.Count : index, message);
            }

            m_dirty = true;
            SyncVisibility();
        }

        public void Hide(string id)
        {
            if (m_messages.RemoveAll(m => m.Id == id) == 0)
                return;

            m_dirty = true;
            SyncVisibility();
        }

        /// <summary>
        /// Applies what should be visible: each message's own condition, and nothing at all while vanilla's Ctrl+F3
        /// "hide UI" toggle is on (this label lives on Jotunn's own canvas, which vanilla's Hud never touches).
        /// Called every frame by <see cref="WizshBoneGUI"/> so toggling Ctrl+F3 takes effect
        /// immediately even while a message stays up.
        /// </summary>
        public void SyncVisibility()
        {
            if (m_text == null)
                return;

            bool userHidden = Hud.IsUserHidden();

            // One bit per message, in list order (a handful of messages at most).
            int visible = 0;

            for (int i = 0; i < m_messages.Count && !userHidden; i++)
            {
                Func<bool> condition = m_messages[i].VisibleWhen;

                if (condition == null || condition())
                    visible |= 1 << i;
            }

            if (!m_dirty && visible == m_lastVisible)
                return;

            m_dirty = false;
            m_lastVisible = visible;

            m_builder.Length = 0;
            int lines = 0;

            for (int i = 0; i < m_messages.Count; i++)
            {
                if ((visible & (1 << i)) == 0)
                    continue;

                Message message = m_messages[i];

                if (lines > 0)
                    m_builder.Append('\n');

                m_builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(message.Color)).Append('>')
                    .Append(message.Text).Append("</color>");
                lines++;
            }

            m_label.text = m_builder.ToString();

            // As high as the text really is: a long message wraps onto a second line, and a box sized by the
            // number of messages would cut that line off (or push the next message out of view).
            m_rect.sizeDelta = new Vector2(LabelWidth, Mathf.Max(LineHeight, m_label.preferredHeight));
            m_text.SetActive(lines > 0);
        }
    }
}
