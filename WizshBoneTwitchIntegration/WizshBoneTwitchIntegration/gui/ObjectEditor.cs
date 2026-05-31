using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ObjectEditor
    {
        private const float EntrySpacing    = 8f;
        private const float ListEntryHeight = 210f; // Label + input + list scroll area

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
        /// Unsupported types are silently skipped.
        /// </summary>
        /// <returns>
        /// Total height consumed (in pixels), suitable for setting on the content
        /// <see cref="RectTransform"/> of a <see cref="UnityEngine.UI.ScrollRect"/>.
        /// </returns>
        public float Build(GameObject parent, object target)
        {
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            float yOffset = m_startY;

            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<EditorHiddenAttribute>() != null)
                    continue;

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

            return Mathf.Abs(yOffset - m_startY);
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