using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace WizshBoneTwitchIntegration.Models
{
    /// <summary>
    /// Base class for data models that support shallow and deep cloning.
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
    }
}