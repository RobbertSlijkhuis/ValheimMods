using System;
using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Marks a <see cref="List{T}"/> of <see cref="string"/> field as dropdown-driven in the UI.
    /// Provide the available options directly via <see cref="Options"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class DropdownOptionsAttribute : Attribute
    {
        public List<string> Options { get; }

        public DropdownOptionsAttribute(params string[] options)
        {
            Options = new List<string>(options);
        }
    }
}