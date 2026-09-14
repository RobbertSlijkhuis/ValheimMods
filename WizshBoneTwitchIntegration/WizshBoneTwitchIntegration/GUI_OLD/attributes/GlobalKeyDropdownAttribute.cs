using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Marks a <see cref="string"/> field as a dropdown populated from the known Valheim global
    /// keys (see <see cref="Types.GlobalKeyType"/>) - e.g. a redeem's Add/Remove condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class GlobalKeyDropdownAttribute : Attribute { }
}
