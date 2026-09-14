using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Overrides the auto-generated label text for a field in <see cref="ObjectEditor"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class EditorLabelAttribute : Attribute
    {
        public string Label { get; }

        public EditorLabelAttribute(string label)
        {
            Label = label;
        }
    }
}