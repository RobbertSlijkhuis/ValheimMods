namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Sidebar tabs for the new UI shell. Labels mirror GUI_OLD's 7 tab names for continuity
    /// into later rounds; only this shell's tab-switching mechanics exist so far - each tab's
    /// real content (Home cards, Profiles list, etc.) is future rounds' scope, and shows a stub
    /// label for now (see <see cref="WizshBoneShellGUI"/>).
    /// </summary>
    internal enum ShellTab
    {
        Home,
        Profiles,
        Redeems,
        Settings,
        CreatureGroups,
        Viewers,
        Debug
    }

    internal static class ShellTabExtensions
    {
        /// <summary>
        /// All tabs, in the order they should appear in the sidebar.
        /// </summary>
        public static readonly ShellTab[] All =
        {
            ShellTab.Home,
            ShellTab.Profiles,
            ShellTab.Redeems,
            ShellTab.Settings,
            ShellTab.CreatureGroups,
            ShellTab.Viewers,
            ShellTab.Debug
        };

        public static string Label(this ShellTab tab)
        {
            switch (tab)
            {
                case ShellTab.Home: return "Home";
                case ShellTab.Profiles: return "Profiles";
                case ShellTab.Redeems: return "Redeems";
                case ShellTab.Settings: return "Settings";
                case ShellTab.CreatureGroups: return "Creature groups";
                case ShellTab.Viewers: return "Viewers";
                case ShellTab.Debug: return "Debug";
                default: return tab.ToString();
            }
        }
    }
}
