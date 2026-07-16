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
                    field.SetValue(clone, DeepCloneValue(nested, field.FieldType));
                }
                else if (value is IList list)
                {
                    field.SetValue(clone, DeepCloneList(list, field.FieldType));
                }
                else
                {
                    field.SetValue(clone, value);
                }
            }

            return clone;
        }

        private static object DeepCloneValue(CloneableData nested, Type type)
        {
            return typeof(CloneableData)
                .GetMethod(nameof(DeepClone))
                .MakeGenericMethod(type)
                .Invoke(nested, null);
        }

        private static IList DeepCloneList(IList list, Type listType)
        {
            IList clone = (IList)Activator.CreateInstance(listType);

            foreach (object item in list)
            {
                if (item is CloneableData nested)
                    clone.Add(DeepCloneValue(nested, item.GetType()));
                else
                    clone.Add(item);
            }

            return clone;
        }
    }
}