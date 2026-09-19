using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The class that decides how individual input fields are rendered in the new UI - text,
    /// number, toggle, and color fields, each as a small value+callback factory method rather
    /// than reflecting over a bound object's <see cref="System.Reflection.FieldInfo"/> like
    /// <c>GUI_OLD/editors/FieldUIBuilder.cs</c>'s dispatcher does.
    ///
    /// This is a deliberately narrower port: <c>FieldUIBuilder</c> is two layers - a reflection/
    /// attribute dispatcher bound to RedeemData/CreatureData/etc. (out of scope until a round
    /// actually edits one of those data-model objects), and this generic per-type widget factory
    /// underneath it, which doesn't know about any data model at all. Only the second layer is
    /// ported here, copy-adapted per the new UI's isolation-from-GUI_OLD constraint, so every
    /// future text/number/color/dropdown field in the new UI renders through one place.
    /// </summary>
    internal static class GuiFieldBuilder
    {
        public const float FieldHeight = 36f;

        public const int FieldFontSize = 13;
        public const int PlaceholderFontSize = 14;

        // Jotunn's default placeholder color (Color.grey) is low-contrast against the input
        // field background.
        public static readonly Color PlaceholderColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        /// <summary>
        /// Creates a plain text <see cref="InputField"/> centered at <paramref name="position"/>.
        /// Wraps Jötunn's own <see cref="GUIManager.CreateInputField"/> - a genuine Jötunn API,
        /// not a GUI_OLD-only wrapper - with this mod's default styling applied.
        /// </summary>
        public static InputField CreateInputField(GameObject parent, Vector2 position, float width, string initialValue = "", string placeholderText = "", int maxLength = 0)
        {
            GameObject inputObj = GUIManager.Instance.CreateInputField(
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - callers often derive position/width from divisions
                // (e.g. a card width split in three) that don't come out even, and legacy uGUI
                // Text doesn't pixel-snap, so a fractional value here blurs the field's text.
                position: new Vector2(Mathf.Round(position.x), Mathf.Round(position.y)),
                contentType: InputField.ContentType.Standard,
                placeholderText: placeholderText,
                fontSize: FieldFontSize,
                width: Mathf.Round(width),
                height: FieldHeight
            );

            InputField inputField = inputObj.GetComponent<InputField>();
            inputField.textComponent.alignment = TextAnchor.MiddleLeft;
            StylePlaceholder(inputField);
            inputField.text = initialValue;
            if (maxLength > 0)
                inputField.characterLimit = maxLength;
            return inputField;
        }

        /// <summary>
        /// Jotunn leaves the placeholder Text at Unity's default alignment (upper-left) and a
        /// low-contrast grey, while the real input text is styled separately (see
        /// <see cref="CreateInputField"/>). This brings the placeholder in line: center-left
        /// aligned, larger, and lighter for readability.
        /// </summary>
        public static void StylePlaceholder(InputField field)
        {
            if (field.placeholder is Text placeholder)
            {
                placeholder.alignment = TextAnchor.MiddleLeft;
                placeholder.fontSize = PlaceholderFontSize;
                placeholder.color = PlaceholderColor;
            }
        }

        /// <summary>
        /// Creates an integer-only <see cref="InputField"/>; <paramref name="onChanged"/> only
        /// fires for values that actually parse as an int - or, with <paramref name="emptyAsZero"/>,
        /// also for an emptied box, reported as 0 (otherwise clearing the box leaves the previous
        /// value in place). <paramref name="maxLength"/> caps the typed length (0 = no cap).
        /// </summary>
        public static InputField CreateIntField(GameObject parent, Vector2 position, float width, int currentValue, Action<int> onChanged, bool emptyAsZero = false, int maxLength = 0)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString(), maxLength: maxLength);
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (int.TryParse(val, out int result))
                    onChanged?.Invoke(result);
                else if (emptyAsZero && string.IsNullOrEmpty(val))
                    onChanged?.Invoke(0);
            });
            return input;
        }

        /// <summary>
        /// Creates a decimal-only <see cref="InputField"/>; <paramref name="onChanged"/> only
        /// fires for values that actually parse as a float.
        /// </summary>
        public static InputField CreateFloatField(GameObject parent, Vector2 position, float width, float currentValue, Action<float> onChanged)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString("G"));
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (float.TryParse(val, out float result))
                    onChanged?.Invoke(result);
            });
            return input;
        }

        /// <summary>
        /// Creates a toggle, left-aligned within the given field slot rather than centered.
        ///
        /// Jotunn's CreateToggle (GUIManager.cs) builds Unity's stock toggle prefab and resizes its
        /// "Background" child (the visible circle) to (<see cref="FieldHeight"/>, FieldHeight), but
        /// leaves that child's anchoredPosition at Unity's stock (10, -10) - calibrated for the
        /// prefab's original 20x20 Background, not our size - untouched. So the *visible* circle's
        /// left edge actually sits at the toggle root's left edge + 10 - FieldHeight, not flush with
        /// the root RectTransform's own left edge like a naive center/half-width calc would assume.
        /// leftAlignedX below solves for the root position that puts the circle's true left edge at
        /// <c>position.x - width/2</c>, so passing the same position/width used for a title Text
        /// above it lines the two up exactly.
        /// </summary>
        public static Toggle CreateBoolField(GameObject parent, Vector2 position, float width, bool currentValue, Action<bool> onChanged)
        {
            GameObject toggleObj = GuiHelper.CreateToggle(
                parent: parent.transform,
                width: FieldHeight,
                height: FieldHeight
            );

            float leftAlignedX = position.x - width / 2f + FieldHeight - 10f;

            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.5f, 1f);
            toggleRt.anchorMax = new Vector2(0.5f, 1f);
            toggleRt.pivot = new Vector2(0.5f, 0.5f);
            // Rounded to whole pixels - see the same note in CreateInputField.
            toggleRt.anchoredPosition = new Vector2(Mathf.Round(leftAlignedX), Mathf.Round(position.y - FieldHeight / 4f));

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = currentValue;
            toggle.onValueChanged.AddListener(val => onChanged?.Invoke(val));
            return toggle;
        }

        /// <summary>
        /// Creates a color swatch button that opens Jötunn's own <see cref="GUIManager.CreateColorPicker"/>
        /// on click. <paramref name="currentHexValue"/>/the callback use the same "#RRGGBB" hex
        /// string format the redeem/creature data models already store colors as.
        /// </summary>
        public static GameObject CreateColorField(GameObject parent, Vector2 position, float width, string currentHexValue, string pickerTitle, Action<string> onChanged, float height = FieldHeight)
        {
            Color initialColor = ParseHexColor(currentHexValue);

            // Button - keeps its default Valheim style so borders are visible
            GameObject swatchBtn = GuiHelper.CreateButton(
                text: "",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                width: width,
                height: height
            );
            swatchBtn.SetActive(true);

            // Solid color overlay - slightly inset so button borders remain visible
            const float inset = 4f;
            GameObject swatchOverlay = new GameObject("ColorOverlay");
            swatchOverlay.transform.SetParent(swatchBtn.transform, false);

            RectTransform overlayRt = swatchOverlay.AddComponent<RectTransform>();
            overlayRt.anchorMin = new Vector2(0f, 0f);
            overlayRt.anchorMax = new Vector2(1f, 1f);
            overlayRt.offsetMin = new Vector2(inset, inset);
            overlayRt.offsetMax = new Vector2(-inset, -inset);

            Image overlayImage = swatchOverlay.AddComponent<Image>();
            overlayImage.color = initialColor;
            overlayImage.raycastTarget = false; // clicks pass through to the button

            swatchBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                GUIManager.Instance.CreateColorPicker(
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: Vector2.zero,
                    original: overlayImage.color,
                    message: pickerTitle,
                    onColorChanged: (Color c) =>
                    {
                        overlayImage.color = c;
                        onChanged?.Invoke("#" + ColorUtility.ToHtmlStringRGB(c));
                    },
                    onColorSelected: (Color c) =>
                    {
                        overlayImage.color = c;
                        onChanged?.Invoke("#" + ColorUtility.ToHtmlStringRGB(c));
                    }
                );
            });

            return swatchBtn;
        }

        private static Color ParseHexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}
