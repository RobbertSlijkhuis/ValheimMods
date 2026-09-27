using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// A vertical stack of selectable buttons with one highlighted entry at a time - extracted
    /// from HelpTab's topic list and RedeemWizard step 1's effect-type list, which duplicated this
    /// exact shape (button styling, highlight color, default-color capture). Builds only the
    /// buttons themselves onto whatever <see cref="Transform"/> the caller supplies - a plain
    /// <see cref="GuiHelper.CreateCard"/> card or a <see cref="ScrollableList.CreateFixed"/>
    /// content container - so callers keep full control of background and scrolling.
    /// </summary>
    internal class SelectorList
    {
        private readonly Dictionary<string, (GameObject Btn, Image Bg, Text Label)> m_buttons =
            new Dictionary<string, (GameObject, Image, Text)>();
        private Color m_defaultColor;

        public string SelectedKey { get; private set; }

        /// <summary>
        /// Builds one top-down stacked button per item, starting at local y = -itemHeight/2, x = 0
        /// (the parent's own horizontal center) - matching both original call sites' loop math.
        /// <paramref name="onClick"/> is a pure notification fired with the clicked item's key; it
        /// does not touch highlighting - callers call <see cref="Select"/> themselves so a click
        /// and an external re-sync funnel through the same code path. Returns the total stacked
        /// height (for callers driving a <see cref="ScrollableList"/> via
        /// <see cref="ScrollableList.SetContentHeight"/>).
        /// </summary>
        public float Build(Transform parent, IEnumerable<(string Key, string Label)> items,
            float itemWidth, float itemHeight, float gap, Action<string> onClick)
        {
            m_buttons.Clear();
            float y = -(itemHeight / 2f);

            foreach ((string key, string label) in items)
            {
                GameObject btnObj = GuiHelper.CreateButton(
                    text: label,
                    parent: parent,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, y),
                    width: itemWidth,
                    height: itemHeight
                );
                btnObj.SetActive(true);

                Image bg = btnObj.GetComponent<Image>();
                if (m_buttons.Count == 0)
                    m_defaultColor = bg.color;

                Text btnLabel = btnObj.GetComponentInChildren<Text>();
                btnLabel.color = GUIManager.Instance.ValheimOrange;
                btnLabel.alignment = TextAnchor.MiddleLeft;
                RectTransform labelRt = btnLabel.rectTransform;
                labelRt.offsetMin = new Vector2(labelRt.offsetMin.x + ListRow.LeftPadding, labelRt.offsetMin.y);

                string capturedKey = key;
                btnObj.GetComponent<Button>().onClick.AddListener(() => onClick(capturedKey));

                m_buttons[key] = (btnObj, bg, btnLabel);

                y -= itemHeight + gap;
            }

            return Mathf.Abs(y);
        }

        /// <summary>
        /// Highlights <paramref name="key"/>'s button (<see cref="ShellSidebar.TabActiveColor"/>)
        /// and resets every other button to its captured default color. Pass null (or an unknown
        /// key) to clear the highlight entirely.
        /// </summary>
        public void Select(string key)
        {
            SelectedKey = key;
            foreach (var kvp in m_buttons)
                kvp.Value.Bg.color = key != null && kvp.Key == key ? ShellSidebar.TabActiveColor : m_defaultColor;
        }
    }

    /// <summary>
    /// The dark-card title + description panel shown beside a <see cref="SelectorList"/> -
    /// extracted from RedeemWizard step 1's effect preview (the only one of the two that already
    /// had a card background) and HelpTab's topic preview (which had none). <paramref name="padding"/>
    /// (previously computed independently, and inconsistently, by each caller) is now the panel's
    /// single source of truth for both the horizontal text inset and the title's top inset/the
    /// gap above the description, so every user of this panel lines up the same way. Only the
    /// card's own position/size stay caller-supplied. Fallback text for an empty selection is the
    /// caller's job via <see cref="UpdateContent"/>.
    /// </summary>
    internal class SelectorPreviewPanel
    {
        private const float TitleHeight = 26f;

        private Text m_title;
        private Text m_description;

        /// <summary>
        /// Builds the card (drawn first, so later siblings draw on top of it) plus a top-pivoted
        /// title <see cref="Text"/> and a top-pivoted, wrap-and-grow-downward description
        /// <see cref="Text"/>, both centered on <paramref name="cardTopCenter"/>.x and inset from
        /// the card's edges by <paramref name="padding"/> on every side (title top, description
        /// gap under the title, and both sides' text width).
        /// </summary>
        public void Build(GameObject parent, Vector2 cardTopCenter, float cardWidth, float cardHeight, float padding)
        {
            GuiHelper.CreateCard(parent, cardTopCenter, cardWidth, cardHeight);

            float textWidth = cardWidth - 2f * padding;
            float titleTopY = cardTopCenter.y - padding;
            float descriptionTopY = titleTopY - TitleHeight - padding;

            m_title = GUIManager.Instance.CreateText(
                text: "",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(cardTopCenter.x, titleTopY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: textWidth,
                height: TitleHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_title.alignment = TextAnchor.MiddleLeft;
            GuiHelper.PivotToTop(m_title.rectTransform, titleTopY);

            m_description = GuiHelper.CreateTabDescription(
                "", parent, new Vector2(cardTopCenter.x, descriptionTopY), width: textWidth);
            GuiHelper.MakeDescriptionExpandDownward(m_description, descriptionTopY);
        }

        /// <summary>Sets the shown title/description - callers decide fallback strings themselves.</summary>
        public void UpdateContent(string title, string description)
        {
            m_title.text = title;
            m_description.text = description;
        }
    }
}
