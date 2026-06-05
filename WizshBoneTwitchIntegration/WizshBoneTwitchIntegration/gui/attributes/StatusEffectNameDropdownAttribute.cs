using System;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Marks a <see cref="string"/> field as a dropdown populated at runtime
    /// from all values defined in <see cref="WizshBoneTwitchIntegration.Types.StatusEffectType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class StatusEffectNameDropdownAttribute : Attribute { }
}