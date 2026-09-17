using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Configs;

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
        /// fires for values that actually parse as an int.
        /// </summary>
        public static InputField CreateIntField(GameObject parent, Vector2 position, float width, int currentValue, Action<int> onChanged)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString());
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (int.TryParse(val, out int result))
                    onChanged?.Invoke(result);
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
            GameObject toggleObj = GUIManager.Instance.CreateToggle(
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

        public const float RectToggleWidth = 40f;
        public const float RectToggleHeight = 20f;
        private const float RectToggleBorderThickness = 1f;
        private const float RectTogglePadding = 2f;

        // RedesignUI.dc.html's boolean-graphic track/knob colors.
        public static readonly Color RectToggleEnabledColor = new Color(0.227f, 0.420f, 0.247f);  // #3a6b3f
        public static readonly Color RectToggleDisabledColor = new Color(0.227f, 0.125f, 0.125f); // #3a2020
        public static readonly Color RectToggleBorderColor = new Color(0f, 0f, 0f, 0.376f);        // #00000060
        public static readonly Color RectToggleKnobColor = new Color(0.910f, 0.878f, 0.816f);      // #e8e0d0

        /// <summary>
        /// Rectangle-track toggle matching RedesignUI.dc.html's boolean graphic (Home's Redeem
        /// status/Auto resolve/Enable on login/Chatting toggles in the mockup) - a green/dark-red
        /// track with a square knob that slides left/right, as an alternate to
        /// <see cref="CreateBoolField"/>'s stock Jötunn circle toggle. Built from plain
        /// <see cref="Image"/>s rather than Jötunn's toggle prefab, since that prefab's look is a
        /// fixed checkbox sprite swap and can't be recolored/reshaped into a track+knob.
        /// </summary>
        public static Toggle CreateRectBoolField(GameObject parent, Vector2 position, bool currentValue, Action<bool> onChanged, float width = RectToggleWidth, float height = RectToggleHeight)
        {
            // Every computed dimension below is rounded, not just the root position - a caller
            // passing a non-whole width/height (today's two call sites don't, but that's not
            // guaranteed forever) could otherwise leave the track/knob edges on a fractional
            // canvas unit.
            width = Mathf.Round(width);
            height = Mathf.Round(height);

            GameObject root = new GameObject("RectToggle", typeof(RectTransform));
            root.transform.SetParent(parent.transform, false);

            RectTransform rootRt = (RectTransform)root.transform;
            rootRt.anchorMin = new Vector2(0.5f, 1f);
            rootRt.anchorMax = new Vector2(0.5f, 1f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = new Vector2(width, height);
            // Rounded to whole pixels - see the same note in CreateInputField.
            rootRt.anchoredPosition = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));

            // The border color fills the whole root; the inset Track image on top leaves a
            // 1px rim visible, matching the mockup's `border:1px solid #00000060` track div.
            Image border = root.AddComponent<Image>();
            border.color = RectToggleBorderColor;

            GameObject trackObj = new GameObject("Track", typeof(RectTransform));
            trackObj.transform.SetParent(root.transform, false);
            RectTransform trackRt = (RectTransform)trackObj.transform;
            trackRt.anchorMin = Vector2.zero;
            trackRt.anchorMax = Vector2.one;
            trackRt.offsetMin = new Vector2(RectToggleBorderThickness, RectToggleBorderThickness);
            trackRt.offsetMax = new Vector2(-RectToggleBorderThickness, -RectToggleBorderThickness);
            Image track = trackObj.AddComponent<Image>();
            track.raycastTarget = false;

            float trackWidthLocal = Mathf.Round(width - RectToggleBorderThickness * 2f);
            float knobSize = Mathf.Round(height - RectToggleBorderThickness * 2f - RectTogglePadding * 2f);
            float knobLeftX = Mathf.Round(RectTogglePadding);
            float knobRightX = Mathf.Round(trackWidthLocal - RectTogglePadding - knobSize);

            GameObject knobObj = new GameObject("Knob", typeof(RectTransform));
            knobObj.transform.SetParent(trackObj.transform, false);
            RectTransform knobRt = (RectTransform)knobObj.transform;
            knobRt.anchorMin = new Vector2(0f, 0.5f);
            knobRt.anchorMax = new Vector2(0f, 0.5f);
            knobRt.pivot = new Vector2(0f, 0.5f);
            knobRt.sizeDelta = new Vector2(knobSize, knobSize);
            Image knob = knobObj.AddComponent<Image>();
            knob.color = RectToggleKnobColor;
            knob.raycastTarget = false;

            Toggle toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = border;
            toggle.transition = Selectable.Transition.None;
            toggle.isOn = currentValue;

            void ApplyVisual(bool on)
            {
                track.color = on ? RectToggleEnabledColor : RectToggleDisabledColor;
                knobRt.anchoredPosition = new Vector2(Mathf.Round(on ? knobRightX : knobLeftX), 0f);
            }

            ApplyVisual(currentValue);
            toggle.onValueChanged.AddListener(val =>
            {
                ApplyVisual(val);
                onChanged?.Invoke(val);
            });

            return toggle;
        }

        /// <summary>
        /// Single place that decides whether a boolean field renders as Jötunn's stock circle
        /// toggle (<see cref="CreateBoolField"/>) or the mockup's rectangle track+knob
        /// (<see cref="CreateRectBoolField"/>), based on <see cref="PluginConfig.configAlternativeToggles"/>.
        /// Every boolean-field call site should go through this instead of calling either widget
        /// directly, so flipping that one config value changes every toggle in the UI at once.
        ///
        /// Occupies the same <see cref="FieldHeight"/>-wide, left-aligned footprint within the
        /// given <paramref name="position"/>/<paramref name="width"/> slot that
        /// <see cref="CreateBoolField"/>'s circle does, so a caller's surrounding layout math
        /// (e.g. a status word positioned to the right of the toggle) doesn't need to know which
        /// style is active.
        /// </summary>
        public static Toggle CreateStyledBoolField(GameObject parent, Vector2 position, float width, bool currentValue, Action<bool> onChanged)
        {
            if (!PluginConfig.configAlternativeToggles.Value)
                return CreateBoolField(parent, position, width, currentValue, onChanged);

            float centerX = position.x - width / 2f + FieldHeight / 2f;
            return CreateRectBoolField(parent, new Vector2(centerX, position.y), currentValue, onChanged, width: FieldHeight);
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
            GameObject swatchBtn = GUIManager.Instance.CreateButton(
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
