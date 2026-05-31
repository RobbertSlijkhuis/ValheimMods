using System;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Marks a field as hidden from <see cref="ObjectEditor"/>.
    /// Apply to fields on data objects that should not appear in the generated UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class EditorHiddenAttribute : Attribute { }
}