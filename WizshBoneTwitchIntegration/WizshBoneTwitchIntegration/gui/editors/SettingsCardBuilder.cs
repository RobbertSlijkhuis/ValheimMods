using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Renders a single Settings-tab setting as a self-contained card: bold title on top, the
    /// input control on its own row underneath, and the tooltip as smaller grey text below that -
    /// used only by <see cref="RulesTab"/>. Every other tab keeps rendering fields as horizontal
    /// label/field/tooltip rows via <see cref="ObjectEditor"/>/<see cref="FieldUIBuilder"/>.
    /// </summary>
    internal static class SettingsCardBuilder
    {
        private const float CardPaddingX = 20f;
        private const float CardPaddingTop = 14f;
        private const float CardPaddingBottom = 14f;

        private const float TitleHeight = 22f;
        private const float TitleControlGap = 8f;
        private const float ControlTooltipGap = 8f;

        private const int TitleFontSize = 15;
        private const float MinTooltipHeight = 18f;

        // Compact widths for controls that don't need to fill the card - matches the reference
        // design where a numeric/toggle input stays small while a free-form string field spans
        // the full card width.
        private const float ToggleControlWidth = FieldUIBuilder.FieldHeight;
        private const float CompactControlWidth = 160f;
        private const float DropdownControlWidth = 240f;

        private static readonly Color CardBackgroundColor = new Color(0.07f, 0.07f, 0.07f, 0.85f);
        private static readonly Color TooltipColor = new Color(0.78f, 0.78f, 0.78f, 1f);

        /// <summary>
        /// Builds one card at <paramref name="topCenterPosition"/> (top-center anchored, matching
        /// every other tab's layout convention), <paramref name="cardWidth"/> wide.
        /// </summary>
        /// <returns>Total height consumed by the card, for the caller to stack the next one below it.</returns>
        internal static float BuildCard(GameObject parent, float cardWidth, Vector2 topCenterPosition, string title, string tooltip, IBoundField boundField)
        {
            GameObject card = new GameObject("SettingsCard");
            card.transform.SetParent(parent.transform, false);

            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 1f);
            cardRt.anchorMax = new Vector2(0.5f, 1f);
            cardRt.pivot = new Vector2(0.5f, 1f);
            cardRt.anchoredPosition = topCenterPosition;

            Image background = card.AddComponent<Image>();
            background.color = CardBackgroundColor;

            float innerWidth = cardWidth - 2f * CardPaddingX;

            // Card-local coordinates: x = 0 is the card's own horizontal center, so every child
            // below is placed independent of the outer panel's pixel geometry.
            float y = -CardPaddingTop;

            Text titleText = GUIManager.Instance.CreateText(
                text: title,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, y),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: TitleFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: innerWidth,
                height: TitleHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            titleText.alignment = TextAnchor.MiddleLeft;

            y -= TitleHeight + TitleControlGap;

            float controlWidth = GetControlWidth(boundField.WrappedType, innerWidth);
            float controlX = -innerWidth / 2f + controlWidth / 2f;
            float controlHeight = FieldUIBuilder.FieldHeight;

            FieldUIBuilder.BuildBoundFieldControl(card, boundField, new Vector2(controlX, y), controlWidth);

            y -= controlHeight + (tooltip != null ? ControlTooltipGap : 0f);

            float tooltipHeight = 0f;
            if (tooltip != null)
            {
                Text tooltipText = GUIManager.Instance.CreateText(
                    text: tooltip,
                    parent: card.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, y),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.TooltipFontSize,
                    color: TooltipColor,
                    outline: false,
                    outlineColor: Color.black,
                    width: innerWidth,
                    height: MinTooltipHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                tooltipText.alignment = TextAnchor.MiddleLeft;
                tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
                tooltipText.verticalOverflow = VerticalWrapMode.Overflow;

                tooltipHeight = Mathf.Max(MinTooltipHeight, tooltipText.preferredHeight);
                tooltipText.rectTransform.sizeDelta = new Vector2(innerWidth, tooltipHeight);

                y -= tooltipHeight;
            }

            float totalHeight = CardPaddingTop + TitleHeight + TitleControlGap + controlHeight
                + (tooltip != null ? ControlTooltipGap + tooltipHeight : 0f) + CardPaddingBottom;

            cardRt.sizeDelta = new Vector2(cardWidth, totalHeight);

            return totalHeight;
        }

        private static float GetControlWidth(System.Type wrapped, float innerWidth)
        {
            if (wrapped == typeof(bool) || wrapped == typeof(bool?))
                return ToggleControlWidth;

            if (wrapped == typeof(int) || wrapped == typeof(int?) || wrapped == typeof(float) || wrapped == typeof(float?))
                return CompactControlWidth;

            if (wrapped.IsEnum)
                return DropdownControlWidth;

            return innerWidth;
        }
    }
}
