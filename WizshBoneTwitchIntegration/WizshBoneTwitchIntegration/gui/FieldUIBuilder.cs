using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    internal static class FieldUIBuilder
    {
        private const float LabelWidth    = 130f;
        private const float LabelFieldGap = 10f;
        private const float TooltipGap    = 12f;
        private const float TooltipWidth  = 180f;
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
            return field.FieldType == typeof(List<string>) ? 210f : FieldHeight;
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
                bool listBuilt = BuildListField(parent, field, currentValue as List<string>, rowPosition, listDropdownOptions, labelText);
                if (listBuilt && tooltip != null)
                {
                    float tooltipX = rowPosition.x + (LabelWidth + 200f) / 2f + TooltipGap + TooltipWidth / 2f;
                    BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
                }
                return listBuilt;
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
                BuildStringField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth);
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

        private static bool BuildListField(GameObject parent, FieldInfo field, List<string> currentValue, Vector2 rowPosition, List<string> dropdownOptions, string labelText)
        {
            // Label — same width and position as all other field labels
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

            // Editor — starts immediately to the right of the label, same row
            float editorAnchorX = rowPosition.x + LabelWidth / 2f + LabelFieldGap + ListEditorAnchorOffset;
            Vector2 editorPosition = new Vector2(editorAnchorX, rowPosition.y);

            ListEditor editor = dropdownOptions != null && dropdownOptions.Count > 0
                ? new ListEditor(currentValue ?? new List<string>(), dropdownOptions)
                : new ListEditor(currentValue ?? new List<string>());

            editor.Build(parent, editorPosition);

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
    }
}