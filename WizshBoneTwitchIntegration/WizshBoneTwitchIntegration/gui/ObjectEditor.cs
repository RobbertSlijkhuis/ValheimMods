using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ObjectEditor
    {
        private const float EntrySpacing     = 8f;
        private const float SectionHeaderHeight = 28f;
        private const float SectionHeaderSpacing = 4f;

        private readonly float m_startX;
        private readonly float m_startY;
        private readonly float m_fieldWidth;

        /// <param name="startX">Anchor-relative X of the label's center for each row.</param>
        /// <param name="startY">Anchor-relative Y of the first row.</param>
        /// <param name="fieldWidth">Width of the input field portion of each row.</param>
        public ObjectEditor(float startX = -80f, float startY = -20f, float fieldWidth = 200f)
        {
            m_startX     = startX;
            m_startY     = startY;
            m_fieldWidth = fieldWidth;
        }

        /// <summary>
        /// Iterates all public instance fields on <paramref name="target"/> and builds
        /// a label + input row for each supported type (<see cref="string"/>, <see cref="int"/>, 
        /// <see cref="float"/>, <see cref="bool"/>, <see cref="List{T}"/> of <see cref="string"/>).
        /// Fields whose type derives from <see cref="CloneableData"/> are rendered as an
        /// indented sub-section with a header label.
        /// Unsupported types are silently skipped.
        /// </summary>
        /// <returns>
        /// Total height consumed (in pixels), suitable for setting on the content
        /// <see cref="RectTransform"/> of a <see cref="UnityEngine.UI.ScrollRect"/>.
        /// </returns>
        public float Build(GameObject parent, object target)
        {
            return BuildFields(parent, target, m_startY);
        }

        internal float BuildFields(GameObject parent, object target, float startY)
        {
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            float yOffset = startY;

            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<EditorHiddenAttribute>() != null)
                    continue;

                // Nested CloneableData — render a section header, recurse, then render a section ender
                if (typeof(CloneableData).IsAssignableFrom(field.FieldType))
                {
                    if (field.FieldType == typeof(PositionOffsetData))
                        goto handleField;

                    object nestedValue = field.GetValue(target);
                    if (nestedValue == null)
                        continue;

                    string sectionLabel = field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name;

                    // Center X aligned with the input field column (mirrors FieldUIBuilder.Build's fieldX)
                    float fieldCenterX = m_startX + FieldUIBuilder.LabelWidth / 2f + FieldUIBuilder.LabelFieldGap + m_fieldWidth / 2f;

                    BuildSectionBoundary(parent, "— " + sectionLabel + " —", new Vector2(fieldCenterX, yOffset), m_fieldWidth);
                    yOffset -= SectionHeaderHeight + SectionHeaderSpacing;

                    yOffset -= BuildFields(parent, nestedValue, yOffset);

                    BuildSectionBoundary(parent, "— end of " + sectionLabel + " —", new Vector2(fieldCenterX, yOffset), m_fieldWidth);
                    yOffset -= SectionHeaderHeight + SectionHeaderSpacing;
                    continue;
                }

                // List<T> where T : CloneableData
                if (field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                    typeof(CloneableData).IsAssignableFrom(field.FieldType.GetGenericArguments()[0]))
                {
                    string listLabel = field.GetCustomAttribute<EditorLabelAttribute>()?.Label ?? field.Name;
                    float fieldCenterX = m_startX + FieldUIBuilder.LabelWidth / 2f + FieldUIBuilder.LabelFieldGap + m_fieldWidth / 2f;

                    BuildSectionBoundary(parent, "— " + listLabel + " —", new Vector2(fieldCenterX, yOffset), m_fieldWidth);
                    yOffset -= SectionHeaderHeight + SectionHeaderSpacing;

                    var listEditor = new ObjectListEditor(
                        field.GetValue(target) as IList,
                        field.FieldType.GetGenericArguments()[0],
                        m_startX,
                        m_fieldWidth);
                    float listHeight = listEditor.Build(parent, yOffset);
                    yOffset -= listHeight + EntrySpacing;

                    BuildSectionBoundary(parent, "— end of " + listLabel + " —", new Vector2(fieldCenterX, yOffset), m_fieldWidth);
                    yOffset -= SectionHeaderHeight + SectionHeaderSpacing;
                    continue;
                }

                handleField:
                List<string> dropdownOptions = GetDropdownOptionsForField(target, field);

                bool built = FieldUIBuilder.Build(
                    parent:              parent,
                    target:              target,
                    field:               field,
                    rowPosition:         new Vector2(m_startX, yOffset),
                    fieldWidth:          m_fieldWidth,
                    listDropdownOptions: dropdownOptions
                );

                if (built)
                    yOffset -= FieldUIBuilder.GetEntryHeight(field) + EntrySpacing;
            }

            return Mathf.Abs(yOffset - startY);
        }

        private void BuildSectionBoundary(GameObject parent, string text, Vector2 position, float width)
        {
            var headerObj = GUIManager.Instance.CreateText(
                text: text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: SectionHeaderHeight,
                addContentSizeFitter: false
            );

            var textComp = headerObj.GetComponent<UnityEngine.UI.Text>();
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontStyle = FontStyle.Bold;
        }

        private static List<string> GetDropdownOptionsForField(object target, FieldInfo field)
        {
            if (field.FieldType != typeof(List<string>))
                return null;

            // Attribute-driven dropdown options
            var attr = field.GetCustomAttribute<DropdownOptionsAttribute>();
            if (attr != null)
                return attr.Options;

            // WeatherData.items uses live environment names from EnvMan
            if (target is WeatherData && field.Name == "items")
                return FieldUIBuilder.GetAvailableWeatherNames();

            return null;
        }
    }
}