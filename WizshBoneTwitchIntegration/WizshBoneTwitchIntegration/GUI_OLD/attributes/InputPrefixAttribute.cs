using System;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Renders a small disabled box showing <see cref="Prefix"/> immediately before a string
    /// field's input in <see cref="ObjectEditor"/>, to hint that the value carries this prefix.
    /// Purely cosmetic - does not alter the underlying field value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class InputPrefixAttribute : Attribute
    {
        public string Prefix { get; }

        public InputPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}
