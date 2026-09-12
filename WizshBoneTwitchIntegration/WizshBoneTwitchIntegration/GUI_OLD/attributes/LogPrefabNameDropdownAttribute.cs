using System;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Marks a <see cref="System.Collections.Generic.List{T}"/> of <see cref="string"/> field as
    /// a dropdown-driven list populated from a curated set of log prefabs, plus a
    /// "Biome specific" sentinel entry (see <see cref="FieldUIBuilder.BiomeSpecificSentinel"/>) -
    /// used by the "Log Rain" redeem to pick which log prefabs get rained.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class LogPrefabNameDropdownAttribute : Attribute { }
}
