using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal static class FieldUIBuilder
    {
        private const float LabelWidth    = 130f;
        private const float LabelFieldGap = 10f;
        private const float TooltipGap    = 12f;
        private const float TooltipWidth  = 240f;
        private const float TooltipHeight = 36f;
        public  const float FieldHeight   = 36f;

        /// <summary>Height of one row (label + field side by side).</summary>
        public static float EntryHeight => FieldHeight;

        /// <summary>
        /// Returns the total height consumed by a field row.
        /// Tooltip is rendered beside the field so no extra vertical space is needed.
        /// </summary>
        public static float GetEntryHeight(FieldInfo field)
        {
            if (field.FieldType == typeof(List<string>))   return 210f;
            if (field.FieldType == typeof(PositionOffsetData)) return FieldHeight;
            return FieldHeight;
        }

        public static bool Build(GameObject parent, object target, FieldInfo field, Vector2 rowPosition, float fieldWidth = 200f, List<string> listDropdownOptions = null)
        {
            Type   fieldType    = field.FieldType;
            object currentValue = field.GetValue(target);

            string labelText = field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name;
            string tooltip   = field.GetCustomAttribute<EditorTooltipAttribute>()?.Tooltip;

            // Special case: List<string>
            if (fieldType == typeof(List<string>))
            {
                return BuildListField(parent, field, currentValue as List<string>, rowPosition, fieldWidth, listDropdownOptions, labelText, tooltip);
            }

            // Special case: PositionOffsetData
            if (fieldType == typeof(PositionOffsetData))
            {
                return BuildPositionOffsetField(parent, currentValue as PositionOffsetData, rowPosition, fieldWidth, labelText, tooltip);
            }

            if (fieldType != typeof(string) && fieldType != typeof(int) && fieldType != typeof(float) && fieldType != typeof(bool))
                return false;

            // Label — left side of the row
            Text labelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            // Field — immediately to the right of the label
            float fieldX     = rowPosition.x + LabelWidth / 2f + LabelFieldGap + fieldWidth / 2f;
            Vector2 fieldPos = new Vector2(fieldX, rowPosition.y);

            if (fieldType == typeof(string))
            {
                var colorAttr    = field.GetCustomAttribute<ColorPickerAttribute>();
                var dropdownAttr = field.GetCustomAttribute<DropdownOptionsAttribute>();

                if (colorAttr != null)
                    BuildColorField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth);
                else if (dropdownAttr != null)
                    BuildStringDropdownField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth, dropdownAttr.Options);
                else
                    BuildStringField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth);
            }
            else if (fieldType == typeof(int))
                BuildIntField(parent, target, field, currentValue is int i ? i : 0, fieldPos, fieldWidth);
            else if (fieldType == typeof(float))
                BuildFloatField(parent, target, field, currentValue is float f ? f : 0f, fieldPos, fieldWidth);
            else if (fieldType == typeof(bool))
                BuildBoolField(parent, target, field, currentValue is bool b && b, fieldPos, fieldWidth);

            // Tooltip — to the right of the input field, same vertical position
            if (tooltip != null)
            {
                float tooltipX = fieldX + fieldWidth / 2f + TooltipGap + TooltipWidth / 2f;
                BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
            }

            return true;
        }

        private static void BuildTooltip(GameObject parent, string tooltip, Vector2 position)
        {
            Text t = GUIManager.Instance.CreateText(
                text: tooltip,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: new Color(0.78f, 0.78f, 0.78f, 1f),
                outline: false,
                outlineColor: Color.black,
                width: TooltipWidth,
                height: TooltipHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            t.fontStyle = FontStyle.Italic;
            t.alignment = TextAnchor.MiddleLeft;
            t.transform.SetAsLastSibling();
        }

        // =====================================================================
        // Field builders
        // =====================================================================

        private static void BuildStringField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue);
            input.onValueChanged.AddListener(val => field.SetValue(target, val));
        }

        private static void BuildIntField(GameObject parent, object target, FieldInfo field, int currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString());
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (int.TryParse(val, out int result))
                    field.SetValue(target, result);
            });
        }

        private static void BuildFloatField(GameObject parent, object target, FieldInfo field, float currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString("G"));
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (float.TryParse(val, out float result))
                    field.SetValue(target, result);
            });
        }

        private static void BuildBoolField(GameObject parent, object target, FieldInfo field, bool currentValue, Vector2 position, float width)
        {
            GameObject toggleObj = GUIManager.Instance.CreateToggle(
                parent: parent.transform,
                width: FieldHeight,
                height: FieldHeight
            );

            // Align to the left edge of the field slot rather than its center
            float leftAlignedX = position.x - width / 2f + FieldHeight / 2f + 5f;

            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin        = new Vector2(0.5f, 1f);
            toggleRt.anchorMax        = new Vector2(0.5f, 1f);
            toggleRt.pivot            = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(leftAlignedX, position.y - FieldHeight / 4f);

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = currentValue;
            toggle.onValueChanged.AddListener(val => field.SetValue(target, val));
        }

        private static bool BuildListField(GameObject parent, FieldInfo field, List<string> currentValue, Vector2 rowPosition, float fieldWidth, List<string> dropdownOptions, string labelText, string tooltip)
        {
            // Label — same as all other fields
            Text listLabelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            listLabelComp.alignment = TextAnchor.MiddleLeft;

            // Input/dropdown center — identical to all other field types
            float fieldX     = rowPosition.x + LabelWidth / 2f + LabelFieldGap + fieldWidth / 2f;
            Vector2 fieldPos = new Vector2(fieldX, rowPosition.y);

            ListEditor editor = dropdownOptions != null && dropdownOptions.Count > 0
                ? new ListEditor(currentValue ?? new List<string>(), dropdownOptions)
                : new ListEditor(currentValue ?? new List<string>());

            editor.Build(parent, fieldPos, fieldWidth);

            // Tooltip — to the right of the field, same as all other field types
            if (tooltip != null)
            {
                float tooltipX = fieldX + fieldWidth / 2f + TooltipGap + TooltipWidth / 2f;
                BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
            }

            return true;
        }

        private static bool BuildPositionOffsetField(GameObject parent, PositionOffsetData currentValue, Vector2 rowPosition, float fieldWidth, string labelText, string tooltip)
        {
            // Label
            Text labelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            // Three inputs — X, Y, Z — evenly split across fieldWidth
            float fieldStartX = rowPosition.x + LabelWidth / 2f + LabelFieldGap;
            float subWidth    = (fieldWidth - 10f) / 3f; // 10f = 2 gaps of 5f
            float subHeight   = FieldHeight;
            const float subGap = 5f;
            const float subLabelW = 14f;
            const float subInputW = 14f; // label inside the sub-area

            string[] labels = { "X", "Y", "Z" };
            float[]  values = { currentValue.x, currentValue.y, currentValue.z };

            for (int i = 0; i < 3; i++)
            {
                float centerX = fieldStartX + i * (subWidth + subGap) + subWidth / 2f;

                // Small axis label
                Text axisLabel = GUIManager.Instance.CreateText(
                    text: labels[i],
                    parent: parent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(centerX - subWidth / 2f + subLabelW / 2f, rowPosition.y),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 11,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: false,
                    outlineColor: Color.black,
                    width: subLabelW,
                    height: subHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                axisLabel.alignment = TextAnchor.MiddleCenter;

                float inputW  = subWidth - subLabelW - 2f;
                float inputCX = centerX - subWidth / 2f + subLabelW + inputW / 2f + 2f;

                int capturedIndex = i;
                InputField input = CreateInputField(
                    parent,
                    new Vector2(inputCX, rowPosition.y),
                    inputW,
                    values[i].ToString("G")
                );
                input.contentType = InputField.ContentType.DecimalNumber;
                input.onValueChanged.AddListener(val =>
                {
                    if (!float.TryParse(val, out float result)) return;
                    if (capturedIndex == 0) currentValue.x = result;
                    else if (capturedIndex == 1) currentValue.y = result;
                    else currentValue.z = result;
                });
            }

            if (tooltip != null)
            {
                float fieldEndX  = fieldStartX + fieldWidth;
                float tooltipX   = fieldEndX + TooltipGap + TooltipWidth / 2f;
                BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
            }

            return true;
        }

        public static List<string> GetAvailableWeatherNames()
        {
            try
            {
                if (EnvMan.instance == null)
                    return GetFallbackWeatherNames();

                var environments = EnvMan.instance.m_environments;
                if (environments == null || environments.Count == 0)
                    return GetFallbackWeatherNames();

                return environments
                    .Select(env => env.m_name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToList();
            }
            catch (Exception)
            {
                return GetFallbackWeatherNames();
            }
        }

        private static List<string> GetFallbackWeatherNames()
        {
            return new List<string>
            {
                "Clear", "Twilight_Clear", "Misty", "Darklands_dark", "Heath clear",
                "DeepForest Mist", "GDKing", "Rain", "LightRain", "ThunderStorm",
                "Eikthyr", "GoblinKing", "nofogts", "SwampRain", "Bonemass",
                "Snow", "Twilight_Snow", "Twilight_SnowStorm", "SnowStorm",
                "Moder", "Ashrain", "Crypt", "SunkenCrypt"
            };
        }

        // Mirrors ListEditor's private constants — used to compute layout alignment
        private const float ListEditorInputW  = 200f;
        private const float ListEditorBtnW    = 60f;
        private const float ListEditorGap     = 10f;
        // startPosition.x offset from label right edge to ListEditor's internal anchor point
        private const float ListEditorAnchorOffset = ListEditorInputW / 2f + ListEditorGap / 2f;

        internal static InputField CreateInputField(GameObject parent, Vector2 position, float width, string initialValue = "")
        {
            GameObject inputObj = GUIManager.Instance.CreateInputField(
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                contentType: InputField.ContentType.Standard,
                placeholderText: "",
                fontSize: 12,
                width: width,
                height: FieldHeight
            );

            InputField inputField = inputObj.GetComponent<InputField>();
            inputField.text = initialValue;
            return inputField;
        }

        private static void BuildStringDropdownField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width, List<string> options)
        {
            GameObject dropdownObj = GUIManager.Instance.CreateDropDown(
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                fontSize: 12,
                width: width,
                height: FieldHeight
            );

            Dropdown dropdown = dropdownObj.GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);

            int index = options.IndexOf(currentValue);
            dropdown.value = index >= 0 ? index : 0;
            dropdown.RefreshShownValue();

            // Write the selected string value back to the field
            dropdown.onValueChanged.AddListener(i => field.SetValue(target, options[i]));
        }

        private static void BuildColorField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width)
        {
            Color initialColor = ParseHexColor(currentValue);

            // Button — keeps its default Valheim style so borders are visible
            GameObject swatchBtn = GUIManager.Instance.CreateButton(
                text: "",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                width: width,
                height: FieldHeight
            );
            swatchBtn.SetActive(true);

            // Solid color overlay — slightly inset so button borders remain visible
            const float inset = 4f;
            GameObject swatchOverlay = new GameObject("ColorOverlay");
            swatchOverlay.transform.SetParent(swatchBtn.transform, false);

            RectTransform overlayRt = swatchOverlay.AddComponent<RectTransform>();
            overlayRt.anchorMin        = new Vector2(0f, 0f);
            overlayRt.anchorMax        = new Vector2(1f, 1f);
            overlayRt.offsetMin        = new Vector2(inset, inset);
            overlayRt.offsetMax        = new Vector2(-inset, -inset);

            Image overlayImage = swatchOverlay.AddComponent<Image>();
            overlayImage.color        = initialColor;
            overlayImage.raycastTarget = false; // clicks pass through to the button

            swatchBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Color currentColor = ParseHexColor(field.GetValue(target) as string ?? "");

                GUIManager.Instance.CreateColorPicker(
                    anchorMin:       new Vector2(0.5f, 0.5f),
                    anchorMax:       new Vector2(0.5f, 0.5f),
                    position:        Vector2.zero,
                    original:        currentColor,
                    message:         field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name,
                    onColorChanged:  (Color c) =>
                    {
                        field.SetValue(target, "#" + ColorUtility.ToHtmlStringRGB(c));
                        overlayImage.color = c;
                    },
                    onColorSelected: (Color c) =>
                    {
                        field.SetValue(target, "#" + ColorUtility.ToHtmlStringRGB(c));
                        overlayImage.color = c;
                    }
                );
            });
        }

        private static Color ParseHexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}