using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.GuiOld;

namespace WizshBoneTwitchIntegration.Models
{
    internal class ViewerEntry : CloneableData
    {
        [EditorLabel("Viewer Name (lower case only)")]
        public string name;

        [EditorLabel("Creature color")]
        [EditorTooltip("The color of the creature's main texture in hex format (e.g. #ffffff for white).")]
        [ColorPicker]
        public string color = "#ffffff";

        [EditorLabel("Creature emission color")]
        [EditorTooltip("The color of the creature's glow in hex format. Leave empty to use the creature color.")]
        [ColorPicker]
        public string emissionColor = "";

        [EditorHidden] public List<string> effects = new List<string>();
        [EditorHidden] public Color parsedColor;
        [EditorHidden] public Color parsedEmissionColor;

        // Read-only legacy key: viewers.yaml used to store the creature color as "color1". It is only
        // ever deserialized (never written back) so MigrateLegacyFields can carry it over to color.
        [EditorHidden] public string color1;

        private static readonly HashSet<string> SkippedFields = new HashSet<string>
        {
            nameof(parsedColor),
            nameof(parsedEmissionColor),
            nameof(color1)
        };

        public ViewerEntry() { }

        public void Init()
        {
            ColorUtility.TryParseHtmlString(color, out parsedColor);

            if (string.IsNullOrEmpty(emissionColor) || !ColorUtility.TryParseHtmlString(emissionColor, out parsedEmissionColor))
                parsedEmissionColor = parsedColor;
        }

        /// <summary>
        /// Carries a pre-<see cref="color"/> "color1" value over to <see cref="color"/>, then clears it
        /// so the next save writes the new key. A no-op for entries that don't have the legacy key.
        /// </summary>
        public void MigrateLegacyFields()
        {
            if (string.IsNullOrEmpty(color1))
                return;

            color = color1;
            color1 = null;
        }

        /// <summary>
        /// Returns a dictionary with only fields that differ from their defaults,
        /// with <see cref="name"/> always included as it is required.
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            object defaults = new ViewerEntry();
            var result      = new Dictionary<string, object>();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (SkippedFields.Contains(field.Name))
                    continue;

                object value = field.GetValue(this);

                if (field.Name == nameof(name))
                {
                    result[field.Name] = value ?? "";
                    continue;
                }

                if (value == null || IsEqual(value, field.GetValue(defaults)))
                    continue;

                result[field.Name] = value;
            }

            return result;
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            if (a is IList listA && b is IList listB)
            {
                if (listA.Count != listB.Count) return false;
                for (int i = 0; i < listA.Count; i++)
                    if (!Equals(listA[i], listB[i])) return false;
                return true;
            }

            return a.Equals(b);
        }
    }
}
