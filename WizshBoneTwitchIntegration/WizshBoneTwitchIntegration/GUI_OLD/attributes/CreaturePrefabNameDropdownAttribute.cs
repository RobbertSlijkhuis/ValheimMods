using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Marks a <see cref="string"/> field as a dropdown populated at runtime from every
    /// registered prefab with both a <c>Humanoid</c> and a <c>MonsterAI</c> component -
    /// the same criteria the mod uses elsewhere to identify spawnable creatures.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class CreaturePrefabNameDropdownAttribute : Attribute { }
}
