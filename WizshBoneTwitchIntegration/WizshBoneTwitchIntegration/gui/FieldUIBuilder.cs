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
        public  const float FieldHeight   = 36f;

        /// <summary>Height of one row (label + field side by side).</summary>
        public static float EntryHeight => FieldHeight;

        /// <summary>
        /// Builds a label + input field side by side for a single <paramref name="field"/> on <paramref name="target"/>.
        /// Supports <see cref="string"/>, <see cref="int"/>, <see cref="float"/>, <see cref="bool"/>, and <see cref="List{T}"/> of <see cref="string"/>.
        /// For List<string> fields, provide <paramref name="listDropdownOptions"/> to use dropdown mode instead of text input.
        /// Returns false for unsupported types.
        /// </summary>
        public static bool Build(GameObject parent, object target, FieldInfo field, Vector2 rowPosition, float fieldWidth = 200f, List<string> listDropdownOptions = null)
        {
            Type   fieldType     = field.FieldType;
            object currentValue  = field.GetValue(target);

            // Special case: List<string>
            if (fieldType == typeof(List<string>))
                return BuildListField(parent, field, currentValue as List<string>, rowPosition, listDropdownOptions);

            if (fieldType != typeof(string) && fieldType != typeof(int) && fieldType != typeof(float) && fieldType != typeof(bool))
                return false;

            // Label — left side of the row
            GUIManager.Instance.CreateText(
                text: field.Name,
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
            );

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

            return true;
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

            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin        = new Vector2(0.5f, 1f);
            toggleRt.anchorMax        = new Vector2(0.5f, 1f);
            toggleRt.pivot            = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = position;

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = currentValue;
            toggle.onValueChanged.AddListener(val => field.SetValue(target, val));
        }

        private static bool BuildListField(GameObject parent, FieldInfo field, List<string> currentValue, Vector2 rowPosition, List<string> dropdownOptions = null)
        {
            // Label above the list editor
            GUIManager.Instance.CreateText(
                text: field.Name,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth + 200f,
                height: FieldHeight,
                addContentSizeFitter: false
            );

            // List editor starts below the label
            Vector2 editorPosition = new Vector2(rowPosition.x, rowPosition.y - FieldHeight - 5f);
            
            // Create list editor with or without dropdown options
            ListEditor editor = dropdownOptions != null && dropdownOptions.Count > 0
                ? new ListEditor(currentValue ?? new List<string>(), dropdownOptions)
                : new ListEditor(currentValue ?? new List<string>());
            
            editor.Build(parent, editorPosition);

            return true;
        }

        /// <summary>
        /// Gets all available weather/environment names from EnvMan.
        /// </summary>
        public static List<string> GetAvailableWeatherNames()
        {
            try
            {
                if (EnvMan.instance == null)
                    return GetFallbackWeatherNames();

                // Get environments from EnvMan
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
                "Clear",
                "Twilight_Clear",
                "Misty",
                "Darklands_dark",
                "Heath clear",
                "DeepForest Mist",
                "GDKing",
                "Rain",
                "LightRain",
                "ThunderStorm",
                "Eikthyr",
                "GoblinKing",
                "nofogts",
                "SwampRain",
                "Bonemass",
                "Snow",
                "Twilight_Snow",
                "Twilight_SnowStorm",
                "SnowStorm",
                "Moder",
                "Ashrain",
                "Crypt",
                "SunkenCrypt"
            };
        }

        // =====================================================================
        // UI element factory
        // =====================================================================

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