using System;
using Jotunn.GUI;
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

        /// <summary>Default bounds for number fields that don't pass their own min/max.</summary>
        public const int DefaultMin = -1000;
        public const int DefaultMax = 1000;

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
        ///
        /// <paramref name="onChanged"/> always receives a value clamped to
        /// [<paramref name="min"/>, <paramref name="max"/>]. The box text itself is only snapped
        /// to the clamped value once editing ends, so typing e.g. "30" into a field with a minimum
        /// of 5 isn't rewritten to "5" after the first digit.
        /// </summary>
        public static InputField CreateIntField(GameObject parent, Vector2 position, float width, int currentValue, Action<int> onChanged, bool emptyAsZero = false, int maxLength = 0, int min = DefaultMin, int max = DefaultMax)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString(), maxLength: maxLength);
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (int.TryParse(val, out int result))
                    onChanged?.Invoke(Mathf.Clamp(result, min, max));
                else if (emptyAsZero && string.IsNullOrEmpty(val))
                    onChanged?.Invoke(Mathf.Clamp(0, min, max));
            });
            input.onEndEdit.AddListener(val =>
            {
                int result;
                if (!int.TryParse(val, out result))
                {
                    if (!(emptyAsZero && string.IsNullOrEmpty(val)))
                        return;
                    result = 0;
                }

                string clamped = Mathf.Clamp(result, min, max).ToString();
                if (input.text != clamped)
                    input.text = clamped;
            });
            return input;
        }

        /// <summary>
        /// Creates a decimal-only <see cref="InputField"/>; <paramref name="onChanged"/> only
        /// fires for values that actually parse as a float, clamped to
        /// [<paramref name="min"/>, <paramref name="max"/>]. The box text is snapped to the
        /// clamped value once editing ends (see <see cref="CreateIntField"/>).
        /// </summary>
        public static InputField CreateFloatField(GameObject parent, Vector2 position, float width, float currentValue, Action<float> onChanged, float min = DefaultMin, float max = DefaultMax)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString("G"));
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (float.TryParse(val, out float result))
                    onChanged?.Invoke(Mathf.Clamp(result, min, max));
            });
            input.onEndEdit.AddListener(val =>
            {
                if (!float.TryParse(val, out float result))
                    return;

                string clamped = Mathf.Clamp(result, min, max).ToString("G");
                if (input.text != clamped)
                    input.text = clamped;
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
        /// When <paramref name="singleCloseButton"/> is true the picker's Done/Cancel pair is replaced
        /// by one "Close" button that runs the Done action, i.e. it always keeps the current color.
        /// <paramref name="applyLive"/> commits every change as it happens (the picker stays open, but
        /// nothing - including <see cref="GuiHelper.CloseOpenColorPicker"/> - can revert it) and
        /// implies <paramref name="singleCloseButton"/>, since there's nothing left to cancel.
        /// </summary>
        public static GameObject CreateColorField(GameObject parent, Vector2 position, float width, string currentHexValue, string pickerTitle, Action<string> onChanged, float height = FieldHeight, bool singleCloseButton = false, bool applyLive = false)
        {
            singleCloseButton |= applyLive;
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
                // Jötunn's Create() only closes (Done) an already-open picker and opens nothing, so
                // close it ourselves first - that fires its own close cleanup - and then open this
                // one. Cancels (reverts) it, except an applyLive picker, which keeps its changes.
                GuiHelper.CloseOpenColorPicker();

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
                        OnOwnPickerClosed();
                    }
                );

                // Every close (Done, Close, Cancel, CloseOpenColorPicker) runs onColorSelected, so
                // the setup below is always undone by OnOwnPickerClosed.
                s_ownPickerOpen = true;
                OpenPickerAppliesLive = applyLive;
                EnsureLiveHexInput();
                ApplyPickerButtonLayout(singleCloseButton);
            });

            return swatchBtn;
        }

        /// <summary>True while the picker currently open was opened with <c>applyLive</c>.</summary>
        public static bool OpenPickerAppliesLive { get; private set; }

        // The picker prefab is a reused singleton, so the first-seen Done/Cancel layout is kept and
        // restored on every open before (optionally) collapsing it to a single Close button.
        private static Button s_doneButton;
        private static Button s_cancelButton;
        private static Text s_doneLabel;
        private static string s_doneLabelText;
        private static Vector2 s_doneAnchoredPos;
        private static Vector2 s_doneSizeDelta;

        private static void ApplyPickerButtonLayout(bool singleCloseButton)
        {
            Transform pickerTransform = GUIManager.CustomGUIFront?.transform.Find("ColorPicker");
            if (pickerTransform == null)
                return;

            if (s_doneButton == null || s_cancelButton == null)
            {
                foreach (Button btn in pickerTransform.GetComponentsInChildren<Button>(true))
                {
                    string method = btn.onClick.GetPersistentEventCount() > 0 ? btn.onClick.GetPersistentMethodName(0) : "";
                    string label = btn.GetComponentInChildren<Text>(true)?.text ?? "";
                    string key = (method + "|" + btn.name + "|" + label).ToLowerInvariant();
                    if (key.Contains("done"))
                        s_doneButton = btn;
                    else if (key.Contains("cancel"))
                        s_cancelButton = btn;
                }

                if (s_doneButton == null || s_cancelButton == null)
                {
                    Jotunn.Logger.LogWarning("[WBTI] ColorPicker Done/Cancel buttons not found, can't customize them");
                    s_doneButton = null;
                    s_cancelButton = null;
                    return;
                }

                RectTransform doneRt = (RectTransform)s_doneButton.transform;
                s_doneLabel = s_doneButton.GetComponentInChildren<Text>(true);
                s_doneLabelText = s_doneLabel != null ? s_doneLabel.text : null;
                s_doneAnchoredPos = doneRt.anchoredPosition;
                s_doneSizeDelta = doneRt.sizeDelta;
            }

            RectTransform done = (RectTransform)s_doneButton.transform;
            RectTransform cancel = (RectTransform)s_cancelButton.transform;

            // Restore defaults first (a previous open may have collapsed it)
            done.anchoredPosition = s_doneAnchoredPos;
            done.sizeDelta = s_doneSizeDelta;
            if (s_doneLabel != null)
                s_doneLabel.text = s_doneLabelText;
            s_cancelButton.gameObject.SetActive(true);

            if (!singleCloseButton)
                return;

            // Span the area both buttons covered. sizeDelta is only a real width for point anchors,
            // so skip the stretch (Close just keeps Done's size) for anything else.
            if (done.parent == cancel.parent
                && done.anchorMin == done.anchorMax && cancel.anchorMin == cancel.anchorMax
                && done.anchorMin == cancel.anchorMin)
            {
                float left = Mathf.Min(done.anchoredPosition.x - done.sizeDelta.x * done.pivot.x, cancel.anchoredPosition.x - cancel.sizeDelta.x * cancel.pivot.x);
                float right = Mathf.Max(done.anchoredPosition.x + done.sizeDelta.x * (1f - done.pivot.x), cancel.anchoredPosition.x + cancel.sizeDelta.x * (1f - cancel.pivot.x));
                done.sizeDelta = new Vector2(right - left, done.sizeDelta.y);
                done.anchoredPosition = new Vector2(left + done.sizeDelta.x * done.pivot.x, done.anchoredPosition.y);
            }

            if (s_doneLabel != null)
                s_doneLabel.text = "Close";
            s_cancelButton.gameObject.SetActive(false);
        }

        private static InputField s_hookedHexInput;
        private static int s_hexOriginalCharLimit;
        // True only while a picker opened through CreateColorField is up, so the shared picker
        // behaves exactly like Jötunn's for any other mod that opens it.
        private static bool s_ownPickerOpen;

        /// <summary>
        /// Jötunn's picker only applies its hex box on end-edit (Enter), so typing a hex and then
        /// clicking Done silently kept the old color. Hooks the (singleton, reused) picker's hex
        /// input once so a complete "RRGGBB" applies as soon as it's typed or pasted - but only
        /// while <see cref="s_ownPickerOpen"/> (the picker is never opened with alpha here).
        /// </summary>
        private static void EnsureLiveHexInput()
        {
            Transform pickerTransform = GUIManager.CustomGUIFront?.transform.Find("ColorPicker");
            ColorPicker picker = pickerTransform != null ? pickerTransform.GetComponent<ColorPicker>() : null;
            InputField hex = picker != null ? picker.hexaComponent : null;
            if (hex == null)
                return;

            if (hex != s_hookedHexInput)
            {
                s_hookedHexInput = hex;
                hex.onValueChanged.AddListener(val =>
                {
                    if (!s_ownPickerOpen || val == null)
                        return;

                    string cleaned = val.Trim().TrimStart('#');
                    if (cleaned != val)
                    {
                        // Re-fires this listener with the cleaned text.
                        hex.text = cleaned;
                        return;
                    }

                    // Only a complete, valid value - SetHexa resets the box to the old color on junk
                    if (cleaned.Length == 6 && IsHexDigits(cleaned))
                        picker.SetHexa(cleaned);
                });
            }

            // Leave room for a pasted leading '#', otherwise a 6-digit limit would truncate the
            // last digit before the listener above gets a chance to strip the '#'.
            s_hexOriginalCharLimit = hex.characterLimit;
            if (hex.characterLimit > 0 && hex.characterLimit < 7)
                hex.characterLimit = 7;
        }

        private static bool IsHexDigits(string s)
        {
            foreach (char c in s)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Undoes everything CreateColorField customized on the shared picker (button layout, hex
        /// character limit, live flags). Runs from the picker's own close callback.
        /// </summary>
        private static void OnOwnPickerClosed()
        {
            s_ownPickerOpen = false;
            OpenPickerAppliesLive = false;

            if (s_hookedHexInput != null)
                s_hookedHexInput.characterLimit = s_hexOriginalCharLimit;

            if (s_doneButton != null && s_cancelButton != null)
                ApplyPickerButtonLayout(false);
        }

        private static Color ParseHexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}
