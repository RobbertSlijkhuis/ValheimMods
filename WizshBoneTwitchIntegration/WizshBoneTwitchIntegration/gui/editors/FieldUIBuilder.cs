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
        internal const float LabelWidth = 210f;
        internal const float LabelFieldGap = 10f;
        internal const float TooltipGap = 12f;
        internal const float TooltipWidth = 340f;
        private const float TooltipHeight = 36f;
        public const float FieldHeight = 36f;
        public const float InputHeight = 32f;

        public const int LabelFontSize = 13;
        public const int FieldFontSize = 12;
        public const int PlaceholderFontSize = 14;
        private const int SubLabelFontSize = 11;
        internal const int TooltipFontSize = 12;

        // Jotunn's default placeholder color (Color.grey) is low-contrast against the input field background.
        public static readonly Color PlaceholderColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        /// <summary>Height of one row (label + field side by side).</summary>
        public static float EntryHeight => FieldHeight;

        /// <summary>
        /// Returns the total height consumed by a field row.
        /// Tooltip is rendered beside the field so no extra vertical space is needed.
        /// </summary>
        public static float GetEntryHeight(FieldInfo field)
        {
            if (field.FieldType == typeof(List<string>)) return 210f;
            if (field.FieldType == typeof(PositionOffsetData)) return FieldHeight;
            return FieldHeight;
        }

        public static bool Build(GameObject parent, object target, FieldInfo field, Vector2 rowPosition, float fieldWidth = 200f, List<string> listDropdownOptions = null, Action onRebuild = null)
        {
            Type fieldType = field.FieldType;
            object currentValue = field.GetValue(target);

            string labelText = field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name;
            string tooltip = field.GetCustomAttribute<EditorTooltipAttribute>()?.Tooltip;

            // A view class field that proxies reads/writes to a real field elsewhere -
            // render using the wrapped type's primitive builder, writing through the
            // bound field instead of field.SetValue(target, ...).
            if (currentValue is IBoundField boundField)
            {
                List<DropdownOption> boundDropdownOptions = null;
                if (field.GetCustomAttribute<CreaturePrefabNameDropdownAttribute>() != null)
                    boundDropdownOptions = EnsureIncludesCurrentValue(GetAvailableCreaturePrefabOptions(), boundField.GetValue() as string ?? "");
                else if (field.GetCustomAttribute<DoorPrefabNameDropdownAttribute>() != null)
                    boundDropdownOptions = EnsureIncludesCurrentValue(GetAvailableDoorPrefabOptions(), boundField.GetValue() as string ?? "");

                return BuildBoundField(parent, boundField, rowPosition, fieldWidth, labelText, tooltip, boundDropdownOptions);
            }

            // Special case: List<string>
            if (fieldType == typeof(List<string>))
            {
                return BuildListField(parent, target, field, currentValue as List<string>, rowPosition, fieldWidth, listDropdownOptions, labelText, tooltip);
            }

            // Special case: PositionOffsetData
            if (fieldType == typeof(PositionOffsetData))
            {
                return BuildPositionOffsetField(parent, currentValue as PositionOffsetData, rowPosition, fieldWidth, labelText, tooltip);
            }

            bool isString = fieldType == typeof(string);
            bool isInt = fieldType == typeof(int);
            bool isFloat = fieldType == typeof(float);
            bool isBool = fieldType == typeof(bool);
            bool isNullableInt = fieldType == typeof(int?);
            bool isNullableFloat = fieldType == typeof(float?);
            bool isNullableBool = fieldType == typeof(bool?);

            if (!isString && !isInt && !isFloat && !isBool && !isNullableInt && !isNullableFloat && !isNullableBool)
                return false;

            // Label - left side of the row
            Text labelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            // Field - immediately to the right of the label
            float fieldX = rowPosition.x + LabelWidth / 2f + LabelFieldGap + fieldWidth / 2f;
            Vector2 fieldPos = new Vector2(fieldX, rowPosition.y);

            if (isString)
            {
                var colorAttr    = field.GetCustomAttribute<ColorPickerAttribute>();
                var dropdownAttr = field.GetCustomAttribute<DropdownOptionsAttribute>();
                var seDropdown   = field.GetCustomAttribute<StatusEffectNameDropdownAttribute>();
                var creatureDropdown = field.GetCustomAttribute<CreaturePrefabNameDropdownAttribute>();
                var doorDropdown = field.GetCustomAttribute<DoorPrefabNameDropdownAttribute>();
                var onChanged    = field.GetCustomAttribute<OnValueChangedAttribute>();
                var prefixAttr   = field.GetCustomAttribute<InputPrefixAttribute>();

                if (colorAttr != null)
                {
                    BuildColorField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth);
                }
                else if (seDropdown != null)
                {
                    string current = currentValue as string ?? "";
                    List<DropdownOption> options = EnsureIncludesCurrentValue(GetAvailableStatusEffectOptions(), current);
                    SearchableDropdown dd = BuildStringDropdownField(parent, target, field, current, fieldPos, fieldWidth, options);
                    if (onChanged != null)
                        dd.OnValueChanged += _ =>
                        {
                            InvokeOnValueChanged(parent, target, onChanged.MethodName);
                            onRebuild?.Invoke();
                        };
                }
                else if (creatureDropdown != null)
                {
                    string current = currentValue as string ?? "";
                    List<DropdownOption> options = EnsureIncludesCurrentValue(GetAvailableCreaturePrefabOptions(), current);
                    BuildStringDropdownField(parent, target, field, current, fieldPos, fieldWidth, options);
                }
                else if (doorDropdown != null)
                {
                    string current = currentValue as string ?? "";
                    List<DropdownOption> options = EnsureIncludesCurrentValue(GetAvailableDoorPrefabOptions(), current);
                    BuildStringDropdownField(parent, target, field, current, fieldPos, fieldWidth, options);
                }
                else if (dropdownAttr != null)
                {
                    SearchableDropdown dd = BuildStringDropdownField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth, dropdownAttr.Options);
                    if (onChanged != null)
                        dd.OnValueChanged += _ => InvokeOnValueChanged(parent, target, onChanged.MethodName);
                }
                else if (prefixAttr != null)
                {
                    InputField inp = BuildStringFieldWithPrefix(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth, prefixAttr.Prefix);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else
                {
                    InputField inp = BuildStringField(parent, target, field, currentValue as string ?? "", fieldPos, fieldWidth);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
            }
            else
            {
                var onChanged = field.GetCustomAttribute<OnValueChangedAttribute>();

                if (isInt)
                {
                    InputField inp = BuildIntField(parent, target, field, currentValue is int i ? i : 0, fieldPos, fieldWidth);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else if (isFloat)
                {
                    InputField inp = BuildFloatField(parent, target, field, currentValue is float f ? f : 0f, fieldPos, fieldWidth);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else if (isBool)
                {
                    Toggle tog = BuildBoolField(parent, target, field, currentValue is bool b && b, fieldPos, fieldWidth);
                    if (onChanged != null)
                        tog.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else if (isNullableInt)
                {
                    InputField inp = BuildNullableIntField(parent, target, field, currentValue as int?, fieldPos, fieldWidth);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else if (isNullableFloat)
                {
                    InputField inp = BuildNullableFloatField(parent, target, field, currentValue as float?, fieldPos, fieldWidth);
                    if (onChanged != null)
                        inp.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
                else if (isNullableBool)
                {
                    Toggle tog = BuildBoolField(parent, target, field, currentValue is bool nb && nb, fieldPos, fieldWidth);
                    if (onChanged != null)
                        tog.onValueChanged.AddListener(_ => InvokeOnValueChanged(parent, target, onChanged.MethodName));
                }
            }

            // Tooltip - to the right of the input field, same vertical position
            if (tooltip != null)
            {
                float tooltipX = fieldX + fieldWidth / 2f + TooltipGap + TooltipWidth / 2f;
                BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
            }

            return true;
        }

        /// <summary>
        /// Renders a label + input row for an <see cref="IBoundField"/>, dispatching by its
        /// wrapped type to the same primitive input styles as a direct field, but reading/
        /// writing through the bound field's delegates instead of a target object's field.
        /// </summary>
        private static bool BuildBoundField(GameObject parent, IBoundField boundField, Vector2 rowPosition, float fieldWidth, string labelText, string tooltip, List<DropdownOption> dropdownOptions = null)
        {
            Text labelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            float fieldX = rowPosition.x + LabelWidth / 2f + LabelFieldGap + fieldWidth / 2f;
            Vector2 fieldPos = new Vector2(fieldX, rowPosition.y);

            bool built = BuildBoundFieldControl(parent, boundField, fieldPos, fieldWidth, dropdownOptions);
            if (!built)
                return false;

            if (tooltip != null)
            {
                float tooltipX = fieldX + fieldWidth / 2f + TooltipGap + TooltipWidth / 2f;
                BuildTooltip(parent, tooltip, new Vector2(tooltipX, rowPosition.y));
            }

            return true;
        }

        /// <summary>
        /// Builds just the input control for an <see cref="IBoundField"/> (no label, no tooltip),
        /// centered at <paramref name="fieldPos"/> with the given width, wired to read/write
        /// through the bound field's delegates. Extracted out of <see cref="BuildBoundField"/> so
        /// callers with their own label/tooltip placement can reuse the exact same per-type
        /// dispatch and write-back logic.
        /// </summary>
        internal static bool BuildBoundFieldControl(GameObject parent, IBoundField boundField, Vector2 fieldPos, float fieldWidth, List<DropdownOption> dropdownOptions = null)
        {
            Type wrapped = boundField.WrappedType;

            bool isString = wrapped == typeof(string);
            bool isInt = wrapped == typeof(int);
            bool isFloat = wrapped == typeof(float);
            bool isBool = wrapped == typeof(bool);
            bool isNullableInt = wrapped == typeof(int?);
            bool isNullableFloat = wrapped == typeof(float?);
            bool isNullableBool = wrapped == typeof(bool?);
            bool isEnum = wrapped.IsEnum;

            if (!isString && !isInt && !isFloat && !isBool && !isNullableInt && !isNullableFloat && !isNullableBool && !isEnum)
                return false;

            object currentValue = boundField.GetValue();

            if (isString)
            {
                string current = currentValue as string ?? "";
                if (dropdownOptions != null)
                {
                    SearchableDropdown dd = new SearchableDropdown();
                    dd.Build(parent, fieldPos, fieldWidth, FieldHeight, dropdownOptions, current);
                    dd.OnValueChanged += val => boundField.SetValue(val);
                }
                else
                {
                    InputField inp = CreateInputField(parent, fieldPos, fieldWidth, current);
                    inp.onValueChanged.AddListener(val => boundField.SetValue(val));
                }
            }
            else if (isInt)
            {
                int value = currentValue is int i ? i : 0;
                InputField inp = CreateInputField(parent, fieldPos, fieldWidth, value.ToString());
                inp.contentType = InputField.ContentType.IntegerNumber;
                inp.onValueChanged.AddListener(val =>
                {
                    if (int.TryParse(val, out int result))
                        boundField.SetValue(result);
                });
            }
            else if (isFloat)
            {
                float value = currentValue is float f ? f : 0f;
                InputField inp = CreateInputField(parent, fieldPos, fieldWidth, value.ToString("G"));
                inp.contentType = InputField.ContentType.DecimalNumber;
                inp.onValueChanged.AddListener(val =>
                {
                    if (float.TryParse(val, out float result))
                        boundField.SetValue(result);
                });
            }
            else if (isBool)
            {
                bool value = currentValue is bool b && b;
                Toggle tog = BuildBoundBoolField(parent, value, fieldPos, fieldWidth);
                tog.onValueChanged.AddListener(val => boundField.SetValue(val));
            }
            else if (isNullableInt)
            {
                int? value = currentValue as int?;
                string initial = value.HasValue ? value.Value.ToString() : "";
                InputField inp = CreateInputField(parent, fieldPos, fieldWidth, initial);
                inp.contentType = InputField.ContentType.IntegerNumber;
                inp.onValueChanged.AddListener(val =>
                {
                    if (string.IsNullOrWhiteSpace(val))
                        boundField.SetValue((int?)null);
                    else if (int.TryParse(val, out int result))
                        boundField.SetValue((int?)result);
                });
            }
            else if (isNullableFloat)
            {
                float? value = currentValue as float?;
                string initial = value.HasValue ? value.Value.ToString("G") : "";
                InputField inp = CreateInputField(parent, fieldPos, fieldWidth, initial);
                inp.contentType = InputField.ContentType.DecimalNumber;
                inp.onValueChanged.AddListener(val =>
                {
                    if (string.IsNullOrWhiteSpace(val))
                        boundField.SetValue((float?)null);
                    else if (float.TryParse(val, out float result))
                        boundField.SetValue((float?)result);
                });
            }
            else if (isNullableBool)
            {
                bool value = currentValue is bool nb && nb;
                Toggle tog = BuildBoundBoolField(parent, value, fieldPos, fieldWidth);
                tog.onValueChanged.AddListener(val => boundField.SetValue((bool?)val));
            }
            else if (isEnum)
            {
                string current = currentValue?.ToString() ?? Enum.GetNames(wrapped)[0];
                List<string> names = new List<string>(Enum.GetNames(wrapped));
                SearchableDropdown dd = new SearchableDropdown();
                dd.Build(parent, fieldPos, fieldWidth, FieldHeight, names, current);
                dd.OnValueChanged += val => boundField.SetValue(Enum.Parse(wrapped, val));
            }

            return true;
        }

        private static Toggle BuildBoundBoolField(GameObject parent, bool currentValue, Vector2 position, float width)
        {
            GameObject toggleObj = GUIManager.Instance.CreateToggle(
                parent: parent.transform,
                width: FieldHeight,
                height: FieldHeight
            );

            float leftAlignedX = position.x - width / 2f + FieldHeight / 2f + 5f;

            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.5f, 1f);
            toggleRt.anchorMax = new Vector2(0.5f, 1f);
            toggleRt.pivot = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(leftAlignedX, position.y - FieldHeight / 4f);

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = currentValue;
            return toggle;
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
                fontSize: TooltipFontSize,
                color: new Color(0.78f, 0.78f, 0.78f, 1f),
                outline: false,
                outlineColor: Color.black,
                width: TooltipWidth,
                height: TooltipHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            t.alignment = TextAnchor.MiddleLeft;
            t.transform.SetAsLastSibling();
        }

        // =====================================================================
        // Field builders
        // =====================================================================

        private static InputField BuildStringField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue);
            input.onValueChanged.AddListener(val => field.SetValue(target, val));
            return input;
        }

        private const float PrefixBoxWidth = 60f;
        private const float PrefixBoxGap = 6f;

        /// <summary>
        /// Renders a disabled prefix box (e.g. "WBTI") to the left of the actual input, both
        /// sharing the row's normal field width. The box only ever displays the prefix; the
        /// input shows the value with the prefix stripped, but the underlying field is saved
        /// with the prefix prepended (e.g. "WBTI Spawn Troll").
        /// </summary>
        private static InputField BuildStringFieldWithPrefix(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width, string prefix)
        {
            float prefixX = position.x - width / 2f + PrefixBoxWidth / 2f;
            InputField prefixField = CreateInputField(parent, new Vector2(prefixX, position.y), PrefixBoxWidth, prefix);
            prefixField.textComponent.alignment = TextAnchor.MiddleCenter;
            prefixField.interactable = false;

            // interactable = false normally tints the field with Selectable's disabledColor
            // (dimmed grey) - this box should stay non-interactable but still look enabled.
            ColorBlock cb = prefixField.colors;
            cb.disabledColor = cb.normalColor;
            prefixField.colors = cb;

            float inputWidth = width - PrefixBoxWidth - PrefixBoxGap;
            float inputX = prefixX + PrefixBoxWidth / 2f + PrefixBoxGap + inputWidth / 2f;

            string prefixWithSpace = prefix + " ";
            string displayValue = currentValue.StartsWith(prefixWithSpace) ? currentValue.Substring(prefixWithSpace.Length) : currentValue;

            InputField input = CreateInputField(parent, new Vector2(inputX, position.y), inputWidth, displayValue);
            input.onValueChanged.AddListener(val => field.SetValue(target, string.IsNullOrEmpty(val) ? "" : prefixWithSpace + val));
            return input;
        }

        private static InputField BuildIntField(GameObject parent, object target, FieldInfo field, int currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString());
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (int.TryParse(val, out int result))
                    field.SetValue(target, result);
            });
            return input;
        }

        private static InputField BuildFloatField(GameObject parent, object target, FieldInfo field, float currentValue, Vector2 position, float width)
        {
            InputField input = CreateInputField(parent, position, width, currentValue.ToString("G"));
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (float.TryParse(val, out float result))
                    field.SetValue(target, result);
            });
            return input;
        }

        private static InputField BuildNullableFloatField(GameObject parent, object target, FieldInfo field, float? currentValue, Vector2 position, float width)
        {
            string initial = currentValue.HasValue ? currentValue.Value.ToString("G") : "";
            InputField input = CreateInputField(parent, position, width, initial);
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (string.IsNullOrWhiteSpace(val))
                    field.SetValue(target, (float?)null);
                else if (float.TryParse(val, out float result))
                    field.SetValue(target, (float?)result);
            });
            return input;
        }

        private static InputField BuildNullableIntField(GameObject parent, object target, FieldInfo field, int? currentValue, Vector2 position, float width)
        {
            string initial = currentValue.HasValue ? currentValue.Value.ToString() : "";
            InputField input = CreateInputField(parent, position, width, initial);
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                if (string.IsNullOrWhiteSpace(val))
                    field.SetValue(target, (int?)null);
                else if (int.TryParse(val, out int result))
                    field.SetValue(target, (int?)result);
            });
            return input;
        }

        private static Toggle BuildBoolField(GameObject parent, object target, FieldInfo field, bool currentValue, Vector2 position, float width)
        {
            GameObject toggleObj = GUIManager.Instance.CreateToggle(
                parent: parent.transform,
                width: FieldHeight,
                height: FieldHeight
            );

            // Align to the left edge of the field slot rather than its center
            float leftAlignedX = position.x - width / 2f + FieldHeight / 2f + 5f;

            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.5f, 1f);
            toggleRt.anchorMax = new Vector2(0.5f, 1f);
            toggleRt.pivot = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(leftAlignedX, position.y - FieldHeight / 4f);

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = currentValue;
            toggle.onValueChanged.AddListener(val => field.SetValue(target, val));
            return toggle;
        }

        private static bool BuildListField(GameObject parent, object target, FieldInfo field, List<string> currentValue, Vector2 rowPosition, float fieldWidth, List<string> dropdownOptions, string labelText, string tooltip)
        {
            // Label - same as all other fields
            Text listLabelComp = GUIManager.Instance.CreateText(
                text: labelText,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            listLabelComp.alignment = TextAnchor.MiddleLeft;

            // Input/dropdown center - identical to all other field types
            float fieldX = rowPosition.x + LabelWidth / 2f + LabelFieldGap + fieldWidth / 2f;
            Vector2 fieldPos = new Vector2(fieldX, rowPosition.y);

            string sentinelValue = null;
            Action onSentinelSelected = null;

            if (field.GetCustomAttribute<LogPrefabNameDropdownAttribute>() != null)
            {
                dropdownOptions = new List<string>(GetAvailableLogPrefabOptions()) { BiomeSpecificSentinel };
                sentinelValue = BiomeSpecificSentinel;

                var onChanged = field.GetCustomAttribute<OnValueChangedAttribute>();
                if (onChanged != null)
                    onSentinelSelected = () => InvokeOnValueChanged(parent, target, onChanged.MethodName);
            }

            ListEditor editor = dropdownOptions != null && dropdownOptions.Count > 0
                ? new ListEditor(currentValue ?? new List<string>(), dropdownOptions, sentinelValue, onSentinelSelected)
                : new ListEditor(currentValue ?? new List<string>());

            editor.Build(parent, fieldPos, fieldWidth);

            // Tooltip - to the right of the field, same as all other field types
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
                fontSize: LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            // Three inputs - X, Y, Z - evenly split across fieldWidth
            float fieldStartX = rowPosition.x + LabelWidth / 2f + LabelFieldGap;
            float subWidth = (fieldWidth - 10f) / 3f; // 10f = 2 gaps of 5f
            float subHeight = FieldHeight;
            const float subGap = 5f;
            const float subLabelW = 14f;

            string[] labels = { "X", "Y", "Z" };
            float[] values = { currentValue.x, currentValue.y, currentValue.z };

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
                    fontSize: SubLabelFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: false,
                    outlineColor: Color.black,
                    width: subLabelW,
                    height: subHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                axisLabel.alignment = TextAnchor.MiddleCenter;

                float inputW = subWidth - subLabelW - 2f;
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
                float fieldEndX = fieldStartX + fieldWidth;
                float tooltipX = fieldEndX + TooltipGap + TooltipWidth / 2f;
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

        // Prefab registration doesn't change mid-session, so this scan (every registered
        // prefab, two GetComponent calls each) only needs to run once and is cached here.
        private static List<DropdownOption> s_cachedCreaturePrefabOptions;

        /// <summary>
        /// Value = prefab name (what gets stored), Label = the creature's localized display
        /// name where available, matching how <c>WizshBoneTwitchIntegration.cs</c> already
        /// identifies spawnable creatures (Humanoid + MonsterAI).
        /// </summary>
        public static List<DropdownOption> GetAvailableCreaturePrefabOptions()
        {
            if (s_cachedCreaturePrefabOptions != null)
                return s_cachedCreaturePrefabOptions;

            if (ZNetScene.instance == null)
                return new List<DropdownOption>();

            var options = new List<DropdownOption>();
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                if (prefab == null)
                    continue;

                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();
                if (humanoid == null || monsterAI == null)
                    continue;

                string label = !string.IsNullOrEmpty(humanoid.m_name)
                    ? Localization.instance.Localize(humanoid.m_name)
                    : name;
                options.Add(new DropdownOption(name, label));
            }

            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            s_cachedCreaturePrefabOptions = options;
            return options;
        }

        /// <summary>
        /// Resolves the same localized label <see cref="GetAvailableCreaturePrefabOptions"/> shows
        /// in the dropdown, for use anywhere else a creature's prefab name is displayed (e.g. an
        /// entry list). Falls back to <paramref name="prefabName"/> itself if it's not a known
        /// Humanoid+MonsterAI creature.
        /// </summary>
        public static string GetCreatureDisplayName(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return prefabName;

            foreach (DropdownOption option in GetAvailableCreaturePrefabOptions())
            {
                if (option.Value == prefabName)
                    return option.Label;
            }

            return prefabName;
        }

        // Prefab registration doesn't change mid-session, so this scan only needs to run once
        // and is cached here, mirroring s_cachedCreaturePrefabOptions.
        private static List<DropdownOption> s_cachedDoorPrefabOptions;
        private static HashSet<string> s_cachedPlaceablePrefabNames;

        /// <summary>
        /// Every prefab reachable from a build tool's piece list (Hammer, Cultivator, Hoe,
        /// artisan tables, ...) - i.e. everything a player can actually place, as opposed to
        /// dungeon/world-only variants that share a component (e.g. <c>Door</c>) but were never
        /// meant to be built and can end up in a broken state (some can't be closed again once
        /// opened). Found by scanning every registered prefab's <see cref="ItemDrop"/> for a
        /// <see cref="PieceTable"/> (<c>ItemData.SharedData.m_buildPieces</c>) and collecting the
        /// union of every table's <see cref="PieceTable.m_pieces"/>.
        /// </summary>
        private static HashSet<string> GetPlaceablePrefabNames()
        {
            if (s_cachedPlaceablePrefabNames != null)
                return s_cachedPlaceablePrefabNames;

            if (ZNetScene.instance == null)
                return new HashSet<string>();

            var names = new HashSet<string>();
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                PieceTable pieceTable = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;
                if (pieceTable == null)
                    continue;

                foreach (GameObject piece in pieceTable.m_pieces)
                {
                    if (piece != null)
                        names.Add(piece.name);
                }
            }

            s_cachedPlaceablePrefabNames = names;
            return names;
        }

        /// <summary>
        /// Value = prefab name (what gets stored), Label = the door's localized piece name
        /// where available. Matches how <see cref="Extensions.SpawnAbilityExtension"/> identifies
        /// a spawned piece as a toggleable door (a <c>Door</c> component on the prefab), further
        /// restricted to prefabs a player can actually place (see <see cref="GetPlaceablePrefabNames"/>)
        /// since some non-placeable doors can't be closed again once opened.
        /// </summary>
        public static List<DropdownOption> GetAvailableDoorPrefabOptions()
        {
            if (s_cachedDoorPrefabOptions != null)
                return s_cachedDoorPrefabOptions;

            if (ZNetScene.instance == null)
                return new List<DropdownOption>();

            HashSet<string> placeableNames = GetPlaceablePrefabNames();

            var options = new List<DropdownOption>();
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                if (!placeableNames.Contains(name))
                    continue;

                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                if (prefab == null)
                    continue;

                Door door = prefab.GetComponent<Door>();
                if (door == null)
                    continue;

                Piece piece = prefab.GetComponent<Piece>();
                string label = piece != null && !string.IsNullOrEmpty(piece.m_name)
                    ? Localization.instance.Localize(piece.m_name)
                    : name;
                options.Add(new DropdownOption(name, label));
            }

            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            s_cachedDoorPrefabOptions = options;
            return options;
        }

        // Selected value in a LogPrefabNameDropdown list that means "replace this list with the
        // 8 default per-biome logs and switch to biome-based selection" instead of being added
        // as a literal (invalid) prefab name - handled by the field's OnValueChanged method.
        public const string BiomeSpecificSentinel = "Biome specific (sets 8 default per-biome logs)";

        // Log prefabs have no shared identifying component to scan for (unlike Door/Humanoid+MonsterAI),
        // so this is a curated candidate list seeded from every live "Log Rain" redeem config, filtered
        // to prefabs that actually resolve so a renamed/missing prefab doesn't appear as an option.
        private static readonly string[] LogPrefabCandidates =
        {
            "beech_log_half", "PineTree_log_half", "SwampTree1_log", "FirTree_log_half",
            "Birch_log_half", "yggashoot_log_half", "AshlandsTreeLogHalf2", "Oak_log_half"
        };

        private static List<string> s_cachedLogPrefabOptions;

        public static List<string> GetAvailableLogPrefabOptions()
        {
            if (s_cachedLogPrefabOptions != null)
                return s_cachedLogPrefabOptions;

            if (ZNetScene.instance == null)
                return new List<string>(LogPrefabCandidates);

            var options = LogPrefabCandidates.Where(name => PrefabManager.Instance.GetPrefab(name) != null).ToList();
            s_cachedLogPrefabOptions = options;
            return options;
        }

        /// <summary>
        /// Returns <paramref name="options"/> with <paramref name="currentValue"/> added in
        /// (sorted) if it's missing - so a saved value that no longer matches a dropdown's
        /// live filter (e.g. a creature without the required components) still displays
        /// correctly instead of being silently swapped for the first option.
        /// </summary>
        private static List<DropdownOption> EnsureIncludesCurrentValue(List<DropdownOption> options, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue) || options.Any(o => o.Value == currentValue))
                return options;

            var withCurrent = new List<DropdownOption>(options) { new DropdownOption(currentValue, currentValue) };
            withCurrent.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return withCurrent;
        }

        // Mirrors ListEditor's private constants - used to compute layout alignment
        private const float ListEditorInputW = 200f;
        private const float ListEditorBtnW = 60f;
        private const float ListEditorGap = 10f;
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
                fontSize: FieldFontSize,
                width: width,
                height: FieldHeight
            );

            InputField inputField = inputObj.GetComponent<InputField>();
            inputField.textComponent.alignment = TextAnchor.MiddleLeft;
            StylePlaceholder(inputField);
            inputField.text = initialValue;
            return inputField;
        }

        /// <summary>
        /// Jotunn leaves the placeholder Text at Unity's default alignment (upper-left) and a low-contrast
        /// grey, while the real input text is styled separately (see <see cref="CreateInputField"/>). This
        /// brings the placeholder in line: center-left aligned, larger, and lighter for readability.
        /// </summary>
        internal static void StylePlaceholder(InputField field)
        {
            if (field.placeholder is Text placeholder)
            {
                placeholder.alignment = TextAnchor.MiddleLeft;
                placeholder.fontSize = PlaceholderFontSize;
                placeholder.color = PlaceholderColor;
            }
        }

        private static SearchableDropdown BuildStringDropdownField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width, List<string> options)
        {
            SearchableDropdown dropdown = new SearchableDropdown();
            dropdown.Build(parent, position, width, FieldHeight, options, currentValue);
            dropdown.OnValueChanged += val => field.SetValue(target, val);
            return dropdown;
        }

        private static SearchableDropdown BuildStringDropdownField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width, List<DropdownOption> options)
        {
            SearchableDropdown dropdown = new SearchableDropdown();
            dropdown.Build(parent, position, width, FieldHeight, options, currentValue);
            dropdown.OnValueChanged += val => field.SetValue(target, val);
            return dropdown;
        }

        private static void BuildColorField(GameObject parent, object target, FieldInfo field, string currentValue, Vector2 position, float width)
        {
            Color initialColor = ParseHexColor(currentValue);

            // Button - keeps its default Valheim style so borders are visible
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
                Color currentColor = ParseHexColor(field.GetValue(target) as string ?? "");

                GUIManager.Instance.CreateColorPicker(
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: Vector2.zero,
                    original: currentColor,
                    message: field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name,
                    onColorChanged: (Color c) =>
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

        public static float LookupStatusEffectTTL(string name)
        {
            if (ObjectDB.instance == null || string.IsNullOrEmpty(name))
                return 10f;

            foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
            {
                if (se != null && se.name == name)
                    return se.m_ttl > 0f ? se.m_ttl : 10f;
            }

            return 10f;
        }

        private static void InvokeOnValueChanged(GameObject parent, object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method == null) return;

            method.Invoke(target, null);

            // Sync any InputFields whose displayed value is out of sync with target's float fields
            var floatFields = target.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (InputField inputField in parent.GetComponentsInChildren<InputField>())
            {
                foreach (var f in floatFields)
                {
                    if (f.FieldType != typeof(float)) continue;
                    string expected = ((float)f.GetValue(target)).ToString("G");
                    if (inputField.text != expected && float.TryParse(inputField.text, out _))
                    {
                        inputField.text = expected;
                        break;
                    }
                }
            }
        }

        private static List<string> GetAvailableStatusEffectNames()
        {
            List<string> names = new List<string>();

            foreach (PropertyInfo prop in typeof(Types.StatusEffectType)
                .GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (prop.PropertyType != typeof(string))
                    continue;

                try
                {
                    string value = prop.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(value))
                        names.Add(value);
                }
                catch
                {
                    // Skip properties whose backing assets aren't loaded yet
                }
            }

            names.Sort();
            return names;
        }

        // Status effect names don't change mid-session, so this lookup (reflection over
        // StatusEffectType plus an ObjectDB scan for display names) only needs to run once
        // and is cached here, mirroring GetAvailableCreaturePrefabOptions.
        private static List<DropdownOption> s_cachedStatusEffectOptions;

        /// <summary>
        /// Value = the status effect's internal name (matching <see cref="Types.StatusEffectType"/>
        /// and what gets stored), Label = its localized display name where a matching loaded
        /// <see cref="StatusEffect"/> asset is found, else the value itself.
        /// </summary>
        public static List<DropdownOption> GetAvailableStatusEffectOptions()
        {
            if (s_cachedStatusEffectOptions != null)
                return s_cachedStatusEffectOptions;

            if (ObjectDB.instance == null)
                return new List<DropdownOption>();

            Dictionary<string, string> displayNames = new Dictionary<string, string>();
            foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
            {
                if (se != null && !string.IsNullOrEmpty(se.m_name))
                    displayNames[se.name] = Localization.instance.Localize(se.m_name);
            }

            var options = new List<DropdownOption>();
            foreach (string name in GetAvailableStatusEffectNames())
            {
                string label = displayNames.TryGetValue(name, out string localized) ? localized : name;
                options.Add(new DropdownOption(name, label));
            }

            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            s_cachedStatusEffectOptions = options;
            return options;
        }
    }
}