using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Save-and-apply-live-effects helper for editing <see cref="ProfileSettingsHelper.Current"/>
    /// from the new UI - every field write in Home/Settings calls this immediately after, so there's
    /// no separate Save button (same "each edit writes straight through" behavior as the old UI).
    /// Deliberately a new-UI-only copy of <c>GUI_OLD/tabs/RulesSettingsViews.cs</c>'s
    /// <c>RulesSettingsViewHelper.Persist()</c> rather than a shared reference, per the isolation-
    /// from-GUI_OLD constraint.
    /// </summary>
    internal static class ProfileSettingsPersistHelper
    {
        public static void Persist()
        {
            ProfileSettingsHelper.Save();
            ProfileSettingsHelper.ApplyToLiveComponents();
        }
    }
}
