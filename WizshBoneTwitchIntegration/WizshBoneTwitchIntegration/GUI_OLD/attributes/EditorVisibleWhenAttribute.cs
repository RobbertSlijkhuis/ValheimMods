using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Marks a field as visible in <see cref="ObjectEditor"/> only when the sibling
    /// string field named <see cref="FieldName"/> currently holds one of <see cref="Values"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class EditorVisibleWhenAttribute : Attribute
    {
        public string FieldName { get; }
        public string[] Values { get; }

        public EditorVisibleWhenAttribute(string fieldName, params string[] values)
        {
            FieldName = fieldName;
            Values = values;
        }
    }
}
