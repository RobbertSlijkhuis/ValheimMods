using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Marks a <see cref="string"/> field as a dropdown populated at runtime from every
    /// registered prefab with a <c>Door</c> component - used by the "Doors of Doom" redeem
    /// to pick which door prefab gets spawned.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class DoorPrefabNameDropdownAttribute : Attribute { }
}
