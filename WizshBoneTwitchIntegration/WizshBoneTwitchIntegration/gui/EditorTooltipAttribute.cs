using System;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Adds a descriptive tooltip line rendered below the input field in <see cref="ObjectEditor"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class EditorTooltipAttribute : Attribute
    {
        public string Tooltip { get; }

        public EditorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}