using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace WizshBoneTwitchIntegration.Models
{
    /// <summary>
    /// Base class for data models that support shallow and deep cloning,
    /// and minimal serialization (only non-default fields).
    /// </summary>
    internal abstract class CloneableData
    {
        public T Clone<T>() where T : CloneableData, new()
        {
            T clone = new T();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                field.SetValue(clone, field.GetValue(this));

            return clone;
        }

        public T DeepClone<T>() where T : CloneableData, new()
        {
            T clone = new T();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(this);

                if (value is CloneableData nested)
                {
                    object deepCloned = typeof(CloneableData)
                        .GetMethod(nameof(DeepClone))
                        .MakeGenericMethod(field.FieldType)
                        .Invoke(nested, null);

                    field.SetValue(clone, deepCloned);
                }
                else
                {
                    field.SetValue(clone, value);
                }
            }

            return clone;
        }

        /// <summary>
        /// Returns a dictionary containing only fields whose values differ from
        /// the defaults of a freshly constructed instance of the same type.
        /// Nested <see cref="CloneableData"/> fields are recursively minimised
        /// and only included when they contain at least one non-default value.
        /// </summary>
        public Dictionary<string, object> ToMinimalDictionary()
        {
            object defaults = Activator.CreateInstance(GetType());
            var result      = new Dictionary<string, object>();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value        = field.GetValue(this);
                object defaultValue = field.GetValue(defaults);

                if (value is CloneableData nested)
                {
                    Dictionary<string, object> nestedDict = nested.ToMinimalDictionary();
                    if (nestedDict.Count > 0)
                        result[field.Name] = nestedDict;
                }
                else if (!IsEqual(value, defaultValue))
                {
                    result[field.Name] = value;
                }
            }

            return result;
        }

        // ── private helpers ──────────────────────────────────────────────────

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            // Compare lists by content
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