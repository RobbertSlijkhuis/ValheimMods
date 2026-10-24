using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Save-and-apply-live-effects helper for editing <see cref="ProfileSettingsHelper.Current"/>
    /// from the new UI - every field write in Home/Settings calls this immediately after, so there's
    /// no separate Save button ("each edit writes straight through").
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
